using UnityEngine;
using UnityEngine.UI;

// Показывает один элемент функции в панели PCScreen.
public class FunctionItemView : MonoBehaviour
{
    // Изображение кнопки функции.
    [SerializeField] private Image iconImage;

    // Заполняет элемент данными функции.
    public void Bind(FunctionData data)
    {
        if (data == null)
        {
            return;
        }

        if (iconImage != null)
        {
            iconImage.sprite = data.icon;
        }
    }
}
