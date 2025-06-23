using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameCore;

public class TeenagerCustomer : Customer
{
    public bool tryingToBuyAlcohol = false;

    void Start()
    {
        customerType = CustomerType.Teenager;
        serviceSpeed = 12f;
        patience = 90f; // 1.5 минуты
        tryingToBuyAlcohol = Random.Range(0f, 1f) < 0.3f;
    }

    public override void ReactToWaiting()
    {
        // Нервничает, постукивает ногой
    }

    public override float GetKickFineRisk()
    {
        return 0.5f; // 50% риск штрафа 1500₽
    }
}

