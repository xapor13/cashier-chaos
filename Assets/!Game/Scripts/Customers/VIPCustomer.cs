using UnityEngine;
using GameCore;

public class VIPCustomer : Customer
{
    public bool isMysteryShoper = false;

    void Start()
    {
        customerType = CustomerType.VIP;
        serviceSpeed = 10f;
        patience = 60f; // 1 минута
        isMysteryShoper = Random.Range(0f, 1f) < 0.2f;
    }

    public override void ReactToWaiting()
    {
        // Высокомерное поведение, угрозы жалоб
    }

    public override float GetKickFineRisk()
    {
        return 0.8f; // 80% риск штрафа 5000₽
    }
}

