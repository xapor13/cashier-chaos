using UnityEngine;
using GameCore;

public class ElderlyCustomer : Customer
{
    private const float ElderlyServiceSpeed = 25f; // Медленное обслуживание
    private const float ElderlyPatience = 240f; // 4 минуты терпения
    private const float ElderlyMoveSpeed = 1f; // Медленная скорость
    private const int MinItemCount = 3; // Минимальное количество товаров
    private const int MaxItemCount = 12; // Максимальное количество товаров
    private const float AlcoholChance = 0.05f; // 5% шанс покупки алкоголя
    private const float CigaretteChance = 0.1f; // 10% шанс покупки сигарет
    private const float DiscountMultiplier = 0.8f; // 20% скидка в среду
    private const float KickFineRisk = 0.7f; // 70% риск штрафа за кик
    private const float AngryStressIncrease = 3f; // Стресс при расстройстве
    private const float WaitingStressIncrease = 2f; // Стресс при просьбе о помощи
    private const float KickStressIncrease = 15f; // Стресс при кике
    private const float AngryLeaveFineChance = 0.3f; // 30% шанс штрафа при уходе
    private const float AngryLeaveFineMultiplier = 0.5f; // Множитель штрафа при уходе
    private const float SpeechBubbleHeight = 2f; // Высота пузыря над клиентом
    private const float SpeechBubbleDuration = 3f; // Длительность пузыря

    [Header("Зависимости")]
    [SerializeField] private TimeManager timeManager;
    [SerializeField] private NotificationSystem notificationSystem;

    [Header("Особенности пожилого клиента")]
    [SerializeField] private bool isOnPensionerDiscount = false;
    [SerializeField] private float discountMultiplier = DiscountMultiplier;

    protected override void Start()
    {
        customerType = CustomerType.Elderly;
        serviceSpeed = ElderlyServiceSpeed;
        patience = ElderlyPatience;
        moveSpeed = ElderlyMoveSpeed;
        itemCount = Random.Range(MinItemCount, MaxItemCount);

        isOnPensionerDiscount = timeManager != null && timeManager.GetCurrentDayOfWeek() == WeekDay.Wednesday;

        hasAlcohol = Random.Range(0f, 1f) < AlcoholChance;
        hasCigarettes = Random.Range(0f, 1f) < CigaretteChance;
        normalColor = new Color(0.8f, 0.8f, 1f); // Голубоватый оттенок

        base.Start();
    }

    public override void ReactToWaiting()
    {
        Debug.Log("Пожилой клиент: 'Извините, можно мне помочь?'");
        stressManager?.AddStress(WaitingStressIncrease);
        ShowSpeechBubble("Помогите, пожалуйста!");
    }

    public override float GetKickFineRisk()
    {
        return KickFineRisk;
    }

    protected override float CalculateIncome()
    {
        float income = base.CalculateIncome();
        if (isOnPensionerDiscount)
        {
            income *= discountMultiplier;
            Debug.Log("Применена скидка для пенсионера 20%");
        }
        return income;
    }

    protected override void BecomeAngry()
    {
        isAngry = true;
        Debug.Log("Пожилой клиент расстроился из-за долгого ожидания");
        stressManager?.AddStress(AngryStressIncrease);
    }

    public override void GetKicked(bool isProvoked = false)
    {
        Debug.Log("Пожилой клиент шокирован грубостью!");
        fineSystem?.ApplyFine(FineType.KickElderly, 1f, isProvoked);
        stressManager?.AddStress(KickStressIncrease);
        base.LeaveStore();
    }

    protected override void HandleAngryLeave()
    {
        base.HandleAngryLeave();
        Debug.Log("Пожилой клиент: 'Безобразие! Пожалуюсь администрации!'");

        if (Random.Range(0f, 1f) < AngryLeaveFineChance)
        {
            fineSystem?.ApplyFine(FineType.KickElderly, AngryLeaveFineMultiplier);
        }
    }

    private void ShowSpeechBubble(string text)
    {
        if (notificationSystem == null)
        {
            return;
        }

        GameObject bubble = new GameObject("SpeechBubble");
        bubble.transform.SetParent(transform);
        bubble.transform.localPosition = Vector3.up * SpeechBubbleHeight;

        // Здесь можно добавить UI элементы для текста (заглушка для NotificationSystem)
        notificationSystem.ShowNotification(text, Color.white); // Пример вызова

        Destroy(bubble, SpeechBubbleDuration);
    }
}