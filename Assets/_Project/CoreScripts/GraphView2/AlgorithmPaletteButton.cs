using System;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class AlgorithmPaletteButton : MonoBehaviour
{
    [SerializeField] private Button button;
    [SerializeField] private Image icon;

    private AlgorithmBlockData _data;
    private Action<AlgorithmPaletteButton, AlgorithmBlockData> _onClick;

    private void Awake()
    {
        if (button == null)
            button = GetComponent<Button>();
    }

    public void Setup(AlgorithmBlockData data, Action<AlgorithmPaletteButton, AlgorithmBlockData> onClick)
    {
        _data = data;
        _onClick = onClick;

        if (icon != null)
            icon.sprite = data.Sprite;

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(HandleClick);

        gameObject.SetActive(true);
    }

    private void HandleClick()
    {
        _onClick?.Invoke(this, _data);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }

    public void Show()
    {
        gameObject.SetActive(true);
    }
}