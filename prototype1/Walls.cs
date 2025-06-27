using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Walls {

    public Walls(){}

    public void init(){
        int aux;
        if(left > right) {
            aux = right;
            right = left;
            left = aux;
        }
        if(down > up) {
            aux = up;
            up = down;
            down = aux;
        }
    }
    public Walls(int up,int down,int left,int right){
        this.up = up;
        this.down = down;
        this.left = left;
        this.right = right;
    }
    
    public Walls(Walls value) {
        this.up = value.up;
        this.down = value.down;
        this.left = value.left;
        this.right = value.right;
    }

    public int up, down, left, right;
}
