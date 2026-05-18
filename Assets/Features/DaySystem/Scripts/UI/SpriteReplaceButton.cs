using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Настройки результата для конкретного дня.
[System.Serializable]
public class DaySpriteReplaceResult
{
    // Id дня из DayData, например day_01.
    public string dayId;

    // Спрайт папки, который ставится после клика в этом дне.
    public Sprite replacementSprite;

    // Текст результата, который появляется после клика в этом дне.
    public string resultText;

    // Trigger в Animator для анимации папки в этом дне.
    public string animationTrigger;
}

// Меняет спрайт выбранного объекта и показывает результат после нажатия на кнопку.
public class SpriteReplaceButton : MonoBehaviour
{
    // Контроллер текущего дня для выбора нужного текста.
    [SerializeField] private DayController dayController;

    // Кнопка, по нажатию на которую выполняется замена.
    [SerializeField] private Button button;

    // UI-изображение, у которого нужно заменить спрайт.
    [SerializeField] private Image targetImage;

    // SpriteRenderer для замены спрайта, если объект не UI.
    [SerializeField] private SpriteRenderer targetSpriteRenderer;

    // Новый спрайт, который будет поставлен вместо текущего.
    [SerializeField] private Sprite replacementSprite;

    // Animator папки, если после клика нужно запускать анимацию.
    [SerializeField] private Animator folderAnimator;

    // Общий trigger анимации, если для дня не задан свой.
    [SerializeField] private string fallbackAnimationTrigger;

    // Настройки спрайтов и текстов для разных дней.
    [SerializeField] private List<DaySpriteReplaceResult> dayResults = new List<DaySpriteReplaceResult>();

    // Текст, который появляется после замены спрайта.
    [SerializeField] private TMP_Text resultText;

    // Номер дня для ручного выбора текста, если DayController не задан.
    [SerializeField] private int fallbackDayNumber = 1;

    // Задержка перед показом текста, если сначала должна проиграться анимация.
    [SerializeField] private float resultTextDelay;

    // Скрывает текст результата при запуске сцены.
    [SerializeField] private bool hideResultTextOnStart = true;

    private Coroutine showTextCoroutine;
    private Sprite initialImageSprite;
    private Sprite initialRendererSprite;
    private bool hasSequenceController;

    // Автоматически находит кнопку на этом объекте, если ссылка не задана.
    private void Awake()
    {
        if (dayController == null)
        {
            dayController = FindFirstObjectByType<DayController>();
        }

        if (button == null)
        {
            button = GetComponent<Button>();
        }

        hasSequenceController = GetComponent<FolderSequenceCheckController>() != null;

        if (targetImage != null)
        {
            initialImageSprite = targetImage.sprite;
        }

        if (targetSpriteRenderer != null)
        {
            initialRendererSprite = targetSpriteRenderer.sprite;
        }

        if (folderAnimator == null)
        {
            folderAnimator = GetComponent<Animator>();
        }
    }

    // Подготавливает текст результата перед первым кликом.
    private void Start()
    {
        ResetViewForDay();
    }

    // Подписывается на клик кнопки, смену дня и сбрасывает старое состояние.
    private void OnEnable()
    {
        if (hasSequenceController)
        {
            return;
        }

        if (button != null)
        {
            button.onClick.AddListener(ReplaceSprite);
        }

        if (dayController != null)
        {
            dayController.DayInitialized += OnDayInitialized;
        }

        ResetViewForDay();
    }

    // Отписывается от клика кнопки и смены дня.
    private void OnDisable()
    {
        if (hasSequenceController)
        {
            return;
        }

        if (button != null)
        {
            button.onClick.RemoveListener(ReplaceSprite);
        }

        if (dayController != null)
        {
            dayController.DayInitialized -= OnDayInitialized;
        }
    }

    // Ставит новый спрайт в выбранный Image или SpriteRenderer.
    public void ReplaceSprite()
    {
        Sprite currentReplacementSprite = GetReplacementSprite();

        if (currentReplacementSprite == null)
        {
            Debug.LogWarning("SpriteReplaceButton: не задан replacementSprite.");
            return;
        }

        if (targetImage != null)
        {
            targetImage.sprite = currentReplacementSprite;
        }

        if (targetSpriteRenderer != null)
        {
            targetSpriteRenderer.sprite = currentReplacementSprite;
        }

        if (targetImage == null && targetSpriteRenderer == null)
        {
            Debug.LogWarning("SpriteReplaceButton: не задан объект для замены спрайта.");
        }

        PlayFolderAnimation();
        ShowResultText();
    }

    // Сбрасывает папку и текст при запуске нового дня.
    private void OnDayInitialized(DayData dayData)
    {
        ResetViewForDay();
    }

