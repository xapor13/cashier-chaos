using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;

[RequireComponent(typeof(NavMeshAgent))]
public class Customer : MonoBehaviour
{
    [SerializeField] private CustomerData customerData;
    private int customerTypeIndex; // Set via Initialize, not in Inspector

    public UnityEvent onCustomerNeedsHelp = new UnityEvent();
    public UnityEvent onCustomerServed = new UnityEvent();
    public UnityEvent onCustomerLeft = new UnityEvent();

    private NavMeshAgent agent;
    private float remainingPatience;
    private float scanTimeRemaining;
    private bool needsHelp;
    private bool isServed;
    private int itemCount = 1; // Placeholder: Randomize later
    private Transform targetDestination; // Cash register or exit

    public string CustomerType => customerData.customerTypes[customerTypeIndex].name;
    public bool NeedsHelp => needsHelp;
    public bool IsServed => isServed;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    public void Initialize(Transform cashRegisterTransform, int typeIndex)
    {
        customerTypeIndex = typeIndex;
        var typeData = customerData.customerTypes[customerTypeIndex];
        agent.speed = typeData.moveSpeed;
        remainingPatience = typeData.patienceTime;
        scanTimeRemaining = typeData.scanTimePerItem * itemCount;
        needsHelp = typeData.requiresHelpFrequently && Random.value < 0.5f; // 50% chance for Elderly
        isServed = false;
        targetDestination = cashRegisterTransform;

        // Move to cash register
        agent.SetDestination(targetDestination.position);

        if (needsHelp)
            onCustomerNeedsHelp.Invoke();
    }

    private void Update()
    {
        if (isServed || needsHelp) return;

        // Start scanning only when at cash register
        if (agent.remainingDistance <= agent.stoppingDistance && !agent.pathPending)
        {
            remainingPatience -= Time.deltaTime;
            scanTimeRemaining -= Time.deltaTime;

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

    public void ProvideHelp()
    {
        if (needsHelp)
        {
            needsHelp = false;
            scanTimeRemaining -= 2f; // Help takes 2-3 seconds
        }
    }

    public void KickCustomer(EconomyManager economyManager)
    {
        var typeData = customerData.customerTypes[customerTypeIndex];
        if (Random.value < typeData.kickFineProbability)
        {
            economyManager.ApplyFine(typeData.kickFine);
        }
        onCustomerLeft.Invoke();
        MoveToExit();
    }

    public bool TryBuyRestrictedItem(GameTimeManager timeManager, EconomyManager economyManager)
    {
        var typeData = customerData.customerTypes[customerTypeIndex];
        if (!typeData.triesRestrictedItems) return false;

        if (customerTypeIndex == 1 && Random.value < 0.5f) // Teen trying alcohol/tobacco
        {
            if (timeManager.IsAlcoholRestricted || timeManager.IsNoTobaccoDay())
            {
                economyManager.ApplyFine(customerData.customerTypes[customerTypeIndex].kickFine);
                return true;
            }
        }
        return false;
    }

    private void MoveToExit()
    {
        // Set destination to exit (set via CustomerManager)
        if (targetDestination != null)
        {
            agent.SetDestination(targetDestination.position);
        }
    }

    public void SetExitDestination(Transform exitTransform)
    {
        targetDestination = exitTransform;
    }
}