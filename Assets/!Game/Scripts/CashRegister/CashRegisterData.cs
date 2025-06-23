using UnityEngine;

[CreateAssetMenu(fileName = "CashRegisterData", menuName = "Game/CashRegisterData", order=5)]
public class CashRegisterData : ScriptableObject
{
    [System.Serializable]
    public struct RegisterTypeData
    {
        public string name; // Basic, Improved, Premium
        public float incomeRate; // TR/min
        public float purchaseCost; // TR
        public float reliability; // 0-1 (chance of not breaking)
    }

    public RegisterTypeData[] registerTypes;
}