using UnityEngine;
using TMPro;

public class TimeDisplay : MonoBehaviour
{
    [SerializeField] private GameTimeManager timeManager;
    [SerializeField] private EconomyManager economyManager;
    [SerializeField] private TMP_Text timeText;
    [SerializeField] private TMP_Text balanceText;
    [SerializeField] private TMP_Text statusText; // For peak hours and alcohol restriction

    private void OnEnable()
    {
        if (economyManager != null)
            economyManager.onBalanceChanged.AddListener(UpdateBalanceText);
        if (timeManager != null)
        {
            timeManager.onPeakHoursStarted.AddListener(UpdateStatusText);
            timeManager.onPeakHoursEnded.AddListener(UpdateStatusText);
            timeManager.onAlcoholRestrictionStarted.AddListener(UpdateStatusText);
            timeManager.onDayStarted.AddListener(UpdateStatusText);
        }
    }

    private void OnDisable()
    {
        if (economyManager != null)
            economyManager.onBalanceChanged.RemoveListener(UpdateBalanceText);
        if (timeManager != null)
        {
            timeManager.onPeakHoursStarted.RemoveListener(UpdateStatusText);
            timeManager.onPeakHoursEnded.RemoveListener(UpdateStatusText);
            timeManager.onAlcoholRestrictionStarted.RemoveListener(UpdateStatusText);
            timeManager.onDayStarted.RemoveListener(UpdateStatusText);
        }
    }

    private void Update()
    {
        timeText.text = $"{timeManager.GetDayOfWeekName()}, {timeManager.GetFormattedTime()}";
        UpdateBalanceText();
        UpdateStatusText();
    }

    private void UpdateBalanceText()
    {
        if (balanceText != null && economyManager != null)
            balanceText.text = $"Баланс: {economyManager.CurrentBalance:F0} TR";
    }

    private void UpdateStatusText()
    {
        if (statusText != null && timeManager != null)
        {
            string status = "";
            if (timeManager.IsPeakHoursActive)
                status += "Пиковые часы\n";
            if (timeManager.IsAlcoholRestricted)
                status += "Запрет на алкоголь\n";
            if (timeManager.IsNoTobaccoDay())
                status += "День без табака\n";
            else if (timeManager.IsPensionerDiscountDay())
                status += "Скидки для пенсионеров\n";
            else if (timeManager.IsYouthDay())
                status += "Молодежный день\n";
            else if (timeManager.IsFamilyDay())
                status += "Семейный день (+50% доход)\n";
            statusText.text = status.Trim();
        }
    }
}