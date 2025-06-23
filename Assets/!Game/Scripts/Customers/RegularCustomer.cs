using UnityEngine;
using GameCore;

public class RegularCustomer : Customer
{
    void Start()
    {
        customerType = CustomerType.Regular;
        serviceSpeed = 10f;
        patience = 150f; // 2.5 минуты
    }

    public override void ReactToWaiting()
    {
        // Спокойное ожидание
    }

    public override float GetKickFineRisk()
    {
        return 0.2f; // 20% риск штрафа 800₽
    }
}
