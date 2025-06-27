using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class SeedGenerator : MonoBehaviour
{

    public int seed;
    public bool randomSeed = false;

    void Awake() {
        if(!randomSeed){
            Random.InitState(seed);
            Debug.Log(seed);
        }
        else {
            Debug.Log("SEMILLA random");
        }
        
    }

}
