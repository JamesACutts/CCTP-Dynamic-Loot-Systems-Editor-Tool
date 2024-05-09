using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditorInternal;
using Random = UnityEngine.Random;
using UnityEngine;
using UnityEditor.Callbacks;

public static class DatabaseTab
{
    internal static ItemDatabase itemDatabase;
    internal static string itemFolderPath = "Assets/Resources/ItemAssets";
    internal static string[] itemTypes = { "Firearm", "Ammunition", "Armor", "Tactical Gear", "Attachment", "Medical Supply", "Consumable", "Quest Item", "Key", "Miscellaneous", "Electronic", "Currency" };
    internal static string searchFilter = "";
    internal static Vector2 scrollPosition;
    internal static Item newItem = null;


    // Initialize the item database on editor startup
    [InitializeOnLoadMethod]
    [Obsolete]
    private static void Initialize()
    {
        LoadItemDatabase();
        EditorApplication.quitting += SaveItemDatabase;
        EditorApplication.playmodeStateChanged += SaveItemDatabaseOnExitPlayMode;
    }
    public static void Draw(LootForgePro editorWindow)
    {
        GUILayout.BeginHorizontal();
        DrawItemList();
        DrawItemDetails();
        GUILayout.EndHorizontal();
    }
    private static void DrawItemList()
    {
        GUILayout.BeginVertical(GUILayout.Width(200));

        // Search bar
        GUILayout.BeginHorizontal();
        GUILayout.Label("Search", EditorStyles.boldLabel, GUILayout.Width(50));
        searchFilter = EditorGUILayout.TextField(searchFilter, EditorStyles.toolbarSearchField);
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();

        // List of items
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
        if (itemDatabase != null)
        {
            foreach (Item item in itemDatabase.items)
            {
                if (!string.IsNullOrEmpty(searchFilter) && !item.itemName.ToLower().Contains(searchFilter.ToLower()))
                    continue;

                DrawItemButton(item);
            }
        }
        else
        {
            GUILayout.Label("No items in the database.", EditorStyles.boldLabel);
        }

        EditorGUILayout.EndScrollView();
        GUILayout.EndVertical();
    }

    // Draw a button for an individual item in the list
    private static void DrawItemButton(Item item)
    {
        GUIStyle buttonStyle = new GUIStyle(GUI.skin.button);
        buttonStyle.alignment = TextAnchor.MiddleLeft;
        buttonStyle.fixedHeight = 30;
        buttonStyle.fontSize = 12;

        if (newItem != null && newItem.itemID == item.itemID)
        {
            GUI.backgroundColor = new Color(0.1725f, 0.3647f, 0.5294f, 1f);
        }
        else
        {
            GUI.backgroundColor = Color.white;
        }

        if (GUILayout.Button(item.itemName, buttonStyle))
        {
            newItem = ScriptableObject.CreateInstance<Item>(); // Create a new instance
            newItem.itemID = item.itemID;
            newItem.itemName = item.itemName;
            newItem.itemType = item.itemType;
            newItem.isUsable = item.isUsable;
            newItem.spawnProbability = item.spawnProbability;
            newItem.description = item.description;
            newItem.devNotes = item.devNotes;
            newItem.itemPrefab = item.itemPrefab;
        }

        GUI.backgroundColor = Color.white;
    }

