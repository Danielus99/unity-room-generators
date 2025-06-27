using UnityEngine;

public class CellClass
{
    public Vector3Int position;
    public int[] wallStates = new int[4] {0,0,0,0}; //up, down, left, right
    public int roomIndex;
    
}
