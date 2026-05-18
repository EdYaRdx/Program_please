using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Настройки последовательной проверки папок для одного дня.
[System.Serializable]
public class DayFolderSequenceData
{
    // Id дня из DayData, например day_02.
    public string dayId;

    // Группа папок, которая должна быть активна в этом дне.
    public GameObject foldersGroup;

    // Родительский объект, внутри которого лежат папки дня.
    public Transform folderRoot;

    // Автоматически брать папки из дочерних Image внутри folderRoot.
    public bool collectFoldersFromChildren = true;

    // Папки, которые нужно проверять по порядку.
    public List<Image> folderImages = new List<Image>();

    // Спрайт, который ставится на папку после проверки.
    public Sprite checkedFolderSprite;

    // Текст, который появляется после проверки всех папок.
    public string resultText;

    // Пауза между проверкой папок.
    public float stepDelay = 0.08f;
}

// По кнопке последовательно меняет спрайты папок текущего дня и показывает результат.
public class FolderSequenceCheckController : MonoBehaviour
{
    // Контроллер текущего дня.
    [SerializeField] private DayController dayController;

    // Кнопка запуска проверки папок.
    [SerializeField] private Button startButton;

    // Текст результата после проверки.
    [SerializeField] private TMP_Text resultText;

    // Настройки папок для разных дней.
    [SerializeField] private List<DayFolderSequenceData> daySequences = new List<DayFolderSequenceData>();

    // Скрывать текст результата при старте нового дня.
    [SerializeField] private bool hideResultTextOnReset = true;

    // Блокировать кнопку, пока идет проверка папок.
    [SerializeField] private bool disableButtonDuringSequence = true;

    private readonly List<Image> cachedImages = new List<Image>();
    private readonly List<Sprite> cachedInitialSprites = new List<Sprite>();
    private Coroutine sequenceCoroutine;

    // Автоматически находит ссылки, если они не заданы вручную.
    private void Awake()
    {
        if (dayController == null)
        {
            dayController = FindFirstObjectByType<DayController>();
        }

        if (startButton == null)
        {
            startButton = GetComponent<Button>();
        }

        CacheInitialSprites();
    }

    // Подписывается на кнопку и смену дня.
    private void OnEnable()
    {
        if (startButton != null)
        {
            startButton.onClick.AddListener(StartFolderSequence);
        }

        if (dayController != null)
        {
            dayController.DayInitialized += OnDayInitialized;
        }

        UpdateActiveFolderGroup();
        ResetView();
    }

    // Отписывается от кнопки и смены дня.
    private void OnDisable()
    {
        if (startButton != null)
        {
            startButton.onClick.RemoveListener(StartFolderSequence);
        }

        if (dayController != null)
        {
            dayController.DayInitialized -= OnDayInitialized;
        }
    }

    // Сбрасывает папки при запуске нового дня.
    private void OnDayInitialized(DayData dayData)
    {
        UpdateActiveFolderGroup();
        CacheInitialSprites();
        ResetView();
    }

    // Запускает последовательную проверку папок текущего дня.
    public void StartFolderSequence()
    {
        DayFolderSequenceData sequenceData = GetCurrentSequenceData();

        if (sequenceData == null)
        {
            Debug.LogWarning("FolderSequenceCheckController: нет настроек папок для текущего дня.");
            return;
        }

        if (sequenceCoroutine != null)
        {
            StopCoroutine(sequenceCoroutine);
        }

        ResetView();
        sequenceCoroutine = StartCoroutine(PlayFolderSequence(sequenceData));
    }

    // Возвращает папки и текст в состояние до проверки.
    private void ResetView()
    {
        if (sequenceCoroutine != null)
        {
            StopCoroutine(sequenceCoroutine);
            sequenceCoroutine = null;
        }

        for (int i = 0; i < cachedImages.Count; i++)
        {
            Image image = cachedImages[i];

            if (image != null)
            {
                image.sprite = cachedInitialSprites[i];
            }
        }

        if (resultText != null)
        {
            resultText.text = string.Empty;

            if (hideResultTextOnReset)
            {
                resultText.gameObject.SetActive(false);
            }
        }

        if (startButton != null)
        {
            startButton.interactable = true;
        }
    }

    // Включает объекты папок текущего дня и выключает остальные.
    private void UpdateActiveFolderGroup()
    {
        for (int i = 0; i < daySequences.Count; i++)
        {
            DayFolderSequenceData sequenceData = daySequences[i];

            if (sequenceData == null)
            {
                continue;
            }

            SetSequenceObjectActive(sequenceData, IsSequenceActiveForCurrentDay(sequenceData));
        }
    }

