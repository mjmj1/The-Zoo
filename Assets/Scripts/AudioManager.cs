using EventHandler;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Pool;
using UnityEngine.SceneManagement;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Mixer & Groups")]
    [SerializeField] private AudioMixer mixer;
    [SerializeField] private AudioMixerGroup bgmGroup;
    [SerializeField] private AudioMixerGroup sfxGroup;

    [Header("BGM Clips")]
    [Tooltip("Title 씬에서 재생할 BGM")]
    [SerializeField] private AudioClip titleBGM;
    [Tooltip("Lobby 씬에서 재생할 BGM")]
    [SerializeField] private AudioClip lobbyBGM;
    [Tooltip("InGame 씬에서 재생할 BGM")]
    [SerializeField] private AudioClip inGameBGM;

    [Header("BGM Source")]
    [SerializeField] private AudioSource bgmSource;

    [Header("SFX Pool Settings")]
    [SerializeField] private int poolCapacity = 10;
    [SerializeField] private int poolMaxSize = 20;
    private ObjectPool<AudioSource> sfxPool;

    void Awake()
    {
        if (!Instance) {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else Destroy(gameObject);
    }

    void OnEnable()
    {
        GamePlayEventHandler.UIChanged += OnUIChanged;
    }

    private void OnUIChanged(string obj)
    {
        switch (obj)
        {
            case "Title":
                PlayBGM(titleBGM);
                break;
            case "Lobby":
                PlayBGM(lobbyBGM);
                break;
            case "InGame":
                PlayBGM(inGameBGM);
                break;
        }
    }

    void OnDisable()
    {
        GamePlayEventHandler.UIChanged -= OnUIChanged;
    }

    #region BGM API
    public void PlayBGM(AudioClip clip, float volume = 1f, bool loop = true)
    {
        if (bgmSource.clip == clip && bgmSource.isPlaying) return;
        bgmSource.clip = clip;
        bgmSource.outputAudioMixerGroup = bgmGroup;
        bgmSource.volume = volume;
        bgmSource.loop = loop;
        bgmSource.Play();
    }

    public void StopBGM() => bgmSource.Stop();

    public void SetBGMVolume(float linear)
        => mixer.SetFloat("BGMVolume", Mathf.Log10(Mathf.Clamp01(linear)) * 20f);
    #endregion
}