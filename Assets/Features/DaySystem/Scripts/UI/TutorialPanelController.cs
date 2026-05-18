using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Показывает короткое обучение в начале первого дня.
public class TutorialPanelController : MonoBehaviour
{
    // Контроллер текущего дня.
    [SerializeField] private DayController dayController;

    // Панель обучения.
    [SerializeField] private GameObject tutorialPanel;

    // Картинка текущего экрана обучения.
    [SerializeField] private Image tutorialImage;

    // Текст текущего экрана обучения.
    [SerializeField] private TMP_Text tutorialText;

    // Кнопка перехода к следующему экрану.
    [SerializeField] private Button nextButton;

    // Кнопка закрытия обучения.
    [SerializeField] private Button closeButton;

    // День, на котором нужно показать обучение.
    [SerializeField] private string tutorialDayId = "day_01";

    // Спрайты экранов обучения.
    [SerializeField] private List<Sprite> tutorialSprites = new List<Sprite>();

    // Тексты экранов обучения.
    [SerializeField] private List<string> tutorialTexts = new List<string>();

    // Показывать обучение только один раз за запуск сцены.
    [SerializeField] private bool showOnlyOnce = true;

    private int currentPageIndex;
    private bool wasShown;
    private CanvasGroup tutorialCanvasGroup;

    // Автоматически находит ссылки, если они не заданы вручную.
    private void Awake()
    {
        if (dayController == null)
        {
            dayController = FindFirstObjectByType<DayController>();
        }

        if (tutorialPanel != null)
        {
            tutorialCanvasGroup = tutorialPanel.GetComponent<CanvasGroup>();

            if (tutorialCanvasGroup == null && tutorialPanel == gameObject)
            {
                tutorialCanvasGroup = tutorialPanel.AddComponent<CanvasGroup>();
            }

            SetPanelVisible(false);
        }
    }

    // Проверяет текущий день, если событие уже успело пройти раньше.
    private void Start()
    {
        TryOpenForCurrentDay();
    }

    // Подписывается на кнопки и смену дня.
    private void OnEnable()
    {
        if (dayController != null)
        {
            dayController.DayInitialized += OnDayInitialized;
        }

        if (nextButton != null)
        {
            nextButton.onClick.AddListener(ShowNextPage);
        }

        if (closeButton != null)
        {
            closeButton.onClick.AddListener(CloseTutorial);
        }
    }

    // Отписывается от кнопок и смены дня.
    private void OnDisable()
    {
        if (dayController != null)
        {
            dayController.DayInitialized -= OnDayInitialized;
        }

        if (nextButton != null)
        {
            nextButton.onClick.RemoveListener(ShowNextPage);
        }

        if (closeButton != null)
        {
            closeButton.onClick.RemoveListener(CloseTutorial);
        }
    }

    // Открывает обучение, если стартовал нужный день.
    private void OnDayInitialized(DayData dayData)
    {
        TryOpenForDay(dayData);
    }

    // Открывает обучение, если текущий день подходит.
    private void TryOpenForCurrentDay()
    {
        if (dayController == null)
        {
            return;
        }

        TryOpenForDay(dayController.CurrentDay);
    }

    // Открывает обучение, если переданный день подходит.
    private void TryOpenForDay(DayData dayData)
    {
        if (dayData == null || dayData.dayId != tutorialDayId)
        {
            return;
        }

        if (showOnlyOnce && wasShown)
        {
            return;
        }

        OpenTutorial();
    }

    // Открывает обучение с первой страницы.
    public void OpenTutorial()
    {
        if (tutorialPanel == null)
        {
            Debug.LogWarning("TutorialPanelController: не задан tutorialPanel.");
            return;
        }

        wasShown = true;
        currentPageIndex = 0;
        SetPanelVisible(true);
        UpdatePageView();
    }

    // Показывает следующий экран или закрывает обучение после последнего.
    public void ShowNextPage()
    {
        currentPageIndex++;

        if (currentPageIndex >= GetPagesCount())
        {
            CloseTutorial();
            return;
        }

        UpdatePageView();
    }

    // Закрывает обучение.
    public void CloseTutorial()
    {
        if (tutorialPanel != null)
        {
            SetPanelVisible(false);
        }
    }

    // Обновляет картинку и текст текущего экрана.
    private void UpdatePageView()
    {
        if (tutorialImage != null)
        {
            tutorialImage.sprite = currentPageIndex < tutorialSprites.Count ? tutorialSprites[currentPageIndex] : null;
        }

        if (tutorialText != null)
        {
            tutorialText.text = currentPageIndex < tutorialTexts.Count ? tutorialTexts[currentPageIndex] : string.Empty;
        }
    }

    // Возвращает количество экранов обучения.
    private int GetPagesCount()
    {
        return Mathf.Max(tutorialSprites.Count, tutorialTexts.Count);
    }

    // Показывает или скрывает панель без отключения самого контроллера.
    private void SetPanelVisible(bool visible)
    {
        if (tutorialPanel == null)
        {
            return;
        }

        if (tutorialCanvasGroup != null)
        {
            tutorialPanel.SetActive(true);
            tutorialCanvasGroup.alpha = visible ? 1f : 0f;
            tutorialCanvasGroup.interactable = visible;
            tutorialCanvasGroup.blocksRaycasts = visible;
            return;
        }

        tutorialPanel.SetActive(visible);
    }
}
