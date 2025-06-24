using UnityEngine;

// Создает пункт меню для создания ассета CustomerData в UnityEngine
[CreateAssetMenu(fileName = "CustomerData", menuName = "Game/CustomerData", order = 3)]
public class CustomerData : ScriptableObject
{
    // Структура для хранения данных о типах клиента
    [System.Serializable]
    public struct CustomerTypeData
    {
        [Header("Основные характеристики")]
        public string name; // Имя типа клиента (Пожилой, Подросток, Обычный, Агрессивный, VIP)
        public float scanTimePerItem; // Время сканирования одного предмета (в секундах)
        public float patienceTime; // Время терпения клиента (в секундах)

        [Header("Штрафы и вероятности")]
        public float kickFine; // Штраф за изгнание клиента (в валюте TR)
        public float kickFineProbability; // Вероятность штрафа за изгнание (от 0 до 1)

        [Header("Особенности поведения")]
        public bool requiresHelpFrequently; // Требуется ли частая помощь (для Пожилых)
        public bool triesRestrictedItems; // Пытается ли взять запрещенные предметы (для Подростков)
        public bool requiresSpecialService; // Требуется ли особое обслуживание (для VIP)

        [Header("Параметры перемещения")]
        public float moveSpeed; // Скорость перемещения (для NavMeshAgent)
        [Header("Визуальный вид")]
        public GameObject prefab; // Префаб для данного типа клиента
    }

    // Массив данных о всех типах клиентов
    [Header("Список типов клиентов")]
    public CustomerTypeData[] customerTypes;
}