using System.Collections.Generic;
using UnityEngine;


[System.Serializable]
public class MazeObject
{
    public GameObject prefab;
    public int instances;
    //public bool inWall;
}

[System.Serializable]
public class RoomConfigs
{
    
    public List<MazeObject> objects;
    public int instances;
    public RoomStyle style;
}
