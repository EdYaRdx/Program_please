using UnityEngine;

[System.Serializable]
public class AlgorithmBlockData
{
    [SerializeField] private string id;
    [SerializeField] private Sprite sprite;

    public string Id => id;
    public Sprite Sprite => sprite;
}