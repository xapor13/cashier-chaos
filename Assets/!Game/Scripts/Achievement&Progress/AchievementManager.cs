using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AchievementManager : MonoBehaviour
{
    public static AchievementManager Instance;
    public List<Achievement> achievements;

    void Start() { InitializeAchievements(); }

    public void UpdateProgress(string achievementId, int value)
    {

    }
    public void UnlockAchievement(string achievementId)
    {

    }
    void InitializeAchievements()
    {
        
    }
}