    // Проверяет, относится ли набор папок к текущему дню.
    private bool IsSequenceActiveForCurrentDay(DayFolderSequenceData sequenceData)
    {
        string dayId = dayController != null && dayController.CurrentDay != null
            ? dayController.CurrentDay.dayId
            : string.Empty;

        return string.IsNullOrEmpty(dayId) == false && sequenceData.dayId == dayId;
    }

    // Включает или выключает группу, root или отдельные папки.
    private void SetSequenceObjectActive(DayFolderSequenceData sequenceData, bool active)
    {
        GameObject mainObject = GetMainSequenceObject(sequenceData);

        if (mainObject != null)
        {
            mainObject.SetActive(IsMainObjectActiveForAnyCurrentSequence(mainObject));
            return;
        }

        for (int i = 0; i < sequenceData.folderImages.Count; i++)
        {
            Image image = sequenceData.folderImages[i];

            if (image != null)
            {
                image.gameObject.SetActive(active);
            }
        }
    }

    // Возвращает главный объект набора папок.
    private GameObject GetMainSequenceObject(DayFolderSequenceData sequenceData)
    {
        if (sequenceData.foldersGroup != null)
        {
            return sequenceData.foldersGroup;
        }

        if (sequenceData.folderRoot != null)
        {
            return sequenceData.folderRoot.gameObject;
        }

        return null;
    }

    // Проверяет, нужен ли объект хотя бы одному набору текущего дня.
    private bool IsMainObjectActiveForAnyCurrentSequence(GameObject mainObject)
    {
        for (int i = 0; i < daySequences.Count; i++)
        {
            DayFolderSequenceData sequenceData = daySequences[i];

            if (sequenceData != null && GetMainSequenceObject(sequenceData) == mainObject && IsSequenceActiveForCurrentDay(sequenceData))
            {
                return true;
            }
        }

        return false;
    }

    // Меняет спрайты папок по порядку.
    private IEnumerator PlayFolderSequence(DayFolderSequenceData sequenceData)
    {
        if (disableButtonDuringSequence && startButton != null)
        {
            startButton.interactable = false;
        }

        List<Image> folderImages = GetFolderImages(sequenceData);

        for (int i = 0; i < folderImages.Count; i++)
        {
            Image folderImage = folderImages[i];

            if (folderImage != null && sequenceData.checkedFolderSprite != null)
            {
                folderImage.sprite = sequenceData.checkedFolderSprite;
            }

            if (sequenceData.stepDelay > 0f)
            {
                yield return new WaitForSeconds(sequenceData.stepDelay);
            }
        }

        if (resultText != null)
        {
            resultText.text = sequenceData.resultText;
            resultText.gameObject.SetActive(true);
        }

        if (startButton != null)
        {
            startButton.interactable = true;
        }

        sequenceCoroutine = null;
    }

    // Запоминает стартовые спрайты всех папок.
    private void CacheInitialSprites()
    {
        cachedImages.Clear();
        cachedInitialSprites.Clear();

        for (int i = 0; i < daySequences.Count; i++)
        {
            DayFolderSequenceData sequenceData = daySequences[i];

            if (sequenceData == null)
            {
                continue;
            }

            List<Image> folderImages = GetFolderImages(sequenceData);

            for (int j = 0; j < folderImages.Count; j++)
            {
                Image image = folderImages[j];

                if (image != null && cachedImages.Contains(image) == false)
                {
                    cachedImages.Add(image);
                    cachedInitialSprites.Add(image.sprite);
                }
            }
        }
    }

    // Находит настройки для текущего дня.
    private DayFolderSequenceData GetCurrentSequenceData()
    {
        string dayId = dayController != null && dayController.CurrentDay != null
            ? dayController.CurrentDay.dayId
            : string.Empty;

        for (int i = 0; i < daySequences.Count; i++)
        {
            DayFolderSequenceData sequenceData = daySequences[i];

            if (sequenceData != null && sequenceData.dayId == dayId)
            {
                return sequenceData;
            }
        }

        return null;
    }

    // Возвращает папки из folderRoot или из ручного списка.
    private List<Image> GetFolderImages(DayFolderSequenceData sequenceData)
    {
        if (sequenceData == null)
        {
            return new List<Image>();
        }

        if (sequenceData.collectFoldersFromChildren && sequenceData.folderRoot != null)
        {
            Image[] childImages = sequenceData.folderRoot.GetComponentsInChildren<Image>(true);
            List<Image> result = new List<Image>();

            for (int i = 0; i < childImages.Length; i++)
            {
                Image image = childImages[i];

                if (image != null && image.transform != sequenceData.folderRoot)
                {
                    result.Add(image);
                }
            }

            return result;
        }

        return sequenceData.folderImages;
    }
}
