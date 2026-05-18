using UnityEngine;

public class GraphViewController : MonoBehaviour
{
    [SerializeField] private DayController _dayController;
    [SerializeField] private GameObject[] _daysViews;
    [SerializeField] private AlgorithmPalettePanel[] _panels;
    [SerializeField] private bool _runOnDayInitialized = true;
    [SerializeField] private bool _hideOtherViews = true;

    private void Awake()
    {
        if (_dayController == null)
        {
            _dayController = FindFirstObjectByType<DayController>();
        }
    }

    private void OnEnable()
    {
        if (_dayController == null)
        {
            _dayController = FindFirstObjectByType<DayController>();
        }

        if (_dayController != null)
        {
            _dayController.DayInitialized += OnDayInitialized;
        }

        if (_runOnDayInitialized)
        {
            RunCurrentDayView();
        }
    }

    private void OnDisable()
    {
        if (_dayController != null)
        {
            _dayController.DayInitialized -= OnDayInitialized;
        }
    }

    private void OnDayInitialized(DayData dayData)
    {
        if (_runOnDayInitialized)
        {
            RunView(GetDayIndex(dayData));
        }
    }

    public void RunCurrentDayView()
    {
        if (_dayController == null || _dayController.CurrentDay == null)
        {
            Debug.LogWarning("GraphViewController: текущий день еще не инициализирован.");
            return;
        }

        int dayIndex = GetDayIndex(_dayController.CurrentDay);

        if (dayIndex < 0)
        {
            Debug.LogError($"GraphViewController: не удалось получить индекс дня из id {_dayController.CurrentDay.dayId}.");
            return;
        }

        RunView(dayIndex);
    }

    public void RunView(int dayCount)
    {
        if (dayCount < 0 || dayCount >= _daysViews.Length || dayCount >= _panels.Length)
        {
            Debug.LogError($"GraphViewController: некорректный индекс дня {dayCount}.");
            return;
        }

        HideAllViews();
        ClearAllPanels();

        if (_daysViews[dayCount] == null || _panels[dayCount] == null)
        {
            Debug.LogError($"GraphViewController: не заполнен view или panel для индекса {dayCount}.");
            return;
        }

        _daysViews[dayCount].SetActive(true);
        _panels[dayCount].Build();
    }

    private void HideAllViews()
    {
        for (int i = 0; i < _daysViews.Length; i++)
        {
            if (_daysViews[i] != null)
            {
                _daysViews[i].SetActive(false);
            }
        }
    }

    private void ClearAllPanels()
    {
        for (int i = 0; i < _panels.Length; i++)
        {
            if (_panels[i] != null)
            {
                _panels[i].ClearPanel();
            }
        }
    }

    private int GetDayIndex(DayData dayData)
    {
        if (dayData == null || string.IsNullOrEmpty(dayData.dayId))
        {
            return -1;
        }

        string[] parts = dayData.dayId.Split('_');

        if (parts.Length == 0 || int.TryParse(parts[parts.Length - 1], out int dayNumber) == false)
        {
            return -1;
        }

        return dayNumber - 1;
    }
}
