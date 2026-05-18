using UnityEngine;
using UnityEngine.Video;

// Проигрывает фоновую музыку и дает паузить ее на время видео.
public class BackgroundMusicController : MonoBehaviour
{
    // Текущий контроллер музыки на сцене.
    public static BackgroundMusicController Instance { get; private set; }

    // Музыкальный файл, можно использовать VideoClip с аудиодорожкой.
    [SerializeField] private VideoClip musicClip;

    // Плеер для проигрывания аудио из VideoClip без вывода картинки.
    [SerializeField] private VideoPlayer musicPlayer;

    // Источник звука для фоновой музыки.
    [SerializeField] private AudioSource audioSource;

    // Громкость фоновой музыки.
    [SerializeField] private float volume = 0.35f;

    // Запускать музыку автоматически при старте сцены.
    [SerializeField] private bool playOnStart = true;

    // Не уничтожать музыку при переходе между сценами.
    [SerializeField] private bool dontDestroyOnLoad = true;

    // Подготавливает одиночный музыкальный контроллер.
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        if (dontDestroyOnLoad)
        {
            DontDestroyOnLoad(gameObject);
        }

        PrepareAudio();
    }

    // Запускает музыку при старте сцены.
    private void Start()
    {
        if (playOnStart)
        {
            PlayMusic();
        }
    }

    // Запускает или продолжает фоновую музыку.
    public void PlayMusic()
    {
        PrepareAudio();

        if (musicPlayer == null || musicClip == null)
        {
            Debug.LogWarning("BackgroundMusicController: не задан musicPlayer или musicClip.");
            return;
        }

        if (musicPlayer.clip != musicClip)
        {
            musicPlayer.clip = musicClip;
        }

        if (musicPlayer.isPlaying == false)
        {
            musicPlayer.Play();
        }
    }

    // Ставит фоновую музыку на паузу.
    public void PauseMusic()
    {
        if (musicPlayer != null && musicPlayer.isPlaying)
        {
            musicPlayer.Pause();
        }
    }

    // Возвращает фоновую музыку после паузы.
    public void ResumeMusic()
    {
        PlayMusic();
    }

    // Настраивает VideoPlayer и AudioSource для музыки.
    private void PrepareAudio()
    {
        if (musicPlayer == null)
        {
            musicPlayer = GetComponent<VideoPlayer>();
        }

        if (musicPlayer == null)
        {
            musicPlayer = gameObject.AddComponent<VideoPlayer>();
        }

        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }

        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        audioSource.playOnAwake = false;
        audioSource.loop = true;
        audioSource.mute = false;
        audioSource.volume = volume;

        musicPlayer.playOnAwake = false;
        musicPlayer.isLooping = true;
        musicPlayer.renderMode = VideoRenderMode.APIOnly;
        musicPlayer.audioOutputMode = VideoAudioOutputMode.AudioSource;
        musicPlayer.controlledAudioTrackCount = 1;
        musicPlayer.EnableAudioTrack(0, true);
        musicPlayer.SetTargetAudioSource(0, audioSource);
    }
}
