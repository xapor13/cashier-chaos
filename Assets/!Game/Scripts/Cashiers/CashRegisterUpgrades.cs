using UnityEngine;
using GameCore;

public class CashRegisterUpgrades : MonoBehaviour
{
    private const float BasicToEnhancedCost = 7000f; // Стоимость улучшения до Enhanced
    private const float EnhancedToPremiumCost = 10000f; // Стоимость улучшения до Premium
    private const float EnhancedIncomeMultiplier = 1.67f; // Множитель дохода Enhanced (25/15)
    private const float PremiumIncomeMultiplier = 2.67f; // Множитель дохода Premium (40/15)
    private const float EnhancedReliabilityMultiplier = 0.7f; // Множитель надежности Enhanced
    private const float PremiumReliabilityMultiplier = 0.3f; // Множитель надежности Premium
    private const float BaseIncomeMultiplier = 1f; // Базовый множитель дохода
    private const float BaseReliabilityMultiplier = 1f; // Базовый множитель надежности

    [Header("Зависимости")]
    [SerializeField] private EconomyManager economyManager;
    [SerializeField] private NotificationSystem notificationSystem;

    [Header("Стоимость улучшений")]
    [SerializeField] private float basicToEnhancedCost = BasicToEnhancedCost;
    [SerializeField] private float enhancedToPremiumCost = EnhancedToPremiumCost;

    public bool CanUpgradeRegister(CashRegister register, CashRegisterType newType)
    {
        if (register == null || !IsValidUpgrade(register.GetRegisterType(), newType))
        {
            return false;
        }

        float cost = GetUpgradeCost(register.GetRegisterType(), newType);
        return economyManager != null && economyManager.CanAfford(cost);
    }

    public bool UpgradeRegister(CashRegister register, CashRegisterType newType)
    {
        if (!CanUpgradeRegister(register, newType))
        {
            return false;
        }

        float cost = GetUpgradeCost(register.GetRegisterType(), newType);
        economyManager?.AddExpense(cost);

        CashRegisterType oldType = register.GetRegisterType();
        register.registerType = newType; // Прямой доступ, так как поле публичное
        register.SetIncomeByType(); // Обновляем параметры кассы

        Debug.Log($"Касса {register.GetRegisterID()} улучшена: {oldType} → {newType} за {cost}₽");
        notificationSystem?.ShowNotification($"Касса улучшена до {newType}", Color.green);

        return true;
    }

    private bool IsValidUpgrade(CashRegisterType currentType, CashRegisterType targetType)
    {
        return GetUpgradeCost(currentType, targetType) > 0;
    }

    public float GetUpgradeCost(CashRegisterType currentType, CashRegisterType targetType)
    {
        return (currentType, targetType) switch
        {
            (CashRegisterType.Basic, CashRegisterType.Enhanced) => basicToEnhancedCost,
            (CashRegisterType.Enhanced, CashRegisterType.Premium) => enhancedToPremiumCost,
            (CashRegisterType.Basic, CashRegisterType.Premium) => basicToEnhancedCost + enhancedToPremiumCost,
            _ => 0f
        };
    }

    public float GetIncomeMultiplier(CashRegisterType type)
    {
        return type switch
        {
            CashRegisterType.Basic => BaseIncomeMultiplier,
            CashRegisterType.Enhanced => EnhancedIncomeMultiplier,
            CashRegisterType.Premium => PremiumIncomeMultiplier,
            _ => BaseIncomeMultiplier
        };
    }

    public float GetReliabilityMultiplier(CashRegisterType type)
    {
        return type switch
        {
            CashRegisterType.Basic => BaseReliabilityMultiplier,
            CashRegisterType.Enhanced => EnhancedReliabilityMultiplier,
            CashRegisterType.Premium => PremiumReliabilityMultiplier,
            _ => BaseReliabilityMultiplier
        };
    }

    public string GetTypeDescription(CashRegisterType type)
    {
        return type switch
        {
            CashRegisterType.Basic => "15₽/мин, базовая надежность",
            CashRegisterType.Enhanced => "25₽/мин, меньше сбоев",
            CashRegisterType.Premium => "40₽/мин, редкие поломки",
            _ => "Неизвестный тип"
        };
    }
}