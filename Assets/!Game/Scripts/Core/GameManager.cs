using UnityEngine;
using GameCore;

public class GameManager : MonoBehaviour
{
    private const float DayStartTime = 8f; // Начало дня в 8:00
    private const float DayEndTime = 10f; // Конец дня в 1:00
    private const float SecondsToGameHours = 60f; // Конвертация секунд в игровые часы
    private const float PausedTimeScale = 0f; // Временной масштаб при паузе
    private const float NormalTimeScale = 1f; // Нормальный временной масштаб

    [Header("Зависимости")]
    [SerializeField] private EconomyManager economyManager;

    [Header("Состояние игры")]
    [SerializeField] private GameState currentState = GameState.MainMenu;
    [SerializeField] private float currentGameTime = DayStartTime;
    [SerializeField] private int currentDay = 1;
    [SerializeField] private bool isGamePaused = false;
    [SerializeField] private bool isGameActive = false;

    [Header("Настройки игры")]
    [SerializeField] private float gameTimeScale = 1f; // Скорость течения игрового времени

    private void Start()
    {
        BeginNewGame();
    }

    private void Update()
    {
        if (isGameActive && !isGamePaused)
        {
            Debug.Log("Начало Advance Game Time");
            AdvanceGameTime();
        }
    }

    private void BeginNewGame()
    {
        InitializeGameState();
        LogGameStart();
    }

    private void InitializeGameState()
    {
        currentState = GameState.Playing;
        isGameActive = true;
        currentGameTime = DayStartTime;
        currentDay = 1;
    }

    private void LogGameStart()
    {
        Debug.Log($"Игра началась - День {currentDay}");
    }

    private void AdvanceGameTime()
    {
        currentGameTime += Time.deltaTime * gameTimeScale / SecondsToGameHours;

        if (HasDayEnded())
        {
            CompleteDay();
        }
    }

    private bool HasDayEnded()
    {
        return currentGameTime >= DayEndTime;
    }

    private void CompleteDay()
    {
        LogDayEnd();
        AdvanceToNextDay();
        ProcessDailyExpenses();
    }

    private void LogDayEnd()
    {
        Debug.Log($"День {currentDay} завершен");
    }

    private void AdvanceToNextDay()
    {
        currentDay++;
        currentGameTime = DayStartTime;
    }

    private void ProcessDailyExpenses()
    {
        if (economyManager != null)
        {
            economyManager.ProcessDailyExpenses();
        }
    }

    public void PauseGame()
    {
        if (isGamePaused) return;

        isGamePaused = true;
        Time.timeScale = PausedTimeScale;
        currentState = GameState.Paused;
    }

    public void ResumeGame()
    {
        if (!isGamePaused) return;

        isGamePaused = false;
        Time.timeScale = NormalTimeScale;
        currentState = GameState.Playing;
    }

    public void TriggerGameOver()
    {
        if (!isGameActive) return;

        currentState = GameState.GameOver;
        isGameActive = false;
        Debug.Log("Игра окончена!");
    }

    public void TriggerVictory()
    {
        if (!isGameActive) return;

        currentState = GameState.Victory;
        isGameActive = false;
        Debug.Log("Победа! Цель достигнута!");
    }

    // Геттеры для доступа к состоянию
    public GameState GetCurrentState() => currentState;
    public float GetCurrentGameTime() => currentGameTime;
    public int GetCurrentDay() => currentDay;
    public bool IsGamePaused() => isGamePaused;
    public bool IsGameActive() => isGameActive;
}