using Cysharp.Threading.Tasks;
using System.Collections;
using UnityEditor.TerrainTools;
using UnityEngine;

public class GameManager : MonoBehaviour
{

    [SerializeField]private VoxelTilePlacerWfc _voxelPlacer;
    [SerializeField]private TilesGridLogic _buildingLogic;
    [SerializeField]private UIManager _uiManager;
    [SerializeField]private TileGridPresenter _presenter;

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }


    

    private async UniTaskVoid Start()
    {
        _voxelPlacer = GameObject.Find("TilePlacer").GetComponent<VoxelTilePlacerWfc>();
        _uiManager = FindAnyObjectByType<UIManager>();

        await UniTask.WaitUntil(() => _voxelPlacer.spawnedTiles?.Length > 0);

        _buildingLogic = new TilesGridLogic(_voxelPlacer.trueSpawnedTiles);
        _presenter = new TileGridPresenter(_buildingLogic, _uiManager);

        _uiManager.SetPresenter(_presenter);
    }




    public void StartPlacingBuilding(ReplaceableObjects obj)
    {
        _buildingLogic.StartPlacingBuilding(obj);
    }
}
