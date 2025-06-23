using UnityEngine;
using UnityEngine.Events;

public class EconomyManager : MonoBehaviour
{
    [SerializeField] private EconomySettings settings;
    [SerializeField] private GameTimeManager timeManager;
    //[SerializeField] private CashRegister[] cashRegisters; // Assigned in Inspector

    public UnityEvent onBalanceChanged = new UnityEvent();
    public UnityEvent onBankruptcy = new UnityEvent();

    private float currentBalance;
    private int criticalDebtDays;
    private bool hasMechanic;
    private bool hasAssistant;
    private bool hasGuard;

    public float CurrentBalance => currentBalance;
    public bool HasMechanic => hasMechanic;
    public bool HasAssistant => hasAssistant;
    public bool HasGuard => hasGuard;

    private void Awake()
    {
        currentBalance = settings.initialBalance;
        criticalDebtDays = 0;
    }

    private void OnEnable()
    {
        timeManager.onDayStarted.AddListener(OnDayStarted);
        timeManager.onDayEnded.AddListener(OnDayEnded);
    }

    private void OnDisable()
    {
        timeManager.onDayStarted.RemoveListener(OnDayStarted);
        timeManager.onDayEnded.RemoveListener(OnDayEnded);
    }

    private void Update()
    {
        CalculateIncome();
    }

    private void OnDayStarted()
    {
        // Apply daily expenses
        float dailyExpenses = settings.rentCost + settings.electricityCost + settings.collectionCost;
        //dailyExpenses += settings.cashRegisterMaintenanceCost * cashRegisters.Length;
        if (hasMechanic) dailyExpenses += settings.mechanicCost;
        if (hasAssistant) dailyExpenses += settings.assistantCost;
        if (hasGuard) dailyExpenses += settings.guardCost;

        currentBalance -= dailyExpenses;
        onBalanceChanged.Invoke();

        // Check critical debt
        if (currentBalance <= 0)
        {
            criticalDebtDays++;
            if (criticalDebtDays >= settings.criticalDebtDaysLimit)
            {
                onBankruptcy.Invoke();
            }
        }
        else
        {
            criticalDebtDays = 0;
        }
    }

    private void OnDayEnded()
    {
        // Apply weekend bonus if applicable
        if (timeManager.IsFamilyDay())
        {
            currentBalance += CalculateTotalIncome() * settings.weekendBonusPercent;
            onBalanceChanged.Invoke();
        }
    }

    private void CalculateIncome()
    {
        float incomePerSecond = CalculateTotalIncome() / 60f; // TR/min to TR/sec
        currentBalance += incomePerSecond * Time.deltaTime;
        onBalanceChanged.Invoke();
    }

    private float CalculateTotalIncome()
    {
        float totalIncome = 0f;
        bool hasQueues = false; // Placeholder: Check queues later

        // foreach (var register in cashRegisters)
        // {
        //     if (register.IsOperational)
        //     {
        //         totalIncome += register.IncomeRate;
        //     }
        //     // Placeholder: Check if register has queue
        // }

        // Apply efficiency bonus if no queues
        if (!hasQueues)
        {
            totalIncome *= (1f + settings.efficiencyBonusPercent);
        }

        return totalIncome;
    }

    public void ApplyFine(float amount)
    {
        currentBalance -= amount;
        onBalanceChanged.Invoke();
    }

    public bool PurchaseCashRegister(int type)
    {
        float cost = type switch
        {
            1 => settings.improvedCashRegisterCost,
            2 => settings.premiumCashRegisterCost,
            _ => settings.basicCashRegisterCost
        };

        if (currentBalance >= cost)
        {
            currentBalance -= cost;
            onBalanceChanged.Invoke();
            return true;
        }
        return false;
    }

    public void HireStaff(string staffType, bool hire)
    {
        switch (staffType)
        {
            case "Mechanic": hasMechanic = hire; break;
            case "Assistant": hasAssistant = hire; break;
            case "Guard": hasGuard = hire; break;
        }
    }
}