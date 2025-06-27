using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.PlayerLoop;
using UnityEngine.Tilemaps;

[Serializable]
public class RoomTemplate {

    public RoomTemplate(){
        this.completed = false;
        if (maxInstances < minInstances) maxInstances = minInstances;
    }

    /*public void init() {
        //if(!isInBorder && !inInterior) inInterior = true;
        if(maxInstances < minInstances) maxInstances = minInstances;
    }*/
    public String name;

    public Vector2Int minSize, maxSize;

    [SerializeField]
    public int minInstances, maxInstances;

    /*public void setMin(int value){
        this.minInstances = value;
    }*/

    public float chance, minDoorDistance, maxDoorDistance;

    public TileBase tile;

    public bool isInBorder;

    private bool completed;

    public void setAsCompleted(){
        completed = true;
    }

    public bool isCompleted(){
        return completed;
    }

    [HideInInspector]
    public Walls dimensions;
}
