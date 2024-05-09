using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

public static class SpawnConfigTab
{

    // Spawn point database list
    internal static List<SpawnPoint> spawnPointDatabase = new List<SpawnPoint>();

    // Types of items for dropdown selection
    internal static string[] validItemTypes;

    internal static Vector2 spawnPointScrollPosition;

    // Currently edited or added spawn point
    internal static SpawnPoint newSpawnPoint = new SpawnPoint();
    static int numberOfCombos = 10;
    static Vector3 areaSize = Vector3.one * 10f;
    static float radius = 10f;
    static Vector3 centerPosition = Vector3.zero;

    // Serialized class representing a spawn point in the database
    [System.Serializable]
    public class SpawnPoint
    {
        public int spawnID = 1;
        public string spawnPointName;
        public List<string> validItemTypes = new List<string>();
        public string description;
        public string devNotes;
    }


    public static void Draw(LootForgePro editorWindow)
    {
        validItemTypes = GetValidItemTypes();

        GUILayout.BeginHorizontal();
        GUILayout.BeginVertical(GUILayout.Width(200));
        // List of spawn points
        GUILayout.Label("Spawn Point List", EditorStyles.boldLabel);
        spawnPointScrollPosition = GUILayout.BeginScrollView(spawnPointScrollPosition);

        foreach (SpawnPoint spawnPoint in spawnPointDatabase)
        {
            DrawSpawnPointButton(spawnPoint);
        }

        GUILayout.EndScrollView();
        GUILayout.EndVertical();

        // Spawn point details
        GUILayout.BeginVertical();

        if (spawnPointDatabase.Count > 0 || newSpawnPoint != null)
        {
            // Display details of the selected spawn point
            DrawSpawnPointDetails();
        }

        GUILayout.EndVertical();
        GUILayout.EndHorizontal();
    }

    private static string[] GetValidItemTypes()
    {
        if (DatabaseTab.itemDatabase == null)
        {
            // If itemDatabase is null, return an empty array or handle it accordingly
            return new string[0];
        }

        return DatabaseTab.itemTypes;
    }


    // Draw a button for an individual spawn point in the list
    private static void DrawSpawnPointButton(SpawnPoint spawnPoint)
    {
        GUIStyle buttonStyle = new GUIStyle(GUI.skin.button);
        buttonStyle.alignment = TextAnchor.MiddleLeft;
        buttonStyle.fixedHeight = 30;
        buttonStyle.fontSize = 12;

        if (newSpawnPoint != null && newSpawnPoint.spawnPointName == spawnPoint.spawnPointName)
        {
            GUI.backgroundColor = new Color(0.1725f, 0.3647f, 0.5294f, 1f);
        }
        else
        {
            GUI.backgroundColor = Color.white;
        }

        if (GUILayout.Button(spawnPoint.spawnPointName, buttonStyle))
        {
            newSpawnPoint = new SpawnPoint
            {
                spawnID = spawnPoint.spawnID,
                spawnPointName = spawnPoint.spawnPointName,
                validItemTypes = spawnPoint.validItemTypes,
                devNotes = spawnPoint.devNotes,
                description = spawnPoint.description,
            };
        }

        GUI.backgroundColor = Color.white;
    }

