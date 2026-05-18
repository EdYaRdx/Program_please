using System;
using UnityEngine;

// Контроллер текущего дня без прямой работы с UI.
public class DayController : MonoBehaviour
{
    // Текущий инициализированный день.
    public DayData CurrentDay { get; private set; }

    // Событие для подписчиков, которым нужны данные нового дня.
    public event Action<DayData> DayInitialized;

    // Инициализирует день и уведомляет подписчиков.
    public void Initialize(DayData dayData)
    {
        if (dayData == null)
        {
            Debug.LogError("DayController: нельзя инициализировать день, потому что DayData не задан.");
            return;
        }

        CurrentDay = dayData;
        DayInitialized?.Invoke(dayData);
    }
}
