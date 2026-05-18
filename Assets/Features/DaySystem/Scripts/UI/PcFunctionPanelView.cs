using System.Collections.Generic;
using UnityEngine;

// Показывает список доступных функций в PCScreen.
public class PcFunctionPanelView : MonoBehaviour
{
    // Контроллер, от которого приходит событие инициализации дня.
    [SerializeField] private DayController dayController;

    // Контейнер, внутрь которого создаются элементы функций.
    [SerializeField] private Transform contentRoot;

    // Префаб одного элемента функции.
    [SerializeField] private FunctionItemView itemPrefab;

    // Созданные элементы текущего списка функций.
    private readonly List<FunctionItemView> spawnedItems = new List<FunctionItemView>();

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

    // Пересобирает список функций текущего дня.
    private void OnDayInitialized(DayData dayData)
    {
        ClearItems();

        if (contentRoot == null || itemPrefab == null)
        {
            return;
        }

        if (dayData.availableFunctions == null)
        {
            return;
        }

        foreach (FunctionData functionData in dayData.availableFunctions)
        {
            FunctionItemView item = Instantiate(itemPrefab, contentRoot);
            item.Bind(functionData);
            spawnedItems.Add(item);
        }
    }

    // Удаляет старые элементы списка функций.
    private void ClearItems()
    {
        foreach (FunctionItemView item in spawnedItems)
        {
            if (item != null)
            {
                Destroy(item.gameObject);
            }
        }

        spawnedItems.Clear();
    }
}
