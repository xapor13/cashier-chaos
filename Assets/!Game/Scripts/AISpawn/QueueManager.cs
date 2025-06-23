using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QueueManager : MonoBehaviour
{
    public Dictionary<CashRegister, Queue<Customer>> registerQueues;

    public void AddToQueue(CashRegister register, Customer customer)
    public Customer GetNextInQueue(CashRegister register)
    public int GetQueueLength(CashRegister register)
    public CashRegister GetShortestQueue()
}
