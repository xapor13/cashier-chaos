using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;

// Компонент, управляющий поведением клиента в магазине
[RequireComponent(typeof(NavMeshAgent), typeof(CapsuleCollider))] // Требуется CapsuleCollider
public class Customer : MonoBehaviour
{
    [SerializeField] private CustomerData customerData; // Данные о типах клиентов (ScriptableObject)
    private int customerTypeIndex; // Индекс типа клиента (устанавливается через Initialize)

    // События для уведомления о действиях клиента
    public UnityEvent onCustomerNeedsHelp = new UnityEvent(); // Клиент нуждается в помощи
    public UnityEvent onCustomerServed = new UnityEvent(); // Клиент обслужен
    public UnityEvent onCustomerLeft = new UnityEvent(); // Клиент покинул магазин

    private NavMeshAgent agent; // Компонент для навигации по NavMesh
    private CapsuleCollider capsuleCollider; // Коллайдер клиента
    private float remainingPatience; // Оставшееся время терпения клиента
    private float scanTimeRemaining; // Оставшееся время сканирования товаров
    private bool needsHelp; // Требуется ли клиенту помощь
    private bool isServed; // Обслужен ли клиент
    private int itemCount = 1; // Количество товаров (временное значение, позже рандомизировать)
    private Transform targetDestination; // Цель движения (касса или выход)

    // Свойства для доступа к состоянию клиента
    public string CustomerType => customerData.customerTypes[customerTypeIndex].name; // Тип клиента
    public bool NeedsHelp => needsHelp; // Нуждается ли клиент в помощи
    public bool IsServed => isServed; // Обслужен ли клиент

    // Инициализация компонентов при создании объекта
    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>(); // Получение NavMeshAgent
        capsuleCollider = GetComponent<CapsuleCollider>(); // Получение CapsuleCollider

        // Проверка наличия коллайдера
        if (capsuleCollider == null)
        {
            Debug.LogError($"Клиент {gameObject.name} не имеет CapsuleCollider! Добавьте компонент.");
        }
    }

    // Инициализация клиента с указанием кассы и типа
    public void Initialize(Transform cashRegisterTransform, int typeIndex)
    {
        customerTypeIndex = typeIndex; // Установка индекса типа клиента
        var typeData = customerData.customerTypes[customerTypeIndex]; // Данные о типе клиента
        agent.speed = typeData.moveSpeed; // Установка скорости движения
        remainingPatience = typeData.patienceTime; // Установка времени терпения
        scanTimeRemaining = typeData.scanTimePerItem * itemCount; // Время сканирования товаров
        needsHelp = typeData.requiresHelpFrequently && Random.value < 0.5f; // 50% шанс для Пожилых
        isServed = false; // Клиент еще не обслужен
        targetDestination = cashRegisterTransform; // Установка цели (касса)

        // Проверка и активация NavMeshAgent
        if (!agent.isActiveAndEnabled)
        {
            agent.enabled = true; // Включение агента
        }

        // Настройка коллайдера
        if (capsuleCollider != null)
        {
            capsuleCollider.radius = agent.radius; // Синхронизация с NavMeshAgent
            capsuleCollider.height = agent.height; // Синхронизация с NavMeshAgent
            capsuleCollider.center = new Vector3(0, agent.height / 2, 0); // Центрирование
            capsuleCollider.isTrigger = false; // Физический коллайдер для избегания
        }

        // Размещение агента на NavMesh
        NavMeshHit navHit;
        if (NavMesh.SamplePosition(transform.position, out navHit, 5f, NavMesh.AllAreas))
        {
            agent.Warp(navHit.position); // Телепортация на ближайшую точку NavMesh
        }
        else
        {
            Debug.LogWarning($"Не удалось разместить клиента {CustomerType} на NavMesh в точке {transform.position}!");
            return;
        }

        // Установка движения к кассе
        if (agent.isOnNavMesh)
        {
            agent.SetDestination(targetDestination.position); // Установка цели движения
        }
        else
        {
            Debug.LogWarning($"Клиент {CustomerType} не находится на NavMesh после Warp в точке {transform.position}!");
        }

        // Уведомление о необходимости помощи
        if (needsHelp)
            onCustomerNeedsHelp.Invoke();
    }

    // Обновление состояния клиента каждый кадр
    private void Update()
    {
        if (isServed || needsHelp) return; // Пропуск, если клиент обслужен или ждет помощи

        // Начало сканирования товаров, если клиент достиг кассы
        if (agent.isActiveAndEnabled && agent.isOnNavMesh && !agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
        {
            remainingPatience -= Time.deltaTime; // Уменьшение терпения
            scanTimeRemaining -= Time.deltaTime; // Уменьшение времени сканирования

            if (scanTimeRemaining <= 0) // Завершение сканирования
            {
                isServed = true;
                onCustomerServed.Invoke(); // Уведомление об обслуживании
                MoveToExit(); // Перемещение к выходу
            }
            else if (remainingPatience <= 0) // Терпение закончилось
            {
                onCustomerLeft.Invoke(); // Уведомление об уходе
                MoveToExit(); // Перемещение к выходу
            }
        }
    }

    // Оказание помощи клиенту
    public void ProvideHelp()
    {
        if (needsHelp)
        {
            needsHelp = false; // Сброс флага помощи
            scanTimeRemaining -= 2f; // Ускорение сканирования на 2 секунды
        }
    }

    // Изгнание клиента с возможным штрафом
    public void KickCustomer(EconomyManager economyManager)
    {
        var typeData = customerData.customerTypes[customerTypeIndex]; // Данные о типе клиента
        if (Random.value < typeData.kickFineProbability) // Проверка вероятности штрафа
        {
            economyManager.ApplyFine(typeData.kickFine); // Начисление штрафа
        }
        onCustomerLeft.Invoke(); // Уведомление об уходе
        MoveToExit(); // Перемещение к выходу
    }

    // Попытка покупки запрещенных товаров
    public bool TryBuyRestrictedItem(GameTimeManager timeManager, EconomyManager economyManager)
    {
        var typeData = customerData.customerTypes[customerTypeIndex]; // Данные о типе клиента
        if (!typeData.triesRestrictedItems) return false; // Пропуск, если клиент не покупает запрещенные товары

        if (customerTypeIndex == 1 && Random.value < 0.5f) // Подросток с 50% шансом
        {
            if (timeManager.IsAlcoholRestricted || timeManager.IsNoTobaccoDay()) // Проверка ограничений
            {
                economyManager.ApplyFine(customerData.customerTypes[customerTypeIndex].kickFine); // Штраф
                return true; // Попытка зафиксирована
            }
        }
        return false; // Нет нарушения
    }

    // Перемещение клиента к выходу
    private void MoveToExit()
    {
        // Установка цели движения к выходу
        if (targetDestination != null && agent.isActiveAndEnabled && agent.isOnNavMesh)
        {
            agent.SetDestination(targetDestination.position); // Установка цели
        }
        else
        {
            Debug.LogWarning($"Невозможно переместить клиента {CustomerType} к выходу: агент не на NavMesh или отключен!");
        }
    }

    // Установка цели выхода
    public void SetExitDestination(Transform exitTransform)
    {
        targetDestination = exitTransform; // Сохранение трансформа выхода
    }
}