using UnityEngine;
using System.Collections.Generic;
using GameCore;

public class CashRegisterManager : MonoBehaviour
{
    private const int InitialRegisterCount = 2; // Количество касс в начале игры
    private const float AutoManagementCheckInterval = 30f; // Интервал проверки автоуправления
    private const float QueueLengthToTurnOn = 3f; // Длина очереди для включения касс
    private const float QueueLengthToTurnOff = 1f; // Длина очереди для выключения касс
    private const int MinWorkingRegisters = 2; // Минимальное количество рабочих касс
    private const int BasicRegisterIndex = 0; // Индекс префаба базовой кассы
    private const int EnhancedRegisterIndex = 1; // Индекс префаба улучшенной кассы
    private const int PremiumRegisterIndex = 2; // Индекс префаба премиум-кассы

    [Header("Зависимости")]
    [SerializeField] private QueueManager queueManager;

    [Header("Управление кассами")]
    [SerializeField] private List<CashRegister> allRegisters = new List<CashRegister>();
    [SerializeField] private Transform[] registerPositions;
    [SerializeField] private GameObject[] registerPrefabs; // Префабы: Basic, Enhanced, Premium

    [Header("Ограничения касс")]
    [SerializeField] private int maxRegisters = 4;
    [SerializeField] private int currentRegisterCount = 0;

    [Header("Автоуправление")]
    [SerializeField] private bool autoManagementEnabled = false;
    private float lastManagementCheck = 0f;

    private void Start()
    {
        InitializeStartingRegisters();
    }

    private void Update()
    {
        if (autoManagementEnabled && Time.time - lastManagementCheck >= AutoManagementCheckInterval)
        {
            AutoManageRegisters();
            lastManagementCheck = Time.time;
        }
    }

    private void InitializeStartingRegisters()
    {
        for (int i = 0; i < InitialRegisterCount && i < registerPositions.Length; i++)
        {
            AddNewRegister(CashRegisterType.Basic);
        }
    }

    public bool AddNewRegister(CashRegisterType type)
    {
        if (IsRegisterLimitReached())
        {
            Debug.Log("Достигнут лимит касс!");
            return false;
        }

        if (IsNoPositionAvailable())
        {
            Debug.Log("Нет свободных позиций для касс!");
            return false;
        }

        GameObject prefab = GetRegisterPrefab(type);
        if (prefab == null)
        {
            return false;
        }

        return CreateNewRegister(type, prefab);
    }

    private bool IsRegisterLimitReached()
    {
        return currentRegisterCount >= maxRegisters;
    }

    private bool IsNoPositionAvailable()
    {
        return registerPositions.Length <= currentRegisterCount;
    }

    private GameObject GetRegisterPrefab(CashRegisterType type)
    {
        return type switch
        {
            CashRegisterType.Basic => registerPrefabs[BasicRegisterIndex],
            CashRegisterType.Enhanced => registerPrefabs[EnhancedRegisterIndex],
            CashRegisterType.Premium => registerPrefabs[PremiumRegisterIndex],
            _ => registerPrefabs[BasicRegisterIndex]
        };
    }

    private bool CreateNewRegister(CashRegisterType type, GameObject prefab)
    {
        Transform position = registerPositions[currentRegisterCount];
        GameObject newRegisterObj = Instantiate(prefab, position.position, position.rotation);
        CashRegister newRegister = newRegisterObj.GetComponent<CashRegister>();

        if (newRegister != null)
        {
            newRegister.registerID = currentRegisterCount + 1;
            newRegister.registerType = type;
            allRegisters.Add(newRegister);
            currentRegisterCount++;
            Debug.Log($"Добавлена касса {type} (ID: {newRegister.registerID})");
            return true;
        }

        Destroy(newRegisterObj);
        return false;
    }

    public void RemoveRegister(CashRegister register)
    {
        if (allRegisters.Remove(register))
        {
            currentRegisterCount--;
            Destroy(register.gameObject);
            Debug.Log($"Касса {register.registerID} удалена");
        }
    }

    public CashRegister GetAvailableRegister()
    {
        foreach (CashRegister register in allRegisters)
        {
            if (register.IsAvailable())
            {
                return register;
            }
        }
        return null;
    }

    public List<CashRegister> GetWorkingRegisters()
    {
        List<CashRegister> workingRegisters = new List<CashRegister>();
        foreach (CashRegister register in allRegisters)
        {
            if (register.currentState == CashRegisterState.Working)
            {
                workingRegisters.Add(register);
            }
        }
        return workingRegisters;
    }

    public List<CashRegister> GetBrokenRegisters()
    {
        List<CashRegister> brokenRegisters = new List<CashRegister>();
        foreach (CashRegister register in allRegisters)
        {
            if (register.currentState == CashRegisterState.Broken)
            {
                brokenRegisters.Add(register);
            }
        }
        return brokenRegisters;
    }

    public int GetBrokenRegistersCount()
    {
        return GetBrokenRegisters().Count;
    }

    public int GetTotalRegisters()
    {
        return allRegisters.Count;
    }

    public float GetTotalDailyIncome()
    {
        float totalIncome = 0f;
        foreach (CashRegister register in allRegisters)
        {
            if (register.currentState == CashRegisterState.Working)
            {
                totalIncome += register.incomePerMinute;
            }
        }
        return totalIncome;
    }

    private void AutoManageRegisters()
    {
        if (queueManager == null)
        {
            return;
        }

        float averageQueueLength = queueManager.GetAverageQueueLength();

        if (averageQueueLength > QueueLengthToTurnOn)
        {
            TurnOnIdleRegisters();
        }
        else if (averageQueueLength < QueueLengthToTurnOff)
        {
            TurnOffExcessRegisters();
        }
    }

    private void TurnOnIdleRegisters()
    {
        foreach (CashRegister register in allRegisters)
        {
            if (register.currentState == CashRegisterState.Off)
            {
                register.ChangeState(CashRegisterState.Working);
                Debug.Log($"Касса {register.registerID} включена автоматически");
                break;
            }
        }
    }

    private void TurnOffExcessRegisters()
    {
        if (GetWorkingRegisters().Count <= MinWorkingRegisters)
        {
            return;
        }

        foreach (CashRegister register in allRegisters)
        {
            if (register.currentState == CashRegisterState.Working && !register.isOccupied)
            {
                register.ChangeState(CashRegisterState.Off);
                Debug.Log($"Касса {register.registerID} выключена для экономии");
                break;
            }
        }
    }

    public void EnableAutoManagement(bool enabled)
    {
        autoManagementEnabled = enabled;
        Debug.Log($"Автоуправление кассами: {(enabled ? "включено" : "выключено")}");
    }

    public void UpdateMaxRegisters(int newMax)
    {
        maxRegisters = newMax;
        Debug.Log($"Максимальное количество касс: {maxRegisters}");
    }

    // Геттеры для доступа к состоянию
    public List<CashRegister> GetAllRegisters() => allRegisters;
    public int GetMaxRegisters() => maxRegisters;
    public int GetCurrentRegisterCount() => currentRegisterCount;
    public bool IsAutoManagementEnabled() => autoManagementEnabled;
}