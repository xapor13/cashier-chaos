using UnityEngine;

public class CustomerManager : MonoBehaviour
{
    [SerializeField] private GameTimeManager timeManager;
    [SerializeField] private EconomyManager economyManager;
    [SerializeField] private CashRegisterManager cashRegisterManager;
    [SerializeField] private CustomerData customerData;
    [SerializeField] private Transform customerParent; // Parent for customer GameObjects
    [SerializeField] private Transform entranceTransform; // Spawn point
    [SerializeField] private Transform exitTransform; // Exit point

    private float spawnTimer;
    private float baseSpawnInterval = 30f; // Seconds between spawns

    private void OnEnable()
    {
        timeManager.onPeakHoursStarted.AddListener(() => baseSpawnInterval *= 0.5f); // Double spawn rate
        timeManager.onPeakHoursEnded.AddListener(() => baseSpawnInterval *= 2f);
    }

    private void OnDisable()
    {
        timeManager.onPeakHoursStarted.RemoveListener(() => baseSpawnInterval *= 0.5f);
        timeManager.onPeakHoursEnded.RemoveListener(() => baseSpawnInterval *= 2f);
    }

    private void Update()
    {
        spawnTimer -= Time.deltaTime;
        if (spawnTimer <= 0)
        {
            SpawnCustomer();
            spawnTimer = baseSpawnInterval;
        }
    }

    private void SpawnCustomer()
    {
        // Find available cash register
        CashRegister availableRegister = cashRegisterManager.GetAvailableRegister();
        if (availableRegister == null) return;

        // Determine customer type based on day
        int typeIndex = Random.Range(0, customerData.customerTypes.Length);
        if (timeManager.IsPensionerDiscountDay())
            typeIndex = Random.value < 0.7f ? 0 : Random.Range(0, customerData.customerTypes.Length); // More Elderly
        else if (timeManager.IsYouthDay())
            typeIndex = Random.value < 0.7f ? 1 : Random.Range(0, customerData.customerTypes.Length); // More Teens
        else if (timeManager.IsFamilyDay())
            typeIndex = Random.Range(0, customerData.customerTypes.Length); // Mixed

        // Get customer prefab from CustomerData
        GameObject customerPrefab = customerData.customerTypes[typeIndex].prefab;
        if (customerPrefab == null)
        {
            Debug.LogWarning($"Prefab for customer type {customerData.customerTypes[typeIndex].name} is not assigned!");
            return;
        }

        // Spawn customer at entrance
        GameObject customerObj = Instantiate(customerPrefab, entranceTransform.position, Quaternion.identity, customerParent);
        Customer customer = customerObj.GetComponent<Customer>();
        customer.Initialize(availableRegister.transform, typeIndex); // Pass typeIndex
        customer.SetExitDestination(exitTransform);
        availableRegister.AssignCustomer(customer);

        // Check for restricted item purchase
        customer.TryBuyRestrictedItem(timeManager, economyManager);
    }
}