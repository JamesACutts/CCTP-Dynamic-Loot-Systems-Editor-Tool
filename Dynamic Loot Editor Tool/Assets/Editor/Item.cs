using UnityEngine;

[System.Serializable]
[CreateAssetMenu(fileName = "New Item", menuName = "LootForgePro/Item")]
internal class Item : ScriptableObject
{
    // Item properties
    public int itemID = 1;
    public string itemName;
    public string itemType;
    public bool isUsable;
    public string itemPrefabPath;

    // Spawn-related properties
    public float spawnProbability;

    // Description and developer notes
    public string description;
    public string devNotes;
    public GameObject itemPrefab;
}
