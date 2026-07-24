using System.Collections.Generic;
using UnityEngine;

public class ObstacleSpawner : MonoBehaviour
{
    public GameObject[] obstaclePrefabs;
    public float scrollSpeed = 5f;
    public float spawnX = 15f;
    public float obstacleY = -2f;
    public float minSpawnInterval = 1.5f;
    public float maxSpawnInterval = 3f;
    public float destroyThreshold = -20f;

    private List<Transform> activeObstacles = new List<Transform>();
    private float timeUntilNextSpawn;

    void Start()
    {
        SetNextSpawnTime();
    }

    void Update()
    {
        timeUntilNextSpawn -= Time.deltaTime;

        if (timeUntilNextSpawn <= 0f)
        {
            SpawnObstacle();
            SetNextSpawnTime();
        }

        for (int i = activeObstacles.Count - 1; i >= 0; i--)
        {
            activeObstacles[i].Translate(Vector3.left * scrollSpeed * Time.deltaTime);

            if (activeObstacles[i].position.x < destroyThreshold)
            {
                Destroy(activeObstacles[i].gameObject);
                activeObstacles.RemoveAt(i);
            }
        }
    }

    void SpawnObstacle()
    {
        int randomIndex = Random.Range(0, obstaclePrefabs.Length);
        GameObject chosenPrefab = obstaclePrefabs[randomIndex];
    
        GameObject obstacle = Instantiate(chosenPrefab, new Vector3(spawnX, obstacleY, 0f), Quaternion.identity);
        activeObstacles.Add(obstacle.transform);
    }

    void SetNextSpawnTime()
    {
        timeUntilNextSpawn = Random.Range(minSpawnInterval, maxSpawnInterval);
    }

}
