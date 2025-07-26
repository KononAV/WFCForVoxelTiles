using UnityEngine;

public class ReplaceableObjects : MonoBehaviour
{
    protected Renderer Renderer;
    public Vector2 Size = new Vector2(.8f, .8f);



    public virtual void SetTransparent(bool avaliable) =>
       Renderer.material.color = avaliable ? Color.green : Color.red;
    public virtual void SetNormal()
    {
        Renderer.material.color = Color.white;
    }
}
