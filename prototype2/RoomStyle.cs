using UnityEngine;
using UnityEngine.Tilemaps;

[System.Serializable]
public class RoomStyle
{
    public TileBase walls;
    public TileBase floor;
    [Range(0.0f, 1.0f)]
    public float apparitionProbability;
    public bool specialStyle;

    public RoomStyle(TileBase wall, TileBase floor)
    {
        this.walls = wall;
        this.floor = floor;
    }
}
