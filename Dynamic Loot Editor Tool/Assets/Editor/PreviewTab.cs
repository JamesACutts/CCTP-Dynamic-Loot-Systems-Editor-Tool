using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
using System;
using Random = UnityEngine.Random;
using Object = UnityEngine.Object;

public static class PreviewTab
{
    static PreviewTab()
    {
        EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
    }

    private static void OnPlayModeStateChanged(PlayModeStateChange state)
    {
        if (state == PlayModeStateChange.EnteredPlayMode)
        {
            SpawnLootAtAllSpawnPoints();
        }
    }

    // Data for visualization
    private static Dictionary<string, float> lootDistribution = new Dictionary<string, float>();
    private static Dictionary<string, int> itemStatistics = new Dictionary<string, int>();

    public static void Draw(LootForgePro editorWindow)
    {
        GUILayout.Label("Preview Tab Content", EditorStyles.boldLabel);
        GUILayout.Space(10);

        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();

        // Spawn Loot button in the Preview tab
        if (GUILayout.Button(new GUIContent("Spawn Loot", "Spawn loot based on item probability at all spawn points"), GUILayout.MaxWidth(150)))
        {
            SpawnLootAtAllSpawnPoints();
        }

        // Spawn Loot button in the Preview tab
        if (GUILayout.Button(new GUIContent("Clear Loot", ""), GUILayout.MaxWidth(150)))
        {
            ResetData();
            SpawnPointScript[] allSpawnPoints = GameObject.FindObjectsOfType<SpawnPointScript>();
            foreach (var spawnPoint in allSpawnPoints)
            {
                DeleteExistingLoot(spawnPoint);
            }
        }

        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();

        // Data visualization section
        GUILayout.Label("Data Visualization", EditorStyles.boldLabel);
        GUILayout.BeginVertical(EditorStyles.helpBox);

        // Loot Distribution Chart
        GUILayout.Label("Loot Distribution Chart", EditorStyles.boldLabel);
        DrawLootDistributionChart();

        GUILayout.Space(10);

        // Item Statistics Table
        GUILayout.Label("Item Statistics Table", EditorStyles.boldLabel);
        DrawItemStatisticsTable();

        GUILayout.EndVertical();
    }

    private static void SpawnLootAtAllSpawnPoints()
    {
        // Reset data before spawning loot
        ResetData();

        // Find all spawn point objects in the scene
        SpawnPointScript[] allSpawnPoints = GameObject.FindObjectsOfType<SpawnPointScript>();

        foreach (var spawnPoint in allSpawnPoints)
        {
            DeleteExistingLoot(spawnPoint);

            // If the spawn point is not empty, determine the loot based on item probability
            List<Item> lootToSpawn = DetermineLootToSpawn(spawnPoint);

            // Spawn loot as children of the current spawn point
            SpawnLoot(spawnPoint, lootToSpawn);
        }

        // Update data visualization after spawning loot
        UpdateDataVisualizations();
    }

    private static List<Item> DetermineLootToSpawn(SpawnPointScript spawnPoint)
    {
        // Filter items based on probability and type
        List<Item> validItems = DatabaseTab.itemDatabase.items
            .Where(item => item.spawnProbability > Random.value)
            .Where(item => spawnPoint.validItemTypes.Contains(item.itemType))
            .ToList();

        return validItems;
    }

    private static void SpawnLoot(SpawnPointScript spawnPoint, List<Item> lootToSpawn)
    {
        // Check if there's any loot to spawn
        if (lootToSpawn.Count > 0)
        {
            // Calculate the total spawn probability of all available items
            float totalSpawnProbability = lootToSpawn.Sum(item => item.spawnProbability);

            // Generate a random value between 0 and the total spawn probability
            float randomValue = Random.Range(0f, totalSpawnProbability);

            // Iterate over the available items to find the one to spawn
            float cumulativeProbability = 0f;
            foreach (var item in lootToSpawn)
            {
                cumulativeProbability += item.spawnProbability;
                if (randomValue <= cumulativeProbability)
                {
                    // Instantiate the selected item's prefab
                    GameObject spawnedLoot = Object.Instantiate(item.itemPrefab);

                    spawnedLoot.transform.position = spawnPoint._spawnPointPos;

                    // Set the spawned loot as a child of the spawn point
                    spawnedLoot.transform.parent = spawnPoint.transform;

                    // Update item statistics
                    if (itemStatistics.ContainsKey(item.itemType))
                    {
                        itemStatistics[item.itemType]++;
                    }
                    else
                    {
                        itemStatistics[item.itemType] = 1;
                    }

                    // Exit the loop since we found the item to spawn
                    break;
                }
            }
        }
    }

    private static void DeleteExistingLoot(SpawnPointScript spawnPoint)
    {
        // Find all child objects of the spawn point and destroy them
        foreach (Transform child in spawnPoint.transform)
        {
            GameObject.DestroyImmediate(child.gameObject);
        }
    }

    private static void UpdateDataVisualizations()
    {
        // Calculate loot distribution
        lootDistribution.Clear();
        float totalSpawnedItems = itemStatistics.Values.Sum();
        foreach (var kvp in itemStatistics)
        {
            lootDistribution[kvp.Key] = kvp.Value / totalSpawnedItems;
        }
    }

    private static void DrawLootDistributionChart()
    {
        // Draw a bar chart or pie chart based on the loot distribution data
        foreach (var kvp in lootDistribution)
        {
            GUILayout.Label($"{kvp.Key}: {kvp.Value.ToString("P")}");
        }
    }

    private static void DrawItemStatisticsTable()
    {
        // Draw a table summarizing item statistics
        GUILayout.BeginHorizontal();
        GUILayout.Label("Item");
        GUILayout.Label("Quantity");
        GUILayout.EndHorizontal();

        int rowIndex = 0;
        foreach (var kvp in itemStatistics)
        {
            GUIStyle rowStyle = (rowIndex % 2 == 0) ? EditorStyles.helpBox : GUIStyle.none;
            GUILayout.BeginHorizontal(rowStyle);
            GUILayout.Label(kvp.Key);
            GUILayout.Label(kvp.Value.ToString());
            GUILayout.EndHorizontal();
            rowIndex++;
        }
    }

    private static void ResetData()
    {
        // Reset item statistics and loot distribution
        itemStatistics.Clear();
        lootDistribution.Clear();
    }
}
