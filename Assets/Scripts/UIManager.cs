using UnityEngine;

public class UIManager : MonoBehaviour
{
    private TileGridPresenter _presenter;

    public void SetPresenter(TileGridPresenter presenter)
    {
        _presenter = presenter;
    }

    public void OnDeleteButtonClicked()
    {
        _presenter.DeleteButton();
    }

    public void OnPlaceBuildingClicked(ReplaceableObjects obj)
    {
        _presenter.StartPlacingBuilding(obj);
    }
}
