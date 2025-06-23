namespace GameCore
{
    /// <summary>
    /// Состояния игры.
    /// </summary>
    public enum GameState
    {
        MainMenu, // Главное меню
        Playing, // Активная игра
        Paused, // Пауза
        GameOver, // Поражение
        Victory // Победа
    }

    /// <summary>
    /// Состояния кассового аппарата.
    /// </summary>
    public enum CashRegisterState
    {
        Working, // Работает
        NeedsAttention, // Требует внимания
        Broken, // Сломана
        Off // Выключена
    }

    /// <summary>
    /// Типы кассовых аппаратов.
    /// </summary>
    public enum CashRegisterType
    {
        Basic, // 15₽/мин
        Enhanced, // 25₽/мин
        Premium // 40₽/мин
    }

    /// <summary>
    /// Типы клиентов магазина.
    /// </summary>
    public enum CustomerType
    {
        Elderly, // Пожилой
        Teenager, // Подросток
        Regular, // Обычный
        Aggressive, // Агрессивный
        VIP // VIP
    }

    /// <summary>
    /// Уровни стресса сотрудников.
    /// </summary>
    public enum StressLevel
    {
        Normal, // 0-30
        Tired, // 31-60
        Stressed, // 61-80
        Panic // 81-100
    }

    /// <summary>
    /// Уровни развития магазина.
    /// </summary>
    public enum StoreLevel
    {
        Startup, // 0-100k₽, 4 кассы
        Developing, // 100k-250k₽, 8 касс
        Stable, // 250k-500k₽, 12 касс
        Network // 500k+₽, сеть
    }

    /// <summary>
    /// Дни недели с событиями.
    /// </summary>
    public enum WeekDay
    {
        Monday, // Без табака
        Tuesday, // Обычный день
        Wednesday, // Скидки пенсионерам
        Thursday, // Обычный день
        Friday, // Молодежный день
        Saturday, // Семейный день
        Sunday // Семейный день
    }

    /// <summary>
    /// Типы товаров в магазине.
    /// </summary>
    public enum ShopItemType
    {
        BasicRegister, // Базовая касса
        EnhancedRegister, // Улучшенная касса
        PremiumRegister, // Премиум касса
        Mechanic, // Механик
        Assistant, // Помощник
        SecurityGuard // Охранник
    }

    /// <summary>
    /// Типы штрафов.
    /// </summary>
    public enum FineType
    {
        AlcoholToMinor = 10000, // Продажа алкоголя несовершеннолетнему
        AlcoholAfterHours = 5000, // Продажа алкоголя после 22:00
        KickElderly = 2000, // Выгон пожилого клиента
        KickVIP = 5000, // Выгон VIP-клиента
        IgnoreBrokenRegister = 1000, // Игнорирование сломанной кассы
        MassCustomerLeave = 2000 // Массовый уход клиентов
    }
}