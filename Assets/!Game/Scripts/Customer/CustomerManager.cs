using UnityEngine;
using UnityEngine.AI;

public class CustomerManager : MonoBehaviour
{
    [SerializeField] private GameTimeManager timeManager; // Менеджер игрового времени
    [SerializeField] private EconomyManager economyManager; // Менеджер экономики
    [SerializeField] private CashRegisterManager cashRegisterManager; // Менеджер касс
    [SerializeField] private CustomerData customerData; // Данные о клиентах
    [SerializeField] private Transform customerParent; // Родительский объект для клиентов
    [SerializeField] private Transform entranceTransform; // Точка спавна клиентов
    [SerializeField] private Transform exitTransform; // Точка выхода клиентов

    private float spawnTimer; // Таймер для спавна клиентов
    private float baseSpawnInterval = 30f; // Базовый интервал спавна (сек)

    // Подписка на события пиковых часов
    private void OnEnable()
    {
        timeManager.onPeakHoursStarted.AddListener(() => baseSpawnInterval *= 0.5f); // Удвоение частоты
        timeManager.onPeakHoursEnded.AddListener(() => baseSpawnInterval *= 2f); // Возврат к норме
    }

    // Отписка от событий
    private void OnDisable()
    {
        timeManager.onPeakHoursStarted.RemoveListener(() => baseSpawnInterval *= 0.5f);
        timeManager.onPeakHoursEnded.RemoveListener(() => baseSpawnInterval *= 2f);
    }

    // Обновление таймера спавна
    private void Update()
    {
        spawnTimer -= Time.deltaTime;
        if (spawnTimer <= 0)
        {
            SpawnCustomer();
            spawnTimer = baseSpawnInterval;
        }
    }

    // Спавн нового клиента
    private void SpawnCustomer()
    {
        // Поиск свободной кассы
        CashRegister availableRegister = cashRegisterManager.GetAvailableRegister();
        if (availableRegister == null) return;

        // Определение типа клиента в зависимости от дня
        int typeIndex = Random.Range(0, customerData.customerTypes.Length);
        if (timeManager.IsPensionerDiscountDay())
            typeIndex = Random.value < 0.7f ? 0 : Random.Range(0, customerData.customerTypes.Length); // Больше пожилых
        else if (timeManager.IsYouthDay())
            typeIndex = Random.value < 0.7f ? 1 : Random.Range(0, customerData.customerTypes.Length); // Больше подростков
        else if (timeManager.IsFamilyDay())
            typeIndex = Random.Range(0, customerData.customerTypes.Length); // Смешанный состав

        // Получение префаба клиента
        GameObject customerPrefab = customerData.customerTypes[typeIndex].prefab;
        if (customerPrefab == null)
        {
            Debug.LogWarning($"Префаб для типа клиента {customerData.customerTypes[typeIndex].name} не назначен!");
            return;
        }

        // Проверка наличия необходимых компонентов в префабе
        if (!customerPrefab.GetComponent<Customer>() || !customerPrefab.GetComponent<NavMeshAgent>() || !customerPrefab.GetComponent<CapsuleCollider>())
        {
            Debug.LogError($"Префаб {customerPrefab.name} не имеет всех необходимых компонентов (Customer, NavMeshAgent, CapsuleCollider)!");
            return;
        }

        // Поиск ближайшей точки на NavMesh для спавна
        NavMeshHit navHit;
        Vector3 spawnPosition = entranceTransform.position;
        if (NavMesh.SamplePosition(entranceTransform.position, out navHit, 5f, NavMesh.AllAreas))
        {
            spawnPosition = navHit.position; // Используем ближайшую точку на NavMesh
        }
        else
        {
            Debug.LogWarning($"Не удалось найти точку на NavMesh для спавна клиента около {entranceTransform.position}!");
            return; // Пропускаем спавн
        }

        // Спавн клиента
        GameObject customerObj = Instantiate(customerPrefab, spawnPosition, Quaternion.identity, customerParent);
        Customer customer = customerObj.GetComponent<Customer>();
        customer.Initialize(availableRegister.transform, typeIndex); // Инициализация клиента
        customer.SetExitDestination(exitTransform); // Установка точки выхода
        availableRegister.AssignCustomer(customer); // Назначение клиента кассе

        // Проверка покупки запрещенных товаров
        customer.TryBuyRestrictedItem(timeManager, economyManager);
    }
}