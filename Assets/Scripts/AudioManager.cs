using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("Audio Sources")]
    public AudioSource musicSource;
    public AudioSource actionMusicSource;
    public AudioSource sfxSource;

    [Header("BGM Tracks")]
    public AudioClip menuBGM;
    public AudioClip levelBGM;
    public AudioClip actionBGM;

    [Header("SFX Tracks")]
    public AudioClip levelSelectSound;
    public AudioClip stationConsumeSound;
    public AudioClip incineratorConsumeSound;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        StopActionBGM(); // Ensure action BGM doesn't carry over between scenes

        if (scene.name == "MainMenu" || scene.name == "Credits")
        {
            PlayBGM(menuBGM);
        }
        else if (scene.name.StartsWith("Level_"))
        {
            PlayBGM(levelBGM);
        }
    }

    public void PlayBGM(AudioClip bgmClip)
    {
        if (bgmClip == null) return;

        if (musicSource.clip == bgmClip && musicSource.isPlaying)
            return; // Already playing

        musicSource.clip = bgmClip;
        musicSource.volume = 1f; // Ensure volume is reset in case it was faded out
        musicSource.Play();
    }

    public void PlaySFX(AudioClip clip)
    {
        if (clip != null)
        {
            sfxSource.PlayOneShot(clip);
        }
    }

    public void PlayActionBGM()
    {
        if (actionBGM != null && actionMusicSource != null)
        {
            if (actionMusicSource.clip == actionBGM && actionMusicSource.isPlaying) return;
            actionMusicSource.clip = actionBGM;
            // actionMusicSource.volume = 1f;
            actionMusicSource.Play();
        }
    }

    public void StopActionBGM()
    {
        if (actionMusicSource != null && actionMusicSource.isPlaying)
        {
            actionMusicSource.Stop();
        }
    }

    public IEnumerator FadeOutBGM(float duration)
    {
        float startVolume = musicSource.volume;
        float actionStartVolume = actionMusicSource != null ? actionMusicSource.volume : 0f;

        while (musicSource.volume > 0 || (actionMusicSource != null && actionMusicSource.volume > 0))
        {
            musicSource.volume -= startVolume * Time.deltaTime / duration;
            if (actionMusicSource != null)
            {
                actionMusicSource.volume -= actionStartVolume * Time.deltaTime / duration;
            }
            yield return null;
        }

        musicSource.Stop();
        musicSource.volume = startVolume;

        if (actionMusicSource != null)
        {
            actionMusicSource.Stop();
            actionMusicSource.volume = actionStartVolume;
        }
    }
}
