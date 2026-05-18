using UnityEngine;
using UnityEngine.SceneManagement;

// Запускает стартовый день из базы по индексу при старте сцены.
public class DayBootstrap : MonoBehaviour
{
    // База с доступными днями прототипа.
    [SerializeField] private DayDatabase dayDatabase;

    // Контроллер, который инициализирует выбранный день.
    [SerializeField] private DayController dayController;

    // Индекс дня, который запускается первым.
    [SerializeField] private int startDayIndex;

    // Главный экран, который показывается после перехода на следующий день.
    [SerializeField] private GameObject mainScreen;

    // PCScreen, который скрывается после перехода на следующий день.
    [SerializeField] private GameObject pcScreen;

    // Диалоговая панель, которая открывается после запуска нового дня.
    [SerializeField] private DialoguePanelController dialoguePanelController;

    // Индекс текущего запущенного дня.
    private int currentDayIndex = -1;

    // Находит необязательные ссылки в сцене.
    private void Awake()
    {
        if (mainScreen == null)
        {
            mainScreen = FindSceneObjectByName("Mainscreen");
        }

        if (pcScreen == null)
        {
            pcScreen = FindSceneObjectByName("PCscreen");
        }

        if (dialoguePanelController == null)
        {
            dialoguePanelController = FindFirstObjectByType<DialoguePanelController>(FindObjectsInactive.Include);
        }
    }

    // Запускает стартовый день по индексу после загрузки сцены.
    private void Start()
    {
        if (dayDatabase == null || dayController == null)
        {
            Debug.LogError("DayBootstrap: не задана база дней или контроллер дня.");
            return;
        }

        LaunchByIndex(startDayIndex);
    }

    // Запускает день по его id.
    public void LaunchById(string dayId)
    {
        if (dayDatabase == null || dayController == null)
        {
            Debug.LogError("DayBootstrap: не задана база дней или контроллер дня.");
            return;
        }

        DayData dayData = dayDatabase.GetById(dayId);
        if (dayData == null)
        {
            Debug.LogError($"DayBootstrap: день с id '{dayId}' не найден.");
            return;
        }

        currentDayIndex = dayDatabase.days.IndexOf(dayData);
        dayController.Initialize(dayData);
    }

    // Запускает день по индексу в базе.
    public void LaunchByIndex(int index)
    {
        if (dayDatabase == null || dayController == null)
        {
            Debug.LogError("DayBootstrap: не задана база дней или контроллер дня.");
            return;
        }

        DayData dayData = dayDatabase.GetByIndex(index);
        if (dayData == null)
        {
            Debug.LogError($"DayBootstrap: день с индексом {index} не найден.");
            return;
        }

        currentDayIndex = index;
        dayController.Initialize(dayData);
    }

    // Запускает следующий день из базы.
    public void LaunchNextDay()
    {
        if (dayDatabase == null || dayController == null)
        {
            Debug.LogError("DayBootstrap: не задана база дней или контроллер дня.");
            return;
        }

        if (currentDayIndex < 0)
        {
            Debug.LogWarning("DayBootstrap: текущий день еще не запущен.");
            return;
        }

        int nextIndex = currentDayIndex + 1;
        DayData nextDay = dayDatabase.GetByIndex(nextIndex);

        if (nextDay == null)
        {
            Debug.LogWarning($"DayBootstrap: следующего дня с индексом {nextIndex} нет.");
            return;
        }

        LaunchByIndex(nextIndex);
        ReturnToMainScreen();
        OpenCurrentDayDialogue();
    }

    // Возвращает игрока на главный экран дня.
    private void ReturnToMainScreen()
    {
        if (pcScreen != null)
        {
            pcScreen.SetActive(false);
        }

        if (mainScreen != null)
        {
            mainScreen.SetActive(true);
        }
    }

    // Открывает диалог текущего дня.
    private void OpenCurrentDayDialogue()
    {
        if (dialoguePanelController != null)
        {
            dialoguePanelController.OpenDialogue();
        }
    }

    // Ищет объект в сцене, включая неактивные.
    private GameObject FindSceneObjectByName(string objectName)
    {
        GameObject[] rootObjects = SceneManager.GetActiveScene().GetRootGameObjects();

        foreach (GameObject rootObject in rootObjects)
        {
            Transform found = FindChildByName(rootObject.transform, objectName);
            if (found != null)
            {
                return found.gameObject;
            }
        }

        return null;
    }

    // Рекурсивно ищет дочерний объект по имени.
    private Transform FindChildByName(Transform parent, string objectName)
    {
        if (parent.name == objectName)
        {
            return parent;
        }

        foreach (Transform child in parent)
        {
            Transform found = FindChildByName(child, objectName);
            if (found != null)
            {
                return found;
            }
        }

        return null;
    }
}
