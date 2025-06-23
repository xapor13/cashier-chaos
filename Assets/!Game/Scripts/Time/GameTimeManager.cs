using UnityEngine;
using UnityEngine.Events;
using System;

public class GameTimeManager : MonoBehaviour
{
    [SerializeField] private TimeSettings timeSettings; // Reference to settings

    // Events for other systems
    public UnityEvent onPeakHoursStarted = new UnityEvent();
    public UnityEvent onPeakHoursEnded = new UnityEvent();
    public UnityEvent onAlcoholRestrictionStarted = new UnityEvent();
    public UnityEvent onDayStarted = new UnityEvent();
    public UnityEvent onDayEnded = new UnityEvent();

    private float currentGameTime; // Seconds since day start
    private int currentDayOfWeek; // 0 = Monday, 6 = Sunday
    private bool isPeakHoursActive;
    private bool isAlcoholRestricted;

    public float CurrentGameTime => currentGameTime;
    public int CurrentHour => Mathf.FloorToInt(currentGameTime / Mathf.Max(timeSettings.realSecondsPerGameHour, 0.1f)) + timeSettings.startHour;
    public int CurrentMinute => Mathf.FloorToInt((currentGameTime % Mathf.Max(timeSettings.realSecondsPerGameHour, 0.1f)) / (Mathf.Max(timeSettings.realSecondsPerGameHour, 0.1f) / 60f));
    public bool IsPeakHoursActive => isPeakHoursActive;
    public bool IsAlcoholRestricted => isAlcoholRestricted;
    public DayOfWeek DayOfWeek => (DayOfWeek)currentDayOfWeek;

    private void Start()
    {
        StartNewDay();
    }

    private void Update()
    {
        UpdateGameTime();
    }

    private void StartNewDay()
    {
        currentGameTime = 0f;
        currentDayOfWeek = (currentDayOfWeek + 1) % 7; // Advance to next day
        isPeakHoursActive = false;
        isAlcoholRestricted = false;
        onDayStarted.Invoke();
    }

    private void UpdateGameTime()
    {
        currentGameTime += Time.deltaTime;

        // Check if day is over
        float totalDaySeconds = (timeSettings.endHour - timeSettings.startHour) * Mathf.Max(timeSettings.realSecondsPerGameHour, 0.1f);
        if (currentGameTime >= totalDaySeconds)
        {
            onDayEnded.Invoke();
            StartNewDay();
            return;
        }

        // Update peak hours
        int currentHour = CurrentHour;
        bool shouldBePeakHours =
            (currentHour >= timeSettings.firstPeakHours.x && currentHour < timeSettings.firstPeakHours.y) ||
            (currentHour >= timeSettings.secondPeakHours.x && currentHour < timeSettings.secondPeakHours.y);

        if (shouldBePeakHours != isPeakHoursActive)
        {
            isPeakHoursActive = shouldBePeakHours;
            if (isPeakHoursActive)
                onPeakHoursStarted.Invoke();
            else
                onPeakHoursEnded.Invoke();
        }

        // Update alcohol restriction
        bool shouldBeRestricted = currentHour >= timeSettings.alcoholRestrictionHour;
        if (shouldBeRestricted != isAlcoholRestricted)
        {
            isAlcoholRestricted = shouldBeRestricted;
            if (isAlcoholRestricted)
                onAlcoholRestrictionStarted.Invoke();
        }
    }

    // Helper method to get formatted time string (e.g., "08:30")
    public string GetFormattedTime()
    {
        return $"{CurrentHour:00}:{CurrentMinute:00}";
    }

    // Helper method to get day of week name in Russian
    public string GetDayOfWeekName()
    {
        switch (DayOfWeek)
        {
            case DayOfWeek.Monday: return "Понедельник";
            case DayOfWeek.Tuesday: return "Вторник";
            case DayOfWeek.Wednesday: return "Среда";
            case DayOfWeek.Thursday: return "Четверг";
            case DayOfWeek.Friday: return "Пятница";
            case DayOfWeek.Saturday: return "Суббота";
            case DayOfWeek.Sunday: return "Воскресенье";
            default: return "Неизвестный день";
        }
    }

    // Helper method to check special day conditions
    public bool IsNoTobaccoDay() => DayOfWeek == DayOfWeek.Monday;
    public bool IsPensionerDiscountDay() => DayOfWeek == DayOfWeek.Wednesday;
    public bool IsYouthDay() => DayOfWeek == DayOfWeek.Friday;
    public bool IsFamilyDay() => DayOfWeek == DayOfWeek.Saturday || DayOfWeek == DayOfWeek.Sunday;
}