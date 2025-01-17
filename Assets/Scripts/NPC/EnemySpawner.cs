using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [Serializable]
    public class EnemyTypes
    {
        public GameObject gameObject;
    }

    [SerializeField]
    private EnemyTypes[] enemies;

    [SerializeField]
    private int maxSpawn;
    public List<GameObject> spawnedEnemies = new List<GameObject>();

    [SerializeField]
    private float minWaitTime; // default is 3f
    [SerializeField]
    private float maxWaitTime; // default is 5f
    [SerializeField]
    private float minDistance;
    
    private TileGrid grid;
    private DayAndNightCycle time;
    private GameObject player;

    public void Initialize(TileGrid g, DayAndNightCycle dayNight, GameObject p)
    {
        // Get tilemap structure and time and player
        grid = g;
        time = dayNight;
        player = p;
    }

    public IEnumerator Spawn()
    {
        // Choose random wait time
        float interval = UnityEngine.Random.Range(minWaitTime, maxWaitTime);
        yield return new WaitForSeconds(interval);

        Debug.Log("try spawn with " + FindObjectOfType<PlayerPosition>().currentArea + " " + spawnedEnemies.Count);
        if (FindObjectOfType<PlayerPosition>().currentArea.Contains("Overworld") && !FindObjectOfType<PlayerPosition>().currentArea.Contains("Village"))
        {
            while (FindObjectOfType<PlayerPosition>().currentArea.Contains("Overworld") && !FindObjectOfType<PlayerPosition>().currentArea.Contains("Village") 
                && spawnedEnemies.Count <= maxSpawn)
            {
                // Spawn enemies
                if (time.isDay)
                {
                    dayEnemies();
                }
                else
                {
                    nightEnemies();
                }
                Debug.Log("spawn " + spawnedEnemies.Count);

                // Choose random wait time
                interval = UnityEngine.Random.Range(minWaitTime, maxWaitTime);
                yield return new WaitForSeconds(interval);
            }
        }
        else if (FindObjectOfType<PlayerPosition>().currentArea.Contains("Underground"))
        {
            while (FindObjectOfType<PlayerPosition>().currentArea.Contains("Underground") && spawnedEnemies.Count <= maxSpawn)
            {
                // Spawn enemies
                dungeonEnemies();
                Debug.Log("spawn " + spawnedEnemies.Count);

                // Choose random wait time
                interval = UnityEngine.Random.Range(minWaitTime, maxWaitTime);
                yield return new WaitForSeconds(interval);
            }
        }
    }

    private void Update()
    {
        if (spawnedEnemies.Count > 0)
        {
            GameObject[] enemies = spawnedEnemies.ToArray();
            foreach (GameObject enemy in enemies)
            {
                if (!enemy)
                    continue;

                // Calculate current distance from player
                float distance = Vector3.Distance(player.transform.position, enemy.transform.position);

                // Check if outside range
                if (distance > minDistance)
                {
                    Debug.Log("despawn");
                    // maybe use a coroutine?
                    spawnedEnemies.Remove(enemy);
                    Destroy(enemy);

                    // Spawn another enemy
                    StartCoroutine(Spawn());
                }
            }
        }
    }

    private void dayEnemies()
    {
        foreach (EnemyTypes enemy in enemies)
        {
            switch (enemy.gameObject.name)
            {
                case "Slime":
                    spawnEnemy(enemy);
                    break;
                default:
                    break;
            }
        }
    }

    private void nightEnemies()
    {
        foreach (EnemyTypes enemy in enemies)
        {
            switch (enemy.gameObject.name)
            {
                case "Zombie":
                    spawnEnemy(enemy);
                    break;
                default:
                    break;
            }
        }
    }

    private void dungeonEnemies()
    {
        if (UnityEngine.Random.Range(0,2) == 1)
        {
            foreach (EnemyTypes enemy in enemies)
            {
                if (enemy.gameObject.name.Contains("Golem"))
                {
                    spawnEnemyUnderground(enemy);
                }
            }
        }
        else
        {
            foreach (EnemyTypes enemy in enemies)
            {
                if (enemy.gameObject.name.Contains("Skeleton"))
                {
                    spawnEnemyUnderground(enemy);
                }
            }
        }
    }

    private void spawnEnemy(EnemyTypes enemy)
    {
        Vector3 spawnPoint = new Vector3();

        float xCoord, yCoord;

        Vector2 playerMovement = player.GetComponent<PlayerController>().movement;

        // Generate enemy spawn point
        do
        {
            // Choose random spawn point around player
            xCoord = UnityEngine.Random.Range((player.transform.position.x-20), (player.transform.position.x+20));
            yCoord = UnityEngine.Random.Range((player.transform.position.y-20), (player.transform.position.y+20));
        }
        while (!grid.CheckLand(new Vector2(xCoord, yCoord)) || !grid.CheckCliff(new Vector2(xCoord, yCoord)));

        // Generate spawn point
        spawnPoint = new Vector3(xCoord, yCoord);

        // Instantiate and spawn enemy
        GameObject e = Instantiate(enemy.gameObject, spawnPoint, Quaternion.identity, transform);
        spawnedEnemies.Add(e);
    }

    private void spawnEnemyUnderground(EnemyTypes enemy)
    {
        Vector3 spawnPoint = new Vector3();

        float xCoord, yCoord;

        Vector2 playerMovement = player.GetComponent<PlayerController>().movement;

        // Generate enemy spawn point
        do
        {
            // Choose random spawn point around player
            xCoord = UnityEngine.Random.Range((player.transform.position.x-20), (player.transform.position.x+20));
            yCoord = UnityEngine.Random.Range((player.transform.position.y-20), (player.transform.position.y+20));
        }
        while (!grid.CheckDungeon(new Vector2(xCoord, yCoord)));

        // Generate spawn point
        spawnPoint = new Vector3(xCoord, yCoord);

        // Instantiate and spawn enemy
        GameObject e = Instantiate(enemy.gameObject, spawnPoint, Quaternion.identity, transform);
        spawnedEnemies.Add(e);
    }

    public GameObject spawnEnemy(string type, Vector3 spawnPoint)
    {
        // Check if safe to spawn
        if (!grid.CheckLand(spawnPoint) || !grid.CheckCliff(spawnPoint))
        {
            Debug.Log(type + " cant spawn!");
            return null;
        }

        // Get enemy
        foreach (EnemyTypes enemy in enemies)
        {
            if (enemy.gameObject.name.Equals(type))
            {
                // Instantiate and spawn enemy
                GameObject e = Instantiate(enemy.gameObject, spawnPoint, Quaternion.identity, transform);
                return e;
            }
        }

        return null;
    }

    public void SpawnGoblins()
    {
        foreach (EnemyTypes enemy in enemies)
        {
            switch (enemy.gameObject.name)
            {
                case "Goblin":
                    // Look for camp points
                    var camps = FindObjectsOfType<CampData>();
                    foreach (CampData camp in camps)
                    {
                        // Spawn goblins
                        GameObject goblin1 = spawnEnemy(enemy.gameObject.name, new Vector2((int)camp.transform.position.x+1.5f, (int)camp.transform.position.y+.7f));
                        if (goblin1)
                        {
                            goblin1.GetComponent<SpriteRenderer>().flipX = true;
                            goblin1.GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Kinematic;
                            camp.goblins.Add(goblin1);
                            goblin1.transform.parent = camp.transform;
                            goblin1.SetActive(false);
                        }
                        GameObject goblin2 = spawnEnemy(enemy.gameObject.name, new Vector2((int)camp.transform.position.x+.5f, (int)camp.transform.position.y+1.2f));
                        if (goblin2)
                        {
                            goblin2.GetComponent<SpriteRenderer>().flipX = true;
                            goblin2.GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Kinematic;
                            camp.goblins.Add(goblin2);
                            goblin2.transform.parent = camp.transform;
                            goblin2.SetActive(false);
                        }
                        camp.gameObject.SetActive(false);
                    }
                    break;
                default:
                    break;
            }
        }
    }

    public void despawnEnemies()
    {
        GameObject[] enemies = spawnedEnemies.ToArray();
        foreach (GameObject enemy in enemies)
        {
            spawnedEnemies.Remove(enemy);
            Destroy(enemy);
        }
    }
}