    // Возвращает UI в состояние до клика.
    private void ResetViewForDay()
    {
        if (showTextCoroutine != null)
        {
            StopCoroutine(showTextCoroutine);
            showTextCoroutine = null;
        }

        if (folderAnimator != null)
        {
            ResetAnimationTriggers();
            folderAnimator.Rebind();
            folderAnimator.Update(0f);
        }

        if (targetImage != null)
        {
            targetImage.sprite = initialImageSprite;
        }

        if (targetSpriteRenderer != null)
        {
            targetSpriteRenderer.sprite = initialRendererSprite;
        }

        if (resultText != null)
        {
            resultText.text = string.Empty;

            if (hideResultTextOnStart)
            {
                resultText.gameObject.SetActive(false);
            }
        }
    }

    // Запускает показ текста результата.
    private void ShowResultText()
    {
        if (resultText == null)
        {
            return;
        }

        if (showTextCoroutine != null)
        {
            StopCoroutine(showTextCoroutine);
        }

        showTextCoroutine = StartCoroutine(ShowResultTextAfterDelay());
    }

    // Ждет нужную задержку и показывает текст.
    private IEnumerator ShowResultTextAfterDelay()
    {
        if (resultTextDelay > 0f)
        {
            yield return new WaitForSeconds(resultTextDelay);
        }

        resultText.text = GetResultText();
        resultText.gameObject.SetActive(true);
        showTextCoroutine = null;
    }

    // Возвращает спрайт для текущего дня или общий fallback-спрайт.
    private Sprite GetReplacementSprite()
    {
        DaySpriteReplaceResult dayResult = GetCurrentDayResult();

        if (dayResult != null && dayResult.replacementSprite != null)
        {
            return dayResult.replacementSprite;
        }

        return replacementSprite;
    }

    // Запускает анимацию папки для текущего дня.
    private void PlayFolderAnimation()
    {
        if (folderAnimator == null)
        {
            return;
        }

        string animationTrigger = GetAnimationTrigger();

        if (string.IsNullOrEmpty(animationTrigger))
        {
            return;
        }

        folderAnimator.SetTrigger(animationTrigger);
    }

    // Сбрасывает trigger-параметры, чтобы старая анимация не срабатывала на новом дне.
    private void ResetAnimationTriggers()
    {
        if (string.IsNullOrEmpty(fallbackAnimationTrigger) == false)
        {
            folderAnimator.ResetTrigger(fallbackAnimationTrigger);
        }

        for (int i = 0; i < dayResults.Count; i++)
        {
            DaySpriteReplaceResult dayResult = dayResults[i];

            if (dayResult != null && string.IsNullOrEmpty(dayResult.animationTrigger) == false)
            {
                folderAnimator.ResetTrigger(dayResult.animationTrigger);
            }
        }
    }

    // Возвращает trigger анимации для текущего дня или общий fallback-trigger.
    private string GetAnimationTrigger()
    {
        DaySpriteReplaceResult dayResult = GetCurrentDayResult();

        if (dayResult != null && string.IsNullOrEmpty(dayResult.animationTrigger) == false)
        {
            return dayResult.animationTrigger;
        }

        return fallbackAnimationTrigger;
    }

    // Возвращает захардкоженный текст для текущего дня.
    private string GetResultText()
    {
        DaySpriteReplaceResult dayResult = GetCurrentDayResult();

        if (dayResult != null && string.IsNullOrEmpty(dayResult.resultText) == false)
        {
            return dayResult.resultText;
        }

        string dayId = dayController != null && dayController.CurrentDay != null
            ? dayController.CurrentDay.dayId
            : string.Empty;

        switch (dayId)
        {
            case "day_01":
                return "Анна = Анна";
            case "day_02":
                return "Алексей = Алексей";
            case "day_03":
                return "Найдено: 3 / 3";
        }

        switch (fallbackDayNumber)
        {
            case 1:
                return "Анна = Анна";
            case 2:
                return "Алексей = Алексей";
            case 3:
                return "Найдено: 3 / 3";
            default:
                return string.Empty;
        }
    }

    // Ищет настройки результата для текущего дня.
    private DaySpriteReplaceResult GetCurrentDayResult()
    {
        string dayId = dayController != null && dayController.CurrentDay != null
            ? dayController.CurrentDay.dayId
            : string.Empty;

        if (string.IsNullOrEmpty(dayId))
        {
            return null;
        }

        for (int i = 0; i < dayResults.Count; i++)
        {
            DaySpriteReplaceResult dayResult = dayResults[i];

            if (dayResult != null && dayResult.dayId == dayId)
            {
                return dayResult;
            }
        }

        return null;
    }
}
