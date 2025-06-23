using UnityEngine;

[CreateAssetMenu(fileName = "CustomerData", menuName = "Game/CustomerData", order=3)]
public class CustomerData : ScriptableObject
{
    [System.Serializable]
    public struct CustomerTypeData
    {
        public string name; // Elderly, Teen, Normal, Aggressive, VIP
        public float scanTimePerItem; // Seconds per item
        public float patienceTime; // Seconds
        public float kickFine; // TR
        public float kickFineProbability; // 0-1
        public bool requiresHelpFrequently; // For Elderly
        public bool triesRestrictedItems; // For Teen
        public bool requiresSpecialService; // For VIP
        public float moveSpeed; // NavMeshAgent speed
        public GameObject prefab; // Prefab for this customer type
    }

    public CustomerTypeData[] customerTypes;
}