# Порядок создания скриптов для Cashier Chaos: Retail Revolution

## Этап 1: Базовые системы и менеджеры (Неделя 1)

### 1.1 GameManager.cs - Главный контроллер игры

Основной скрипт, который управляет всем игровым процессом. Отвечает за состояние игры, время и общую координацию систем.

```csharp
using UnityEngine;
using System.Collections;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Game State")]
    public GameState currentState = GameState.MainMenu;
    public float gameTime = 8f;// Начало в 8:00
    public int currentDay = 1;
    public bool isPaused = false;

    [Header("Game Settings")]
    public float timeScale = 1f;// Скорость игры
    public bool isGameRunning = false;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        StartGame();
    }

    void Update()
    {
        if (isGameRunning && !isPaused)
        {
            UpdateGameTime();
        }
    }

    void StartGame()
    {
        currentState = GameState.Playing;
        isGameRunning = true;
        gameTime = 8f;
        currentDay = 1;

        Debug.Log("Игра началась - День " + currentDay);
    }

    void UpdateGameTime()
    {
        gameTime += Time.deltaTime * timeScale / 60f;// Конвертация в игровое время

        if (gameTime >= 23f)// Конец рабочего дня в 23:00
        {
            EndDay();
        }
    }

    void EndDay()
    {
        Debug.Log("День " + currentDay + " завершен");
        currentDay++;
        gameTime = 8f;// Начало нового дня

// Обработка ежедневных расходов
        if (EconomyManager.Instance != null)
        {
            EconomyManager.Instance.ProcessDailyExpenses();
        }
    }

    public void PauseGame()
    {
        isPaused = true;
        Time.timeScale = 0f;
        currentState = GameState.Paused;
    }

    public void ResumeGame()
    {
        isPaused = false;
        Time.timeScale = 1f;
        currentState = GameState.Playing;
    }

    public void GameOver()
    {
        currentState = GameState.GameOver;
        isGameRunning = false;
        Debug.Log("Игра окончена!");
    }

    public void Victory()
    {
        currentState = GameState.Victory;
        isGameRunning = false;
        Debug.Log("Победа! Цель достигнута!");
    }
}

```

### 1.2 GameState.cs - Перечисления состояний

Содержит все перечисления для состояний игры, касс и клиентов.

```csharp
// Состояния игры
public enum GameState
{
    MainMenu,
    Playing,
    Paused,
    GameOver,
    Victory
}

// Состояния касс
public enum CashRegisterState
{
    Working,// Работает нормально
    NeedsAttention,// Требует внимания
    Broken,// Сломана
    Off// Выключена
}

// Типы касс
public enum CashRegisterType
{
    Basic,// Базовая касса - 15₽/мин
    Enhanced,// Улучшенная касса - 25₽/мин
    Premium// Премиум касса - 40₽/мин
}

// Типы клиентов
public enum CustomerType
{
    Elderly,// Пожилой клиент
    Teenager,// Подросток
    Regular,// Обычный клиент
    Aggressive,// Агрессивный клиент
    VIP// VIP клиент
}

// Уровни стресса
public enum StressLevel
{
    Normal,// 0-30 - Нормальное состояние
    Tired,// 31-60 - Легкая усталость
    Stressed,// 61-80 - Сильный стресс
    Panic// 81-100 - Паника
}

// Уровни магазина
public enum StoreLevel
{
    Startup,// 0-100k₽ - 4 кассы
    Developing,// 100k-250k₽ - 8 касс
    Stable,// 250k-500k₽ - 12 касс
    Network// 500k+₽ - сеть магазинов
}

// Дни недели для особых событий
public enum WeekDay
{
    Monday,// День без табака
    Tuesday,// Обычный день
    Wednesday,// Скидки для пенсионеров
    Thursday,// Обычный день
    Friday,// Молодежный день
    Saturday,// Семейный день
    Sunday// Семейный день
}

```

### 1.3 TimeManager.cs - Система времени

Управляет игровым временем, определяет пиковые часы и ограничения.

```csharp
using UnityEngine;
using System;

public class TimeManager : MonoBehaviour
{
    public static TimeManager Instance;

    [Header("Time Settings")]
    public float currentTime = 8f;// 8:00 утра
    public int currentDay = 1;
    public bool isPeakHours = false;

    [Header("Peak Hours")]
    public float morningPeakStart = 12f;// 12:00
    public float morningPeakEnd = 14f;// 14:00
    public float eveningPeakStart = 18f;// 18:00
    public float eveningPeakEnd = 20f;// 20:00

    [Header("Restrictions")]
    public float alcoholSaleEndTime = 22f;// 22:00

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Update()
    {
        if (GameManager.Instance != null)
        {
            currentTime = GameManager.Instance.gameTime;
            currentDay = GameManager.Instance.currentDay;
        }

        UpdatePeakHours();
    }

    void UpdatePeakHours()
    {
        isPeakHours = (currentTime >= morningPeakStart && currentTime <= morningPeakEnd) ||
                      (currentTime >= eveningPeakStart && currentTime <= eveningPeakEnd);
    }

    public bool IsAlcoholSaleAllowed()
    {
        return currentTime < alcoholSaleEndTime;
    }

    public bool IsCigaretteSaleAllowed()
    {
        WeekDay today = GetCurrentDayOfWeek();
        return today != WeekDay.Monday;// Понедельник - день без табака
    }

    public WeekDay GetCurrentDayOfWeek()
    {
        int dayIndex = (currentDay - 1) % 7;
        return (WeekDay)dayIndex;
    }

    public float GetPeakHourMultiplier()
    {
        return isPeakHours ? 2f : 1f;
    }

    public string GetFormattedTime()
    {
        int hours = Mathf.FloorToInt(currentTime);
        int minutes = Mathf.FloorToInt((currentTime - hours) * 60);
        return string.Format("{0:00}:{1:00}", hours, minutes);
    }

    public string GetDayInfo()
    {
        WeekDay today = GetCurrentDayOfWeek();
        string dayName = today.ToString();

        switch (today)
        {
            case WeekDay.Monday:
                return dayName + " (День без табака)";
            case WeekDay.Wednesday:
                return dayName + " (Скидки для пенсионеров)";
            case WeekDay.Friday:
                return dayName + " (Молодежный день)";
            case WeekDay.Saturday:
            case WeekDay.Sunday:
                return dayName + " (Семейный день)";
            default:
                return dayName;
        }
    }
}

```

## Этап 2: Экономическая система (Неделя 1-2)

### 2.1 EconomyManager.cs - Основа экономики

Управляет балансом, доходами, расходами и штрафами.

