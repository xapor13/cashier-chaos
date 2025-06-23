using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StressManager : MonoBehaviour
{
    public static StressManager Instance;
    public float currentStress = 0f;
    public float maxStress = 100f;

    public enum StressLevel { Normal, Tired, Stressed, Panic }
    public StressLevel currentLevel;

    void Update() { ApplyStressEffects(); }

    public void AddStress(float amount)
    public void ReduceStress(float amount)
    public void ApplyStressEffects()
    public StressLevel GetStressLevel()
}

