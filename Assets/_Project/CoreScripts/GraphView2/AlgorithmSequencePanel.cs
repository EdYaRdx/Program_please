using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class AlgorithmSequencePanel : MonoBehaviour
{
    [Header("Slots in fill order")]
    [SerializeField] private List<AlgorithmTargetSlot> slots = new();

    [Header("Expected order by block id")]
    [SerializeField] private List<string> correctOrder = new();

    [Header("Behaviour")]
    [SerializeField] private bool rejectWrongStep = true;

    [Header("Events")]
    [SerializeField] private UnityEvent onWrongStep;
    [SerializeField] private UnityEvent onCompleted;

    private readonly List<AlgorithmBlockData> _placedBlocks = new();

    public bool TryAddBlock(AlgorithmBlockData block)
    {
        if (block == null)
            return false;

        AlgorithmTargetSlot nextSlot = GetNextFreeSlot();
        if (nextSlot == null)
            return false;

        if (rejectWrongStep && !IsNextBlockCorrect(block))
        {
            onWrongStep?.Invoke();
            return false;
        }

        nextSlot.Fill(block.Sprite);
        _placedBlocks.Add(block);

        if (!rejectWrongStep && !IsCurrentSequenceCorrect())
            onWrongStep?.Invoke();

        if (IsCompletedSuccessfully())
            onCompleted?.Invoke();

        return true;
    }

    public void ClearAll()
    {
        _placedBlocks.Clear();

        foreach (var slot in slots)
        {
            if (slot != null)
                slot.Clear();
        }
    }

    private AlgorithmTargetSlot GetNextFreeSlot()
    {
        foreach (var slot in slots)
        {
            if (slot != null && !slot.IsFilled)
                return slot;
        }

        return null;
    }

    private bool IsNextBlockCorrect(AlgorithmBlockData block)
    {
        if (correctOrder == null || correctOrder.Count == 0)
            return true;

        int nextIndex = _placedBlocks.Count;
        if (nextIndex >= correctOrder.Count)
            return false;

        return block.Id == correctOrder[nextIndex];
    }

    private bool IsCurrentSequenceCorrect()
    {
        if (correctOrder == null || correctOrder.Count == 0)
            return true;

        if (_placedBlocks.Count > correctOrder.Count)
            return false;

        for (int i = 0; i < _placedBlocks.Count; i++)
        {
            if (_placedBlocks[i].Id != correctOrder[i])
                return false;
        }

        return true;
    }

    private bool IsCompletedSuccessfully()
    {
        if (slots.Count == 0)
            return false;

        foreach (var slot in slots)
        {
            if (slot == null || !slot.IsFilled)
                return false;
        }

        if (correctOrder == null || correctOrder.Count == 0)
            return true;

        if (_placedBlocks.Count != correctOrder.Count)
            return false;

        for (int i = 0; i < _placedBlocks.Count; i++)
        {
            if (_placedBlocks[i].Id != correctOrder[i])
                return false;
        }

        return true;
    }
}