```csharp
using UnityEngine;
using System.Collections;

public class EconomyManager : MonoBehaviour
{
    public static EconomyManager Instance;
    
    [Header("Balance")]
    public float currentBalance = 50000f; // Стартовый капитал
    public float dailyIncome = 0f;
    public float dailyExpenses = 0f;
    
    [Header("Daily Expenses")]
    public float rentCost = 5000f;        // Аренда
    public float electricityCost = 1000f; // Электричество
    public float collectionCost = 500f;   // Инкассация
    public float registerMaintenanceCost = 200f; // Обслуживание за кассу
    
    [Header("Bonuses")]
    public float efficiencyBonus = 0.2f;  // 20% за отсутствие очередей
    public float weekendBonus = 0.5f;     // 50% в выходные
    
    [Header("Critical Settings")]
    public float criticalBalance = 0f;
    public int bankruptcyDays = 3;
    public int currentBankruptcyDays = 0;
    public float victoryGoal = 500000f;   // Цель для победы
    
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    void Start()
    {
        ResetDailyCounters();
    }
    
    public void AddIncome(float amount)
    {
        float finalAmount = amount;
        
        // Применяем бонус за эффективность
        if (IsEfficientOperation())
        {
            finalAmount *= (1f + efficiencyBonus);
        }
        
        // Применяем бонус выходного дня
        if (IsWeekend())
        {
            finalAmount *= (1f + weekendBonus);
        }
        
        currentBalance += finalAmount;
        dailyIncome += finalAmount;
        
        // Проверяем условие победы
        if (currentBalance >= victoryGoal && GameManager.Instance != null)
        {
            GameManager.Instance.Victory();
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
        
        Debug.Log("Применен штраф: " + amount + "₽");
        
        // Увеличиваем стресс от штрафа
        if (StressManager.Instance != null)
        {
            StressManager.Instance.AddStress(20f);
        }
        
        CheckBankruptcy();
    }
    
    public bool CanAfford(float cost)
    {
        return currentBalance >= cost;
    }
    
    public void ProcessDailyExpenses()
    {
        float totalExpenses = rentCost + electricityCost + collectionCost;
        
        // Добавляем стоимость обслуживания касс
        if (CashRegisterManager.Instance != null)
        {
            totalExpenses += CashRegisterManager.Instance.GetTotalRegisters() * registerMaintenanceCost;
        }
        
        // Добавляем зарплату персонала
        if (StaffManager.Instance != null)
        {
            totalExpenses += StaffManager.Instance.GetDailyStaffCost();
        }
        
        AddExpense(totalExpenses);
        
        Debug.Log("Ежедневные расходы: " + totalExpenses + "₽");
        Debug.Log("Дневной доход: " + dailyIncome + "₽");
        Debug.Log("Текущий баланс: " + currentBalance + "₽");
        
        ResetDailyCounters();
    }
    
    void ResetDailyCounters()
    {
        dailyIncome = 0f;
        dailyExpenses = 0f;
    }
    
    void CheckBankruptcy()
    {
        if (currentBalance <= criticalBalance)
        {
            currentBankruptcyDays++;
            Debug.Log("Критический баланс! Дней до банкротства: " + (bankruptcyDays - currentBankruptcyDays));
            
            if (currentBankruptcyDays >= bankruptcyDays)
            {
                if (GameManager.Instance != null)
                {
                    GameManager.Instance.GameOver();
                }
            }
        }
        else
        {
            currentBankruptcyDays = 0; // Сброс счетчика при восстановлении баланса
        }
    }
    
    bool IsEfficientOperation()
    {
        // Проверяем, есть ли длинные очереди
        if (QueueManager.Instance != null)
        {
            return QueueManager.Instance.GetAverageQueueLength() <= 2;
        }
        return true;
    }
    
    bool IsWeekend()
    {
        if (TimeManager.Instance != null)
        {
            WeekDay today = TimeManager.Instance.GetCurrentDayOfWeek();
            return today == WeekDay.Saturday || today == WeekDay.Sunday;
        }
        return false;
    }
    
    public float GetBalancePercentage()
    {
        return currentBalance / victoryGoal;
    }
}
```

### 2.2 ShopManager.cs - Система покупок

```csharp
using UnityEngine;
using System.Collections.Generic;

public class ShopManager : MonoBehaviour
{
    public static ShopManager Instance;
    
    [System.Serializable]
    public class ShopItem
    {
        public string itemName;
        public float cost;
        public string description;
        public bool isPurchased;
        public ShopItemType itemType;
    }
    
    public enum ShopItemType
    {
        BasicRegister,
        EnhancedRegister,
        PremiumRegister,
        Mechanic,
        Assistant,
        SecurityGuard
    }
    
    [Header("Shop Items")]
    public List<ShopItem> availableItems = new List<ShopItem>();
    
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            InitializeShopItems();
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    void InitializeShopItems()
    {
        availableItems.Add(new ShopItem 
        { 
            itemName = "Базовая касса", 
            cost = 8000f, 
            description = "15₽/мин, базовая надежность",
            itemType = ShopItemType.BasicRegister
        });
        
        availableItems.Add(new ShopItem 
        { 
            itemName = "Улучшенная касса", 
            cost = 15000f, 
            description = "25₽/мин, меньше сбоев",
            itemType = ShopItemType.EnhancedRegister
        });
        
        availableItems.Add(new ShopItem 
        { 
            itemName = "Премиум касса", 
            cost = 25000f, 
            description = "40₽/мин, редкие поломки",
            itemType = ShopItemType.PremiumRegister
        });
        
        availableItems.Add(new ShopItem 
        { 
            itemName = "Механик", 
            cost = 3000f, 
            description = "Быстрый ремонт (5 сек)",
            itemType = ShopItemType.Mechanic
        });
        
        availableItems.Add(new ShopItem 
        { 
            itemName = "Помощник", 
            cost = 2000f, 
            description = "Помогает клиентам автоматически",
            itemType = ShopItemType.Assistant
        });
        
        availableItems.Add(new ShopItem 
        { 
            itemName = "Охранник", 
            cost = 2500f, 
            description = "Снижает риск краж и агрессии",
            itemType = ShopItemType.SecurityGuard
        });
    }
    
    public bool PurchaseItem(string itemName)
    {
        ShopItem item = availableItems.Find(x => x.itemName == itemName);
        if (item == null) return false;
        
        if (item.isPurchased)
        {
            Debug.Log("Товар уже куплен!");
            return false;
        }
        
        if (!EconomyManager.Instance.CanAfford(item.cost))
        {
            Debug.Log("Недостаточно средств!");
            return false;
        }
        
        // Покупаем товар
        EconomyManager.Instance.AddExpense(item.cost);
        item.isPurchased = true;
        
        // Применяем эффект товара
        ApplyItemEffect(item);
        
        Debug.Log($"Куплен товар: {item.itemName} за {item.cost}₽");
        return true;
    }
    
    void ApplyItemEffect(ShopItem item)
    {
        switch (item.itemType)
        {
            case ShopItemType.BasicRegister:
                CashRegisterManager.Instance?.AddNewRegister(CashRegisterType.Basic);
                break;
            case ShopItemType.EnhancedRegister:
                CashRegisterManager.Instance?.AddNewRegister(CashRegisterType.Enhanced);
                break;
            case ShopItemType.PremiumRegister:
                CashRegisterManager.Instance?.AddNewRegister(CashRegisterType.Premium);
                break;
            case ShopItemType.Mechanic:
                StaffManager.Instance?.HireStaff("Mechanic");
                break;
            case ShopItemType.Assistant:
                StaffManager.Instance?.HireStaff("Assistant");
                break;
            case ShopItemType.SecurityGuard:
                StaffManager.Instance?.HireStaff("SecurityGuard");
                break;
        }
    }
    
    public void UnlockNewItems()
    {
        // Разблокировка новых товаров в зависимости от прогресса
        float balance = EconomyManager.Instance.currentBalance;
        
        if (balance >= 100000f)
        {
            // Разблокировка расширенного ассортимента
            Debug.Log("Разблокированы новые товары!");
        }
    }
    
    public List<ShopItem> GetAvailableItems()
    {
        return availableItems.FindAll(x => !x.isPurchased);
    }
}
```

