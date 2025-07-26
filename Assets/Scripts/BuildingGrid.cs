using UnityEngine;
using Cysharp.Threading.Tasks;

public class BuildingGrid : MonoBehaviour
{
    public Vector2 GridSize = new Vector2(40, 40);
    public Building[,] grid;

    private ReplaceableObjects flyingBuilding;
    private Camera mainCamera;
    private VoxelTilePlacerWfc voxelTIlePlacer;
    private bool gameProcess = true;

    void Start()
    {
        voxelTIlePlacer = GameObject.Find("TilePlacer").GetComponent<VoxelTilePlacerWfc>();
        grid = new Building[40, 40];
        mainCamera = Camera.main;
        FlyingBuildingSettings().Forget();
    }

    public void StartPlacingBuilding(ReplaceableObjects building)
    {
        if (flyingBuilding != null) Destroy(flyingBuilding.gameObject);
        flyingBuilding = Instantiate(building);
    }

    private async UniTaskVoid FlyingBuildingSettings()
    {
        while (gameProcess)
        {
            await UniTask.Yield(PlayerLoopTiming.Update);

            if (flyingBuilding == null) continue;

            var ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            var groundPlane = new Plane(Vector3.up, Vector3.zero);

            if (groundPlane.Raycast(ray, out float hitDistance))
            {
                Vector3 worldPos = RoundToSize(ray.GetPoint(hitDistance));
                bool available = IsValidPlacement(worldPos);

                flyingBuilding.SetTransparent(available);
                flyingBuilding.transform.position = worldPos;

                if (available && Input.GetMouseButtonDown(0))
                {
                    if (flyingBuilding is Building build)
                        PlaceFlyingBuildingGeneric(worldPos.x, worldPos.z, build);
                    else if (flyingBuilding is VoxelTile voxel)
                        PlaceFlyingBuildingGeneric(worldPos.x, worldPos.z, voxel);
                }
            }
        }
    }

    private bool IsValidPlacement(Vector3 worldPos)
    {
        if (worldPos.x < 0 || worldPos.x > GridSize.x - flyingBuilding.Size.x) return false;
        if (worldPos.z < 0 || worldPos.z > GridSize.y - flyingBuilding.Size.y) return false;

        return flyingBuilding switch
        {
            Building => !IsPlaceTakenForBuilding(worldPos.x, worldPos.z),
            VoxelTile => !IsPlaceTakenForGround(worldPos.x, worldPos.z),
            _ => false
        };
    }

    private bool IsPlaceTakenForBuilding(float x, float z)
    {
        for (int i = 0; i < flyingBuilding.Size.x; i++)
        {
            for (int j = 0; j < flyingBuilding.Size.y; j++)
            {
                int gridX = Mathf.RoundToInt(((x / 0.8f) * 0.2f + x) + i);
                int gridY = Mathf.RoundToInt(((z / 0.8f) * 0.2f + z) + j);

                if (gridX < 0 || gridX >= grid.GetLength(0) ||
                    gridY < 0 || gridY >= grid.GetLength(1)) return true;

                if (grid[gridX, gridY] != null) return true;

                if (gridX + 1 >= voxelTIlePlacer.spawnedTiles.GetLength(0) ||
                    gridY + 1 >= voxelTIlePlacer.spawnedTiles.GetLength(1)) return true;

                if (!voxelTIlePlacer.spawnedTiles[gridX + 1, gridY + 1].CanPlaceBuilding) return true;
            }
        }

        return false;
    }

    private bool IsPlaceTakenForGround(float x, float z)
    {
        var voxel = flyingBuilding as VoxelTile;
        if (voxel == null) return true;

        for (int i = 0; i < flyingBuilding.Size.x; i++)
        {
            for (int j = 0; j < flyingBuilding.Size.y; j++)
            {
                int gx = Mathf.RoundToInt(((x / 0.8f) * 0.2f + x) + i);
                int gy = Mathf.RoundToInt(((z / 0.8f) * 0.2f + z) + j);

                if (gx < 0 || gy < 0 ||
                    gx >= voxelTIlePlacer.spawnedTiles.GetLength(0) ||
                    gy >= voxelTIlePlacer.spawnedTiles.GetLength(1)) return true;

                if (voxelTIlePlacer.spawnedTiles[gx, gy] != null) return true;

                if (!voxel.CanPlaceBuilding) return true;
            }
        }

        return false;
    }

    private void PlaceFlyingBuildingGeneric<T>(float x, float z, T obj) where T : ReplaceableObjects
    {
        for (int i = 0; i < flyingBuilding.Size.x; i++)
        {
            for (int j = 0; j < flyingBuilding.Size.y; j++)
            {
                int gx = Mathf.RoundToInt(((x / 0.8f) * 0.2f + x) + i);
                int gy = Mathf.RoundToInt(((z / 0.8f) * 0.2f + z) + j);

                if (typeof(T) == typeof(Building))
                {
                    grid[gx, gy] = obj as Building;
                }
                else if (typeof(T) == typeof(VoxelTile))
                {
                    voxelTIlePlacer.spawnedTiles[gx, gy] = obj as VoxelTile;
                }
            }
        }

        flyingBuilding.SetNormal();
        flyingBuilding = null;
    }

    private Vector3 RoundToSize(Vector3 pos)
    {
        pos.x -= pos.x % flyingBuilding.Size.x;
        pos.z -= pos.z % flyingBuilding.Size.y;
        return pos;
    }

    private void OnDrawGizmos()
    {
        for (float i = 0.8f; i < GridSize.x; i += 0.8f)
        {
            for (float j = 0.8f; j < GridSize.y; j += 0.8f)
            {
                Gizmos.color = (i + j) % 1.6f == 0 ? Color.white : Color.blue;
                Gizmos.DrawCube(new Vector3(i, 0, j), new Vector3(0.8f, 0.1f, 0.8f));
            }
        }
    }
}
