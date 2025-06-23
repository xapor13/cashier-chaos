using UnityEngine;
using GameCore;

public class FineSystem : MonoBehaviour
{
    private const float ProvocationMultiplier = 1.5f; // +50% при провокации
    private const float CameraMultiplier = 2f; // x2 при наличии камер
    private const float SecurityMultiplier = 1.3f; // +30% при охране
    private const float NightReduction = 0.8f; // -20% ночью
    private const float NightTimeThreshold = 20f; // Ночь начинается в 20:00

    [Header("Зависимости")]
    [SerializeField] private EconomyManager economyManager;
    [SerializeField] private NotificationSystem notificationSystem;
    [SerializeField] private TimeManager timeManager;
    [SerializeField] private CashRegisterManager cashRegisterManager;

    [Header("Модификаторы штрафов")]
    [SerializeField] private bool camerasInstalled = true; // Наличие камер
    [SerializeField] private bool securityPresent = false; // Наличие охраны

    public void ApplyFine(FineType fineType, float multiplier = 1f, bool isProvoked = false)
    {
        float baseFine = (float)fineType;
        float finalFine = CalculateFineWithModifiers(baseFine, isProvoked) * multiplier;

        economyManager?.ApplyFine(finalFine);
        notificationSystem?.ShowFineNotification(finalFine);

        Debug.Log($"Применен штраф {fineType}: {finalFine}₽");
    }

    private float CalculateFineWithModifiers(float baseFine, bool isProvoked)
    {
        float finalFine = baseFine;

        if (isProvoked)
        {
            finalFine *= ProvocationMultiplier;
        }

        if (camerasInstalled)
        {
            finalFine *= CameraMultiplier;
        }

        if (securityPresent)
        {
            finalFine *= SecurityMultiplier;
        }

        if (IsNightTime())
        {
            finalFine *= NightReduction;
        }

        return finalFine;
    }

    private bool IsNightTime()
    {
        return timeManager != null && timeManager.GetCurrentTime() >= NightTimeThreshold;
    }

    public void ApplyKickFine(Customer customer, bool isProvoked = false)
    {
        if (customer == null)
        {
            return;
        }

        float fineChance = customer.GetKickFineRisk();
        if (Random.Range(0f, 1f) <= fineChance)
        {
            FineType fineType = GetKickFineType(customer.customerType);
            ApplyFine(fineType, 1f, isProvoked);
        }
    }

    private FineType GetKickFineType(CustomerType customerType)
    {
        return customerType switch
        {
            CustomerType.Elderly => FineType.KickElderly,
            CustomerType.VIP => FineType.KickVIP,
            _ => FineType.KickElderly
        };
    }

    public void CheckBrokenRegisterFines()
    {
        if (cashRegisterManager == null)
        {
            return;
        }

        int brokenRegisters = cashRegisterManager.GetBrokenRegistersCount();
        if (brokenRegisters > 0)
        {
            float totalFine = brokenRegisters * (float)FineType.IgnoreBrokenRegister;
            ApplyFine(FineType.IgnoreBrokenRegister, brokenRegisters);
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

    // Геттеры для доступа к состоянию
    public bool HasCamerasInstalled() => camerasInstalled;
    public bool HasSecurityPresent() => securityPresent;
}