### 2.3 FineSystem.cs - Система штрафов

Обрабатывает все виды штрафов с учетом модификаторов.

```csharp
using UnityEngine;

public class FineSystem : MonoBehaviour
{
    public static FineSystem Instance;
    
    // Типы штрафов с базовыми суммами
    public enum FineType
    {
        AlcoholToMinor = 10000,
        AlcoholAfterHours = 5000,
        KickElderly = 2000,
        KickVIP = 5000,
        IgnoreBrokenRegister = 1000,
        MassCustomerLeave = 2000
    }
    
    [Header("Fine Modifiers")]
    public float provocationMultiplier = 1.5f;    // +50% при провокации
    public float cameraMultiplier = 2f;           // x2 при наличии камер
    public float securityMultiplier = 1.3f;       // +30% при охране
    public float nightReduction = 0.8f;           // -20% ночью
    
    [Header("Settings")]
    public bool camerasInstalled = true;
    public bool securityPresent = false;
    
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    public void ApplyFine(FineType fineType, float multiplier = 1f, bool isProvoked = false)
    {
        float baseFine = (float)fineType;
        float finalFine = CalculateFineWithModifiers(baseFine, isProvoked);
        finalFine *= multiplier;
        
        if (EconomyManager.Instance != null)
        {
            EconomyManager.Instance.ApplyFine(finalFine);
        }
        
        // Показываем уведомление о штрафе
        if (NotificationSystem.Instance != null)
        {
            NotificationSystem.Instance.ShowFineNotification(finalFine);
        }
        
        Debug.Log($"Применен штраф {fineType}: {finalFine}₽");
    }
    
    public float CalculateFineWithModifiers(float baseFine, bool isProvoked = false)
    {
        float finalFine = baseFine;
        
        // Модификатор провокации
        if (isProvoked)
        {
            finalFine *= provocationMultiplier;
        }
        
        // Модификатор камер
        if (camerasInstalled)
        {
            finalFine *= cameraMultiplier;
        }
        
        // Модификатор охраны
        if (securityPresent)
        {
            finalFine *= securityMultiplier;
        }
        
        // Ночное снижение штрафов
        if (TimeManager.Instance != null && TimeManager.Instance.currentTime >= 20f)
        {
            finalFine *= nightReduction;
        }
        
        return finalFine;
    }
    
    public void ApplyKickFine(Customer customer, bool isProvoked = false)
    {
        if (customer == null) return;
        
        float fineChance = customer.GetKickFineRisk();
        
        if (Random.Range(0f, 1f) <= fineChance)
        {
            FineType fineType = GetKickFineType(customer.customerType);
            ApplyFine(fineType, 1f, isProvoked);
        }
    }
    
    FineType GetKickFineType(CustomerType customerType)
    {
        switch (customerType)
        {
            case CustomerType.Elderly:
                return FineType.KickElderly;
            case CustomerType.VIP:
                return FineType.KickVIP;
            default:
                return FineType.KickElderly; // Базовый штраф для остальных
        }
    }
    
    public void CheckBrokenRegisterFines()
    {
        // Проверяем игнорирование сломанных касс каждые 2 минуты
        if (CashRegisterManager.Instance != null)
        {
            int brokenRegisters = CashRegisterManager.Instance.GetBrokenRegistersCount();
            if (brokenRegisters > 0)
            {
                float totalFine = brokenRegisters * (float)FineType.IgnoreBrokenRegister;
                ApplyFine(FineType.IgnoreBrokenRegister, brokenRegisters);
            }
        }
    }
    
    public void SetSecurityPresence(bool hasGuard)
    {
        securityPresent = hasGuard;
    }
    
    public void SetCameraStatus(bool hasCameras)
    {
        camerasInstalled = hasCameras;
    }
}
```

## Этап 3: Система касс (Неделя 2)

### 3.1 CashRegister.cs - Базовый класс кассы

Основной класс для управления состоянием и работой касс.

```csharp
using UnityEngine;
using System.Collections;

public class CashRegister : MonoBehaviour
{
    [Header("Register Settings")]
    public CashRegisterState currentState = CashRegisterState.Working;
    public CashRegisterType registerType = CashRegisterType.Basic;
    public int registerID;
    
    [Header("Income Settings")]
    public float incomePerMinute = 15f;
    public float breakdownChance = 0.1f;
    public float lastIncomeTime;
    
    [Header("Current Status")]
    public bool isOccupied = false;
    public Customer currentCustomer;
    public float attentionTimer = 0f;
    public float maxAttentionTime = 60f; // 60 секунд для решения проблемы
    
    [Header("Visual Effects")]
    public Renderer screenRenderer;
    public Color workingColor = Color.green;
    public Color attentionColor = Color.yellow;
    public Color brokenColor = Color.red;
    public Color offColor = Color.black;
    
    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip scanningSound;
    private Coroutine incomeCoroutine;
    
    void Start() 
    { 
        SetIncomeByType();
        if (currentState == CashRegisterState.Working)
        {
            StartIncomeGeneration();
        }
        UpdateVisuals();
    }
    
    void Update() 
    { 
        CheckForBreakdown();
        UpdateAttentionTimer();
        UpdateVisuals();
    }
    
    void SetIncomeByType()
    {
        switch (registerType)
        {
            case CashRegisterType.Basic:
                incomePerMinute = 15f;
                breakdownChance = 0.1f;
                break;
            case CashRegisterType.Enhanced:
                incomePerMinute = 25f;
                breakdownChance = 0.07f;
                break;
            case CashRegisterType.Premium:
                incomePerMinute = 40f;
                breakdownChance = 0.03f;
                break;
        }
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
                if (currentCustomer != null)
                {
                    currentCustomer.LeaveStore();
                    currentCustomer = null;
                    isOccupied = false;
                }
                break;
                
            case CashRegisterState.Off:
                StopIncomeGeneration();
                break;
        }
        
        Debug.Log($"Касса {registerID}: {oldState} -> {newState}");
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
    
    IEnumerator ServiceCustomer(Customer customer)
    {
        float serviceTime = customer.GetServiceTime();
        float elapsedTime = 0f;
        
        while (elapsedTime < serviceTime && currentCustomer == customer)
        {
            // Проигрываем звук сканирования
            if (scanningSound != null && audioSource != null)
            {
                audioSource.PlayOneShot(scanningSound, 0.3f);
            }
            
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        
        // Проверяем специальные товары (алкоголь, сигареты)
        if (customer is TeenagerCustomer teenager)
        {
            if (teenager.tryingToBuyAlcohol && !TimeManager.Instance.IsAlcoholSaleAllowed())
            {
                // Штраф за продажу алкоголя несовершеннолетнему
                FineSystem.Instance.ApplyFine(FineSystem.FineType.AlcoholToMinor);
            }
        }
        
        // Завершаем обслуживание
        currentCustomer = null;
        isOccupied = false;
        customer.CompleteService();
    }
    
    void StartIncomeGeneration()
    {
        if (incomeCoroutine != null)
        {
            StopCoroutine(incomeCoroutine);
        }
        incomeCoroutine = StartCoroutine(GenerateIncome());
    }
    
    void StopIncomeGeneration()
    {
        if (incomeCoroutine != null)
        {
            StopCoroutine(incomeCoroutine);
            incomeCoroutine = null;
        }
    }
    
    IEnumerator GenerateIncome()
    {
        while (currentState == CashRegisterState.Working)
        {
            yield return new WaitForSeconds(60f); // Каждую минуту
            
            if (EconomyManager.Instance != null && !isOccupied)
            {
                EconomyManager.Instance.AddIncome(incomePerMinute);
            }
        }
    }
    
    void CheckForBreakdown()
    {
        if (currentState == CashRegisterState.Working && 
            Random.Range(0f, 1f) < breakdownChance * Time.deltaTime)
        {
            ChangeState(CashRegisterState.Broken);
        }
    }
    
    void UpdateAttentionTimer()
    {
        if (currentState == CashRegisterState.NeedsAttention)
        {
            attentionTimer += Time.deltaTime;
            
            if (attentionTimer >= maxAttentionTime)
            {
                // Автоматическое решение проблемы или поломка
                if (Random.Range(0f, 1f) < 0.3f)
                {
                    ChangeState(CashRegisterState.Broken);
                }
                else
                {
                    ChangeState(CashRegisterState.Working);
                }
            }
        }
    }
    
    void UpdateVisuals()
    {
        if (screenRenderer == null) return;
        
        switch (currentState)
        {
            case CashRegisterState.Working:
                screenRenderer.material.color = workingColor;
                break;
            case CashRegisterState.NeedsAttention:
                // Мигающий желтый
                float blink = Mathf.Sin(Time.time * 3f) * 0.5f + 0.5f;
                screenRenderer.material.color = Color.Lerp(Color.yellow, attentionColor, blink);
                break;
            case CashRegisterState.Broken:
                screenRenderer.material.color = brokenColor;
                break;
            case CashRegisterState.Off:
                screenRenderer.material.color = offColor;
                break;
        }
    }
    
    public bool IsAvailable()
    {
        return currentState == CashRegisterState.Working && !isOccupied;
    }
    
    public float GetAttentionProgress()
    {
        return attentionTimer / maxAttentionTime;
    }
}
```

