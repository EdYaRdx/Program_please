using System.Collections.Generic;
using UnityEngine;

// Простой реестр дней для прототипа.
[CreateAssetMenu(fileName = "DayDatabase", menuName = "Day System/Day Database")]
public class DayDatabase : ScriptableObject
{
    // Список всех дней, доступных в прототипе.
    public List<DayData> days = new List<DayData>();

    // Возвращает день по его dayId.
    public DayData GetById(string id)
    {
        if (string.IsNullOrEmpty(id))
        {
            return null;
        }

        foreach (DayData day in days)
        {
            if (day != null && day.dayId == id)
            {
                return day;
            }
        }

        return null;
    }

    // Возвращает день по индексу в списке.
    public DayData GetByIndex(int index)
    {
        if (index < 0 || index >= days.Count)
        {
            return null;
        }

        return days[index];
    }
}
