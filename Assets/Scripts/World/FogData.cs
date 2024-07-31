using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class FogData : MonoBehaviour
{
    public List<Vector2> clearFogCoords = new List<Vector2>();

    private BoundsInt fogBounds;
    private TilemapStructure fogMap;
    private PlayerPosition player;

    private void Awake()
    {
        // // Retrieve tilemap and player components
        // tilemap = GetComponent<TilemapStructure>();
        // player = FindObjectOfType<PlayerPosition>();

        // if (TempData.initFog)
        // {
        //     TempData.initFog = false;
        // }
    }

    // private void OnEnable()
    // {
    //     // Attach delegates
    //     player.PosChange += ClearFog;
    // }

    // private void OnDisable()
    // {
    //     // Detatch delegates
    //     player.PosChange -= ClearFog;
    // }

    public void Initialize(TilemapStructure tilemap)
    {
        // Get fog map
        fogMap = tilemap;
    }

    // Looks at area around player in a 10x10 square and clears fog
    public void ClearFog()
    {
        player = FindObjectOfType<PlayerPosition>();

        // Set bounds
        fogBounds.min = new Vector3Int(player.currentPos.x - 10, player.currentPos.y - 10, 0);
        fogBounds.max = new Vector3Int(player.currentPos.x + 10, player.currentPos.y + 10, 0);

        // Update fog
        for (int x = fogBounds.min.x; x < fogBounds.max.x; x++)
        {
            for (int y = fogBounds.min.y; y < fogBounds.max.y; y++)
            {
                if (!clearFogCoords.Contains(new Vector2(x, y)))
                {
                    fogMap.SetTile(x, y, (int)GroundTileType.Empty, true);
                    clearFogCoords.Add(new Vector2(x, y));
                }
            }
        }

        TempData.tempFog = this;
    }
}