### 3.2 CashRegisterActions.cs - Действия с кассами

Обрабатывает все действия игрока с кассами.

```csharp
using UnityEngine;
using System.Collections;

public class CashRegisterActions : MonoBehaviour
{
    public static CashRegisterActions Instance;
    
    [Header("Action Durations")]
    public float helpDuration = 2.5f;
    public float repairDuration = 12.5f;
    public float rebootDuration = 5f;
    
    [Header("Success Rates")]
    public float rebootSuccessRate = 0.7f;
    public float kickSuccessRate = 0.85f;
    
    [Header("Current Actions")]
    private bool isPerformingAction = false;
    private CashRegister currentRegister;
    
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    public bool CanPerformAction()
    {
        return !isPerformingAction;
    }
    
    public void HelpCustomer(CashRegister register)
    {
        if (!CanPerformAction() || register.currentState != CashRegisterState.NeedsAttention)
        {
            return;
        }
        
        StartCoroutine(HelpCustomerCoroutine(register));
    }
    
    IEnumerator HelpCustomerCoroutine(CashRegister register)
    {
        isPerformingAction = true;
        currentRegister = register;
        
        Debug.Log($"Помогаем клиенту на кассе {register.registerID}");
        
        yield return new WaitForSeconds(helpDuration);
        
        // Решаем проблему
        register.ChangeState(CashRegisterState.Working);
        
        // Снижаем стресс
        if (StressManager.Instance != null)
        {
            StressManager.Instance.ReduceStress(2f);
        }
        
        isPerformingAction = false;
        currentRegister = null;
    }
    
    public void RepairRegister(CashRegister register)
    {
        if (!CanPerformAction() || register.currentState != CashRegisterState.Broken)
        {
            return;
        }
        
        StartCoroutine(RepairRegisterCoroutine(register));
    }
    
    IEnumerator RepairRegisterCoroutine(CashRegister register)
    {
        isPerformingAction = true;
        currentRegister = register;
        
        Debug.Log($"Ремонтируем кассу {register.registerID}");
        
        float repairTime = repairDuration;
        
        // Механик ускоряет ремонт
        if (StaffManager.Instance != null && StaffManager.Instance.HasMechanic())
        {
            repairTime = 5f;
        }
        
        yield return new WaitForSeconds(repairTime);
        
        // Восстанавливаем кассу
        register.ChangeState(CashRegisterState.Working);
        
        // Обновляем достижения
        if (AchievementManager.Instance != null)
        {
```

### 3.3 CashRegisterUpgrades.cs - Улучшения касс

```csharp
using UnityEngine;

public class CashRegisterUpgrades : MonoBehaviour
{
    public static CashRegisterUpgrades Instance;
    
    [Header("Upgrade Costs")]
    public float basicToEnhancedCost = 7000f;
    public float enhancedToPremiumCost = 10000f;
    
    [Header("Upgrade Multipliers")]
    public float enhancedIncomeMultiplier = 1.67f; // 25/15
    public float premiumIncomeMultiplier = 2.67f;  // 40/15
    public float enhancedReliabilityMultiplier = 0.7f; // меньше поломок
    public float premiumReliabilityMultiplier = 0.3f;
    
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    public bool CanUpgradeRegister(CashRegister register, CashRegisterType newType)
    {
        if (register == null) return false;
        
        float cost = GetUpgradeCost(register.registerType, newType);
        if (cost <= 0) return false;
        
        return EconomyManager.Instance != null && EconomyManager.Instance.CanAfford(cost);
    }
    
    public bool UpgradeRegister(CashRegister register, CashRegisterType newType)
    {
        if (!CanUpgradeRegister(register, newType)) return false;
        
        float cost = GetUpgradeCost(register.registerType, newType);
        EconomyManager.Instance.AddExpense(cost);
        
        CashRegisterType oldType = register.registerType;
        register.registerType = newType;
        register.SetIncomeByType(); // Обновляем параметры кассы
        
        Debug.Log($"Касса {register.registerID} улучшена: {oldType} → {newType} за {cost}₽");
        
        // Показываем уведомление
        if (NotificationSystem.Instance != null)
        {
            NotificationSystem.Instance.ShowNotification($"Касса улучшена до {newType}", Color.green);
        }
        
        return true;
    }
    
    public float GetUpgradeCost(CashRegisterType currentType, CashRegisterType targetType)
    {
        if (currentType == CashRegisterType.Basic && targetType == CashRegisterType.Enhanced)
            return basicToEnhancedCost;
        if (currentType == CashRegisterType.Enhanced && targetType == CashRegisterType.Premium)
            return enhancedToPremiumCost;
        if (currentType == CashRegisterType.Basic && targetType == CashRegisterType.Premium)
            return basicToEnhancedCost + enhancedToPremiumCost;
        
        return 0f; // Невозможное улучшение
    }
    
    public float GetIncomeMultiplier(CashRegisterType type)
    {
        switch (type)
        {
            case CashRegisterType.Basic:
                return 1f;
            case CashRegisterType.Enhanced:
                return enhancedIncomeMultiplier;
            case CashRegisterType.Premium:
                return premiumIncomeMultiplier;
            default:
                return 1f;
        }
    }
    
    public float GetReliabilityMultiplier(CashRegisterType type)
    {
        switch (type)
        {
            case CashRegisterType.Basic:
                return 1f;
            case CashRegisterType.Enhanced:
                return enhancedReliabilityMultiplier;
            case CashRegisterType.Premium:
                return premiumReliabilityMultiplier;
            default:
                return 1f;
        }
    }
    
    public string GetTypeDescription(CashRegisterType type)
    {
        switch (type)
        {
            case CashRegisterType.Basic:
                return "15₽/мин, базовая надежность";
            case CashRegisterType.Enhanced:
                return "25₽/мин, меньше сбоев";
            case CashRegisterType.Premium:
                return "40₽/мин, редкие поломки";
            default:
                return "Неизвестный тип";
        }
    }
}
```

