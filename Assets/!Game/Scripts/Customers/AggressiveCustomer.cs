using UnityEngine;
using GameCore;

public class AggressiveCustomer : Customer
{
    public enum AggressionLevel { Calm, Grumbling, Yelling, Scandal }
    public AggressionLevel currentAggression = AggressionLevel.Calm;

    void Start()
    {
        customerType = CustomerType.Aggressive;
        serviceSpeed = 8f;
        patience = 45f; // 30-60 сек
    }

    public override void ReactToWaiting()
    {
        // Эскалация агрессии
        if (currentPatience < patience * 0.7f) currentAggression = AggressionLevel.Grumbling;
        if (currentPatience < patience * 0.4f) currentAggression = AggressionLevel.Yelling;
        if (currentPatience < patience * 0.1f) currentAggression = AggressionLevel.Scandal;
    }

    public override float GetKickFineRisk()
    {
        return 0.1f; // 10% риск штрафа 500₽
    }
}
