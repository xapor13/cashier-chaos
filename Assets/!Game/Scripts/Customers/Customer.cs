using UnityEngine;
using System.Collections;
using GameCore;

public abstract class Customer : MonoBehaviour
{
    private const float DefaultServiceSpeed = 10f; // Секунд на товар
    private const float DefaultPatience = 120f; // Секунд ожидания
    private const float MoveSpeed = 2f; // Скорость движения
    private const float AlcoholChance = 0.2f; // 20% шанс покупки алкоголя
    private const float CigaretteChance = 0.15f; // 15% шанс покупки сигарет
    private const float AngryPatienceThreshold = 0.3f; // Порог злости (30% терпения)
    private const float AngryServiceTimeMultiplier = 1.3f; // Увеличение времени для злых клиентов
    private const float SpecialItemsTimeMultiplier = 1.2f; // Увеличение времени для особых товаров
    private const float BaseIncomePerItem = 50f; // Базовый доход за товар
    private const float VIPIncomeMultiplier = 1.5f; // Бонус дохода для VIP
    private const float AngryIncomeMultiplier = 0.7f; // Штраф дохода для злых клиентов
    private const float AngryStressIncrease = 5f; // Увеличение стресса при злости
    private const float AngryLeaveStressIncrease = 10f; // Увеличение стресса при уходе злого клиента
    private const float ServiceStressReduction = 1f; // Снижение стресса при успешном обслуживании
    private const float KickStressReduction = 10f; // Снижение стресса при кике
    private const int MinItemCount = 1; // Минимальное количество товаров
    private const int MaxItemCount = 8; // Максимальное количество товаров
    private const float ExitTime = 2f; // Время выхода из магазина
    private const float ExitDistance = 10f; // Расстояние выхода влево
    private const float BlinkFrequency = 5f; // Частота мигания для злого клиента

    [Header("Зависимости")]
    [SerializeField] private EconomyManager economyManager;
    [SerializeField] private TimeManager timeManager;
    [SerializeField] private FineSystem fineSystem;
    [SerializeField] private StressManager stressManager;

    [Header("Настройки клиента")]
    [SerializeField] private CustomerType customerType;
    [SerializeField] private float serviceSpeed = DefaultServiceSpeed;
    [SerializeField] private float patience = DefaultPatience;
    [SerializeField] private float currentPatience;
    [SerializeField] private int itemCount;

    [Header("Движение")]
    [SerializeField] private Transform targetRegister;

    [Header("Особые товары")]
    [SerializeField] private bool hasAlcohol = false;
    [SerializeField] private bool hasCigarettes = false;
    [SerializeField] private bool hasSpecialItems = false;

    [Header("Визуальные эффекты")]
    [SerializeField] private Renderer customerRenderer;
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color angryColor = Color.red;
    [SerializeField] private GameObject angerEffect;

    private bool isBeingServed = false;
    private bool hasLeft = false;
    private bool isAngry = false;