### 3.4 CashRegisterManager.cs - Улучшения касс

using UnityEngine;
using System.Collections.Generic;

public class CashRegisterManager : MonoBehaviour
{
public static CashRegisterManager Instance;

```
[Header("Register Management")]
public List<CashRegister> allRegisters = new List<CashRegister>();
public Transform[] registerPositions;
public GameObject[] registerPrefabs; // Basic, Enhanced, Premium

[Header("Register Limits")]
public int maxRegisters = 4;
public int currentRegisterCount = 0;

[Header("Auto-Management")]
public bool autoManagementEnabled = false;
public float managementCheckInterval = 30f;
private float lastManagementCheck = 0f;

void Awake()
{
    if (Instance == null)
    {
        Instance = this;
    }
    else
    {
        Destroy(gameObject);
    }
}

void Start()
{
    InitializeStartingRegisters();
}

void Update()
{
    if (autoManagementEnabled && Time.time - lastManagementCheck >= managementCheckInterval)
    {
        AutoManageRegisters();
        lastManagementCheck = Time.time;
    }
}

void InitializeStartingRegisters()
{
    // Создаем 2 базовые кассы в начале игры
    for (int i = 0; i < 2 && i < registerPositions.Length; i++)
    {
        AddNewRegister(CashRegisterType.Basic);
    }
}

public bool AddNewRegister(CashRegisterType type)
{
    if (currentRegisterCount >= maxRegisters)
    {
        Debug.Log("Достигнут лимит касс!");
        return false;
    }

    if (registerPositions.Length <= currentRegisterCount)
    {
        Debug.Log("Нет свободных позиций для касс!");
        return false;
    }

    GameObject prefab = GetRegisterPrefab(type);
    if (prefab == null) return false;

    Transform position = registerPositions[currentRegisterCount];
    GameObject newRegisterObj = Instantiate(prefab, position.position, position.rotation);

    CashRegister newRegister = newRegisterObj.GetComponent<CashRegister>();
    if (newRegister != null)
    {
        newRegister.registerID = currentRegisterCount + 1;
        newRegister.registerType = type;
        allRegisters.Add(newRegister);
        currentRegisterCount++;

        Debug.Log($"Добавлена новая касса {type} (ID: {newRegister.registerID})");
        return true;
    }

    return false;
}

GameObject GetRegisterPrefab(CashRegisterType type)
{
    switch (type)
    {
        case CashRegisterType.Basic:
            return registerPrefabs[0];
        case CashRegisterType.Enhanced:
            return registerPrefabs[1];
        case CashRegisterType.Premium:
            return registerPrefabs[2];
        default:
            return registerPrefabs[0];
    }
}

public void RemoveRegister(CashRegister register)
{
    if (allRegisters.Contains(register))
    {
        allRegisters.Remove(register);
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

void AutoManageRegisters()
{
    // Автоматическое включение/выключение касс в зависимости от загруженности
    if (QueueManager.Instance != null)
    {
        float averageQueueLength = QueueManager.Instance.GetAverageQueueLength();

        if (averageQueueLength > 3f)
        {
            // Включаем выключенные кассы
            TurnOnIdleRegisters();
        }
        else if (averageQueueLength < 1f)
        {
            // Выключаем лишние кассы для экономии
            TurnOffExcessRegisters();
        }
    }
}

void TurnOnIdleRegisters()
{
    foreach (CashRegister register in allRegisters)
    {
        if (register.currentState == CashRegisterState.Off)
        {
            register.ChangeState(CashRegisterState.Working);
            Debug.Log($"Касса {register.registerID} включена автоматически");
            break; // Включаем по одной
        }
    }
}

void TurnOffExcessRegisters()
{
    int workingCount = GetWorkingRegisters().Count;
    if (workingCount > 2) // Оставляем минимум 2 рабочие кассы
    {
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
}

public void EnableAutoManagement(bool enabled)
{
    autoManagementEnabled = enabled;
    Debug.Log($"Автоуправление кассами: {(enabled ? "включено" : "выключено")}");
}

public void UpdateMaxRegisters(int newMax)
{
    maxRegisters = newMax;
    Debug.Log($"Максимальное количество касс обновлено: {maxRegisters}");
}
}
```

## Этап 4: Система клиентов (Неделя 2-3)

### 4.1 Customer.cs - Базовый класс клиента

```csharp
using UnityEngine;
using System.Collections;

public abstract class Customer : MonoBehaviour
{
    [Header("Customer Settings")]
    public CustomerType customerType;
    public float serviceSpeed = 10f; // сек/товар
    public float patience = 120f; // секунды
    public float currentPatience;
    public int itemCount;
    public bool isAngry = false;
    
    [Header("Movement")]
    public float moveSpeed = 2f;
    public Transform targetRegister;
    
    [Header("Special Items")]
    public bool hasAlcohol = false;
    public bool hasCigarettes = false;
    public bool hasSpecialItems = false;
    
    [Header("Visual")]
    public Renderer customerRenderer;
    public Color normalColor = Color.white;
    public Color angryColor = Color.red;
    public GameObject angerEffect;
    
    protected bool isBeingServed = false;
    protected bool hasLeft = false;
    
    protected virtual void Start()
    {
        currentPatience = patience;
        itemCount = Random.Range(1, 8); // 1-7 товаров
        GenerateSpecialItems();
        SetCustomerAppearance();
    }
    
    protected virtual void Update()
    {
        if (!isBeingServed && !hasLeft)
        {
            UpdatePatience();
            UpdateVisuals();
        }
    }
    
    protected virtual void UpdatePatience()
    {
        currentPatience -= Time.deltaTime;
        
        if (currentPatience <= 0 && !hasLeft)
        {
            ReactToWaiting();
            LeaveStore();
        }
        else if (currentPatience <= patience * 0.3f && !isAngry)
        {
            BecomeAngry();
        }
    }
    
    protected virtual void GenerateSpecialItems()
    {
        // Базовая генерация особых товаров
        hasAlcohol = Random.Range(0f, 1f) < 0.2f; // 20% шанс
        hasCigarettes = Random.Range(0f, 1f) < 0.15f; // 15% шанс
        hasSpecialItems = hasAlcohol || hasCigarettes;
    }
    
    protected virtual void SetCustomerAppearance()
    {
        if (customerRenderer != null)
        {
            customerRenderer.material.color = normalColor;
        }
    }
    
    protected virtual void UpdateVisuals()
    {
        if (customerRenderer != null)
        {
            if (isAngry)
            {
                customerRenderer.material.color = Color.Lerp(normalColor, angryColor, 
                    Mathf.Sin(Time.time * 5f) * 0.5f + 0.5f);
            }
            else
            {
                customerRenderer.material.color = normalColor;
            }
        }
        
        if (angerEffect != null)
        {
            angerEffect.SetActive(isAngry);
        }
    }
    
    protected virtual void BecomeAngry()
    {
        isAngry = true;
        Debug.Log($"Клиент {customerType} разозлился!");
        
        // Увеличиваем стресс игрока
        if (StressManager.Instance != null)
        {
            StressManager.Instance.AddStress(5f);
        }
    }
    
    public virtual void StartService(CashRegister register)
    {
        if (register == null) return;
        
        isBeingServed = true;
        targetRegister = register.transform;
        
        // Проверяем специальные товары
        if (hasAlcohol && !CanBuyAlcohol())
        {
            // Штраф за продажу алкоголя в неподходящее время или несовершеннолетнему
            HandleAlcoholViolation();
        }
        
        if (hasCigarettes && !CanBuyCigarettes())
        {
            // Штраф за продажу сигарет в понедельник
            HandleCigaretteViolation();
        }
    }
    
    protected virtual bool CanBuyAlcohol()
    {
        if (TimeManager.Instance == null) return true;
        
        // Проверяем время продажи алкоголя
        if (!TimeManager.Instance.IsAlcoholSaleAllowed()) return false;
        
        // Для подростков - всегда запрещено
        if (customerType == CustomerType.Teenager) return false;
        
        return true;
    }
    
    protected virtual bool CanBuyCigarettes()
    {
        if (TimeManager.Instance == null) return true;
        
        // Проверяем день без табака
        return TimeManager.Instance.IsCigaretteSaleAllowed();
    }
    
    protected virtual void HandleAlcoholViolation()
    {
        if (FineSystem.Instance != null)
        {
            if (customerType == CustomerType.Teenager)
            {
                FineSystem.Instance.ApplyFine(FineSystem.FineType.AlcoholToMinor);
            }
            else
            {
                FineSystem.Instance.ApplyFine(FineSystem.FineType.AlcoholAfterHours);
            }
        }
    }
    
    protected virtual void HandleCigaretteViolation()
    {
        // Штраф за продажу сигарет в день без табака
        Debug.Log("Штраф за продажу сигарет в понедельник!");
    }
    
    public virtual float GetServiceTime()
    {
        float baseTime = itemCount * serviceSpeed;
        
        // Модификаторы времени обслуживания
        if (isAngry) baseTime *= 1.3f; // Злые клиенты дольше обслуживаются
        if (hasSpecialItems) baseTime *= 1.2f; // Особые товары требуют больше времени
        
        return baseTime;
    }
    
    public virtual void CompleteService()
    {
        isBeingServed = false;
        
        // Добавляем доход
        if (EconomyManager.Instance != null)
        {
            float income = CalculateIncome();
            EconomyManager.Instance.AddIncome(income);
        }
        
        // Снижаем стресс за успешное обслуживание
        if (StressManager.Instance != null && !isAngry)
        {
            StressManager.Instance.ReduceStress(1f);
        }
        
        LeaveStore();
    }
    
    protected virtual float CalculateIncome()
    {
        float baseIncome = itemCount * 50f; // 50₽ за товар
        
        // Бонусы для VIP клиентов
        if (customerType == CustomerType.VIP)
        {
            baseIncome *= 1.5f;
        }
        
        // Штраф за злых клиентов (меньший доход)
        if (isAngry)
        {
            baseIncome *= 0.7f;
        }
        
        return baseIncome;
    }
    
    public virtual void LeaveStore()
    {
        if (hasLeft) return;
        
        hasLeft = true;
        
        // Если клиент ушел не обслуженным и был зол
        if (!isBeingServed && isAngry)
        {
            HandleAngryLeave();
        }
        
        StartCoroutine(LeaveStoreCoroutine());
    }
    
    protected virtual void HandleAngryLeave()
    {
        // Увеличиваем стресс и возможный штраф
        if (StressManager.Instance != null)
        {
            StressManager.Instance.AddStress(10f);
        }
        
        Debug.Log($"Злой клиент {customerType} покинул магазин!");
    }
    
    protected virtual IEnumerator LeaveStoreCoroutine()
    {
        // Анимация выхода из магазина
        float exitTime = 2f;
        Vector3 startPos = transform.position;
        Vector3 exitPos = startPos + Vector3.left * 10f; // Выход влево
        
        float elapsedTime = 0f;
        while (elapsedTime < exitTime)
        {
            transform.position = Vector3.Lerp(startPos, exitPos, elapsedTime / exitTime);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        
        Destroy(gameObject);
    }
    
    // Абстрактные методы для переопределения в наследниках
    public abstract void ReactToWaiting();
    public abstract float GetKickFineRisk();
    
    // Метод для "кика" клиента
    public virtual void GetKicked(bool isProvoked = false)
    {
        Debug.Log($"Клиент {customerType} получил кик!");
        
        // Применяем штраф с учетом типа клиента
        if (FineSystem.Instance != null)
        {
            FineSystem.Instance.ApplyKickFine(this, isProvoked);
        }
        
        // Снижаем стресс игрока (временное облегчение)
        if (StressManager.Instance != null)
        {
            StressManager.Instance.ReduceStress(10f);
        }
        
        // Клиент немедленно покидает магазин
        LeaveStore();
    }
    
    public float GetPatiencePercentage()
    {
        return currentPatience / patience;
    }
    
    public bool IsWaitingTooLong()
    {
        return currentPatience <= patience * 0.3f;
    }
}
```

### 4.2 ElderlyCustomer.cs - Пожилой клиент

```csharp
using UnityEngine;

public class ElderlyCustomer : Customer
{
    [Header("Elderly Specific")]
    public bool isOnPensionerDiscount = false;
    public float discountMultiplier = 0.8f; // 20% скидка в среду
    
    protected override void Start()
    {
        customerType = CustomerType.Elderly;
        serviceSpeed = 25f; // Медленное обслуживание
        patience = 240f; // 4 минуты терпения
        moveSpeed = 1f; // Медленно передвигается
        itemCount = Random.Range(3, 12); // Больше товаров
        
        // Проверяем день скидок для пенсионеров
        if (TimeManager.Instance != null)
        {
            isOnPensionerDiscount = TimeManager.Instance.GetCurrentDayOfWeek() == WeekDay.Wednesday;
        }
        
        base.Start();
        
        // Меньше шансов на алкоголь и сигареты
        hasAlcohol = Random.Range(0f, 1f) < 0.05f; // 5% шанс
        hasCigarettes = Random.Range(0f, 1f) < 0.1f; // 10% шанс
        
        normalColor = new Color(0.8f, 0.8f, 1f); // Голубоватый оттенок
    }
    
    public override void ReactToWaiting()
    {
        // Пожилые клиенты вежливо просят помощи
        Debug.Log("Пожилой клиент: 'Извините, можно мне помочь?'");
        
        // Меньше стресса от пожилых клиентов
        if (StressManager.Instance != null)
        {
            StressManager.Instance.AddStress(2f);
        }
        
        // Показываем речевой пузырь
        ShowSpeechBubble("Помогите, пожалуйста!");
    }
    
    public override float GetKickFineRisk()
    {
        return 0.7f; // 70% риск штрафа 2000₽
    }
    
    protected override float CalculateIncome()
    {
        float income = base.CalculateIncome();
        
        // Применяем скидку для пенсионеров в среду
        if (isOnPensionerDiscount)
        {
            income *= discountMultiplier;
            Debug.Log("Применена скидка для пенсионера 20%");
        }
        
        return income;
    }
    
    protected override void BecomeAngry()
    {
        // Пожилые редко злятся, скорее расстраиваются
        isAngry = true;
        Debug.Log("Пожилой клиент расстроился из-за долгого ожидания");
        
        // Меньше стресса от расстроенных пожилых
        if (StressManager.Instance != null)
        {
            StressManager.Instance.AddStress(3f);
        }
    }
    
    public override void GetKicked(bool isProvoked = false)
    {
        Debug.Log("Пожилой клиент шокирован грубостью!");
        
        // Высокий риск штрафа за кик пожилого
        if (FineSystem.Instance != null)
        {
            FineSystem.Instance.ApplyFine(FineSystem.FineType.KickElderly, 1f, isProvoked);
        }
        
        // Увеличиваем стресс вместо снижения (плохой поступок)
        if (StressManager.Instance != null)
        {
            StressManager.Instance.AddStress(15f);
        }
        
        base.LeaveStore();
    }
    
    protected override void HandleAngryLeave()
    {
        base.HandleAngryLeave();
        Debug.Log("Пожилой клиент: 'Безобразие! Пожалуюсь администрации!'");
        
        // Дополнительный штраф за плохое обслуживание пожилых
        if (Random.Range(0f, 1f) < 0.3f) // 30% шанс
        {
            if (FineSystem.Instance != null)
            {
                FineSystem.Instance.ApplyFine(FineSystem.FineType.KickElderly, 0.5f);
            }
        }
    }
    
    private void ShowSpeechBubble(string text)
    {
        // Показываем речевой пузырь над клиентом
        if (NotificationSystem.Instance != null)
        {
            // Создаем временное уведомление над клиентом
            GameObject bubble = new GameObject("SpeechBubble");
            bubble.transform.SetParent(transform);
            bubble.transform.localPosition = Vector3.up * 2f;
            
            // Здесь можно добавить UI элементы для отображения текста
            // ...
            
            // Удаляем через 3 секунды
            Destroy(bubble, 3f);
        }
    }
}
```

### 4.3 TeenagerCustomer.cs - Подросток

```csharp
public class TeenagerCustomer : Customer
{
    public bool tryingToBuyAlcohol = false;

    void Start()
    {
        customerType = CustomerType.Teenager;
        serviceSpeed = 12f;
        patience = 90f; // 1.5 минуты
        tryingToBuyAlcohol = Random.Range(0f, 1f) < 0.3f;
    }

    public override void ReactToWaiting()
    {
        // Нервничает, постукивает ногой
    }

    public override float GetKickFineRisk()
    {
        return 0.5f; // 50% риск штрафа 1500₽
    }
}

```

### 4.4 RegularCustomer.cs - Обычный клиент

```csharp
public class RegularCustomer : Customer
{
    void Start()
    {
        customerType = CustomerType.Regular;
        serviceSpeed = 10f;
        patience = 150f; // 2.5 минуты
    }

    public override void ReactToWaiting()
    {
        // Спокойное ожидание
    }

    public override float GetKickFineRisk()
    {
        return 0.2f; // 20% риск штрафа 800₽
    }
}

```

### 4.5 AggressiveCustomer.cs - Агрессивный клиент

```csharp
public class AggressiveCustomer : Customer
{
    public enum AggressionLevel { Calm, Grumbling, Yelling, Scandal }
    public AggressionLevel currentAggression = AggressionLevel.Calm;

    void Start()
    {
        customerType = CustomerType.Aggressive;
        serviceSpeed = 8f;
        patience = 45f; // 30-60 сек
    }

    public override void ReactToWaiting()
    {
        // Эскалация агрессии
        if (currentPatience < patience * 0.7f) currentAggression = AggressionLevel.Grumbling;
        if (currentPatience < patience * 0.4f) currentAggression = AggressionLevel.Yelling;
        if (currentPatience < patience * 0.1f) currentAggression = AggressionLevel.Scandal;
    }

    public override float GetKickFineRisk()
    {
        return 0.1f; // 10% риск штрафа 500₽
    }
}

```

### 4.6 VIPCustomer.cs - VIP клиент

```csharp
public class VIPCustomer : Customer
{
    public bool isMysteryShoper = false;

    void Start()
    {
        customerType = CustomerType.VIP;
        serviceSpeed = 10f;
        patience = 60f; // 1 минута
        isMysteryShoper = Random.Range(0f, 1f) < 0.2f;
    }

    public override void ReactToWaiting()
    {
        // Высокомерное поведение, угрозы жалоб
    }

    public override float GetKickFineRisk()
    {
        return 0.8f; // 80% риск штрафа 5000₽
    }
}

```

## Этап 5: Система спавна и AI (Неделя 3)

### 5.1 CustomerSpawner.cs - Генератор клиентов

```csharp
public class CustomerSpawner : MonoBehaviour
{
    public GameObject[] customerPrefabs;
    public Transform[] spawnPoints;
    public float baseSpawnRate = 30f; // секунды
    public float peakHourMultiplier = 2f;

    void Start() { StartCoroutine(SpawnCustomers()); }

    IEnumerator SpawnCustomers()
    {
        while (GameManager.Instance.currentState == GameState.Playing)
        {
            yield return new WaitForSeconds(GetSpawnDelay());
            SpawnRandomCustomer();
        }
    }

    void SpawnRandomCustomer()
    CustomerType GetRandomCustomerType()
    float GetSpawnDelay()
}

```

### 5.2 CustomerAI.cs - ИИ поведение клиентов

```csharp
public class CustomerAI : MonoBehaviour
{
    public Customer customer;
    public NavMeshAgent agent;
    public enum AIState { MovingToRegister, WaitingInQueue, BeingServed, Leaving }
    public AIState currentState;

    void Start() { FindNearestRegister(); }
    void Update() { UpdateAI(); }

    void FindNearestRegister()
    void MoveToRegister(CashRegister target)
    void WaitInQueue()
    void GetServed()
    void LeaveStore()
}

```

### 5.3 QueueManager.cs - Система очередей

```csharp
public class QueueManager : MonoBehaviour
{
    public Dictionary<CashRegister, Queue<Customer>> registerQueues;

    public void AddToQueue(CashRegister register, Customer customer)
    public Customer GetNextInQueue(CashRegister register)
    public int GetQueueLength(CashRegister register)
    public CashRegister GetShortestQueue()
}

```

## Этап 6: Система стресса и "кика" (Неделя 3-4)

### 6.1 StressManager.cs - Управление стрессом

```csharp
public class StressManager : MonoBehaviour
{
    public static StressManager Instance;
    public float currentStress = 0f;
    public float maxStress = 100f;

    public enum StressLevel { Normal, Tired, Stressed, Panic }
    public StressLevel currentLevel;

    void Update() { ApplyStressEffects(); }

    public void AddStress(float amount)
    public void ReduceStress(float amount)
    public void ApplyStressEffects()
    public StressLevel GetStressLevel()
}

```

### 6.2 KickSystem.cs - Система "кика"

```csharp
public class KickSystem : MonoBehaviour
{
    public int kicksUsedToday = 0;
    public int maxKicksPerDay = 5;
    public float kickStressReduction = 10f;

    public bool CanUseKick()
    public void PerformKick(CashRegister register)
    public float CalculateKickFineChance(Customer customer)
    public void ApplyKickConsequences(Customer customer)
}

```

### 6.3 StressEffects.cs - Эффекты стресса

```csharp
public class StressEffects : MonoBehaviour
{
    public Camera playerCamera;
    public float shakeIntensity = 0f;

    void Update()
    {
        ApplyCameraShake();
        ApplyErrorChance();
    }

    void ApplyCameraShake()
    void ApplyErrorChance() // 20% ошибок при высоком стрессе
    void TriggerRandomActions() // При панике
}

```

## Этап 7: Персонал и помощники (Неделя 4)

### 7.1 Staff.cs - Базовый класс персонала

```csharp
public abstract class Staff : MonoBehaviour
{
    public string staffName;
    public float dailyCost;
    public bool isHired = false;
    public float efficiency = 1f;

    public abstract void PerformDuties();
    public virtual void HireStaff()
    public virtual void FireStaff()
}

```

### 7.2 Mechanic.cs - Механик

```csharp
public class Mechanic : Staff
{
    public float repairSpeed = 5f; // быстрый ремонт

    void Start()
    {
        staffName = "Механик";
        dailyCost = 3000f;
    }

    public override void PerformDuties()
    {
        // Автоматически чинит сломанные кассы
        StartCoroutine(AutoRepair());
    }

    IEnumerator AutoRepair()
}

```

### 7.3 Assistant.cs - Помощник

```csharp
public class Assistant : Staff
{
    void Start()
    {
        staffName = "Помощник";
        dailyCost = 2000f;
    }

    public override void PerformDuties()
    {
        // Автоматически помогает клиентам
        StartCoroutine(AutoHelp());
    }

    IEnumerator AutoHelp()
}

```

### 7.4 SecurityGuard.cs - Охранник

```csharp
public class SecurityGuard : Staff
{
    public float theftReduction = 0.3f;
    public float aggressionReduction = 0.2f;

    void Start()
    {
        staffName = "Охранник";
        dailyCost = 2500f;
    }

    public override void PerformDuties()
    {
        // Снижает кражи и агрессию
        ReduceTheftRisk();
        CalmAggressiveCustomers();
    }
}

```

## Этап 8: Система достижений и прогрессии (Неделя 4-5)

### 8.1 Achievement.cs - Класс достижения

```csharp
[System.Serializable]
public class Achievement
{
    public string id;
    public string title;
    public string description;
    public bool isUnlocked = false;
    public int targetValue;
    public int currentProgress;

    public bool CheckProgress(int newValue)
    public void Unlock()
}

```

### 8.2 AchievementManager.cs - Менеджер достижений

```csharp
public class AchievementManager : MonoBehaviour
{
    public static AchievementManager Instance;
    public List<Achievement> achievements;

    void Start() { InitializeAchievements(); }

    public void UpdateProgress(string achievementId, int value)
    public void UnlockAchievement(string achievementId)
    void InitializeAchievements()
}

```

### 8.3 ProgressionManager.cs - Система прогрессии

```csharp
public class ProgressionManager : MonoBehaviour
{
    public enum StoreLevel { Startup, Developing, Stable, Network }
    public StoreLevel currentLevel = StoreLevel.Startup;
    public int maxCashRegisters = 4;

    public void CheckLevelUp()
    public void UnlockNewFeatures(StoreLevel level)
    public bool CanUpgradeToLevel(StoreLevel targetLevel)
}

```

## Этап 9: Система сохранения (Неделя 5)

### 9.1 SaveSystem.cs - Система сохранения

```csharp
[System.Serializable]
public class GameData
{
    public float balance;
    public int currentDay;
    public int storeLevel;
    public List<bool> unlockedAchievements;
    public List<bool> purchasedItems;
    public List<int> registerTypes;
    public List<bool> hiredStaff;
}

public class SaveSystem : MonoBehaviour
{
    public static void SaveGame(GameData data)
    public static GameData LoadGame()
    public static bool HasSaveFile()
    public static void DeleteSave()
}

```

### 9.2 GameDataManager.cs - Управление данными

```csharp
public class GameDataManager : MonoBehaviour
{
    public static GameDataManager Instance;
    public GameData currentData;

    void Start() { LoadGameData(); }
    void OnApplicationPause(bool pauseStatus) { SaveGameData(); }
    void OnApplicationFocus(bool hasFocus) { SaveGameData(); }

    public void SaveGameData()
    public void LoadGameData()
    public void ResetGameData()
}

```

## Этап 10: UI скрипты (Неделя 5-6)

### 10.1 UIManager.cs - Основной UI менеджер

```csharp
public class UIManager : MonoBehaviour
{
    public static UIManager Instance;
    public Text balanceText;
    public Text timeText;
    public Text dayText;
    public Slider stressSlider;
    public GameObject pauseMenu;
    public GameObject shopMenu;

    void Update() { UpdateUI(); }

    void UpdateUI()
    public void ShowPauseMenu()
    public void ShowShopMenu()
    public void ShowAchievements()
}

```

### 10.2 CashRegisterUI.cs - UI кассы

```csharp
public class CashRegisterUI : MonoBehaviour
{
    public CashRegister linkedRegister;
    public Image statusIndicator;
    public Button helpButton;
    public Button repairButton;
    public Button rebootButton;
    public Button kickButton;

    void Update() { UpdateRegisterUI(); }

    void UpdateRegisterUI()
    public void OnHelpClick()
    public void OnRepairClick()
    public void OnRebootClick()
    public void OnKickClick()
}

```

### 10.3 NotificationSystem.cs - Система уведомлений

```csharp
public class NotificationSystem : MonoBehaviour
{
    public static NotificationSystem Instance;
    public GameObject notificationPrefab;
    public Transform notificationParent;

    public void ShowNotification(string message, Color color)
    public void ShowFineNotification(float amount)
    public void ShowAchievementNotification(Achievement achievement)
    IEnumerator DestroyNotification(GameObject notification, float delay)
}

```

## Этап 11: Аудио система (Неделя 6)

### 11.1 AudioManager.cs - Менеджер звуков

```csharp
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;
    public AudioSource musicSource;
    public AudioSource sfxSource;

    [Header("Sound Effects")]
    public AudioClip scanningSound;
    public AudioClip breakdownSound;
    public AudioClip repairSound;
    public AudioClip kickSound;
    public AudioClip fineSound;
    public AudioClip achievementSound;

    public void PlaySFX(AudioClip clip, float volume = 1f)
    public void PlayMusic(AudioClip clip, bool loop = true)
    public void SetMusicVolume(float volume)
    public void SetSFXVolume(float volume)
}

```

## Этап 12: Финальная интеграция (Неделя 6-7)

### 12.1 GameController.cs - Главный контроллер

```csharp
public class GameController : MonoBehaviour
{
    void Start()
    {
        InitializeAllSystems();
        StartGameSession();
    }

    void InitializeAllSystems()
    {
        // Инициализация всех менеджеров в правильном порядке
    }

    void StartGameSession()
    void EndGameSession()
    void CheckWinConditions()
    void CheckLoseConditions()
}

```

### 12.2 SceneTransitionManager.cs - Переходы между сценами

```csharp
public class SceneTransitionManager : MonoBehaviour
{
    public static SceneTransitionManager Instance;

    public void LoadMainMenu()
    public void LoadGameScene()
    public void RestartLevel()
    public void QuitGame()

    IEnumerator LoadSceneAsync(string sceneName)
}

```

## Порядок тестирования скриптов:

1. **Тестируйте по частям** - каждый скрипт отдельно
2. **Создавайте заглушки** - для еще не созданных систем
3. **Используйте Debug.Log** - для отслеживания работы
4. **Тестируйте граничные случаи** - экстремальные значения
5. **Интеграционное тестирование** - взаимодействие систем

Этот порядок обеспечивает пошаговое создание всех необходимых скриптов с минимальными зависимостями между этапами.