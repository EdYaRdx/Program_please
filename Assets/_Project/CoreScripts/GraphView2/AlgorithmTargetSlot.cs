using UnityEngine;
using UnityEngine.UI;

public class AlgorithmTargetSlot : MonoBehaviour
{
    [SerializeField] private GameObject emptyState;
    [SerializeField] private GameObject filledState;
    [SerializeField] private Image filledIcon;

    public bool IsFilled { get; private set; }

    public void Fill(Sprite sprite)
    {
        IsFilled = true;

        if (emptyState != null)
            emptyState.SetActive(false);

        if (filledState != null)
            filledState.SetActive(true);

        if (filledIcon != null)
            filledIcon.sprite = sprite;
    }

    public void Clear()
    {
        IsFilled = false;

        if (emptyState != null)
            emptyState.SetActive(true);

        if (filledState != null)
            filledState.SetActive(false);

        if (filledIcon != null)
            filledIcon.sprite = null;
    }
}