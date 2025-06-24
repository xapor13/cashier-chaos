using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class CashRegisterUI : MonoBehaviour
{
    [SerializeField] private CashRegister cashRegister; // Ссылка на кассу
    [SerializeField] private EconomyManager economyManager; // Менеджер экономики
    [SerializeField] private EconomyUIDisplay economyUIDisplay; // UI экономики
    [SerializeField] private TMP_Text statusText; // Текст состояния кассы
    [SerializeField] private Button helpButton; // Кнопка помощи
    [SerializeField] private Button repairButton; // Кнопка ремонта
    [SerializeField] private Button rebootButton; // Кнопка перезагрузки
    [SerializeField] private Button kickRegisterButton; // Кнопка обработки кассы
    [SerializeField] private Button toggleOffButton; // Кнопка включения/выключения
    [SerializeField] private Button selectButton; // Кнопка выбора кассы

    private Customer currentCustomer; // Текущий клиент у кассы

    // Подписка на события кнопок
    private void OnEnable()
    {
        if (helpButton) helpButton.onClick.AddListener(OnHelpClicked);
        if (repairButton) repairButton.onClick.AddListener(OnRepairClicked);
        if (rebootButton) rebootButton.onClick.AddListener(OnRebootClicked);
        if (kickRegisterButton) kickRegisterButton.onClick.AddListener(OnKickRegisterClicked);
        if (toggleOffButton) toggleOffButton.onClick.AddListener(OnToggleOffClicked);
        cashRegister.onStateChanged.AddListener(UpdateUI);
    }

    // Отписка от событий
    private void OnDisable()
    {
        if (helpButton) helpButton.onClick.RemoveAllListeners();
        if (repairButton) repairButton.onClick.RemoveAllListeners();
        if (rebootButton) rebootButton.onClick.RemoveAllListeners();
        if (kickRegisterButton) kickRegisterButton.onClick.RemoveAllListeners();
        if (toggleOffButton) toggleOffButton.onClick.RemoveAllListeners();
        if (selectButton) selectButton.onClick.RemoveAllListeners();
        cashRegister.onStateChanged.RemoveListener(UpdateUI);
    }

    // Обновление UI каждый кадр
    private void Update()
    {
        UpdateUI();
    }

    // Обновление состояния UI
    private void UpdateUI()
    {
        currentCustomer = cashRegister.CurrentCustomer;
        if (statusText)
            statusText.text = $"Состояние: {cashRegister.CurrentState}, Клиент: {(currentCustomer ? currentCustomer.CustomerType : "Нет")}";

        if (helpButton) helpButton.interactable = currentCustomer && currentCustomer.NeedsHelp;
        if (repairButton) repairButton.interactable = cashRegister.CurrentState == CashRegister.RegisterState.Broken;
        if (rebootButton) rebootButton.interactable = cashRegister.CurrentState == CashRegister.RegisterState.NeedsAttention || cashRegister.CurrentState == CashRegister.RegisterState.Broken;
        if (kickRegisterButton) kickRegisterButton.interactable = cashRegister.CurrentState == CashRegister.RegisterState.NeedsAttention || cashRegister.CurrentState == CashRegister.RegisterState.Broken;
        if (toggleOffButton) toggleOffButton.interactable = true;
        if (toggleOffButton) toggleOffButton.GetComponentInChildren<TMP_Text>().text = cashRegister.CurrentState == CashRegister.RegisterState.Off ? "Включить" : "Выключить";
    }

    // Обработка нажатия кнопки помощи
    private void OnHelpClicked()
    {
        if (currentCustomer)
            economyUIDisplay.HelpCustomer(currentCustomer);
    }

    // Обработка нажатия кнопки ремонта
    private void OnRepairClicked()
    {
        cashRegister.Repair(economyManager.HasMechanic);
        economyUIDisplay.ShowNotification("Начался ремонт кассы!");
    }

    // Обработка нажатия кнопки перезагрузки
    private void OnRebootClicked()
    {
        cashRegister.Reboot();
        economyUIDisplay.ShowNotification("Началась перезагрузка кассы!");
    }

    // Обработка нажатия кнопки обработки кассы
    private void OnKickRegisterClicked()
    {
        cashRegister.KickRegister();
        economyUIDisplay.ShowNotification("Касса обработана!");
    }

    // Обработка нажатия кнопки включения/выключения
    private void OnToggleOffClicked()
    {
        cashRegister.ToggleOff();
        economyUIDisplay.ShowNotification(cashRegister.CurrentState == CashRegister.RegisterState.Off ? "Касса выключена!" : "Касса включена!");
    }
}