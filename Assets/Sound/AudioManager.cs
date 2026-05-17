using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    // -------------------------
    // Audio Sources
    // -------------------------

    [Header("Audio Sources")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource sfxSource;

    [Header("Disco Music (3D - va en el objeto de la disco en la escena)")]
    [SerializeField] private AudioSource discoMusicSource;

    // -------------------------
    // Musica
    // -------------------------

    [Header("Music Tracks")]
    [SerializeField] private AudioClip menuMusic;
    [SerializeField] private AudioClip cinematicMusic;
    [SerializeField] private AudioClip level1Music;
    [SerializeField] private AudioClip level2Music;
    [SerializeField] private AudioClip level3Music;

    // -------------------------
    // SFX
    // -------------------------

    [Header("SFX Clips")]
    [SerializeField] private List<SoundEntry> sfxClips = new();

    // -------------------------
    // Volumen
    // -------------------------

    [Header("Volume Settings")]
    [Range(0f, 1f)] public float musicVolume = 1f;
    [Range(0f, 1f)] public float sfxVolume = 1f;

    // -------------------------
    // Unity lifecycle
    // -------------------------

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        musicSource.volume = musicVolume;

        // Arranca la musica de la disco automaticamente
        if (discoMusicSource != null)
        {
            discoMusicSource.loop = true;
            discoMusicSource.volume = musicVolume;
            discoMusicSource.Play();
        }
    }

    // -------------------------
    // Musica por track
    // -------------------------

    public enum MusicTrack
    {
        Menu,
        Cinematic,
        Level1,
        Level2,
        Level3
    }

    public void PlayMusic(MusicTrack track, float fadeDuration = 0f)
    {
        AudioClip clip = track switch
        {
            MusicTrack.Menu      => menuMusic,
            MusicTrack.Cinematic => cinematicMusic,
            MusicTrack.Level1    => level1Music,
            MusicTrack.Level2    => level2Music,
            MusicTrack.Level3    => level3Music,
            _                    => null
        };

        if (clip == null)
        {
            Debug.LogWarning($"[AudioManager] No hay clip asignado para el track: {track}");
            return;
        }

        if (musicSource.clip == clip && musicSource.isPlaying) return;

        if (fadeDuration > 0f)
            StartCoroutine(FadeToTrack(clip, fadeDuration));
        else
            PlayMusicDirect(clip);
    }

    private void PlayMusicDirect(AudioClip clip)
    {
        musicSource.clip = clip;
        musicSource.loop = true;
        musicSource.volume = musicVolume;
        musicSource.Play();
    }

    public void StopMusic(float fadeDuration = 0f)
    {
        if (fadeDuration > 0f)
            StartCoroutine(FadeOut(musicSource, fadeDuration));
        else
            musicSource.Stop();
    }

    public void StopDiscoMusic(float fadeDuration = 0f)
{
    if (discoMusicSource == null) return;

    if (fadeDuration > 0f)
        StartCoroutine(FadeOut(discoMusicSource, fadeDuration));
    else
        discoMusicSource.Stop();
}

    // -------------------------
    // SFX
    // -------------------------

    public void PlaySFX(string name)
    {
        SoundEntry entry = sfxClips.Find(s => s.name == name);

        if (entry == null || entry.clip == null)
        {
            Debug.LogWarning($"[AudioManager] SFX no encontrado: '{name}'");
            return;
        }

        sfxSource.PlayOneShot(entry.clip, sfxVolume);
    }

    // -------------------------
    // Volumen
    // -------------------------

    public void MuffleMusic(float holdDuration)
    {
        StartCoroutine(MuffleMusicCoroutine(holdDuration));
    }

    private IEnumerator MuffleMusicCoroutine(float holdDuration)
    {
        float elapsed = 0f;
        float muteIn  = 0.15f;
        while (elapsed < muteIn)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / muteIn;
            musicSource.pitch  = Mathf.Lerp(1f, 0.55f, t);
            musicSource.volume = Mathf.Lerp(musicVolume, musicVolume * 0.2f, t);
            yield return null;
        }

        yield return new WaitForSeconds(holdDuration);

        elapsed = 0f;
        float restoreTime = 2.5f;
        while (elapsed < restoreTime)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / restoreTime;
            musicSource.pitch  = Mathf.Lerp(0.55f, 1f, t);
            musicSource.volume = Mathf.Lerp(musicVolume * 0.2f, musicVolume, t);
            yield return null;
        }
        musicSource.pitch  = 1f;
        musicSource.volume = musicVolume;
    }

    public void SetMusicVolume(float value)
    {
        musicVolume = Mathf.Clamp01(value);
        musicSource.volume = musicVolume;

        if (discoMusicSource != null)
            discoMusicSource.volume = musicVolume;
    }

    public void SetSFXVolume(float value)
    {
        sfxVolume = Mathf.Clamp01(value);
    }

    // -------------------------
    // Fade coroutines
    // -------------------------

    private IEnumerator FadeToTrack(AudioClip newClip, float duration)
    {
        yield return StartCoroutine(FadeOut(musicSource, duration / 2f));

        musicSource.clip = newClip;
        musicSource.loop = true;
        musicSource.Play();

        yield return StartCoroutine(FadeIn(musicSource, duration / 2f));
    }

    private IEnumerator FadeOut(AudioSource source, float duration)
    {
        float startVolume = source.volume;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            source.volume = Mathf.Lerp(startVolume, 0f, elapsed / duration);
            yield return null;
        }

        source.volume = 0f;
        source.Stop();
    }

    private IEnumerator FadeIn(AudioSource source, float duration)
    {
        float elapsed = 0f;
        source.volume = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            source.volume = Mathf.Lerp(0f, musicVolume, elapsed / duration);
            yield return null;
        }

        source.volume = musicVolume;
    }
}

// -------------------------
// Clase auxiliar para SFX
// -------------------------

[System.Serializable]
public class SoundEntry
{
    public string name;
    public AudioClip clip;
}