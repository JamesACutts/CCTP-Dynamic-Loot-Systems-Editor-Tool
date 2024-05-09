using System;
using UnityEditor;
using UnityEngine;
using static DatabaseTab;

public class LootForgePro : EditorWindow
{
    // Enumeration for different tabs in the editor window
    private enum Tab
    {
        ItemDatabase,
        SpawnConfiguration,
        Preview
    }

    // Current active tab
    private Tab currentTab = Tab.ItemDatabase;

    // MenuItem to show the LootForgePro window
    [MenuItem("Tools/LootForgePro")]
    public static void ShowWindow()
    {
        GetWindow<LootForgePro>("LootForgePro");
    }

    // Called when the window is enabled
    private void OnEnable()
    {
        // Load item database and spawn point database when the window is enabled
        itemDatabase = ItemDatabase.Load();
        SpawnConfigTab.LoadSpawnPointDatabase();
    }

    // Main GUI rendering function
    private void OnGUI()
    {
        // Draw tabs and add space
        DrawTabs();
        GUILayout.Space(10);

        // Display the content of the current tab
        switch (currentTab)
        {
            case Tab.ItemDatabase:
                DatabaseTab.Draw(this);
                break;
            case Tab.SpawnConfiguration:
                SpawnConfigTab.Draw(this);
                break;
            case Tab.Preview:
                PreviewTab.Draw(this);
                break;
        }
    }

    // Draw tabs at the top of the window
    private void DrawTabs()
    {
        GUILayout.BeginHorizontal();

        // Iterate through each tab and draw its button
        foreach (Tab tab in Enum.GetValues(typeof(Tab)))
        {
            DrawTabButton(tab.ToString(), tab);
        }

        GUILayout.EndHorizontal();
    }

    // Draw individual tab buttons
    private void DrawTabButton(string label, Tab tab)
    {
        // Highlight the currently selected tab button
        GUI.backgroundColor = tab == currentTab ? Color.gray : Color.white;

        // Draw the tab button, and change the current tab when clicked
        if (GUILayout.Button(label))
        {
            currentTab = tab;
        }

        // Reset background color to default
        GUI.backgroundColor = Color.white;
    }
}
