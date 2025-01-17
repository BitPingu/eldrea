using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CampGeneration : MonoBehaviour
{
    [SerializeField]
    private PoissonDiscSamplingGenerator sampling;
    private List<Vector2> campPoints = new List<Vector2>();

    public GameObject camp;

    private List<GameObject> structures = new List<GameObject>();

    public void Initialize(TileGrid grid)
    {
        if (TempData.loadGame)
        {
            // Load camp points
            List<int> campCoordsX = SaveSystem.Load().saveCampCoordsX;
            List<int> campCoordsY = SaveSystem.Load().saveCampCoordsY;

            for (int i=0; i<campCoordsX.Count; i++)
            {
                campPoints.Add(new Vector2(campCoordsX[i], campCoordsY[i]));
            }
        }
        else
        {
            // Generate camp coords
            campPoints = sampling.GeneratePoints(grid.GetTilemap(TilemapType.Ground));
        }

        List<Vector2> safeCampPoints = new List<Vector2>();

        // Generate camps
        foreach (Vector2 point in campPoints)
        {
            // Check if safe to spawn
            if (!grid.CheckLand(new Vector2(point.x, point.y)) || !grid.CheckCliff(new Vector2(point.x, point.y)))
                continue;

            // Spawn camp
            var c = Instantiate(camp, new Vector3(point.x+.5f, point.y+.5f), Quaternion.identity, transform);
            structures.Add(c);

            safeCampPoints.Add(point);
        }

        // Save camp points
        TempData.tempCamps = safeCampPoints;
    }

    public void GetCampStructure(int x, int y, GameObject chu)
    {
        foreach (GameObject structure in structures)
        {
            if (Vector3Int.FloorToInt(structure.transform.position) == new Vector3(x,y))
            {
                structure.transform.parent = chu.transform;
                break;
            }
        }
    }
}
