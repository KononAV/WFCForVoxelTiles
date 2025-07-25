﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class VoxelTilePlacerWfc : MonoBehaviour
{
    public List<VoxelTile> TilePrefabs;
    public Vector2Int MapSize = new Vector2Int(10, 10);

    public VoxelTile[,] spawnedTiles;

    public VoxelTile[,] trueSpawnedTiles;


    private void Start()
    {
        spawnedTiles = new VoxelTile[MapSize.x, MapSize.y];

        foreach (VoxelTile tilePrefab in TilePrefabs)
        {
            tilePrefab.CalculateSidesColors();
        }

        int countBeforeAdding = TilePrefabs.Count;
        for (int i = 0; i < countBeforeAdding; i++)
        {
            VoxelTile clone = null;
            switch (TilePrefabs[i].Rotation)
            {
                case VoxelTile.RotationType.OnlyRotation:
                    break;

                case VoxelTile.RotationType.TwoRotations:
                    TilePrefabs[i].Weight = Mathf.Max(1, TilePrefabs[i].Weight / 2);

                    clone = Instantiate(TilePrefabs[i]);
                    clone.Rotate90();
                    TilePrefabs.Add(clone);
                    break;

                case VoxelTile.RotationType.FourRotations:
                    TilePrefabs[i].Weight = Mathf.Max(1, TilePrefabs[i].Weight / 4);

                    for (int r = 1; r <= 3; r++)
                    {
                        clone = Instantiate(TilePrefabs[i]);
                        for (int rot = 0; rot < r; rot++)
                        {
                            clone.Rotate90();
                        }
                        TilePrefabs.Add(clone);
                    }
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        Generate();
        StartCoroutine(PlaceTiles());
    }

    private IEnumerator PlaceTiles()
    {
        trueSpawnedTiles = new VoxelTile[10, 10];
        for (int i = 0; i < spawnedTiles.GetLength(0); i++)
        {
            for (int j = 0; j < spawnedTiles.GetLength(1); j++)
            {
                VoxelTile tile = spawnedTiles[i, j];
                if (tile != null)
                {
                    trueSpawnedTiles[i,j] = 
                        Instantiate(tile.gameObject, new Vector3((i-1)*.8f, 0, (j-1) * .8f), tile.transform.rotation)
                        .GetComponent<VoxelTile>();;
                    spawnedTiles[i,j]=null;
                }
                
                yield return new WaitForSeconds(0.00f);
            }
        }
        
    }


    public void Generate()
    {
        spawnedTiles = new VoxelTile[MapSize.x, MapSize.y];

        int i = 1, j = 1;
        int iterations = 0;

        Vector2Int current = new Vector2Int(i, j);
        spawnedTiles[i, j] = TilePrefabs[UnityEngine.Random.Range(0, TilePrefabs.Count)];

        while (i*j <= (MapSize.x ) * (MapSize.y ))
        {
            if (iterations++ > 10000)
            {
                Debug.LogWarning("Too many iterations. Aborting generation.");
                return;
            }

            j++;
            if (j >= MapSize.y - 1)
            {
                j = 1;
                i++;
            }
            if (i >= MapSize.x - 1)
            {
                break;
            }

            current = new Vector2Int(i, j);
            VoxelTile newTile = null;

            bool hasUp = spawnedTiles[i - 1, j] != null;
            bool hasBack = spawnedTiles[i, j - 1] != null;

            if (hasUp && hasBack)
            {
                newTile = ChooseOneOfRandom(
                    spawnedTiles[i - 1, j].ColorsBack,
                    spawnedTiles[i, j - 1].ColorsRight);
            }
            else if (hasBack)
            {
                newTile = ChooseRandom(spawnedTiles[i, j - 1].ColorsRight, Direction.Right);
            }
            else if (hasUp)
            {
                newTile = ChooseRandom(spawnedTiles[i - 1, j].ColorsBack, Direction.Back);
            }
            else
            {
                newTile = TilePrefabs[UnityEngine.Random.Range(0, TilePrefabs.Count)];
            }

            if (newTile == null)
            {
                (i, j) = BackTrack(i,j);
                continue;
            }

            spawnedTiles[i, j] = newTile;
        }

        Debug.Log("Generation complete. Total placed tiles: " + spawnedTiles.Length);
    }


    private (int i, int j) BackTrack(int i, int j)
    {
        Debug.LogWarning($"[BackTrack] Starting at ({i}, {j})");

        int stepsToBack = MapSize.x;

        while (stepsToBack > 0)
        {
            spawnedTiles[i, j] = null;
            stepsToBack--;

            j--;

            if (j < 1)
            {
                i--;
                j = MapSize.y - 2;
            }

            
            if (i < 1)
            {
                return (1, 1); 
            }
        }

        return (i, j);
    }




    private VoxelTile ChooseRandom(byte[] colors, Direction direction)
    {
        Stack<VoxelTile> teoreticalVoxels = new Stack<VoxelTile>();
        if (direction == Direction.Right) {

            foreach (var prefab in TilePrefabs)
            {
                if (Enumerable.SequenceEqual(prefab.ColorsLeft, colors))
                {
                    teoreticalVoxels.Push(prefab);
                }
            }

        }
        if(direction == Direction.Back)
        {
            foreach (var prefab in TilePrefabs)
            {
                if (Enumerable.SequenceEqual(prefab.ColorsForward, colors))
                {
                    teoreticalVoxels.Push(prefab);
                }
            }

        }
        

        if (teoreticalVoxels.Count == 0) { return null; }



        return RandomVoxelByWeight(teoreticalVoxels.ToArray());
    }

    private VoxelTile RandomVoxelByWeight(VoxelTile[] stack)
    {
        List<float> chances = new List<float>();
        for (int i = 0; i < stack.Length; i++)
        {
            chances.Add(stack[i].Weight);
        }

        float value = Random.Range(0, chances.Sum());
        float sum = 0;

        for (int i = 0; i < chances.Count; i++)
        {
            sum += chances[i];
            if (value < sum)
            {
                return stack[i];
            }
        }

        return stack[stack.Length - 1];
    }

    private VoxelTile ChooseOneOfRandom(byte[] upper, byte[] lefter)
    {
        Stack<VoxelTile> teoreticalVoxels = new Stack<VoxelTile>();
       
            foreach (var prefab in TilePrefabs)
            {
                if (Enumerable.SequenceEqual(prefab.ColorsLeft, lefter)&&
                Enumerable.SequenceEqual(prefab.ColorsForward, upper))
                {
                    teoreticalVoxels.Push(prefab);
                }
            }

       
        if (teoreticalVoxels.Count == 0) { return null; }



        return RandomVoxelByWeight(teoreticalVoxels.ToArray());

    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.D))
        {
            foreach (VoxelTile spawnedTile in spawnedTiles)
            {
                if (spawnedTile != null) Destroy(spawnedTile.gameObject);
            }

            Generate();
        }
    }

}