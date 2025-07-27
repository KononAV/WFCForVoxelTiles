using System;
using Unity.VisualScripting;
using UnityEngine;

public class TileGridPresenter
{
    private TilesGridLogic _model;
    private UIManager _ui;

    public event Action OnDeletButtonDown;

    public Action<ReplaceableObjects> PlaceBuildingRequested;
    public Action<Building, VoxelTile> ShowVoxelInfo;


    //public event Action<ReplaceableObjects> StartPlacingBuilding;

    public TileGridPresenter(TilesGridLogic model, UIManager ui)
    {
        _model = model;
        _ui = ui;

        AddSubscribetions();
    }

    public void AddSubscribetions()
    {
        PlaceBuildingRequested += _ => _ui.CloseVoxelDiscription();
        PlaceBuildingRequested += _ui.OnPlaceBuildingClicked;
        PlaceBuildingRequested += _model.StartPlacingBuilding;
        

        
        _model.ObjectInformationChanges += (_, _) => _ui.OpenVoxelDiscription();
        _model.ObjectInformationChanges += _ui.OpenVoxelOptions;
        _model.ObjectInformationChanges+=_ui.WriteVoxelInfo;


        _model.DeleteObject += _model.DeleteBuilding;
        
    }
    public void DeleteButton() 
    {
        Debug.LogWarning("DLETING IN PRESENTER");
        _model.DeleteObject?.Invoke();
    
    }

   

    public void StartPlacingBuilding(ReplaceableObjects obj)
    {
        PlaceBuildingRequested?.Invoke(obj);
        //_model.StartPlacingBuilding(obj);
    }
}
