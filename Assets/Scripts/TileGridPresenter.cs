using System;
using UnityEngine;

public class TileGridPresenter
{
    private TilesGridLogic _model;
    private UIManager _ui;

    public event Action OnDeletButtonDown;

    //public event Action<ReplaceableObjects> StartPlacingBuilding;

    public TileGridPresenter(TilesGridLogic model, UIManager ui)
    {
        _model = model;
        _ui = ui;
    }

    public void DeleteButton() 
    { 

        OnDeletButtonDown?.Invoke(); 
    
    }

    public void StartPlacingBuilding(ReplaceableObjects obj)
    {
        _model.StartPlacingBuilding(obj);
    }
}
