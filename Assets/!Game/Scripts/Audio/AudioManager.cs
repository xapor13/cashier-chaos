using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;
    public AudioSource musicSource;
    public AudioSource sfxSource;

    [Header("Sound Effects")]
    public AudioClip scanningSound;
    public AudioClip breakdownSound;
    public AudioClip repairSound;
    public AudioClip kickSound;
    public AudioClip fineSound;
    public AudioClip achievementSound;

    public void PlaySFX(AudioClip clip, float volume = 1f)
    {

    }
    public void PlayMusic(AudioClip clip, bool loop = true)
    {

    }
    public void SetMusicVolume(float volume)
    {

    }
    public void SetSFXVolume(float volume)
    {
        
    }
}

