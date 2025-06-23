using UnityEngine;
using GameCore;

public class EconomyManager : MonoBehaviour
{
    private const float InitialBalance = 50000f; // Стартовый капитал
    private const float RentCost = 5000f; // Аренда
    private const float ElectricityCost = 1000f; // Электричество
    private const float CollectionCost = 500f; // Инкассация
    private const float RegisterMaintenanceCost = 200f; // Обслуживание за кассу
    private const float EfficiencyBonus = 0.2f; // 20% бонус за эффективность
    private const float WeekendBonus = 0.5f; // 50% бонус в выходные
    private const float CriticalBalance = 0f; // Критический баланс
    private const int BankruptcyDaysLimit = 3; // Дни до банкротства
    private const float VictoryGoal = 500000f; // Цель для победы
    private const float FineStressIncrease = 20f; // Увеличение стресса от штрафа
    private const int MaxQueueLengthForEfficiency = 2; // Максимальная длина очереди для бонуса

    [Header("Зависимости")]
    [SerializeField] private GameManager gameManager;
    [SerializeField] private StressManager stressManager;
    [SerializeField] private CashRegisterManager cashRegisterManager;
    [SerializeField] private QueueManager queueManager;
    [SerializeField] private TimeManager timeManager;

    [Header("Финансовое состояние")]
    [SerializeField] private float currentBalance = InitialBalance; // Текущий баланс
    [SerializeField] private float dailyIncome = 0f; // Дневной доход
    [SerializeField] private float dailyExpenses = 0f; // Дневные расходы

    [Header("Критические настройки")]
    [SerializeField] private int currentBankruptcyDays = 0; // Текущие дни с критическим балансом

    private void Start()
    {
        ResetDailyCounters();
    }

    public void AddIncome(float amount)
    {
        float finalAmount = CalculateFinalIncome(amount);
        currentBalance += finalAmount;
        dailyIncome += finalAmount;

        CheckVictoryCondition();
    }

    private float CalculateFinalIncome(float amount)
    {
        float finalAmount = amount;

        if (IsEfficientOperation())
        {
            finalAmount *= (1f + EfficiencyBonus);
        }

        if (IsWeekend())
        {
            finalAmount *= (1f + WeekendBonus);
        }

        return finalAmount;
    }

    private void CheckVictoryCondition()
    {
        if (currentBalance >= VictoryGoal && gameManager != null)
        {
            gameManager.TriggerVictory();
        }
    }

    public void AddExpense(float amount)
    {
        currentBalance -= amount;
        dailyExpenses += amount;
        CheckBankruptcy();
    }

    public void ApplyFine(float amount)
    {
        currentBalance -= amount;
        dailyExpenses += amount;

        Debug.Log($"Применен штраф: {amount}₽");

        if (stressManager != null)
        {
            stressManager.AddStress(FineStressIncrease);
        }

        CheckBankruptcy();
    }

    public bool CanAfford(float cost)
    {
        return currentBalance >= cost;
    }

    public void ProcessDailyExpenses()
    {
        float totalExpenses = CalculateTotalDailyExpenses();
        AddExpense(totalExpenses);

        LogFinancialSummary(totalExpenses);
        ResetDailyCounters();
    }

    private float CalculateTotalDailyExpenses()
    {
        float totalExpenses = RentCost + ElectricityCost + CollectionCost;

        if (cashRegisterManager != null)
        {
            totalExpenses += cashRegisterManager.GetTotalRegisters() * RegisterMaintenanceCost;
        }

        if (staffManager != null)
        {
            totalExpenses += staffManager.GetDailyStaffCost();
        }

        return totalExpenses;
    }

    private void LogFinancialSummary(float totalExpenses)
    {
        Debug.Log($"Ежедневные расходы: {totalExpenses}₽");
        Debug.Log($"Дневной доход: {dailyIncome}₽");
        Debug.Log($"Текущий баланс: {currentBalance}₽");
    }

    private void ResetDailyCounters()
    {
        dailyIncome = 0f;
        dailyExpenses = 0f;
    }

    private void CheckBankruptcy()
    {
        if (currentBalance <= CriticalBalance)
        {
            currentBankruptcyDays++;
            Debug.Log($"Критический баланс! Дней до банкротства: {BankruptcyDaysLimit - currentBankruptcyDays}");

            if (currentBankruptcyDays >= BankruptcyDaysLimit && gameManager != null)
            {
                gameManager.TriggerGameOver();
            }
        }
        else
        {
            currentBankruptcyDays = 0;
        }
    }

    private bool IsEfficientOperation()
    {
        return queueManager != null && queueManager.GetAverageQueueLength() <= MaxQueueLengthForEfficiency;
    }

    private bool IsWeekend()
    {
        if (timeManager != null)
        {
            WeekDay today = timeManager.GetCurrentDayOfWeek();
            return today == WeekDay.Saturday || today == WeekDay.Sunday;
        }
        return false;
    }

    public float GetBalancePercentage()
    {
        return currentBalance / VictoryGoal;
    }

    // Геттеры для доступа к состоянию
    public float GetCurrentBalance() => currentBalance;
    public float GetDailyIncome() => dailyIncome;
    public float GetDailyExpenses() => dailyExpenses;
    public int GetCurrentBankruptcyDays() => currentBankruptcyDays;
}