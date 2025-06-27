using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class RoomClass
{
    [HideInInspector] public int id;
    [HideInInspector] public List<Vector3Int> cells = new List<Vector3Int>();
    [HideInInspector] public Dictionary<int, List<(Vector3Int,Vector3Int)>> acceses = new Dictionary<int, List<(Vector3Int, Vector3Int)>>(); //adjacent room index, position of owned cell, direction to acess the other room
    public RoomStyle style;
    public RoomConfigs config;

    public RoomClass()
    {
        acceses.Add(-1, new List<(Vector3Int, Vector3Int)>());
    }

}
