using UnityEngine;
using GameCore;

public class TimeManager : MonoBehaviour
{
    private const float MorningPeakStart = 12f; // 12:00
    private const float MorningPeakEnd = 14f; // 14:00
    private const float EveningPeakStart = 18f; // 18:00
    private const float EveningPeakEnd = 20f; // 20:00
    private const float AlcoholSaleEndTime = 22f; // 22:00
    private const float PeakHourMultiplier = 2f; // Множитель в часы пик
    private const float NormalHourMultiplier = 1f; // Множитель в обычные часы
    private const int DaysInWeek = 7; // Дней в неделе

    [Header("Зависимости")]
    [SerializeField] private GameManager gameManager;

    [Header("Состояние времени")]
    [SerializeField] private float currentTime = 8f; // Текущее время (8:00)
    [SerializeField] private int currentDay = 1; // Текущий день
    [SerializeField] private bool isPeakHours = false; // Флаг часов пик

    private void Update()
    {
        if (gameManager != null)
        {
            SyncWithGameManager();
        }

        UpdatePeakHoursStatus();
    }

    private void SyncWithGameManager()
    {
        currentTime = gameManager.GetCurrentGameTime();
        currentDay = gameManager.GetCurrentDay();
    }

    private void UpdatePeakHoursStatus()
    {
        isPeakHours = IsMorningPeak() || IsEveningPeak();
    }

    private bool IsMorningPeak()
    {
        return currentTime >= MorningPeakStart && currentTime <= MorningPeakEnd;
    }

    private bool IsEveningPeak()
    {
        return currentTime >= EveningPeakStart && currentTime <= EveningPeakEnd;
    }

    public bool IsAlcoholSaleAllowed()
    {
        return currentTime < AlcoholSaleEndTime;
    }

    public bool IsCigaretteSaleAllowed()
    {
        return GetCurrentDayOfWeek() != WeekDay.Monday; // Без табака по понедельникам
    }

    public WeekDay GetCurrentDayOfWeek()
    {
        int dayIndex = (currentDay - 1) % DaysInWeek;
        return (WeekDay)dayIndex;
    }

    public float GetPeakHourMultiplier()
    {
        return isPeakHours ? PeakHourMultiplier : NormalHourMultiplier;
    }

    public string GetFormattedTime()
    {
        int hours = Mathf.FloorToInt(currentTime);
        int minutes = Mathf.FloorToInt((currentTime - hours) * 60);
        return $"{hours:00}:{minutes:00}";
    }

    public string GetDayInfo()
    {
        WeekDay today = GetCurrentDayOfWeek();
        return GetDayDescription(today);
    }

    private string GetDayDescription(WeekDay day)
    {
        switch (day)
        {
            case WeekDay.Monday:
                return $"{day} (День без табака)";
            case WeekDay.Wednesday:
                return $"{day} (Скидки для пенсионеров)";
            case WeekDay.Friday:
                return $"{day} (Молодежный день)";
            case WeekDay.Saturday:
            case WeekDay.Sunday:
                return $"{day} (Семейный день)";
            default:
                return day.ToString();
        }
    }

    // Геттеры для доступа к состоянию
    public float GetCurrentTime() => currentTime;
    public int GetCurrentDay() => currentDay;
    public bool IsPeakHours() => isPeakHours;
}