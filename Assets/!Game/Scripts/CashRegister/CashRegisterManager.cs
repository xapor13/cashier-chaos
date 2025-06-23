using UnityEngine;

public class CashRegisterManager : MonoBehaviour
{
    [SerializeField] private GameTimeManager timeManager;
    [SerializeField] private EconomyManager economyManager;
    [SerializeField] private CashRegister[] cashRegisters;

    private float brokenFineTimer = 120f; // 2 minutes

    private void Update()
    {
        brokenFineTimer -= Time.deltaTime;
        if (brokenFineTimer <= 0)
        {
            ApplyBrokenFines();
            brokenFineTimer = 120f;
        }
    }

    private void ApplyBrokenFines()
    {
        foreach (var register in cashRegisters)
        {
            if (register.CurrentState == CashRegister.RegisterState.Broken)
            {
                economyManager.ApplyFine(economyManager.GetComponent<EconomySettings>().brokenCashRegisterFine);
            }
        }
    }

    public CashRegister GetAvailableRegister()
    {
        foreach (var register in cashRegisters)
        {
            if (register.IsOperational && register.CurrentCustomer == null)
                return register;
        }
        return null;
    }
}