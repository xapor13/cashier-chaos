using UnityEngine;
using System.Collections;
using GameCore;

public class CashRegisterActions : MonoBehaviour
{
    private const float HelpDuration = 2.5f; // Длительность помощи клиенту
    private const float RepairDuration = 12.5f; // Длительность ремонта
    private const float MechanicRepairDuration = 5f; // Длительность ремонта с механиком
    private const float RebootDuration = 5f; // Длительность перезагрузки
    private const float RebootSuccessRate = 0.7f; // Шанс успеха перезагрузки
    private const float KickSuccessRate = 0.85f; // Шанс успеха кика
    private const float HelpStressReduction = 2f; // Снижение стресса при помощи
    private const float KickStressReduction = 10f; // Снижение стресса при кике
    private const int RepairAchievementIncrement = 1; // Прогресс достижения за ремонт

    [Header("Зависимости")]
    [SerializeField] private StressManager stressManager;
    [SerializeField] private StaffManager staffManager;
    [SerializeField] private AchievementManager achievementManager;
    [SerializeField] private KickSystem kickSystem;
    [SerializeField] private FineSystem fineSystem;

    [Header("Длительность действий")]
    [SerializeField] private float helpDuration = HelpDuration;
    [SerializeField] private float repairDuration = RepairDuration;
    [SerializeField] private float rebootDuration = RebootDuration;

    [Header("Текущее действие")]
    [SerializeField] private bool isPerformingAction = false;
    [SerializeField] private CashRegister currentRegister;

    public bool CanPerformAction()
    {
        return !isPerformingAction;
    }

    public void HelpCustomer(CashRegister register)
    {
        if (!CanPerformAction() || register.GetCurrentState() != CashRegisterState.NeedsAttention)
        {
            return;
        }

        StartCoroutine(HelpCustomerCoroutine(register));
    }

    private IEnumerator HelpCustomerCoroutine(CashRegister register)
    {
        isPerformingAction = true;
        currentRegister = register;

        Debug.Log($"Помогаем клиенту на кассе {register.GetRegisterID()}");

        yield return new WaitForSeconds(helpDuration);

        register.ChangeState(CashRegisterState.Working);
        stressManager?.ReduceStress(HelpStressReduction);

        CompleteAction();
    }

    public void RepairRegister(CashRegister register)
    {
        if (!CanPerformAction() || register.GetCurrentState() != CashRegisterState.Broken)
        {
            return;
        }

        StartCoroutine(RepairRegisterCoroutine(register));
    }

    private IEnumerator RepairRegisterCoroutine(CashRegister register)
    {
        isPerformingAction = true;
        currentRegister = register;

        Debug.Log($"Ремонтируем кассу {register.GetRegisterID()}");

        float repairTime = staffManager != null && staffManager.HasMechanic() ? MechanicRepairDuration : repairDuration;
        yield return new WaitForSeconds(repairTime);

        register.ChangeState(CashRegisterState.Working);
        achievementManager?.UpdateProgress("repairs", RepairAchievementIncrement);

        CompleteAction();
    }

    public void RebootRegister(CashRegister register)
    {
        if (!CanPerformAction() || register.GetCurrentState() == CashRegisterState.Off)
        {
            return;
        }

        StartCoroutine(RebootRegisterCoroutine(register));
    }

    private IEnumerator RebootRegisterCoroutine(CashRegister register)
    {
        isPerformingAction = true;
        currentRegister = register;

        Debug.Log($"Перезагружаем кассу {register.GetRegisterID()}");

        yield return new WaitForSeconds(rebootDuration);

        if (Random.Range(0f, 1f) <= RebootSuccessRate)
        {
            register.ChangeState(CashRegisterState.Working);
            Debug.Log("Перезагрузка успешна!");
        }
        else
        {
            Debug.Log("Перезагрузка не помогла");
        }

        CompleteAction();
    }

    public void KickRegister(CashRegister register)
    {
        if (kickSystem != null && !kickSystem.CanUseKick())
        {
            Debug.Log("Лимит киков исчерпан!");
            return;
        }

        Debug.Log($"Пинаем кассу {register.GetRegisterID()}");

        bool success = Random.Range(0f, 1f) <= KickSuccessRate;
        register.ChangeState(success ? CashRegisterState.Working : CashRegisterState.Broken);

        Debug.Log(success ? "Кик помог!" : "Касса сломалась от кика!");

        stressManager?.ReduceStress(KickStressReduction);
        kickSystem?.UseKick();

        if (register.currentCustomer != null)
        {
            fineSystem?.ApplyKickFine(register.currentCustomer);
        }
    }

    private void CompleteAction()
    {
        isPerformingAction = false;
        currentRegister = null;
    }

    // Геттеры для доступа к состоянию
    public bool IsPerformingAction() => isPerformingAction;
    public CashRegister GetCurrentRegister() => currentRegister;
}