    // Draw the details of the selected or newly added spawn point
    private static void DrawSpawnPointDetails()
    {
        EditorGUI.BeginChangeCheck();
        GUILayout.BeginHorizontal();
        GUILayout.BeginVertical(GUILayout.Width(400));

        // Spawn point details
        GUILayout.BeginHorizontal();
        GUILayout.Label(new GUIContent("Spawn Point", "Spawn point properties"), EditorStyles.boldLabel);
        // New Spawn Point button
        if (GUILayout.Button(new GUIContent("New Point", "Create a new spawn point"), GUILayout.MaxWidth(150)))
        {
            newSpawnPoint = new SpawnPoint();
            newSpawnPoint.spawnID = GetNextUniqueSpawnID();
            GUI.FocusControl(null);
        }

        GUILayout.EndHorizontal();

        newSpawnPoint.spawnPointName = EditorGUILayout.TextField(new GUIContent("Spawn Point Name", "The name of the spawn point"), newSpawnPoint.spawnPointName);

        // Display valid item types with checkboxes
        GUILayout.Label("Valid Item Types", EditorStyles.boldLabel);
        foreach (string itemType in validItemTypes)
        {
            bool isChecked = newSpawnPoint.validItemTypes.Contains(itemType);
            bool newChecked = EditorGUILayout.Toggle(itemType, isChecked);
            if (newChecked != isChecked)
            {
                if (newChecked)
                    newSpawnPoint.validItemTypes.Add(itemType);
                else
                    newSpawnPoint.validItemTypes.Remove(itemType);
            }
        }

        GUILayout.Space(10);
        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();

        // Create Spawn Point button
        if (GUILayout.Button(new GUIContent("Create Point", "Create the current spawn point in the database"), GUILayout.MaxWidth(150)) && IsValidNewSpawnPoint(newSpawnPoint))
        {
            GUI.FocusControl(null);
            spawnPointDatabase.Add(newSpawnPoint);
            SaveSpawnPointDatabase();
            newSpawnPoint = new SpawnPoint();
        }
        if (GUILayout.Button(new GUIContent("Update Point", "Update the selected spawn point in the database"), GUILayout.MaxWidth(150)) && IsValidNewSpawnPoint(newSpawnPoint))
        {
            GUI.FocusControl(null);
            UpdateSelectedSpawnPoint();
        }
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();
        GUILayout.Space(10);
        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();

        if (GUILayout.Button(new GUIContent("Place Point", "Create an empty GameObject in the scene"), GUILayout.MaxWidth(150)))
        {
            AddPlaceSpawn();
            GUI.FocusControl(null);
        }
        // Remove Spawn Point button
        if (GUILayout.Button(new GUIContent("Remove Point", "Remove the selected spawn point from the database"), GUILayout.MaxWidth(150)))
        {
            RemoveSelectedSpawnPoint();
        }

        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();
        GUILayout.EndVertical();

        GUILayout.BeginVertical(GUILayout.Width(400));
        // Description and Developer Notes
        GUILayout.BeginVertical(EditorStyles.helpBox, GUILayout.MaxWidth(400));
        GUILayout.Label("Description", EditorStyles.boldLabel);
        newSpawnPoint.description = EditorGUILayout.TextArea(newSpawnPoint.description, GUILayout.Height(100));
        GUILayout.EndVertical();
        GUILayout.Space(10);

        GUILayout.BeginVertical(EditorStyles.helpBox, GUILayout.MaxWidth(400));
        GUILayout.Label("Dev Notes", EditorStyles.boldLabel);
        newSpawnPoint.devNotes = EditorGUILayout.TextArea(newSpawnPoint.devNotes, GUILayout.Height(100));
        GUILayout.EndVertical();
        GUILayout.Space(10);

        GUILayout.BeginVertical(EditorStyles.helpBox, GUILayout.MaxWidth(400));

        GUILayout.Label("Mass Spawn Point Creation", EditorStyles.boldLabel);

        // Label and input field for Number of Points
        GUILayout.BeginHorizontal();
        GUILayout.Label("Number of Points:");
        numberOfCombos = EditorGUILayout.IntField(numberOfCombos, GUILayout.MaxWidth(120));
        GUILayout.EndHorizontal();

        // Label and input field for Area Size
        GUILayout.BeginHorizontal();
        GUILayout.Label("Radius Size:");
        radius = EditorGUILayout.FloatField(radius, GUILayout.MaxWidth(120));
        GUILayout.EndHorizontal();

        // Label and input field for Center Position
        GUILayout.BeginHorizontal();
        GUILayout.Label("Center Position:");
        centerPosition = EditorGUILayout.Vector3Field("", centerPosition, GUILayout.MaxWidth(200));
        GUILayout.EndHorizontal();

        GUILayout.Space(10);

        // Button to create mass spawn point combos
        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        if (GUILayout.Button(new GUIContent("Create Point Combos", "Create multiple spawn point combos randomly within the specified area")))
        {
            string[] itemTypes = SpawnConfigTab.validItemTypes;
            CreateSpawnCombos(numberOfCombos, itemTypes, centerPosition, areaSize);
        }

        // Button to place spawn points in the scene
        if (GUILayout.Button(new GUIContent("Scatter Spawn Points", "Create empty GameObjects for each spawn point and place them in the scene")))
        {
            MassPlace();
        }
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();

        GUILayout.EndVertical();


        GUILayout.EndVertical();

        GUILayout.EndHorizontal();
    }


    // Save the spawn point database to a JSON file
    private static void SaveSpawnPointDatabase()
    {
        try
        {
            string folderPath = Path.Combine(Application.dataPath, "SpawnPointDatabase");
            if (!Directory.Exists(folderPath))
            {
                AssetDatabase.CreateFolder("Assets", "SpawnPointDatabase");
            }

            string json = JsonUtility.ToJson(new SpawnPointDatabaseWrapper(spawnPointDatabase));
            string spawnPointDatabasePath = Path.Combine(folderPath, "SpawnPointDatabase.json");
            File.WriteAllText(spawnPointDatabasePath, json);
            AssetDatabase.Refresh();
        }
        catch (Exception e)
        {
            Debug.LogError($"An error occurred while saving the spawn point database: {e.Message}\n{e.StackTrace}");
            EditorUtility.DisplayDialog("Error", $"Failed to save the spawn point database. See console for details.", "OK");
        }
    }

