using Cysharp.Threading.Tasks;
using System;
using System.Linq;
using System.Threading;
using UnityEngine;
using UnityEngine.EventSystems;

public class TilesGridLogic//Model
{
    public Vector2 GridSize = new Vector2(7, 7);
    public Building[,] grid;

    private ReplaceableObjects flyingBuilding;

    private (VoxelTile ground, Building building) SelectedObjects;
    private Camera mainCamera;
    private VoxelTile[,] spawnedGround;

    private bool buildPlacing = false;
    private bool mouseDetecting = false;


    private CancellationTokenSource _cancellationTokenSource;

    public Action<Building, VoxelTile> ObjectInformationChanges;

    public Action DeleteObject;


    private CancellationToken placingToken;
    private CancellationToken mouseToken;


    public TilesGridLogic(VoxelTile[,] spawnedGround)
    {
        this.spawnedGround = spawnedGround;   
        grid = new Building[40, 40];
        mainCamera = Camera.main;

        StartMouseDetecting();
        //FlyingBuildingSettings().Forget();
    }

    public void StartPlacingBuilding(ReplaceableObjects building)
    {
        StopPlacingBuilding(building);
        mouseDetecting = false;
        flyingBuilding = UnityEngine.Object.Instantiate(building);
        buildPlacing = true;
        _cancellationTokenSource = new CancellationTokenSource();
        placingToken = _cancellationTokenSource.Token;

        _=FlyingBuildingSettings();

        
    }

    private void StopPlacingBuilding(ReplaceableObjects building)
    {
        buildPlacing=false;
        if (flyingBuilding != null) 
        { 
            _cancellationTokenSource?.Cancel();
            UnityEngine.Object.Destroy(flyingBuilding.gameObject);
        }
        
        

    }

   

    private void StartMouseDetecting()
    {   
        mouseDetecting = true;
        _cancellationTokenSource = new CancellationTokenSource();

        mouseToken = _cancellationTokenSource.Token;
        _ = MouseDetection();
    }


   
    private async UniTask FlyingBuildingSettings()
    {
        while (buildPlacing)
        {
            Debug.Log("placing process");
            
            await UniTask.Yield(PlayerLoopTiming.Update, cancellationToken: placingToken);

           

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

                    buildPlacing = false;
                }
            }
        }
        Debug.Log("End Placing");
        StartMouseDetecting();

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

                if (gridX + 1 >= spawnedGround.GetLength(0) ||
                    gridY + 1 >= spawnedGround.GetLength(1)) return true;

                if (!spawnedGround[gridX + 1, gridY + 1].CanPlaceBuilding) return true;
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
                    gx >= spawnedGround.GetLength(0) ||
                    gy >= spawnedGround.GetLength(1)) return true;

                if (spawnedGround[gx, gy] != null) return true;

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
                    spawnedGround[gx, gy] = obj as VoxelTile;
                }
            }
        }

        flyingBuilding.SetNormal();
        flyingBuilding = null;

    }

    private Vector3 RoundToSize(Vector3 pos)
    {
        pos.x -= pos.x % .8f;//pos.x -= pos.x % flyingBuilding.Size.x
        pos.z -= pos.z % .8f;//pos.z -= pos.z % flyingBuilding.Size.y
        return pos;
    }



  

   async public UniTaskVoid MouseDetection()
    {
        int preX=1;
        int preY=1;
        while (mouseDetecting) 
        {
            if (EventSystem.current.IsPointerOverGameObject())
            {
                await UniTask.Yield(PlayerLoopTiming.Update, cancellationToken: mouseToken); continue;
            }
            //Debug.Log("mouse detecting process");
            if (Input.GetMouseButtonDown(0))
            {
                //SelectedObjects = (null, null);
                ObjectInformationChanges?.Invoke(null, null);
            }
            await UniTask.Yield(PlayerLoopTiming.Update, cancellationToken:mouseToken);

            var ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            var groundPlane = new Plane(Vector3.up, Vector3.zero);

            if (groundPlane.Raycast(ray, out float hitDistance))
            {
                Vector3 worldPos = ray.GetPoint(hitDistance);

                int x = Mathf.RoundToInt(worldPos.x / .8f);
                int y = Mathf.RoundToInt(worldPos.z / .8f);
                
                

                if ((x < 0 || x > Mathf.RoundToInt(GridSize.x + 1 - .8f)) ||
                    (y < 0 || y > Mathf.RoundToInt(GridSize.y + 1 - .8f)))
                {
                    spawnedGround[preX, preY].SetNormal();
                    continue;
                }

                if (preX != x || preY != y){
                    spawnedGround[preX, preY].SetNormal();
                   

                }
                 preX = x+1; preY = y+1;

                spawnedGround[x + 1, y + 1].SetTransparent(true);

                if (Input.GetMouseButton(0))
                {
                    SelectedObjects = (spawnedGround[x+1, y + 1], grid[x,y]);
                    ObjectInformationChanges?.Invoke(SelectedObjects.building, SelectedObjects.ground);
                    Debug.Log(SelectedObjects.ground?.name+ SelectedObjects.building?.name);
                }
            }
        }
    }

  


    public void DeleteBuilding()
    {
        Debug.LogWarning("DELETING");
        if (SelectedObjects.building == null) return;
        for (int i = 0; i < grid.GetLength(0); i++) 
        {
            for(int j = 0; j < grid.GetLength(1); j++)
            {
                if (grid[i, j] == SelectedObjects.building)
                {
                    grid[i, j] = null;
                }
            }
        
        }
        GameObject.Destroy(SelectedObjects.building.gameObject);


        ObjectInformationChanges?.Invoke(SelectedObjects.building, SelectedObjects.ground);
    }

    

    /**//*private *//*void OnDrawGizmos()
    {
        for (float i = 0.8f; i < GridSize.x; i += 0.8f)
        {
            for (float j = 0.8f; j < GridSize.y; j += 0.8f)
            {
                Gizmos.color = (i + j) % 1.6f == 0 ? Color.white : Color.blue;
                Gizmos.DrawCube(new Vector3(i, 0, j), new Vector3(0.8f, 0.1f, 0.8f));
            }
        }
    }*/
}