    protected virtual void Start()
    {
        currentPatience = patience;
        itemCount = Random.Range(MinItemCount, MaxItemCount);
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

    private void UpdatePatience()
    {
        currentPatience -= Time.deltaTime;

        if (currentPatience <= 0 && !hasLeft)
        {
            ReactToWaiting();
            LeaveStore();
        }
        else if (currentPatience <= patience * AngryPatienceThreshold && !isAngry)
        {
            BecomeAngry();
        }
    }

    private void GenerateSpecialItems()
    {
        hasAlcohol = Random.Range(0f, 1f) < AlcoholChance;
        hasCigarettes = Random.Range(0f, 1f) < CigaretteChance;
        hasSpecialItems = hasAlcohol || hasCigarettes;
    }

    private void SetCustomerAppearance()
    {
        if (customerRenderer != null)
        {
            customerRenderer.material.color = normalColor;
        }
    }

    private void UpdateVisuals()
    {
        if (customerRenderer != null)
        {
            customerRenderer.material.color = isAngry
                ? Color.Lerp(normalColor, angryColor, Mathf.Sin(Time.time * BlinkFrequency) * 0.5f + 0.5f)
                : normalColor;
        }

        if (angerEffect != null)
        {
            angerEffect.SetActive(isAngry);
        }
    }

    private void BecomeAngry()
    {
        isAngry = true;
        Debug.Log($"Клиент {customerType} разозлился!");
        stressManager?.AddStress(AngryStressIncrease);
    }

    public virtual void StartService(CashRegister register)
    {
        if (register == null)
        {
            return;
        }

        isBeingServed = true;
        targetRegister = register.transform;

        CheckSpecialItems();
    }

    private void CheckSpecialItems()
    {
        if (hasAlcohol && !CanBuyAlcohol())
        {
            HandleAlcoholViolation();
        }

        if (hasCigarettes && !CanBuyCigarettes())
        {
            HandleCigaretteViolation();
        }
    }

    protected virtual bool CanBuyAlcohol()
    {
        if (timeManager == null)
        {
            return true;
        }

        if (!timeManager.IsAlcoholSaleAllowed() || customerType == CustomerType.Teenager)
        {
            return false;
        }

        return true;
    }

    protected virtual bool CanBuyCigarettes()
    {
        return timeManager != null && timeManager.IsCigaretteSaleAllowed();
    }

    private void HandleAlcoholViolation()
    {
        fineSystem?.ApplyFine(customerType == CustomerType.Teenager
            ? FineType.AlcoholToMinor
            : FineType.AlcoholAfterHours);
    }

    private void HandleCigaretteViolation()
    {
        Debug.Log("Штраф за продажу сигарет в понедельник!");
    }

    public virtual float GetServiceTime()
    {
        float baseTime = itemCount * serviceSpeed;

        if (isAngry)
        {
            baseTime *= AngryServiceTimeMultiplier;
        }

        if (hasSpecialItems)
        {
            baseTime *= SpecialItemsTimeMultiplier;
        }

        return baseTime;
    }

    public virtual void CompleteService()
    {
        isBeingServed = false;

        if (!isAngry)
        {
            stressManager?.ReduceStress(ServiceStressReduction);
        }

        float income = CalculateIncome();
        economyManager?.AddIncome(income);

        LeaveStore();
    }

    private float CalculateIncome()
    {
        float baseIncome = itemCount * BaseIncomePerItem;

        if (customerType == CustomerType.VIP)
        {
            baseIncome *= VIPIncomeMultiplier;
        }

        if (isAngry)
        {
            baseIncome *= AngryIncomeMultiplier;
        }

        return baseIncome;
    }

    public virtual void LeaveStore()
    {
        if (hasLeft)
        {
            return;
        }

        hasLeft = true;

        if (!isBeingServed && isAngry)
        {
            HandleAngryLeave();
        }

        StartCoroutine(LeaveStoreCoroutine());
    }

    private void HandleAngryLeave()
    {
        stressManager?.AddStress(AngryLeaveStressIncrease);
        Debug.Log($"Злой клиент {customerType} покинул магазин!");
    }

    private IEnumerator LeaveStoreCoroutine()
    {
        Vector3 startPos = transform.position;
        Vector3 exitPos = startPos + Vector3.left * ExitDistance;

        float elapsedTime = 0f;
        while (elapsedTime < ExitTime)
        {
            transform.position = Vector3.Lerp(startPos, exitPos, elapsedTime / ExitTime);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        Destroy(gameObject);
    }

    public virtual void GetKicked(bool isProvoked = false)
    {
        Debug.Log($"Клиент {customerType} получил кик!");
        fineSystem?.ApplyKickFine(this, isProvoked);
        stressManager?.ReduceStress(KickStressReduction);
        LeaveStore();
    }

    public float GetPatiencePercentage()
    {
        return currentPatience / patience;
    }

    public bool IsWaitingTooLong()
    {
        return currentPatience <= patience * AngryPatienceThreshold;
    }

    // Абстрактные методы для переопределения в наследниках
    public abstract void ReactToWaiting();
    public abstract float GetKickFineRisk();

    // Геттеры для доступа к состоянию
    public CustomerType GetCustomerType() => customerType;
    public bool IsAngry() => isAngry;
    public bool IsBeingServed() => isBeingServed;
    public bool HasLeft() => hasLeft;
    public bool HasSpecialItems() => hasSpecialItems;
}