using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Кнопка PCScreen для перехода на следующий день.
public class PcNextLevelButton : MonoBehaviour
{
    // Запускатель дней, который переключает текущий день.
    [SerializeField] private DayBootstrap dayBootstrap;

    // UI-кнопка перехода на следующий день.
    [SerializeField] private Button button;

    // Главный экран, который нужно показать после перехода.
    [SerializeField] private GameObject mainScreen;

    // Экран PCScreen, который нужно скрыть после перехода.
    [SerializeField] private GameObject pcScreen;

    // Контроллер диалога, который нужно открыть после перехода.
    [SerializeField] private DialoguePanelController dialoguePanelController;

    // Находит кнопку, если она не назначена вручную.
    private void Awake()
    {
        if (button == null)
        {
            button = GetComponent<Button>();
        }

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

    // Подписывает кнопку на переход.
    private void OnEnable()
    {
        if (button != null)
        {
            button.onClick.AddListener(LaunchNextDay);
        }
    }

    // Отписывает кнопку от перехода.
    private void OnDisable()
    {
        if (button != null)
        {
            button.onClick.RemoveListener(LaunchNextDay);
        }
    }

    // Запускает следующий день.
    public void LaunchNextDay()
    {
        if (dayBootstrap == null)
        {
            Debug.LogError("PcNextLevelButton: не задан DayBootstrap.");
            return;
        }

        if (pcScreen != null)
        {
            pcScreen.SetActive(false);
        }

        if (mainScreen != null)
        {
            mainScreen.SetActive(true);
        }

        dayBootstrap.LaunchNextDay();

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
