using UnityEngine;
using UnityEngine.UI;

// Управляет панелью с изображением бумажного задания.
public class TaskPaperImageController : MonoBehaviour
{
    // Контроллер, из которого берется текущий день.
    [SerializeField] private DayController dayController;

    // Панель, на которой показывается бумажка с заданием.
    [SerializeField] private GameObject taskPaperPanel;

    // UI-изображение для спрайта задания.
    [SerializeField] private Image taskPaperImage;

    // Скрывает панель бумажки при старте сцены.
    private void Start()
    {
        if (taskPaperPanel != null)
        {
            taskPaperPanel.SetActive(false);
        }
    }

    // Открывает бумажку с изображением задания текущего дня.
    public void OpenTaskPaper()
    {
        if (dayController == null)
        {
            Debug.LogError("TaskPaperImageController: не задан DayController.");
            return;
        }

        if (dayController.CurrentDay == null)
        {
            Debug.LogWarning("TaskPaperImageController: текущий день еще не инициализирован.");
            return;
        }

        if (dayController.CurrentDay.taskPaperSprite == null)
        {
            Debug.LogWarning("TaskPaperImageController: у текущего дня не задано изображение бумажки.");
            return;
        }

        if (taskPaperImage != null)
        {
            taskPaperImage.sprite = dayController.CurrentDay.taskPaperSprite;
        }

        if (taskPaperPanel != null)
        {
            taskPaperPanel.SetActive(true);
        }
    }

    // Закрывает панель бумажки с заданием.
    public void CloseTaskPaper()
    {
        if (taskPaperPanel != null)
        {
            taskPaperPanel.SetActive(false);
        }
    }
}
