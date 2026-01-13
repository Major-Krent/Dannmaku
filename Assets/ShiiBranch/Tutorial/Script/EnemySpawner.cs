using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class EnemySpawner : MonoBehaviour
{
    [SerializeField] private GameObject enemyPrefab;
    [SerializeField] private Tilemap walkableTilemap; 
    [SerializeField] private float spawnInterval = 5f; 
    [SerializeField] private int maxEnemies = 5;

    [SerializeField] private Transform player;        
    [SerializeField] private float minDistanceFromPlayer = 5f; 
    [SerializeField] private float maxDistanceFromPlayer = 20f;

    private List<Vector3> validSpawnPositions = new List<Vector3>();
    private List<GameObject> activeEnemies = new List<GameObject>();
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        GatherValidPositions();

        StartCoroutine(SpawnRoutine());
    }

    // Update is called once per frame
    void Update()
    {
        for (int i = activeEnemies.Count - 1; i >= 0; i--)
        {
            if (activeEnemies[i] == null)
            {
                activeEnemies.RemoveAt(i);
            }
        }
    }
    private void GatherValidPositions()
    {
        if (walkableTilemap == null)
        {
            return;
        }

        BoundsInt bounds = walkableTilemap.cellBounds;

        for (int x = bounds.xMin; x < bounds.xMax; x++)
        {
            for (int y = bounds.yMin; y < bounds.yMax; y++)
            {
                Vector3Int cellPos = new Vector3Int(x, y, 0);

                if (walkableTilemap.HasTile(cellPos))
                {

                    Vector3 worldPos = walkableTilemap.GetCellCenterWorld(cellPos);
                    validSpawnPositions.Add(worldPos);
                }
            }
        }
    }
    private IEnumerator SpawnRoutine()
    {
        if (validSpawnPositions.Count == 0) yield break;

        while (true)
        {
            yield return new WaitForSeconds(spawnInterval);

            if (activeEnemies.Count < maxEnemies)
            {
                SpawnEnemy();
            }
        }
    }
    private void SpawnEnemy()
    {
        if (enemyPrefab == null) return;

        Vector3 spawnPos = Vector3.zero;
        bool foundValidSpot = false;

        for (int i = 0; i < 10; i++)
        {
            int randomIndex = Random.Range(0, validSpawnPositions.Count);
            Vector3 candidatePos = validSpawnPositions[randomIndex];

            if (player != null)
            {
                float dist = Vector3.Distance(candidatePos, player.position);
                if (dist >= minDistanceFromPlayer && dist <= maxDistanceFromPlayer)
                {
                    spawnPos = candidatePos;
                    foundValidSpot = true;
                    break;
                }
            }
            else
            {
                spawnPos = candidatePos;
                foundValidSpot = true;
                break;
            }
        }

        if (foundValidSpot)
        {
            GameObject newEnemy = Instantiate(enemyPrefab, spawnPos, Quaternion.identity);
            activeEnemies.Add(newEnemy);
        }
    }
}