    private static void DrawItemDetails()
    {
        GUILayout.BeginHorizontal();
        GUILayout.BeginVertical(GUILayout.Width(400));

        // New Item button
        GUILayout.BeginHorizontal();
        GUILayout.Label(new GUIContent("New Item", "Create a new item"), EditorStyles.boldLabel);
        if (GUILayout.Button(new GUIContent("New Item", "Create a new item"), GUILayout.MaxWidth(150)))
        {
            newItem = ScriptableObject.CreateInstance<Item>();
            newItem.itemID = GetNextUniqueID();
            newItem.itemType = itemTypes[0]; // Set default type
            GUI.FocusControl(null);
        }
        GUILayout.EndHorizontal();

        // Item details
        if (newItem != null)
        {
            // Existing item
            bool isExistingItem = itemDatabase != null && itemDatabase.items.Any(existingItem => existingItem.itemID == newItem.itemID);

            newItem.itemID = EditorGUILayout.IntField(new GUIContent("ID", "The unique identifier for the item"), newItem.itemID);
            newItem.itemName = EditorGUILayout.TextField(new GUIContent("Name", "The name of the item"), newItem.itemName);
            int selectedTypeIndex = Mathf.Clamp(Array.IndexOf(itemTypes, newItem.itemType), 0, itemTypes.Length - 1);
            selectedTypeIndex = EditorGUILayout.Popup(new GUIContent("Item Type", "The type of the item"), selectedTypeIndex, itemTypes);
            newItem.itemType = itemTypes[selectedTypeIndex];
            newItem.isUsable = EditorGUILayout.Toggle(new GUIContent("Usable", "Whether the item is usable"), newItem.isUsable);
            GUILayout.BeginHorizontal();
            GUILayout.EndHorizontal();

            newItem.spawnProbability = EditorGUILayout.Slider("Spawn Probability", newItem.spawnProbability, 0f, 1f);
            GUILayout.BeginHorizontal();
            newItem.itemPrefab = (GameObject)EditorGUILayout.ObjectField(new GUIContent("Prefab", "The prefab associated with the item"), newItem.itemPrefab, typeof(GameObject), false);
            GUILayout.EndHorizontal();
            GUILayout.Space(10);
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();

            // Update Item button
            if (isExistingItem && GUILayout.Button(new GUIContent("Update Item", "Update the current item in the database"), GUILayout.MaxWidth(150)) && IsValidNewItem(newItem))
            {
                GUI.FocusControl(null);
                UpdateItem(newItem);
                newItem = null;
            }

            // Create Item button
            if (!isExistingItem && GUILayout.Button(new GUIContent("Create Item", "Create the current item in the database"), GUILayout.MaxWidth(150)) && IsValidNewItem(newItem))
            {
                GUI.FocusControl(null);
                SaveItem(newItem);
                itemDatabase.items.Add(newItem);
                newItem = null;
            }

            // Remove Item button
            if (isExistingItem && GUILayout.Button(new GUIContent("Remove Item", "Remove the current item from the database"), GUILayout.MaxWidth(150)))
            {
                GUI.FocusControl(null);
                RemoveItem(newItem);
                newItem = null;
            }

            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
        }
        else
        {
            // Display placeholder text when no item is selected
            GUILayout.Label("No item selected.", EditorStyles.boldLabel);
        }

        GUILayout.EndVertical();


        // Description and Developer Notes
        GUILayout.BeginVertical(GUILayout.Width(400));
        if (newItem != null)
        {
            GUILayout.BeginVertical(EditorStyles.helpBox, GUILayout.MaxWidth(400));
            GUILayout.Label("Description", EditorStyles.boldLabel);
            newItem.description = EditorGUILayout.TextArea(newItem.description, GUILayout.Height(100));
            GUILayout.EndVertical();
            GUILayout.Space(10);

            GUILayout.BeginVertical(EditorStyles.helpBox, GUILayout.MaxWidth(400));
            GUILayout.Label("Dev Notes", EditorStyles.boldLabel);
            newItem.devNotes = EditorGUILayout.TextArea(newItem.devNotes, GUILayout.Height(100));
            GUILayout.EndVertical();
        }
        GUILayout.EndVertical();

        GUILayout.EndHorizontal();
    }

    // Save an item as a ScriptableObject asset
    private static void SaveItem(Item item)
    {
        if (!Directory.Exists(itemFolderPath))
        {
            Directory.CreateDirectory(itemFolderPath);
        }

        string assetPath = $"{itemFolderPath}/{item.itemName}.asset";
        AssetDatabase.CreateAsset(item, assetPath);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }
    // Update an existing item in the database
    private static void UpdateItem(Item updatedItem)
    {
        if (itemDatabase == null)
        {
            Debug.LogError("Item Database is not loaded.");
            return;
        }

        Item existingItem = itemDatabase.items.FirstOrDefault(item => item.itemID == updatedItem.itemID);
        if (existingItem != null)
        {
            existingItem.itemName = updatedItem.itemName;
            existingItem.itemType = updatedItem.itemType;
            existingItem.isUsable = updatedItem.isUsable;
            existingItem.spawnProbability = updatedItem.spawnProbability;
            existingItem.description = updatedItem.description;
            existingItem.devNotes = updatedItem.devNotes;
            existingItem.itemPrefab = updatedItem.itemPrefab;

            EditorUtility.SetDirty(existingItem);
            AssetDatabase.SaveAssets();
            Debug.Log("Item updated successfully.");
        }
        else
        {
            Debug.LogError("Item not found in the database.");
        }
    }

