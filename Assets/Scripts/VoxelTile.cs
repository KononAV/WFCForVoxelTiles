using System;
using UnityEngine;

public class VoxelTile : ReplaceableObjects
{
    public float VoxelSize = 0.1f;
    public int TileSideVoxels = 8;
    public Renderer Renderer;

    public bool IsReplaceable = true;
    public bool CanPlaceBuilding = false;



    [Range(1, 100)]
    public int Weight = 50;

    public RotationType Rotation;

    public enum RotationType
    {
        OnlyRotation,
        TwoRotations,
        FourRotations
    }

   public byte[] ColorsRight;
   public byte[] ColorsForward;
   public byte[] ColorsLeft;
   public byte[] ColorsBack;


    private void Start()
    {
        Renderer = GetComponentInChildren<Renderer>(); 
    }


    

    public void CalculateSidesColors()
    {
        
        ColorsRight = new byte[2 * 8];
        ColorsForward = new byte[2 * 8];
        ColorsLeft = new byte[2 * 8];
        ColorsBack = new byte[2 * 8];
        
        for (int y = 0; y < 2; y++)
        {
            for (int i = 0; i < TileSideVoxels; i++)
            {
                ColorsRight[y * TileSideVoxels + i] = GetVoxelColor(y, i, Direction.Back); ;
                ColorsForward[y * TileSideVoxels + i] = GetVoxelColor(y, i, Direction.Right);
                ColorsLeft[y * TileSideVoxels + i] = GetVoxelColor(y, i, Direction.Forward);
                ColorsBack[y * TileSideVoxels + i] = GetVoxelColor(y, i, Direction.Left);
            }
        }
    }

    public void Rotate90()
    {
        transform.Rotate(0, 90, 0);
        
        byte[] colorsRightNew = new byte[8*2 ];
        byte[] colorsForwardNew = new byte[8 * 2];
        byte[] colorsLeftNew = new byte[8*2];
        byte[] colorsBackNew = new byte[8*2];

        for (int layer = 0; layer < 2; layer++)
        {
            for (int offset = 0; offset < 8; offset++)
            {
                colorsRightNew[layer * TileSideVoxels + offset] = ColorsForward[layer * TileSideVoxels + TileSideVoxels - offset - 1];
                colorsForwardNew[layer * TileSideVoxels + offset] = ColorsLeft[layer * TileSideVoxels + offset];
                colorsLeftNew[layer * TileSideVoxels + offset] = ColorsBack[layer * TileSideVoxels + TileSideVoxels - offset - 1];
                colorsBackNew[layer * TileSideVoxels + offset] = ColorsRight[layer * TileSideVoxels + offset];
            }
        }

        ColorsRight = colorsRightNew;
        ColorsForward = colorsForwardNew;
        ColorsLeft = colorsLeftNew;
        ColorsBack = colorsBackNew;
    }

    private byte GetVoxelColor(int verticalLayer, int horizontalOffset, Direction direction)
    {
        var meshCollider = GetComponentInChildren<MeshCollider>();

        float vox = VoxelSize;
        float half = VoxelSize / 2;

        Vector3 rayStart;
        Vector3 rayDir;
        if (direction == Direction.Right)
        {
            rayStart = meshCollider.bounds.min +
                       new Vector3(-half, 0, half + horizontalOffset * vox);
            rayDir = Vector3.right;
        }
        else if (direction == Direction.Forward)
        {
            rayStart = meshCollider.bounds.min +
                       new Vector3(half + horizontalOffset * vox, 0, -half);
            rayDir = Vector3.forward;
        }
        else if (direction == Direction.Left)
        {
            rayStart = meshCollider.bounds.max +
                       new Vector3(half, 0, -half - (TileSideVoxels - horizontalOffset - 1) * vox);
            rayDir = Vector3.left;
        }
        else if (direction == Direction.Back)
        {
            rayStart = meshCollider.bounds.max +
                       new Vector3(-half - (TileSideVoxels - horizontalOffset - 1) * vox, 0, half);
            rayDir = Vector3.back;
        }
        else
        {
            throw new ArgumentException("Wrong direction value", nameof(direction));
        }

        rayStart.y = meshCollider.bounds.min.y + half + verticalLayer * vox;

        //Debug.DrawRay(rayStart, rayDir * .1f, Color.blue, 200f);

        if (Physics.Raycast(new Ray(rayStart, rayDir), out RaycastHit hit, vox))
        {
            byte colorIndex = (byte) (hit.textureCoord.x * 256);

            return colorIndex;
        }

        return 0;
    }

    public override void SetTransparent(bool avaliable) =>
        Renderer.material.color = avaliable ? Color.green : Color.red;
    public override void SetNormal()
    {
        Renderer.material.color = Color.white;
    }
}