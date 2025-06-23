using UnityEngine;
using System.Collections;
using GameCore;

public class CashRegister : MonoBehaviour
{
    private const float BasicIncomePerMinute = 15f; // Доход базовой кассы
    private const float EnhancedIncomePerMinute = 25f; // Доход улучшенной кассы
    private const float PremiumIncomePerMinute = 40f; // Доход премиум-кассы
    private const float BasicBreakdownChance = 0.1f; // Шанс поломки базовой кассы
    private const float EnhancedBreakdownChance = 0.07f; // Шанс поломки улучшенной кассы
    private const float PremiumBreakdownChance = 0.03f; // Шанс поломки премиум-кассы
    private const float MaxAttentionTime = 60f; // Время на решение проблемы
    private const float IncomeInterval = 60f; // Интервал генерации дохода (1 минута)
    private const float ScanningSoundVolume = 0.3f; // Громкость звука сканирования
    private const float BreakProbability = 0.3f; // Вероятность поломки при игноре
    private const float BlinkFrequency = 3f; // Частота мигания для визуального эффекта

    [Header("Зависимости")]
    [SerializeField] private EconomyManager economyManager;
    [SerializeField] private TimeManager timeManager;
    [SerializeField] private FineSystem fineSystem;

    [Header("Настройки кассы")]
    [SerializeField] private CashRegisterState currentState = CashRegisterState.Working;
    [SerializeField] private CashRegisterType registerType = CashRegisterType.Basic;
    [SerializeField] public int registerID; // Публичное поле для ID, как в оригинале

    [Header("Настройки дохода")]
    [SerializeField] private float incomePerMinute = BasicIncomePerMinute;
    [SerializeField] private float breakdownChance = BasicBreakdownChance;

    [Header("Текущий статус")]
    [SerializeField] private bool isOccupied = false;
    [SerializeField] private Customer currentCustomer;
    [SerializeField] private float attentionTimer = 0f;

    [Header("Визуальные эффекты")]
    [SerializeField] private Renderer screenRenderer;
    [SerializeField] private Color workingColor = Color.green;
    [SerializeField] private Color attentionColor = Color.yellow;
    [SerializeField] private Color brokenColor = Color.red;
    [SerializeField] private Color offColor = Color.black;

