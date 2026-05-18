using UnityEngine;
using UnityEngine.Video;

// Управляет видеопанелью кассеты текущего дня.
public class KasetaVideoController : MonoBehaviour
{
    // Контроллер, из которого берется текущий день.
    [SerializeField] private DayController dayController;

    // Панель, на которой показывается видео.
    [SerializeField] private GameObject videoPanel;

    // Компонент, который воспроизводит видеоролик.
    [SerializeField] private VideoPlayer videoPlayer;

    // Источник звука для аудиодорожки видео.
    [SerializeField] private AudioSource audioSource;

    // Индекс аудиодорожки видео, обычно используется первая дорожка.
    [SerializeField] private ushort audioTrackIndex;

    // Подготавливает источник звука для VideoPlayer.
    private void Awake()
    {
        ConfigureAudioOutput();
    }

    // Скрывает видеопанель при старте сцены.
    private void Start()
    {
        if (videoPanel != null)
        {
            videoPanel.SetActive(false);
        }
    }

    // Открывает видео текущего дня и запускает воспроизведение.
    public void OpenCurrentDayVideo()
    {
        if (dayController == null)
        {
            Debug.LogError("KasetaVideoController: не задан DayController.");
            return;
        }

        if (dayController.CurrentDay == null)
        {
            Debug.LogWarning("KasetaVideoController: текущий день еще не инициализирован.");
            return;
        }

        if (dayController.CurrentDay.kasetaClip == null)
        {
            Debug.LogWarning("KasetaVideoController: у текущего дня не задан видеоролик для кассеты.");
            return;
        }

        if (videoPanel != null)
        {
            videoPanel.SetActive(true);
        }

        if (BackgroundMusicController.Instance != null)
        {
            BackgroundMusicController.Instance.PauseMusic();
        }

        if (videoPlayer != null)
        {
            videoPlayer.Stop();
            videoPlayer.clip = dayController.CurrentDay.kasetaClip;
            ConfigureAudioOutput();
            videoPlayer.Play();
        }
    }

    // Закрывает видеопанель и останавливает воспроизведение.
    public void CloseVideo()
    {
        if (videoPlayer != null)
        {
            videoPlayer.Stop();
        }

        if (audioSource != null)
        {
            audioSource.Stop();
        }

        if (BackgroundMusicController.Instance != null)
        {
            BackgroundMusicController.Instance.ResumeMusic();
        }

        if (videoPanel != null)
        {
            videoPanel.SetActive(false);
        }
    }

    // Находит или создает AudioSource для вывода звука видео.
    private void PrepareAudioSource()
    {
        if (videoPlayer == null)
        {
            return;
        }

        if (audioSource == null)
        {
            audioSource = videoPlayer.GetComponent<AudioSource>();
        }

        if (audioSource == null)
        {
            audioSource = videoPlayer.gameObject.AddComponent<AudioSource>();
        }

        audioSource.playOnAwake = false;
        audioSource.mute = false;
        audioSource.volume = 1f;
    }

    // Настраивает VideoPlayer на вывод звука через AudioSource.
    private void ConfigureAudioOutput()
    {
        PrepareAudioSource();

        if (videoPlayer == null || audioSource == null)
        {
            return;
        }

        videoPlayer.audioOutputMode = VideoAudioOutputMode.AudioSource;
        videoPlayer.playOnAwake = false;
        videoPlayer.controlledAudioTrackCount = 1;
        videoPlayer.EnableAudioTrack(audioTrackIndex, true);
        videoPlayer.SetTargetAudioSource(audioTrackIndex, audioSource);
    }
}
