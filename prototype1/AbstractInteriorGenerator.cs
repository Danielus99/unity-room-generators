using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public abstract class AbstractInteriorGenerator : MonoBehaviour
{
    public Tilemap tilemap;

    public TileBase defaultTile;

    public abstract void executeAlgorithm();  

}
