using System.Collections.Generic;
using TMPro;
using UnityEngine;

// Управляет стартовым диалогом Квагелана.
public class DialoguePanelController : MonoBehaviour
{
    // Контроллер, из которого берется текущий день.
    [SerializeField] private DayController dayController;

    // Панель с репликами Квагелана.
    [SerializeField] private GameObject dialoguePanel;

    // Текстовый элемент для текущей реплики.
    [SerializeField] private TMP_Text dialogueText;

    // Реплики текущего открытого диалога.
    private List<string> currentLines = new List<string>();

    // Индекс текущей показанной реплики.
    private int currentLineIndex;

    // Скрывает диалоговую панель до запуска дня.
    private void Awake()
    {
        if (dialoguePanel != null)
        {
            dialoguePanel.SetActive(false);
        }
    }

    // Подписывается на запуск нового дня.
    private void OnEnable()
    {
        if (dayController != null)
        {
            dayController.DayInitialized += OnDayInitialized;
        }
    }

    // Отписывается от запуска нового дня.
    private void OnDisable()
    {
        if (dayController != null)
        {
            dayController.DayInitialized -= OnDayInitialized;
        }
    }

    // Открывает диалог после инициализации дня.
    private void OnDayInitialized(DayData dayData)
    {
        OpenDialogue();
    }

    // Открывает диалог, если день уже был запущен до Start.
    private void Start()
    {
        if (dayController != null && dayController.CurrentDay != null)
        {
            OpenDialogue();
        }
    }

    // Открывает стартовый диалог текущего дня.
    public void OpenDialogue()
    {
        if (dayController == null)
        {
            Debug.LogError("DialoguePanelController: не задан DayController.");
            return;
        }

        if (dayController.CurrentDay == null)
        {
            Debug.LogWarning("DialoguePanelController: текущий день еще не инициализирован.");
            return;
        }

        if (dayController.CurrentDay.kvagelanLines == null || dayController.CurrentDay.kvagelanLines.Count == 0)
        {
            Debug.LogWarning("DialoguePanelController: у текущего дня нет стартовых реплик Квагелана.");
            return;
        }

        currentLines = new List<string>(dayController.CurrentDay.kvagelanLines);
        currentLineIndex = 0;

        UpdateDialogueView();

        if (dialoguePanel != null)
        {
            dialoguePanel.SetActive(true);
        }
    }

    // Показывает следующую реплику или закрывает диалог.
    public void ShowNextLine()
    {
        if (currentLines == null || currentLines.Count == 0)
        {
            return;
        }

        currentLineIndex++;

        if (currentLineIndex >= currentLines.Count)
        {
            CloseDialogue();
            return;
        }

        UpdateDialogueView();
    }

    // Закрывает диалог и очищает текущие реплики.
    public void CloseDialogue()
    {
        if (dialoguePanel != null)
        {
            dialoguePanel.SetActive(false);
        }

        currentLines = new List<string>();
        currentLineIndex = 0;
    }

    // Обновляет текст текущей реплики.
    private void UpdateDialogueView()
    {
        if (dialogueText == null)
        {
            return;
        }

        if (currentLines == null || currentLines.Count == 0)
        {
            dialogueText.text = "";
            return;
        }

        dialogueText.text = currentLines[currentLineIndex];
    }
}
