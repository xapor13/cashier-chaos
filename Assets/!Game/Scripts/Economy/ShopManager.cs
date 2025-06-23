using UnityEngine;
using System.Collections.Generic;
using GameCore;

public class ShopManager : MonoBehaviour
{
    private const float BasicRegisterCost = 8000f; // Стоимость базовой кассы
    private const float EnhancedRegisterCost = 15000f; // Стоимость улучшенной кассы
    private const float PremiumRegisterCost = 25000f; // Стоимость премиум-кассы
    private const float MechanicCost = 3000f; // Стоимость механика
    private const float AssistantCost = 2000f; // Стоимость помощника
    private const float SecurityGuardCost = 2500f; // Стоимость охранника
    private const float UnlockThreshold = 100000f; // Порог баланса для разблокировки товаров

    [System.Serializable]
    public class ShopItem
    {
        public string itemName;
        public float cost;
        public string description;
        public bool isPurchased;
        public ShopItemType itemType;
    }

    [Header("Зависимости")]
    [SerializeField] private EconomyManager economyManager;
    [SerializeField] private CashRegisterManager cashRegisterManager;
    [SerializeField] private StaffManager staffManager;

    [Header("Товары магазина")]
    [SerializeField] private List<ShopItem> availableItems = new List<ShopItem>();

    private void Start()
    {
        InitializeShopItems();
    }

    private void InitializeShopItems()
    {
        availableItems.Add(new ShopItem
        {
            itemName = "Базовая касса",
            cost = BasicRegisterCost,
            description = "15₽/мин, базовая надежность",
            itemType = ShopItemType.BasicRegister
        });

        availableItems.Add(new ShopItem
        {
            itemName = "Улучшенная касса",
            cost = EnhancedRegisterCost,
            description = "25₽/мин, меньше сбоев",
            itemType = ShopItemType.EnhancedRegister
        });

        availableItems.Add(new ShopItem
        {
            itemName = "Премиум касса",
            cost = PremiumRegisterCost,
            description = "40₽/мин, редкие поломки",
            itemType = ShopItemType.PremiumRegister
        });

        availableItems.Add(new ShopItem
        {
            itemName = "Механик",
            cost = MechanicCost,
            description = "Быстрый ремонт (5 сек)",
            itemType = ShopItemType.Mechanic
        });

        availableItems.Add(new ShopItem
        {
            itemName = "Помощник",
            cost = AssistantCost,
            description = "Помогает клиентам автоматически",
            itemType = ShopItemType.Assistant
        });

        availableItems.Add(new ShopItem
        {
            itemName = "Охранник",
            cost = SecurityGuardCost,
            description = "Снижает риск краж и агрессии",
            itemType = ShopItemType.SecurityGuard
        });
    }

    public bool PurchaseItem(string itemName)
    {
        ShopItem item = FindItemByName(itemName);
        if (item == null)
        {
            Debug.Log("Товар не найден!");
            return false;
        }

        if (item.isPurchased)
        {
            Debug.Log("Товар уже куплен!");
            return false;
        }

        if (!CanAffordItem(item.cost))
        {
            Debug.Log("Недостаточно средств!");
            return false;
        }

        ProcessPurchase(item);
        return true;
    }

    private ShopItem FindItemByName(string itemName)
    {
        return availableItems.Find(x => x.itemName == itemName);
    }

    private bool CanAffordItem(float cost)
    {
        return economyManager != null && economyManager.CanAfford(cost);
    }

    private void ProcessPurchase(ShopItem item)
    {
        economyManager?.AddExpense(item.cost);
        item.isPurchased = true;
        ApplyItemEffect(item);
        Debug.Log($"Куплен товар: {item.itemName} за {item.cost}₽");
    }

    private void ApplyItemEffect(ShopItem item)
    {
        switch (item.itemType)
        {
            case ShopItemType.BasicRegister:
                cashRegisterManager?.AddNewRegister(CashRegisterType.Basic);
                break;
            case ShopItemType.EnhancedRegister:
                cashRegisterManager?.AddNewRegister(CashRegisterType.Enhanced);
                break;
            case ShopItemType.PremiumRegister:
                cashRegisterManager?.AddNewRegister(CashRegisterType.Premium);
                break;
            case ShopItemType.Mechanic:
                staffManager?.HireStaff("Mechanic");
                break;
            case ShopItemType.Assistant:
                staffManager?.HireStaff("Assistant");
                break;
            case ShopItemType.SecurityGuard:
                staffManager?.HireStaff("SecurityGuard");
                break;
        }
    }

    public void UnlockNewItems()
    {
        if (economyManager == null)
        {
            return;
        }

        float balance = economyManager.GetCurrentBalance();
        if (balance >= UnlockThreshold)
        {
            Debug.Log("Разблокированы новые товары!");
        }
    }

    public List<ShopItem> GetAvailableItems()
    {
        return availableItems.FindAll(x => !x.isPurchased);
    }

    // Геттеры для доступа к состоянию
    public List<ShopItem> GetAllItems() => availableItems;
}