using UnityEngine;

public class WorldLocator : MonoBehaviour
{
    private Vector3 newPosition;
    private float centerX;
    private float centerY;

    [SerializeField]
    private TileGrid grid;

    private void LateUpdate()
    {
        // Calculate center 
        centerX = grid.width / 2;
        centerY = grid.height / 2;
        newPosition = new Vector3(centerX, centerY, transform.position.z);

        // Align in center of the world
        transform.position = newPosition;
    }
}