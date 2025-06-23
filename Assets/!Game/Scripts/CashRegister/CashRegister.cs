using UnityEngine;
using UnityEngine.Events;

public class CashRegister : MonoBehaviour
{
    [SerializeField] private EconomySettings economySettings;
    [SerializeField] private CashRegisterData cashRegisterData;
    [SerializeField] private int registerType; // 0=Basic, 1=Improved, 2=Premium

    public enum RegisterState { Operational, NeedsAttention, Broken, Off }
    public UnityEvent onStateChanged = new UnityEvent();

    private RegisterState currentState = RegisterState.Operational;
    private float attentionTimer; // 60 seconds for NeedsAttention
    private float actionTimer; // For Repair, Reboot
    private Customer currentCustomer;

    public float IncomeRate => currentState == RegisterState.Operational ? registerType switch
    {
        1 => economySettings.improvedCashRegisterRate,
        2 => economySettings.premiumCashRegisterRate,
        _ => economySettings.basicCashRegisterRate
    } : 0f;

    public RegisterState CurrentState => currentState;
    public bool IsOperational => currentState == RegisterState.Operational;
    public Customer CurrentCustomer => currentCustomer;

    private void Start()
    {
        attentionTimer = 60f;
    }

    private void Update()
    {
        if (currentState == RegisterState.NeedsAttention)
        {
            attentionTimer -= Time.deltaTime;
            if (attentionTimer <= 0)
            {
                SetState(RegisterState.Broken);
            }
        }
        if (actionTimer > 0)
        {
            actionTimer -= Time.deltaTime;
            if (actionTimer <= 0)
            {
                CompleteAction();
            }
        }
    }

    public void SetState(RegisterState newState)
    {
        currentState = newState;
        if (newState == RegisterState.NeedsAttention)
            attentionTimer = 60f;
        onStateChanged.Invoke();
    }

    public void AssignCustomer(Customer customer)
    {
        currentCustomer = customer;
        if (customer.NeedsHelp || Random.value < (1f - cashRegisterData.registerTypes[registerType].reliability))
        {
            SetState(RegisterState.NeedsAttention);
        }
    }

    public void RemoveCustomer()
    {
        currentCustomer = null;
        if (currentState == RegisterState.NeedsAttention)
            SetState(RegisterState.Operational);
    }

    public void HelpCustomer()
    {
        if (currentState == RegisterState.NeedsAttention && currentCustomer != null)
        {
            currentCustomer.ProvideHelp();
            SetState(RegisterState.Operational);
        }
    }

    public void Repair(bool hasMechanic)
    {
        if (currentState == RegisterState.Broken)
        {
            actionTimer = hasMechanic ? 5f : Random.Range(10f, 15f); // 5s with mechanic, 10-15s without
        }
    }

    public void Reboot()
    {
        if (currentState == RegisterState.NeedsAttention || currentState == RegisterState.Broken)
        {
            actionTimer = 5f;
        }
    }

    public void KickRegister()
    {
        if (currentState == RegisterState.NeedsAttention || currentState == RegisterState.Broken)
        {
            if (Random.value < 0.85f) // 85% success
                SetState(RegisterState.Operational);
            else
                SetState(RegisterState.Broken);
        }
    }

    public void ToggleOff()
    {
        SetState(currentState == RegisterState.Off ? RegisterState.Operational : RegisterState.Off);
    }

    private void CompleteAction()
    {
        if (currentState == RegisterState.Broken)
        {
            SetState(RegisterState.Operational); // Repair complete
        }
        else if (currentState == RegisterState.NeedsAttention)
        {
            if (Random.value < 0.7f) // 70% reboot success
                SetState(RegisterState.Operational);
            else
                SetState(RegisterState.Broken);
        }
    }
}