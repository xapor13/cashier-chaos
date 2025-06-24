using UnityEngine;
using System.Collections.Generic;

// Менеджер звуков для управления SFX и BGM
public class AudioManager : MonoBehaviour
{
    // Синглтон
    public static AudioManager Instance { get; private set; }

    [SerializeField] private AudioSource bgmSource; // Источник фоновой музыки
    [SerializeField] private List<AudioSource> sfxSources; // Пул источников для SFX
    [SerializeField] private List<SoundClip> soundClips; // Список звуковых клипов
    [SerializeField] private float bgmVolume = 0.5f; // Громкость BGM
    [SerializeField] private float sfxVolume = 0.8f; // Громкость SFX

    // Структура для хранения звуковых клипов
    [System.Serializable]
    private struct SoundClip
    {
        public string id; // Уникальный идентификатор звука
        public AudioClip clip; // Аудиоклип
        [Range(0f, 1f)] public float volume; // Индивидуальная громкость
        [Range(0.5f, 2f)] public float pitch; // Тон звука
    }

    private int currentSfxSourceIndex = 0; // Индекс текущего источника SFX

    // Инициализация синглтона
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Сохранять между сценами
        }
        else
        {
            Destroy(gameObject); // Уничтожить дубликат
            return;
        }

        // Инициализация пула источников SFX
        if (sfxSources == null || sfxSources.Count == 0)
        {
            sfxSources = new List<AudioSource>();
            for (int i = 0; i < 5; i++) // Создаем 5 источников SFX
            {
                AudioSource source = gameObject.AddComponent<AudioSource>();
                source.playOnAwake = false;
                source.volume = sfxVolume;
                sfxSources.Add(source);
            }
        }

        // Проверка BGM источника
        if (bgmSource == null)
        {
            bgmSource = gameObject.AddComponent<AudioSource>();
            bgmSource.loop = true;
            bgmSource.volume = bgmVolume;
        }

        // Проверка звуковых клипов
        if (soundClips == null)
        {
            soundClips = new List<SoundClip>();
        }
    }

    // Воспроизведение фоновой музыки
    public void PlayBGM(AudioClip clip)
    {
        if (bgmSource != null && clip != null)
        {
            bgmSource.clip = clip;
            bgmSource.volume = bgmVolume;
            bgmSource.Play();
        }
        else
        {
            Debug.LogWarning("BGM клип или источник не назначены!");
        }
    }

    // Остановка фоновой музыки
    public void StopBGM()
    {
        if (bgmSource != null)
        {
            bgmSource.Stop();
        }
    }

    // Воспроизведение звукового эффекта по ID
    public void PlaySFX(string id)
    {
        SoundClip? clip = soundClips.Find(c => c.id == id);
        if (clip.HasValue)
        {
            PlaySFX(clip.Value.clip, clip.Value.volume, clip.Value.pitch);
        }
        else
        {
            Debug.LogWarning($"Звук с ID {id} не найден!");
        }
    }

    // Воспроизведение звукового эффекта с настройками
    private void PlaySFX(AudioClip clip, float volume, float pitch)
    {
        if (clip == null || sfxSources.Count == 0) return;

        AudioSource source = sfxSources[currentSfxSourceIndex];
        source.clip = clip;
        source.volume = sfxVolume * volume;
        source.pitch = pitch;
        source.Play();

        // Переход к следующему источнику в пуле
        currentSfxSourceIndex = (currentSfxSourceIndex + 1) % sfxSources.Count;
    }

    // Установка громкости BGM
    public void SetBGMVolume(float volume)
    {
        bgmVolume = Mathf.Clamp01(volume);
        if (bgmSource != null)
        {
            bgmSource.volume = bgmVolume;
        }
    }

    // Установка громкости SFX
    public void SetSFXVolume(float volume)
    {
        sfxVolume = Mathf.Clamp01(volume);
        foreach (var source in sfxSources)
        {
            source.volume = sfxVolume;
        }
    }
}