    // Remove an item from the database
    private static void RemoveItem(Item itemToRemove)
    {
        if (itemDatabase == null)
        {
            Debug.LogError("Item Database is not loaded.");
            return;
        }

        Item existingItem = itemDatabase.items.FirstOrDefault(item => item.itemID == itemToRemove.itemID);
        if (existingItem != null)
        {
            itemDatabase.items.Remove(existingItem);
            AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(existingItem));
            AssetDatabase.SaveAssets();
            Debug.Log("Item removed successfully.");
        }
        else
        {
            Debug.LogError("Item not found in the database.");
        }
    }

    // Load the item database
    public static void LoadItemDatabase()
    {
        itemDatabase = Resources.Load<ItemDatabase>("ItemDatabase");

        if (itemDatabase == null)
        {
            itemDatabase = ScriptableObject.CreateInstance<ItemDatabase>();
            string folderPath = "Assets/Resources";
            if (!AssetDatabase.IsValidFolder(folderPath))
            {
                AssetDatabase.CreateFolder("Assets", "Resources");
            }

            string assetPath = "Assets/Resources/ItemDatabase.asset";
            AssetDatabase.CreateAsset(itemDatabase, assetPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        LoadItemsFromFolder();
    }

    // Load items from the specified folder and add them to the item database
    private static void LoadItemsFromFolder()
    {
        itemDatabase.items.Clear();

        string[] itemFiles = Directory.GetFiles(itemFolderPath, "*.asset", SearchOption.AllDirectories);

        foreach (string itemFile in itemFiles)
        {
            Item item = AssetDatabase.LoadAssetAtPath<Item>(itemFile);
            if (item != null)
            {
                itemDatabase.items.Add(item);
            }
        }
    }

    // Save the item database
    private static void SaveItemDatabase()
    {
        if (itemDatabase != null)
        {
            itemDatabase.Save();
        }
    }

    // Save the item database when exiting play mode
    private static void SaveItemDatabaseOnExitPlayMode()
    {
        if (!EditorApplication.isPlaying && EditorApplication.isPlayingOrWillChangePlaymode)
        {
            SaveItemDatabase();
        }
    }

    // Get the next unique ID for a new item
    private static int GetNextUniqueID()
    {
        // Find the maximum ID in the existing items and add 1 to get the next unique ID
        int maxID = itemDatabase.items.Count > 0 ? itemDatabase.items.Max(item => item.itemID) : 0;
        return maxID + 1;
    }

    // Check if the new item is valid before adding or updating
    private static bool IsValidNewItem(Item item)
    {
        if (string.IsNullOrWhiteSpace(item.itemName))
        {
            EditorUtility.DisplayDialog("Error", "Item Name cannot be empty.", "OK");
            return false;
        }

        // Check if the name is already assigned to another existing item (excluding the item itself if it's being updated)
        if (itemDatabase.items.Where(existingItem => existingItem.itemID != item.itemID).Any(existingItem => existingItem.itemName == item.itemName))
        {
            EditorUtility.DisplayDialog("Error", "Item Name must be unique.", "OK");
            return false;
        }

        // Check if the ID is already assigned to another existing item (excluding the item itself if it's being updated)
        if (itemDatabase.items.Where(existingItem => existingItem.itemID != item.itemID).Any(existingItem => existingItem.itemID == item.itemID))
        {
            EditorUtility.DisplayDialog("Error", "Item ID must be unique.", "OK");
            return false;
        }

        return true;
    }

}
