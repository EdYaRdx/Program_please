using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

// Режим работы дня для MVP-сценариев.
public enum DayMode
{
    // День показывает только вводную карточку.
    IntroCard,

    // День выполняет один простой поиск.
    SingleSearch,

    // День фильтрует данные по условиям.
    Filter
}

// Описание одного входного поля задания.
[Serializable]
public class DayInputFieldData
{
    // Технический id поля для связи с UI и логикой.
    public string id;

    // Название поля, которое видит пользователь.
    public string displayName;

    // Тип значения: text, number, bool или другой простой маркер.
    public string type;

    // Значение, которое подставляется по умолчанию.
    public string defaultValue;

    // Разрешает пользователю менять значение поля.
    public bool editable = true;

    // Подсказка внутри пустого поля ввода.
    public string placeholder;
}

// Описание одного выходного поля результата.
[Serializable]
public class DayOutputFieldData
{
    // Технический id результата для связи с UI.
    public string id;

    // Название результата, которое видит пользователь.
    public string displayName;

    // Тип результата: text, number, bool или другой простой маркер.
    public string type;

    // Значение результата до выполнения задания.
    public string defaultValue;

    // Разрешает редактировать результат вручную.
    public bool editable = false;
}

// Данные одного дня для data-driven прототипа.
[CreateAssetMenu(fileName = "DayData", menuName = "Day System/Day Data")]
public class DayData : ScriptableObject
{
    [Header("Meta")]
    // Уникальный id дня, например day_01.
    public string dayId;

    // Заголовок дня для экрана задания.
    public string dayTitle;

    // Режим работы дня в MVP.
    public DayMode dayMode;

    [Header("Dialogue")]
    // Список стартовых реплик Квагелана для текущего дня.
    public List<string> kvagelanLines = new List<string>();

    [Header("PC Screen")]
    // Основная строка задания в верхней части PCScreen.
    public string pcTaskHeaderText;

    // Поясняющий текст для нижнего блока PCScreen.
    [TextArea]
    public string pcExplanationText;

    // Сообщение при успешном выполнении.
    [TextArea]
    public string successText;

    // Сообщение при ошибке выполнения.
    [TextArea]
    public string errorText;

    [Header("Task Paper")]
    // Изображение задания для TaskPaperButton на главном экране.
    public Sprite taskPaperSprite;

    [Header("Notebook")]
    // Список разворотов или страниц notebook для текущего дня.
    public List<Sprite> notebookPages = new List<Sprite>();

    // Индекс страницы, с которой notebook открывается в текущем дне.
    public int notebookStartPageIndex;

    [Header("Media")]
    // Видеоролик текущего дня для KasetaButton.
    public VideoClip kasetaClip;

    [Header("Practice")]
    // Список доступных функций или блоков текущего дня.
    public List<FunctionData> availableFunctions = new List<FunctionData>();

    // Входные поля, которые пользователь заполняет или видит.
    public List<DayInputFieldData> inputFields = new List<DayInputFieldData>();

    // Выходные поля, куда выводится результат.
    public List<DayOutputFieldData> outputFields = new List<DayOutputFieldData>();

    // Данные для поиска, фильтрации или демонстрации.
    [TextArea]
    public string dataset;

    [Header("Execution")]
    // Показывает, что задание использует цикл.
    public bool usesLoop;

    // Подпись цикла для UI или пояснения.
    public string loopCaption;

    // Останавливает выполнение после первого совпадения.
    public bool stopOnFirstMatch;

    // Собирает все найденные совпадения.
    public bool collectAllMatches;
}
