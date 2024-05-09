using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu(fileName = "Item Database", menuName = "LootForgePro/Item Database")]
internal class ItemDatabase : ScriptableObject
{
    public List<Item> items = new List<Item>();
    public void Save()
    {
        EditorUtility.SetDirty(this);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    // Load the item database from an asset file
    public static ItemDatabase Load()
    {
        ItemDatabase database = Resources.Load<ItemDatabase>("ItemDatabase");
        if (database == null)
        {
            // If the asset doesn't exist, create a new one
            database = CreateInstance<ItemDatabase>();
            AssetDatabase.CreateAsset(database, "Assets/Resources/ItemDatabase.asset");
            database.Save();
        }
        return database;
    }
}