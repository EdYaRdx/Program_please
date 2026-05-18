using UnityEngine;

// Данные одной функции для PCScreen.
[CreateAssetMenu(fileName = "FunctionData", menuName = "Day System/Function Data")]
public class FunctionData : ScriptableObject
{
    [Header("Meta")]
    // Уникальный id функции для логики и кнопок.
    public string functionId;

    // Название функции, которое видит пользователь.
    public string displayName;

    // Короткое описание работы функции.
    [TextArea]
    public string description;

    // Иконка функции для панели PCScreen.
    public Sprite icon;
}
