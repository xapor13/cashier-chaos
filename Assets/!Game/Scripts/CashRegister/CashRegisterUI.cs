using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class CashRegisterUI : MonoBehaviour
{
    [SerializeField] private CashRegister cashRegister;
    [SerializeField] private EconomyManager economyManager;
    [SerializeField] private EconomyUIDisplay economyUIDisplay;
    [SerializeField] private TMP_Text statusText;
    [SerializeField] private Button helpButton;
    [SerializeField] private Button kickButton;
    [SerializeField] private Button repairButton;
    [SerializeField] private Button rebootButton;
    [SerializeField] private Button kickRegisterButton;
    [SerializeField] private Button toggleOffButton;

    private Customer currentCustomer;

    private void OnEnable()
    {
        if (helpButton) helpButton.onClick.AddListener(OnHelpClicked);
        if (kickButton) kickButton.onClick.AddListener(OnKickClicked);
        if (repairButton) repairButton.onClick.AddListener(OnRepairClicked);
        if (rebootButton) rebootButton.onClick.AddListener(OnRebootClicked);
        if (kickRegisterButton) kickRegisterButton.onClick.AddListener(OnKickRegisterClicked);
        if (toggleOffButton) toggleOffButton.onClick.AddListener(OnToggleOffClicked);
        cashRegister.onStateChanged.AddListener(UpdateUI);
    }

    private void OnDisable()
    {
        if (helpButton) helpButton.onClick.RemoveAllListeners();
        if (kickButton) kickButton.onClick.RemoveAllListeners();
        if (repairButton) repairButton.onClick.RemoveAllListeners();
        if (rebootButton) rebootButton.onClick.RemoveAllListeners();
        if (kickRegisterButton) kickRegisterButton.onClick.RemoveAllListeners();
        if (toggleOffButton) toggleOffButton.onClick.RemoveAllListeners();
        cashRegister.onStateChanged.RemoveListener(UpdateUI);
    }

    private void Update()
    {
        UpdateUI();
    }

    private void UpdateUI()
    {
        currentCustomer = cashRegister.CurrentCustomer;
        if (statusText)
            statusText.text = $"Состояние: {cashRegister.CurrentState}, Клиент: {(currentCustomer ? currentCustomer.CustomerType : "Нет")}";

        if (helpButton) helpButton.interactable = currentCustomer && currentCustomer.NeedsHelp;
        if (kickButton) kickButton.interactable = currentCustomer && !currentCustomer.IsServed;
        if (repairButton) repairButton.interactable = cashRegister.CurrentState == CashRegister.RegisterState.Broken;
        if (rebootButton) rebootButton.interactable = cashRegister.CurrentState == CashRegister.RegisterState.NeedsAttention || cashRegister.CurrentState == CashRegister.RegisterState.Broken;
        if (kickRegisterButton) kickRegisterButton.interactable = cashRegister.CurrentState == CashRegister.RegisterState.NeedsAttention || cashRegister.CurrentState == CashRegister.RegisterState.Broken;
        if (toggleOffButton) toggleOffButton.interactable = true;
        if (toggleOffButton) toggleOffButton.GetComponentInChildren<TMP_Text>().text = cashRegister.CurrentState == CashRegister.RegisterState.Off ? "Включить" : "Выключить";
    }

    private void OnHelpClicked()
    {
        if (currentCustomer)
            economyUIDisplay.HelpCustomer(currentCustomer);
    }

    private void OnKickClicked()
    {
        if (currentCustomer)
            economyUIDisplay.KickCustomer(currentCustomer);
    }

    private void OnRepairClicked()
    {
        cashRegister.Repair(economyManager.HasMechanic);
        economyUIDisplay.ShowNotification("Начался ремонт кассы!");
    }

    private void OnRebootClicked()
    {
        cashRegister.Reboot();
        economyUIDisplay.ShowNotification("Началась перезагрузка кассы!");
    }

    private void OnKickRegisterClicked()
    {
        cashRegister.KickRegister();
        economyUIDisplay.ShowNotification("Касса обработана!");
    }

    private void OnToggleOffClicked()
    {
        cashRegister.ToggleOff();
        economyUIDisplay.ShowNotification(cashRegister.CurrentState == CashRegister.RegisterState.Off ? "Касса выключена!" : "Касса включена!");
    }
}