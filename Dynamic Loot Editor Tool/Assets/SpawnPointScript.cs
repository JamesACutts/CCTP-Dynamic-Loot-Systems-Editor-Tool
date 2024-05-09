using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class SpawnPointScript : MonoBehaviour
{
    public int spawnPointID;
    public string spawnPointName;
    public List<string> validItemTypes;
    public Vector3 _spawnPointPos;

    private void OnEnable()
    {
        // Ensure the values are updated when the object is enabled in Edit mode.
        UpdateSpawnPointPos();
    }

    public void Update()
    {
        UpdateSpawnPointPos();
    }

    private void UpdateSpawnPointPos()
    {
        _spawnPointPos = transform.position;
    }
}
