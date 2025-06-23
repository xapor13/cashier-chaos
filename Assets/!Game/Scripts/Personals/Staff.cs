using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Staff : MonoBehaviour
{
    public string staffName;
    public float dailyCost;
    public bool isHired = false;
    public float efficiency = 1f;

    public abstract void PerformDuties();
    public virtual void HireStaff()
    {

    }
    public virtual void FireStaff()
    {

    }
}

