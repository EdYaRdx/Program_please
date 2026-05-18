using TMPro;
using UnityEngine;

// Показывает верхнюю строку задания в PCScreen.
public class PcTaskHeaderView : MonoBehaviour
{
    // Контроллер, от которого приходит событие инициализации дня.
    [SerializeField] private DayController dayController;

    // Текстовый элемент для одной строки задания.
    [SerializeField] private TMP_Text taskHeaderText;

    // Подписывается на событие запуска дня.
    private void OnEnable()
    {
        if (dayController != null)
        {
            dayController.DayInitialized += OnDayInitialized;
        }
    }

    // Отписывается от события запуска дня.
    private void OnDisable()
    {
        if (dayController != null)
        {
            dayController.DayInitialized -= OnDayInitialized;
        }
    }

    // Обновляет верхнюю строку задания.
    private void OnDayInitialized(DayData dayData)
    {
        if (taskHeaderText != null)
        {
            taskHeaderText.text = dayData.pcTaskHeaderText;
        }
    }
}
