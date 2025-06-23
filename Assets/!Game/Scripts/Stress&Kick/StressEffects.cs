using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StressEffects : MonoBehaviour
{
    public Camera playerCamera;
    public float shakeIntensity = 0f;

    void Update()
    {
        ApplyCameraShake();
        ApplyErrorChance();
    }

    void ApplyCameraShake()
    {

    }
    void ApplyErrorChance() // 20% ошибок при высоком стрессе
    {
    }

    void TriggerRandomActions() // При панике
    {

    }
}

