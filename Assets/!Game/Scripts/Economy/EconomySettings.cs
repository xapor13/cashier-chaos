using UnityEngine;

[CreateAssetMenu(fileName = "EconomySettings", menuName = "Game/EconomySettings", order=2)]
public class EconomySettings : ScriptableObject
{
    [Header("Initial Balance")]
    public float initialBalance = 50000f;
    public float targetBalance = 500000f;
    public int criticalDebtDaysLimit = 3;

    [Header("Cash Register Income (TR/min)")]
    public float basicCashRegisterRate = 15f;
    public float improvedCashRegisterRate = 25f;
    public float premiumCashRegisterRate = 40f;

    [Header("Bonuses")]
    public float efficiencyBonusPercent = 0.2f; // 20%
    public float weekendBonusPercent = 0.5f; // 50%

    [Header("Daily Expenses (TR/day)")]
    public float rentCost = 5000f;
    public float electricityCost = 1000f;
    public float collectionCost = 500f;
    public float cashRegisterMaintenanceCost = 200f; // Per register

    [Header("Fines (TR)")]
    public float underageAlcoholFine = 10000f;
    public float lateAlcoholFine = 5000f;
    public float brokenCashRegisterFine = 1000f; // Every 2 minutes
    public float massCustomerLossFine = 2000f;
    public float[] kickFines = { 2000f, 1500f, 800f, 500f, 5000f }; // Elderly, Teen, Normal, Aggressive, VIP
    public float[] kickFineProbabilities = { 0.7f, 0.5f, 0.2f, 0.1f, 0.8f }; // Probabilities

    [Header("Purchases (TR)")]
    public float basicCashRegisterCost = 8000f;
    public float improvedCashRegisterCost = 15000f;
    public float premiumCashRegisterCost = 25000f;
    public float mechanicCost = 3000f; // Per day
    public float assistantCost = 2000f; // Per day
    public float guardCost = 2500f; // Per day
}