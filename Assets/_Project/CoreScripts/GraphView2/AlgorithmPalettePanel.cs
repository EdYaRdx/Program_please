using ElementaryCase;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AlgorithmPalettePanel : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Transform buttonsRoot;
    [SerializeField] private AlgorithmPaletteButton buttonPrefab;
    [SerializeField] private AlgorithmSequencePanel sequencePanel;

    [Header("Error")]
    [SerializeField] private GameObject _errorPanel;
    [SerializeField] private Button _errorButton;

    [Header("Buttons order on left panel")]
    [SerializeField] private List<AlgorithmBlockData> blocks = new();

    private readonly List<AlgorithmPaletteButton> _spawnedButtons = new();

    public async void OnError()
    {
        _errorPanel.SetActive(true);
        await _errorButton.WaitClickAsync();
        _errorPanel.SetActive(false);
        Build();
    }

    public void Build()
    {
        sequencePanel.ClearAll();
        ClearSpawnedButtons();

        foreach (var block in blocks)
        {
            AlgorithmPaletteButton button = Instantiate(buttonPrefab, buttonsRoot);
            button.Setup(block, HandleButtonClicked);
            _spawnedButtons.Add(button);
        }
    }

    public void ResetPuzzle()
    {
        if (sequencePanel != null)
            sequencePanel.ClearAll();

        foreach (var button in _spawnedButtons)
        {
            if (button != null)
            button.Show();
        }
    }

    public void ClearPanel()
    {
        if (sequencePanel != null)
            sequencePanel.ClearAll();

        ClearSpawnedButtons();
    }

    private void HandleButtonClicked(AlgorithmPaletteButton buttonView, AlgorithmBlockData block)
    {
        if (sequencePanel == null)
            return;

        bool placed = sequencePanel.TryAddBlock(block);
        if (placed)
            buttonView.Hide();
    }

    private void ClearSpawnedButtons()
    {
        for (int i = _spawnedButtons.Count - 1; i >= 0; i--)
        {
            if (_spawnedButtons[i] != null)
                Destroy(_spawnedButtons[i].gameObject);
        }

        _spawnedButtons.Clear();
    }
}
