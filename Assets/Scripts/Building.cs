using UnityEngine;

public class Building : ReplaceableObjects
{

    private void Start()
    {
        Renderer = GetComponentInChildren<Renderer>();
    }
    public override void SetTransparent(bool avaliable) =>
      Renderer.material.color = avaliable ? Color.green : Color.red;
    public override void SetNormal()
    {
        Renderer.material.color = Color.white;
    }
}
