using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    private TileGridPresenter _presenter;

    [Header("info about voxel unit")]
    [SerializeField] private Canvas _canvasinfoAboutVoxelUnit;
    [SerializeField] private TextMeshProUGUI _infoAboutVoxelUnit;

    [Header("voxel options")]
    [SerializeField] Canvas _options;
    [SerializeField] Button delete;
    [SerializeField] Button replace;
    [SerializeField] Text optionType;
    [SerializeField] Image image;

    public void SetPresenter(TileGridPresenter presenter)
    {
        _presenter = presenter;
    }

    
    public void OnPlaceBuildingClicked(ReplaceableObjects obj)
    {
        _presenter.PlaceBuildingRequested?.Invoke(obj);   
    }

    public void OpenVoxelDiscription()
    {
        _canvasinfoAboutVoxelUnit.gameObject.SetActive(true);

    }

    public void CloseVoxelDiscription()
    {
        _canvasinfoAboutVoxelUnit.gameObject.SetActive(false);

    }
    public void WriteVoxelInfo(Building building, VoxelTile ground)
    {
        if(building == null&&ground==null) {CloseVoxelDiscription();CloseVoxelOptions(); }
        
        string information =
            $"Ground info" +
            $"\nname: {ground?.name ?? "null"}"+
            $"\nsize:{ground?.Size.x}x{ground?.Size.y}"+
            $"\ncan place building: {ground?.CanPlaceBuilding}"+
            $"\nis replaceable: {ground?.IsReplaceable}"+

            $"\nbuilding info" +
            $"\nname: {building?.name??"null"}" +
            $"\nsize: {building?.Size.x}x{building?.Size.y}";

        _infoAboutVoxelUnit.text = information;
    }


    public void OpenVoxelOptions(Building building, VoxelTile ground)
    {
        _options.gameObject.SetActive(true);
        if(ground != null)
        image.transform.position = Camera.main.WorldToScreenPoint( ground.transform.position+ Vector3.one);
    }

    public void CloseVoxelOptions()
    {
        _options.gameObject.SetActive(false);
    }

    public void OnDeleteButton()
    {
        _presenter.DeleteButton();
    }
}