    [Header("Аудио")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip scanningSound;

    private Coroutine incomeCoroutine;

    private void Start()
    {
        SetIncomeByType();
        if (currentState == CashRegisterState.Working)
        {
            StartIncomeGeneration();
        }
        UpdateVisuals();
    }

    private void Update()
    {
        CheckForBreakdown();
        UpdateAttentionTimer();
        UpdateVisuals();
    }

    private void SetIncomeByType()
    {
        (incomePerMinute, breakdownChance) = registerType switch
        {
            CashRegisterType.Basic => (BasicIncomePerMinute, BasicBreakdownChance),
            CashRegisterType.Enhanced => (EnhancedIncomePerMinute, EnhancedBreakdownChance),
            CashRegisterType.Premium => (PremiumIncomePerMinute, PremiumBreakdownChance),
            _ => (BasicIncomePerMinute, BasicBreakdownChance)
        };
    }

    public void ChangeState(CashRegisterState newState)
    {
        CashRegisterState oldState = currentState;
        currentState = newState;

        switch (newState)
        {
            case CashRegisterState.Working:
                StartIncomeGeneration();
                attentionTimer = 0f;
                break;
            case CashRegisterState.NeedsAttention:
                StopIncomeGeneration();
                attentionTimer = 0f;
                break;
            case CashRegisterState.Broken:
                StopIncomeGeneration();
                ReleaseCurrentCustomer();
                break;
            case CashRegisterState.Off:
                StopIncomeGeneration();
                break;
        }

        Debug.Log($"Касса {registerID}: {oldState} -> {newState}");
    }

    private void ReleaseCurrentCustomer()
    {
        if (currentCustomer != null)
        {
            currentCustomer.LeaveStore();
            currentCustomer = null;
            isOccupied = false;
        }
    }

    public void ProcessCustomer(Customer customer)
    {
        if (currentState != CashRegisterState.Working || isOccupied)
        {
            return;
        }

        currentCustomer = customer;
        isOccupied = true;
        StartCoroutine(ServiceCustomer(customer));
    }

    private IEnumerator ServiceCustomer(Customer customer)
    {
        float serviceTime = customer.GetServiceTime();
        float elapsedTime = 0f;

        while (elapsedTime < serviceTime && currentCustomer == customer)
        {
            PlayScanningSound();
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        CheckSpecialItems(customer);
        CompleteCustomerService(customer);
    }

    private void PlayScanningSound()
    {
        if (scanningSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(scanningSound, ScanningSoundVolume);
        }
    }

    private void CheckSpecialItems(Customer customer)
    {
        if (customer is TeenagerCustomer teenager && teenager.tryingToBuyAlcohol && timeManager != null)
        {
            if (!timeManager.IsAlcoholSaleAllowed())
            {
                fineSystem?.ApplyFine(FineType.AlcoholToMinor);
            }
        }
    }

    private void CompleteCustomerService(Customer customer)
    {
        if (currentCustomer == customer)
        {
            currentCustomer = null;
            isOccupied = false;
            customer.CompleteService();
        }
    }

    private void StartIncomeGeneration()
    {
        if (incomeCoroutine != null)
        {
            StopCoroutine(incomeCoroutine);
        }
        incomeCoroutine = StartCoroutine(GenerateIncome());
    }

    private void StopIncomeGeneration()
    {
        if (incomeCoroutine != null)
        {
            StopCoroutine(incomeCoroutine);
            incomeCoroutine = null;
        }
    }

    private IEnumerator GenerateIncome()
    {
        while (currentState == CashRegisterState.Working)
        {
            yield return new WaitForSeconds(IncomeInterval);
            if (!isOccupied)
            {
                economyManager?.AddIncome(incomePerMinute);
            }
        }
    }

    private void CheckForBreakdown()
    {
        if (currentState == CashRegisterState.Working && Random.Range(0f, 1f) < breakdownChance * Time.deltaTime)
        {
            ChangeState(CashRegisterState.Broken);
        }
    }

    private void UpdateAttentionTimer()
    {
        if (currentState == CashRegisterState.NeedsAttention)
        {
            attentionTimer += Time.deltaTime;
            if (attentionTimer >= MaxAttentionTime)
            {
                ResolveAttentionIssue();
            }
        }
    }

    private void ResolveAttentionIssue()
    {
        ChangeState(Random.Range(0f, 1f) < BreakProbability ? CashRegisterState.Broken : CashRegisterState.Working);
    }

    private void UpdateVisuals()
    {
        if (screenRenderer == null)
        {
            return;
        }

        screenRenderer.material.color = currentState switch
        {
            CashRegisterState.Working => workingColor,
            CashRegisterState.NeedsAttention => GetBlinkingAttentionColor(),
            CashRegisterState.Broken => brokenColor,
            CashRegisterState.Off => offColor,
            _ => offColor
        };
    }

    private Color GetBlinkingAttentionColor()
    {
        float blink = Mathf.Sin(Time.time * BlinkFrequency) * 0.5f + 0.5f;
        return Color.Lerp(Color.yellow, attentionColor, blink);
    }

    public bool IsAvailable()
    {
        return currentState == CashRegisterState.Working && !isOccupied;
    }

    public float GetAttentionProgress()
    {
        return attentionTimer / MaxAttentionTime;
    }

    // Геттеры для доступа к состоянию
    public CashRegisterState GetCurrentState() => currentState;
    public CashRegisterType GetRegisterType() => registerType;
    public int GetRegisterID() => registerID;
    public bool IsOccupied() => isOccupied;
}