    // Load the spawn point database from a JSON file
    public static void LoadSpawnPointDatabase()
    {
        try
        {
            string folderPath = Path.Combine(Application.dataPath, "SpawnPointDatabase");
            if (!Directory.Exists(folderPath))
            {
                return;
            }

            string spawnPointDatabasePath = Path.Combine(folderPath, "SpawnPointDatabase.json");
            if (File.Exists(spawnPointDatabasePath))
            {
                string json = File.ReadAllText(spawnPointDatabasePath);
                SpawnPointDatabaseWrapper wrapper = JsonUtility.FromJson<SpawnPointDatabaseWrapper>(json);

                // Clear existing spawnPointDatabase before loading new data
                spawnPointDatabase.Clear();

                foreach (SpawnPoint spawnPoint in wrapper.spawnPoints)
                {
                    spawnPointDatabase.Add(spawnPoint);
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"An error occurred while loading the spawn point database: {e.Message}\n{e.StackTrace}");
            EditorUtility.DisplayDialog("Error", $"Failed to load the spawn point database. See console for details.", "OK");
        }
    }

    // Wrapper class for serializing the spawn point database
    [Serializable]
    private class SpawnPointDatabaseWrapper
    {
        public List<SpawnPoint> spawnPoints;

        public SpawnPointDatabaseWrapper(List<SpawnPoint> spawnPoints)
        {
            this.spawnPoints = spawnPoints;
        }
    }

    // Check if the new spawn point is valid before adding
    private static bool IsValidNewSpawnPoint(SpawnPoint spawnPoint)
    {
        if (string.IsNullOrWhiteSpace(spawnPoint.spawnPointName))
        {
            EditorUtility.DisplayDialog("Error", "Spawn Point Name cannot be empty.", "OK");
            return false;
        }

        return true;
    }
    private static void UpdateSelectedSpawnPoint()
    {
        int selectedIndex = spawnPointDatabase.FindIndex(existingSpawnPoint => existingSpawnPoint.spawnPointName == newSpawnPoint.spawnPointName);

        if (selectedIndex != -1)
        {
            // Find the GameObject with the matching spawn point ID
            SpawnPointScript[] spawnPointScripts = GameObject.FindObjectsOfType<SpawnPointScript>();
            SpawnPointScript spawnPointScriptToUpdate = System.Array.Find(spawnPointScripts, script => script.spawnPointID == newSpawnPoint.spawnID);

            // If the GameObject is found, update its properties
            if (spawnPointScriptToUpdate != null)
            {
                spawnPointScriptToUpdate.spawnPointName = newSpawnPoint.spawnPointName;
            }

            List<string> validItemTypes = new List<string>(newSpawnPoint.validItemTypes);

            spawnPointDatabase[selectedIndex] = new SpawnPoint
            {
                spawnID = newSpawnPoint.spawnID,
                spawnPointName = newSpawnPoint.spawnPointName,
                validItemTypes = newSpawnPoint.validItemTypes,
                devNotes = newSpawnPoint.devNotes,
                description = newSpawnPoint.description,
            };

            SaveSpawnPointDatabase();
            newSpawnPoint = new SpawnPoint();
        }
        else
        {
            EditorUtility.DisplayDialog("Error", "Failed to update spawn point. Please try again.", "OK");
        }
    }
    // Remove the selected spawn point from the database
    private static void RemoveSelectedSpawnPoint()
    {
        int selectedIndex = spawnPointDatabase.FindIndex(existingSpawnPoint => existingSpawnPoint.spawnPointName == newSpawnPoint.spawnPointName);

        if (selectedIndex != -1)
        {
            bool confirmed = EditorUtility.DisplayDialog("Confirm Deletion", $"Are you sure you want to remove the spawn point '{newSpawnPoint.spawnPointName}' from the database?", "Yes", "No");

            if (confirmed)
            {
                // Find the GameObject with the matching spawn point ID
                SpawnPointScript[] spawnPointScripts = GameObject.FindObjectsOfType<SpawnPointScript>();
                SpawnPointScript spawnPointScriptToRemove = System.Array.Find(spawnPointScripts, script => script.spawnPointID == newSpawnPoint.spawnID);

                // If the GameObject is found, destroy it
                if (spawnPointScriptToRemove != null)
                {
                    GameObject.DestroyImmediate(spawnPointScriptToRemove.gameObject);
                }

                // Remove the spawn point from the database
                spawnPointDatabase.RemoveAt(selectedIndex);

                // Save the updated database
                SaveSpawnPointDatabase();

                // Reset the newSpawnPoint variable
                newSpawnPoint = new SpawnPoint();
            }
        }
        else
        {
            EditorUtility.DisplayDialog("Error", "Failed to remove spawn point. Please try again.", "OK");
        }
    }
    private static int GetNextUniqueSpawnID()
    {
        // Find the maximum spawn ID in the existing spawn points and add 1 to get the next unique ID
        int maxSpawnID = spawnPointDatabase.Count > 0 ? spawnPointDatabase.Max(spawnPoint => spawnPoint.spawnID) : 0;
        return maxSpawnID + 1;
    }

    private static void AddPlaceSpawn()
    {
        if (newSpawnPoint != null)
        {
            // Create an empty GameObject
            GameObject placeSpawnObject = new GameObject(newSpawnPoint.spawnPointName);

            placeSpawnObject.transform.position = Vector3.zero;

            // Attach a script or component to the GameObject to store the spawn point data
            SpawnPointScript spawnPointScript = placeSpawnObject.AddComponent<SpawnPointScript>();

            // Set the properties of the SpawnPointScript component based on the selected spawn point
            spawnPointScript.spawnPointID = newSpawnPoint.spawnID;
            spawnPointScript.spawnPointName = newSpawnPoint.spawnPointName;
            spawnPointScript.validItemTypes = new List<string>(newSpawnPoint.validItemTypes);

            Selection.activeGameObject = placeSpawnObject;

            Debug.Log($"Added Place Spawn: {newSpawnPoint.spawnPointName}");
        }
        else
        {
            Debug.LogWarning("No spawn point selected to add as a place spawn.");
        }
    }

    private static void CreateSpawnCombos(int numberOfCombos, string[] itemTypes, Vector3 center, Vector3 areaSize)
    {
        for (int i = 0; i < numberOfCombos; i++)
        {
            // Generate random spawn point name
            string spawnPointName = "SpawnPoint_" + i;

            // Generate random number of valid item types
            int numItemTypes = UnityEngine.Random.Range(1, itemTypes.Length + 1); // Random number of item types between 1 and total number of item types
            List<string> validItemTypes = new List<string>();
            for (int j = 0; j < numItemTypes; j++)
            {
                int randomIndex = UnityEngine.Random.Range(0, itemTypes.Length);
                validItemTypes.Add(itemTypes[randomIndex]);
            }

            // Generate random description and dev notes (optional)
            string description = "Description for " + spawnPointName;
            string devNotes = "Dev notes for " + spawnPointName;

            // Create a new spawn point with the generated parameters
            SpawnPoint newSpawnPoint = new SpawnPoint
            {
                spawnID = GetNextUniqueSpawnID(),
                spawnPointName = spawnPointName,
                validItemTypes = validItemTypes,
                description = description,
                devNotes = devNotes
            };

            // Add the spawn point to the database
            spawnPointDatabase.Add(newSpawnPoint);
        }

        // Save the updated database
        SaveSpawnPointDatabase();
    }

    private static void MassPlace()
    {
        if (spawnPointDatabase.Count == 0)
        {
            Debug.LogWarning("No spawn points available in the database.");
            return;
        }

        int numSpawnPointsToPlace = numberOfCombos;

        for (int i = 0; i < numSpawnPointsToPlace; i++)
        {
            // Randomly select a spawn point from the database
            SpawnPoint spawnPoint = spawnPointDatabase[UnityEngine.Random.Range(0, spawnPointDatabase.Count)];

            // Generate random position within the specified spherical volume around the center position
            Vector3 randomPosition = centerPosition + UnityEngine.Random.insideUnitSphere * radius;
            randomPosition.y = centerPosition.y;

            // Create an empty GameObject for the spawn point
            GameObject placeSpawnObject = new GameObject(spawnPoint.spawnPointName);
            placeSpawnObject.transform.position = randomPosition;

            // Attach a script or component to the GameObject to store the spawn point data
            SpawnPointScript spawnPointScript = placeSpawnObject.AddComponent<SpawnPointScript>();

            // Set the properties of the SpawnPointScript component based on the selected spawn point
            spawnPointScript.spawnPointID = spawnPoint.spawnID;
            spawnPointScript.spawnPointName = spawnPoint.spawnPointName;
            spawnPointScript.validItemTypes = new List<string>(spawnPoint.validItemTypes);
        }
    }
}
