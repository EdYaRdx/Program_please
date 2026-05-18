using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// Управляет панелью notebook для текущего дня.
public class NotebookPanelController : MonoBehaviour
{
    // Контроллер, из которого берется текущий день.
    [SerializeField] private DayController dayController;

    // Панель notebook, которую нужно открыть или закрыть.
    [SerializeField] private GameObject noteBookPanel;

    // UI-изображение для текущей страницы notebook.
    [SerializeField] private Image pageImage;

    // Страницы notebook текущего открытого дня.
    private List<Sprite> currentPages;

    // Индекс текущей показанной страницы.
    private int currentPageIndex;

    // Скрывает notebook при старте сцены.
    private void Start()
    {
        if (noteBookPanel != null)
        {
            noteBookPanel.SetActive(false);
        }
    }

    // Открывает notebook текущего дня на стартовой странице.
    public void OpenNotebook()
    {
        if (dayController == null)
        {
            Debug.LogError("NotebookPanelController: не задан DayController.");
            return;
        }

        if (dayController.CurrentDay == null)
        {
            Debug.LogWarning("NotebookPanelController: текущий день еще не инициализирован.");
            return;
        }

        if (dayController.CurrentDay.notebookPages == null || dayController.CurrentDay.notebookPages.Count == 0)
        {
            Debug.LogWarning("NotebookPanelController: у текущего дня нет страниц notebook.");
            return;
        }

        currentPages = dayController.CurrentDay.notebookPages;
        currentPageIndex = Mathf.Clamp(dayController.CurrentDay.notebookStartPageIndex, 0, currentPages.Count - 1);

        UpdatePageView();

        if (noteBookPanel != null)
        {
            noteBookPanel.SetActive(true);
        }
    }

    // Закрывает notebook.
    public void CloseNotebook()
    {
        if (noteBookPanel != null)
        {
            noteBookPanel.SetActive(false);
        }
    }

    // Показывает следующую страницу notebook.
    public void ShowNextPage()
    {
        if (currentPages == null || currentPages.Count == 0)
        {
            return;
        }

        if (currentPageIndex < currentPages.Count - 1)
        {
            currentPageIndex++;
        }

        UpdatePageView();
    }

    // Показывает предыдущую страницу notebook.
    public void ShowPrevPage()
    {
        if (currentPages == null || currentPages.Count == 0)
        {
            return;
        }

        if (currentPageIndex > 0)
        {
            currentPageIndex--;
        }

        UpdatePageView();
    }

    // Обновляет изображение текущей страницы.
    private void UpdatePageView()
    {
        if (pageImage == null)
        {
            return;
        }

        if (currentPages == null || currentPages.Count == 0)
        {
            pageImage.sprite = null;
            return;
        }

        pageImage.sprite = currentPages[currentPageIndex];
    }
}
