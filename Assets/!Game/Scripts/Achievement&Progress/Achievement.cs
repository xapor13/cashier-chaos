using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Achievement
{
    public string id;
    public string title;
    public string description;
    public bool isUnlocked = false;
    public int targetValue;
    public int currentProgress;

    public bool CheckProgress(int newValue)
    public void Unlock()
}

