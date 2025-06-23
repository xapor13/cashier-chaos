using UnityEngine;

[CreateAssetMenu(fileName = "TimeSettings", menuName = "Game/TimeSettings", order = 1)]
public class TimeSettings : ScriptableObject
{
    [Header("Game Day Settings")]
    [Range(10f, 120f)] // Slider for real seconds per game hour
    public float realSecondsPerGameHour = 60f; // 1 game hour = 60 real seconds
    public int startHour = 8; // 08:00
    public int endHour = 23; // 23:00

    [Header("Peak Hours")]
    public Vector2Int firstPeakHours = new Vector2Int(12, 14); // 12:00-14:00
    public Vector2Int secondPeakHours = new Vector2Int(18, 20); // 18:00-20:00

    [Header("Alcohol Restriction")]
    public int alcoholRestrictionHour = 22; // No alcohol sales after 22:00
}