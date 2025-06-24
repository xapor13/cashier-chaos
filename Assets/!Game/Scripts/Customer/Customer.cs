using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;

// Компонент, управляющий поведением клиента в магазине
[RequireComponent(typeof(NavMeshAgent), typeof(CapsuleCollider))]
public class Customer : MonoBehaviour
{
    [SerializeField] private CustomerData customerData; // Данные о типах клиентов
    private int customerTypeIndex; // Индекс типа клиента

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
    private int itemCount = 1; // Количество товаров (временное)
    private Transform targetDestination; // Цель движения (касса или выход)
    private bool hasPlayedScanSound; // Флаг для звука сканирования

    // Свойства для доступа к состоянию клиента
    public string CustomerType => customerData.customerTypes[customerTypeIndex].name; // Тип клиента
    public bool NeedsHelp => needsHelp; // Нуждается ли клиент в помощи
    public bool IsServed => isServed; // Обслужен ли клиент

    // Инициализация компонентов
    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>(); // Получение NavMeshAgent
        capsuleCollider = GetComponent<CapsuleCollider>(); // Получение CapsuleCollider

        // Проверка наличия коллайдера
        if (capsuleCollider == null)
        {
            Debug.LogError($"Клиент {gameObject.name} не имеет CapsuleCollider!");
        }
    }

    // Инициализация клиента
    public void Initialize(Transform cashRegisterTransform, int typeIndex)
    {
        customerTypeIndex = typeIndex; // Установка индекса типа клиента
        var typeData = customerData.customerTypes[customerTypeIndex]; // Данные о типа клиента
        agent.speed = typeData.moveSpeed; // Установка скорости
        remainingPatience = typeData.patienceTime; // Установка терпения
        scanTimeRemaining = typeData.scanTimePerItem * itemCount; // Время сканирования
        needsHelp = typeData.requiresHelpFrequently && Random.value < 0.5f; // 50% шанс для Пожилых
        isServed = false; // Клиент не обслужен
        targetDestination = cashRegisterTransform; // Установка цели
        hasPlayedScanSound = false; // Сброс флага звука

        // Проверка и активация NavMeshAgent
        if (!agent.isActiveAndEnabled)
        {
            agent.enabled = true;
        }

        // Настройка коллайдера
        if (capsuleCollider != null)
        {
            capsuleCollider.radius = agent.radius;
            capsuleCollider.height = agent.height;
            capsuleCollider.center = new Vector3(0, agent.height / 2, 0);
            capsuleCollider.isTrigger = false;
        }

        // Размещение на NavMesh
        NavMeshHit navHit;
        if (NavMesh.SamplePosition(transform.position, out navHit, 5f, NavMesh.AllAreas))
        {
            agent.Warp(navHit.position);
        }
        else
        {
            Debug.LogWarning($"Не удалось разместить клиента {CustomerType} на NavMesh в точке {transform.position}!");
            return;
        }

        // Установка движения к кассе
        if (agent.isOnNavMesh)
        {
            agent.SetDestination(targetDestination.position);
        }
        else
        {
            Debug.LogWarning($"Клиент {CustomerType} не на NavMesh после Warp!");
        }

        // Уведомление о необходимости помощи
        if (needsHelp)
            onCustomerNeedsHelp.Invoke();
    }

    // Обновление состояния клиента
    private void Update()
    {
        if (isServed || needsHelp) return;

        // Начало сканирования у кассы
        if (agent.isActiveAndEnabled && agent.isOnNavMesh && !agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
        {
            remainingPatience -= Time.deltaTime;
            scanTimeRemaining -= Time.deltaTime;

            // Воспроизведение звука сканирования один раз
            if (!hasPlayedScanSound && AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySFX("ScanItem");
                hasPlayedScanSound = true;
            }

            if (scanTimeRemaining <= 0)
            {
                isServed = true;
                onCustomerServed.Invoke();
                MoveToExit();
            }
            else if (remainingPatience <= 0)
            {
                onCustomerLeft.Invoke();
                MoveToExit();
            }
        }
    }

    // Оказание помощи клиенту
    public void ProvideHelp()
    {
        if (needsHelp)
        {
            needsHelp = false;
            scanTimeRemaining -= 2f;

            // Воспроизведение звука помощи
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySFX("HelpCustomer");
            }
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

        // Воспроизведение звука кика
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX("Kick");
        }

        onCustomerLeft.Invoke(); // Уведомление об уходе
        MoveToExit(); // Перемещение к выходу
    }

    // Попытка покупки запрещенных товаров
    public bool TryBuyRestrictedItem(GameTimeManager timeManager, EconomyManager economyManager)
    {
        var typeData = customerData.customerTypes[customerTypeIndex];
        if (!typeData.triesRestrictedItems) return false;

        if (customerTypeIndex == 1 && Random.value < 0.5f) // Подросток
        {
            if (timeManager.IsAlcoholRestricted || timeManager.IsNoTobaccoDay())
            {
                economyManager.ApplyFine(customerData.customerTypes[customerTypeIndex].kickFine);

                // Воспроизведение звука штрафа
                if (AudioManager.Instance != null)
                {
                    AudioManager.Instance.PlaySFX("Fine");
                }
                return true;
            }
        }
        return false;
    }

    // Перемещение к выходу
    public void MoveToExit()
    {
        if (targetDestination != null && agent.isActiveAndEnabled && agent.isOnNavMesh)
        {
            agent.SetDestination(targetDestination.position);
        }
        else
        {
            Debug.LogWarning($"Невозможно переместить клиента {CustomerType} к выходу!");
        }
    }

    // Установка цели выхода
    public void SetExitDestination(Transform exitTransform)
    {
        targetDestination = exitTransform;
    }
}