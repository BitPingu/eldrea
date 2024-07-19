using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerPosition : MonoBehaviour
{
    public Vector2Int prevPos;
    public Vector2Int currentPos;
    public int prevGTile, prevOTile;
    public int currentGTile, currentOTile;
    public int dominantTile;

    // Call other functions when player position changes
    public delegate void OnPosChange();
    public OnPosChange PosChange;

    // Call other functions when scene changes
    public delegate void OnSaveTemp();
    public OnSaveTemp SaveTemp;
    public delegate void OnSceneChange();
    public OnSceneChange SceneChange;

    // Call other functions when tile types change
    public delegate void OnGTileChange();
    public OnGTileChange GTileChange;
    public delegate void OnOTileChange();
    public OnOTileChange OTileChange;

    [SerializeField]
    private Vector3 spawnPoint;

    private TilemapStructure groundMap, overworldMap;
    private List<KeyValuePair<Vector2Int, int>> neighbours;

    public static PlayerPosition player;

    private void Awake()
    {
        if (player == null)
        {
            player = this;
        } 
        else 
        {
            Destroy(gameObject);
            return;
        }

        DontDestroyOnLoad(gameObject);

        // Attach delegates
        PosChange += CheckPosition;
        PosChange += OTileSound;
        PosChange += CheckNearby;
        SceneChange += RetrieveTilemap;
        SceneChange += Spawn;
    }

    private void Update()
    {
        // Retrieve coordinates of player
        currentPos = Vector2Int.FloorToInt(transform.position);
        TempData.tempPos = new Vector3(currentPos.x, currentPos.y);

        // Position check
        if (currentPos != prevPos)
        {
            PosChange(); // Call delegate (and any methods tied to it)
            prevPos = currentPos;
        }
    }

    private void RetrieveTilemap()
    {
        // Retrieve tilemap components
        groundMap = FindObjectOfType<TileGrid>().GetTilemap(TilemapType.Ground);
        overworldMap = FindObjectOfType<TileGrid>().GetTilemap(TilemapType.Overworld);
    }

    // Generates a random spawn point
    private void Spawn()
    {
        if (SceneManager.GetActiveScene().buildIndex == 1)
        {
            if (TempData.initSpawn)
            {
                if (TempData.newGame)
                {
                    // Generate initial spawn point
                    int xCoord, yCoord, currentTile;
                    do
                    {
                        // Choose random spawn point
                        xCoord = Random.Range(0, TempData.tempWidth);
                        yCoord = Random.Range(0, TempData.tempHeight);

                        // Check tile
                        currentTile = groundMap.GetTile(xCoord, yCoord);
                    }
                    while (currentTile != (int)GroundTileType.Land);

                    // Generate spawn point
                    spawnPoint = new Vector3(xCoord, yCoord);
                    TempData.tempWorldSpawn = spawnPoint;
                }
                else
                {
                    // Load player position
                    SaveData saveData = SaveSystem.Load();
                    spawnPoint.x = saveData.saveSpawnPoint[0];
                    spawnPoint.y = saveData.saveSpawnPoint[1];
                    spawnPoint.z = saveData.saveSpawnPoint[2];
                }
                TempData.initSpawn = false;
            }
            else
            {
                spawnPoint = TempData.tempSpawnPoint;
            }
        }
        else if (SceneManager.GetActiveScene().buildIndex == 2 || SceneManager.GetActiveScene().buildIndex == 3)
        {
            // Dungeon or village spawn
            spawnPoint = new Vector3(10 / 2, 0.5f);
        }

        // Set spawn point
        spawnPoint.x += .5f;
        spawnPoint.y += .5f;
        transform.position = spawnPoint;
        prevPos = currentPos = Vector2Int.FloorToInt(transform.position);

        // Retrieve spawn point tile
        prevGTile = currentGTile = groundMap.GetTile(currentPos.x, currentPos.y);
        prevOTile = currentOTile = overworldMap.GetTile(currentPos.x, currentPos.y);

        // Clear fog
        PosChange();
    }

    // Looks at current position
    private void CheckPosition()
    {
        // Get current tiles from player position
        currentGTile = groundMap.GetTile(currentPos.x, currentPos.y);
        currentOTile = overworldMap.GetTile(currentPos.x, currentPos.y);

        // Ground tile check
        if (currentGTile != prevGTile)
        {
            // GTileChange();
            prevGTile = currentGTile;
        }

        // Object tile check
        if (currentOTile != prevOTile)
        {
            OTileChange();
            prevOTile = currentOTile;
        }
    }

    // Change scene based on current tile
    public void EnterBuilding()
    {
        switch (currentOTile)
        {
            case (int)BuildingTileType.House:
                FindObjectOfType<AudioManager>().Stop();
                if (SceneManager.GetActiveScene().buildIndex == 1) 
                {
                    TempData.tempSpawnPoint = new Vector3(currentPos.x, currentPos.y);
                    SaveTemp();
                    Debug.Log("Enter Village");
                    SceneManager.LoadScene("Village");
                }
                else
                {
                    TempData.tempFog2 = FindObjectOfType<FogData>();;
                    Debug.Log("Exit Village");
                    SceneManager.LoadScene("Overworld");
                }
                break;
            case (int)BuildingTileType.Dungeon:
                FindObjectOfType<AudioManager>().Stop();
                if (SceneManager.GetActiveScene().buildIndex == 1)
                {
                    TempData.tempSpawnPoint = new Vector3(currentPos.x, currentPos.y);
                    SaveTemp();
                    Debug.Log("Enter Dungeon");
                    SceneManager.LoadScene("Dungeon");
                }
                else
                {
                    TempData.tempFog2 = FindObjectOfType<FogData>();;
                    Debug.Log("Exit Dungeon");
                    SceneManager.LoadScene("Overworld");
                }
                break;
            default:
                Debug.Log("No interactable tile!");
                break;
        }
    }

    // Play sound based on current object tile
    private void OTileSound()
    {
        // Get sound from current object tile
        if (currentOTile == (int)FoilageTileType.Tree)
        {
            FindObjectOfType<AudioManager>().PlayFx("Tree");
        }
    }

    // Looks at adjacent tiles around player
    public void CheckNearby()
    {
        neighbours = groundMap.GetNeighbors(currentPos.x, currentPos.y);

        int landTiles = 0, villageTiles = 0, waterTiles = 0;

        foreach (var neighbour in neighbours)
        {
            if (neighbour.Value == (int)GroundTileType.Land)
                landTiles++;

            if (neighbour.Value == (int)GroundTileType.Water)
                waterTiles++;
        }

        // Compare water tiles with land tiles
        if (waterTiles >= landTiles - 2)
        {
            dominantTile = (int)GroundTileType.Water;
        }
        else if (waterTiles - 2 > villageTiles)
        {
            dominantTile = (int)GroundTileType.Land;
        }
    }
}