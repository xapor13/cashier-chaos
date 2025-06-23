using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class EconomyUIDisplay : MonoBehaviour
{
    [SerializeField] private EconomyManager economyManager;
    [SerializeField] private CustomerManager customerManager;
    [SerializeField] private CashRegisterManager cashRegisterManager;
    [SerializeField] private TMP_Text incomeText;
    [SerializeField] private TMP_Text expensesText;
    [SerializeField] private TMP_Text cashRegistersText;
    [SerializeField] private Button buyBasicRegisterButton;
    [SerializeField] private Button buyImprovedRegisterButton;
    [SerializeField] private Button buyPremiumRegisterButton;
    [SerializeField] private Button hireMechanicButton;
    [SerializeField] private Button hireAssistantButton;
    [SerializeField] private Button hireGuardButton;
    [SerializeField] private TMP_Text notificationText;

    private float dailyIncome;
    private float dailyExpenses;

    private void OnEnable()
    {
        economyManager.onBalanceChanged.AddListener(UpdateUI);
        if (buyBasicRegisterButton) buyBasicRegisterButton.onClick.AddListener(() => BuyCashRegister(0));
        if (buyImprovedRegisterButton) buyImprovedRegisterButton.onClick.AddListener(() => BuyCashRegister(1));
        if (buyPremiumRegisterButton) buyPremiumRegisterButton.onClick.AddListener(() => BuyCashRegister(2));
        if (hireMechanicButton) hireMechanicButton.onClick.AddListener(() => ToggleHire("Mechanic"));
        if (hireAssistantButton) hireAssistantButton.onClick.AddListener(() => ToggleHire("Assistant"));
        if (hireGuardButton) hireGuardButton.onClick.AddListener(() => ToggleHire("Guard"));
    }

    private void OnDisable()
    {
        economyManager.onBalanceChanged.RemoveListener(UpdateUI);
        if (buyBasicRegisterButton) buyBasicRegisterButton.onClick.RemoveAllListeners();
        if (buyImprovedRegisterButton) buyImprovedRegisterButton.onClick.RemoveAllListeners();
        if (buyPremiumRegisterButton) buyPremiumRegisterButton.onClick.RemoveAllListeners();
        if (hireMechanicButton) hireMechanicButton.onClick.RemoveAllListeners();
        if (hireAssistantButton) hireAssistantButton.onClick.RemoveAllListeners();
        if (hireGuardButton) hireGuardButton.onClick.RemoveAllListeners();
    }

    private void Update()
    {
        UpdateUI();
    }

    private void UpdateUI()
    {
        if (incomeText) incomeText.text = $"Доход: {dailyIncome:F0} TR/день";
        if (expensesText) expensesText.text = $"Расходы: {dailyExpenses:F0} TR/день";
        if (cashRegistersText)
        {
            cashRegistersText.text = "Кассы:\n";
            foreach (var register in cashRegisterManager.GetComponentsInChildren<CashRegister>())
            {
                Customer customer = register.CurrentCustomer;
                cashRegistersText.text += $"Касса ({register.CurrentState}): {(customer ? customer.CustomerType : "Свободна")}\n";
            }
        }
        UpdateButtonStates();
    }

    private void UpdateButtonStates()
    {
        if (buyBasicRegisterButton) buyBasicRegisterButton.interactable = economyManager.CurrentBalance >= 8000f;
        if (buyImprovedRegisterButton) buyImprovedRegisterButton.interactable = economyManager.CurrentBalance >= 15000f;
        if (buyPremiumRegisterButton) buyPremiumRegisterButton.interactable = economyManager.CurrentBalance >= 25000f;
        if (hireMechanicButton) hireMechanicButton.GetComponentInChildren<TMP_Text>().text = economyManager.HasMechanic ? "Уволить механика" : "Нанять механика (3000 TR)";
        if (hireAssistantButton) hireAssistantButton.GetComponentInChildren<TMP_Text>().text = economyManager.HasAssistant ? "Уволить помощника" : "Нанять помощника (2000 TR)";
        if (hireGuardButton) hireGuardButton.GetComponentInChildren<TMP_Text>().text = economyManager.HasGuard ? "Уволить охранника" : "Нанять охранника (2500 TR)";
    }

    private void BuyCashRegister(int type)
    {
        if (economyManager.PurchaseCashRegister(type))
        {
            ShowNotification($"Куплена касса типа {type}!");
        }
        else
        {
            ShowNotification("Недостаточно средств!");
        }
    }

    private void ToggleHire(string staffType)
    {
        bool hire = staffType switch
        {
            "Mechanic" => !economyManager.HasMechanic,
            "Assistant" => !economyManager.HasAssistant,
            "Guard" => !economyManager.HasGuard,
            _ => false
        };
        economyManager.HireStaff(staffType, hire);
        ShowNotification(hire ? $"Нанят {staffType}!" : $"Уволен {staffType}!");
    }

    public void ShowNotification(string message)
    {
        if (notificationText)
        {
            notificationText.text = message;
            // Placeholder: Add fade-out animation later
        }
    }

    public void HelpCustomer(Customer customer)
    {
        customer.ProvideHelp();
        ShowNotification($"Помощь оказана клиенту ({customer.CustomerType})!");
    }

    public void KickCustomer(Customer customer)
    {
        customer.KickCustomer(economyManager);
        ShowNotification($"Клиент ({customer.CustomerType}) изгнан!");
    }
}