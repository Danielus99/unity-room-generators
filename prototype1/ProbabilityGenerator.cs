using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UIElements;

public class ProbabilityGenerator : AbstractInteriorGenerator
{

    public enum RoomsPhase
    {
        BORDERS,
        INSIDE
    }

    private RoomsPhase currentPhase = RoomsPhase.BORDERS;

    [SerializeField]
    Walls[] extWalls;

    [SerializeField]
    List<RoomTemplate> roomTemplates;

    List<RoomTemplate> instancedRooms = new List<RoomTemplate>();

    [SerializeField]
    Vector2Int mainDoor;

    [SerializeField]
    bool canRoomSpawnOnMainDoor;

    int totalTiles, perimeter;

    HashSet<Vector2Int> visited = new HashSet<Vector2Int>();

    static readonly Vector2Int[] cardinalPoints = {Vector2Int.up,Vector2Int.down,Vector2Int.left,Vector2Int.right};

    private Dictionary<Vector2Int, HashSet<string>> tileData = new Dictionary<Vector2Int, HashSet<string>>();

    //-----------------UNITY-----------------

    // Start is called before the first frame update
    void Start()
    {
        Debug.Log("Start de generador");
        if(prepareAlgorithm()) executeAlgorithm();
        else Debug.Log("Falló la carga del algoritmo");
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public override void executeAlgorithm()
    {
        totalTiles = tileData.Count;
        Debug.Log("Total: "+tileData.Count);
        Debug.Log("Path to door test: "+existDoorPath(new Vector2Int(2,2)));

        //Border exploration (left,up,right,down)
        spawnRooms();
        

    }   

    private void borderExploration(Vector2Int point){
        /*if(point != doorPosition){
            roomSpawnChance(point,perimeter);
        }*/
        visited.Add(point);
        if(isBorder(point+new Vector2Int(-1,0)) && !visited.Contains(point+new Vector2Int(-1,0))) borderExploration(point+new Vector2Int(-1,0));
        else if(isBorder(point+new Vector2Int(0,1)) && !visited.Contains(point+new Vector2Int(0,1))) borderExploration(point+new Vector2Int(0,1));
        else if(isBorder(point+new Vector2Int(1,0)) && !visited.Contains(point+new Vector2Int(1,0))) borderExploration(point+new Vector2Int(1,0));
        else if(isBorder(point+new Vector2Int(0,-1)) && !visited.Contains(point+new Vector2Int(0,-1))) borderExploration(point+new Vector2Int(0,-1));
    }

    private bool prepareAlgorithm() {
        fixWallFormat();
        verifyTiles();
        prepareRooms();
        return isValidDoor();
    }

    //-----------------ROOMS-----------------

     private void prepareRooms(){
        for(int i = 0; i < roomTemplates.Count;i++){
            
            if(Random.Range(0f,100f)>roomTemplates[i].chance){
                roomTemplates.Remove(roomTemplates[i]);
                i--;
            }
            /*else{
                roomTemplates[i].init();
            }*/
        }
    }

    void spawnRooms() {

        List<RoomTemplate> rooms = new List<RoomTemplate>();
        visited = new HashSet<Vector2Int>();
        Vector2Int aux = mainDoor;
        switch(currentPhase) {
            case RoomsPhase.BORDERS:
                
                for(int i = 0; i < roomTemplates.Count; i++){
                    if(roomTemplates[i].isInBorder) rooms.Add(roomTemplates[i]);
                }
                
                borderExploration(aux);
            break;

            default:

            break;
        }
        
        while(rooms.Count != 0){
            RoomTemplate selected = rooms[Random.Range(0,rooms.Count)];
            Vector2Int tile = getRandomFromHashSet(visited);
            
            if (expandRoomFromTile(selected,tile)) {
                rooms.Remove(selected);
            }
            else
                Debug.Log("Algo salió mal");
            
        }
        visited.Clear();
        
    }

    void printRoom(RoomTemplate value){
        for(int i = value.dimensions.left; i <= value.dimensions.right;i++)
            for(int j = value.dimensions.down; j <= value.dimensions.up;j++)
                addTileToRoom(value,new Vector2Int(i,j));
    }

    bool expandRoomFromTile(RoomTemplate room, Vector2Int tilePos){
        bool rotateRoom = Random.value > 0.5f;
        int attempts = 0;
        bool completed = false;
        bool firstDirection = Random.value > 0.5f;
        int count;
        bool touchObstacle;
        Walls aux;

        int roomLength = Random.Range(room.minSize.x,room.maxSize.x+1);
        

        do {

            room.dimensions.up = tilePos.y;
            room.dimensions.down = tilePos.y;
            room.dimensions.left = tilePos.x;
            room.dimensions.right = tilePos.x;
            aux = new Walls(room.dimensions);

            touchObstacle = false;

            Debug.Log("iniciando "+room.name + ": up "+aux.up+" down "+aux.down+" left "+aux.left+" right "+aux.right);

            switch(attempts) {
                case 3:
                case 1:
                    Debug.Log("Cambiamos sentido");
                    firstDirection = !firstDirection;
                break;
                case 2:
                    Debug.Log("Rotamos");
                    rotateRoom = !rotateRoom;
                break;
                default:
                break;
            }

            count = 1;
            
            while(count < roomLength && !touchObstacle){
                if(rotateRoom) {
                    if (firstDirection) {
                        aux.up++;
                    } else {
                        aux.down--;
                    }
                } else {
                    if (firstDirection) {
                        aux.right++;
                    } else {
                        aux.left--;
                    }
                }

                Debug.Log("Expandiendo "+room.name+": up "+aux.up+" down "+aux.down+" left "+aux.left+" right "+aux.right+validWallRoomInstance(aux));

                if (!validWallRoomInstance(aux)) touchObstacle = true;
                else {
                    count++;
                    Debug.Log("Continúo");
                }
            }

            if(touchObstacle && count < roomLength) attempts++;
            else completed = true;

        } while (attempts < 4 && !completed);

        //Check attempts in X axis
        if (attempts >= 4) return false;

        //Position Y axis of template
        roomLength = Random.Range(room.minSize.y,room.maxSize.y+1);
        firstDirection = Random.value > 0.5f;
        Debug.Log("FASE 2");
        
        completed = false;
        count = 1;
        Walls aux2;
        attempts = 0;
        do {
            aux2 = new Walls(aux);
            touchObstacle = false;

            switch(attempts) {
                case 1:
                    firstDirection = !firstDirection;
                break;
                default:
                break;
            }

            while(count < roomLength && !touchObstacle){
                    
                if(rotateRoom) {
                    if (firstDirection) {
                        aux2.right++;
                    } else {
                        aux2.left--;
                    }
                } else {
                    if (firstDirection) {
                        aux2.up++;
                    } else {
                        aux2.down--;
                    }
                }
                Debug.Log("Expandiendo up "+aux2.up+" down "+aux2.down+" left "+aux2.left+" right "+aux2.right+validWallRoomInstance(aux2));

                if (!validWallRoomInstance(aux2)) touchObstacle = true;
                else {
                    count++;
                }
            }
            Debug.Log("New attempt? "+(touchObstacle && count < roomLength));
            if(touchObstacle && count < roomLength) attempts++;
            else completed = true;


        } while (attempts < 2 && !completed);

        //Check attempts in Y axis
        if (attempts >= 2) return false;
        else {
            room.dimensions = aux2;
            instancedRooms.Add(room);
            printRoom(room);
            return true;
        }


        /* Código primera propuesta (inacabado)
        //Position x axis of template
        int roomLength = Random.Range(room.minSize.x,room.maxSize.x+1);
        
        do {
            switch(attempts) {
                case 3:
                case 1:
                    firstDirection = !firstDirection;
                break;
                case 2:
                    rotateRoom = !rotateRoom;
                break;
                default:
                break;
            }

            if(rotateRoom) {
                if (firstDirection) {
                    direction = Vector2Int.up;
                    room.dimensions.down = tilePos.y;

                    room.dimensions.up = tilePos.y;
                } else {
                    direction = Vector2Int.down;
                    room.dimensions.up = tilePos.y;

                    room.dimensions.down = tilePos.y;
                }
            } else {
                if (firstDirection) {
                    direction = Vector2Int.right;
                    room.dimensions.left = tilePos.x;

                    room.dimensions.right = tilePos.x;
                } else {
                    direction = Vector2Int.left;
                    room.dimensions.right = tilePos.x;

                    room.dimensions.left = tilePos.x;
                }
            }
            int count = 1;
            bool touchObstacle = false;
            while(count < roomLength || !touchObstacle){
                if(tileData.ContainsKey(aux+direction)){
                    if(tileData[aux+direction].Count == 0){
                        aux = aux + direction;
                        count++;
                        
                        if(rotateRoom) {
                            if (firstDirection) {
                                room.dimensions.down = tilePos.y;
                            } else {
                                room.dimensions.up = tilePos.y;
                            }
                        } else {
                            if (firstDirection) {
                                room.dimensions.right = aux.x;
                            } else {
                                room.dimensions.left = aux.x;
                            }
                        }

                    }  else touchObstacle = true;
                } else touchObstacle = true;
            }

            if(touchObstacle && count < roomLength) attempts++;
            else completed = true;

        } while (attempts < 4 && !completed);

        //Check attempts in X axis
        if (attempts >= 4) return false;

        //Position Y axis of template
        roomLength = Random.Range(room.minSize.y,room.maxSize.y+1);
        firstDirection = Random.value > 0.5f;
        
        if(rotateRoom) {
            if (firstDirection) {
                direction = Vector2Int.up;
                room.dimensions.down = tilePos.y;
            } else {
                direction = Vector2Int.down;
                room.dimensions.up = tilePos.y;
            }
        } else {
            if (firstDirection) {
                direction = Vector2Int.up;
                room.dimensions.down = tilePos.y;
            } else {
                direction = Vector2Int.down;
                room.dimensions.up = tilePos.y;
            }
        }
        
        */

       
    }

    

    //-----------------TILE DATA-----------------

    void addTileToRoom(RoomTemplate room, Vector2Int tilePos){
        tileData[tilePos].Add(room.name);
        tilemap.SetTile(toVector3Int(tilePos),room.tile);
    }

    void removeTileToRoom(RoomTemplate room, Vector2Int tilePos){
        tileData[tilePos].Remove(room.name);
        tilemap.SetTile(toVector3Int(tilePos),defaultTile);
    }

    T getRandomFromHashSet<T>(HashSet<T> hashSet)
    {
        if (hashSet.Count == 0)
            return default;

        // Convert to a List
        List<T> list = new List<T>(hashSet);

        // Get a random index
        int randomIndex = Random.Range(0, list.Count);

        // Return the random item
        return list[randomIndex];
    }
    private void verifyTiles(){
        foreach(Walls room in extWalls){            
            for(int i = room.left; i <= room.right; i++){
                for(int j = room.down; j <= room.up; j++){
                    if(!tileData.ContainsKey(new Vector2Int(i,j))) {
                        tileData.Add(new Vector2Int(i,j),new HashSet<string>());
                        tilemap.SetTile(new Vector3Int(i,j,0),defaultTile);
                    } 
                }
            }
        }
    }

    private bool isValidDoor(){
        if(tileData.ContainsKey(mainDoor)) {
            Debug.Log("Pintando puerta...");
            tilemap.SetTileFlags(toVector3Int(mainDoor),TileFlags.None);
            tilemap.SetColor(toVector3Int(mainDoor),Color.red);
            for(int i = mainDoor.x-1; i <= mainDoor.x+1; i = i + 2){
                if(!tileData.ContainsKey(new Vector2Int(i,mainDoor.y))) return true;
            }

            for(int i = mainDoor.y-1; i <= mainDoor.y+1; i = i + 2){
                if(!tileData.ContainsKey(new Vector2Int(mainDoor.x,i))) return true;
            }
        }
        return false;
    }

    private bool isBorder(Vector2Int point){
        if(tileData.ContainsKey(point))
        for(int i = point.x-1; i <= point.x+1; i++){
            for(int j = point.y-1; j <= point.y+1; j++){
                if(new Vector2Int(i,j) != point && !tileData.ContainsKey(new Vector2Int(i,j))) return true;
            }
        }
        return false;
    }

    private bool existDoorPath(Vector2Int point, string tag = ""){

        if(point == mainDoor || !tileData.ContainsKey(point)){
            visited.Clear();
            return true;
        }
        else{
            visited.Add(point);
            foreach(Vector2Int cardinal in cardinalPoints){
                Vector2Int newPosition = point + cardinal;
                if(tileData.ContainsKey(newPosition)){
                    if(!visited.Contains(newPosition)){
                        if (tag != ""){
                            if (tileData[newPosition].Contains(tag)) return existDoorPath(newPosition,tag);
                        } 
                        else return existDoorPath(newPosition);
                    }
                }
            }
        }
        visited.Clear();
        return false;
    }

    //-----------------MATHS AREA-----------------

    bool validWallRoomInstance(Walls room){
        foreach(RoomTemplate instanced in instancedRooms){
            Debug.Log("Hay habitaciones ya instanciadas");
            if(intersection(room,instanced.dimensions) != null) return false;
        }
        return isWallInsideWalls(room);
    }

    private void fixWallFormat(){

        for(int i = 0; i < extWalls.Length;i++){
            extWalls[i].init();
        }
    }

    private bool isWallInsideWalls(Walls value){
        int result = 0;
        List<Walls> inwalls = new List<Walls>();
        Walls intersect;
        foreach(Walls room in extWalls){
            intersect = intersection(value,room);
            if(intersect != null) {
                inwalls.Add(intersect);
                result += surfaceArea(intersect);
            }
        }

        for(int i = 0; i < inwalls.Count; i++) {
            for(int j = i+1; j < inwalls.Count; j++){
                intersect = intersection(inwalls[i],inwalls[j]);
                if(intersect != null)
                    result -= surfaceArea(intersect);
            }
        }
        return result == surfaceArea(value);
    }

    private bool isInsideWalls(Vector2Int point,Walls walls){
        if(point.x >= walls.left && point.x <= walls.right && point.y >= walls.down && point.y <= walls.up) return true;
        return false;
    }

    /*
    private bool isInsideWalls(Vector2Int point){
        foreach(Walls room in extWalls){
            if(point.x >= room.left && point.x <= room.right && point.y >= room.down && point.y <= room.up) return true;
        }
        return false;
    }

    private int totalTiles(){
        int total = 0;
        foreach(Walls room in extWalls){
            total += surfaceArea(room);
        }
        for(int i=0;i<extWalls.Length-1;i++){
            for(int j = i + 1; j<extWalls.Length; j++){
                Walls? intersect = intersection(extWalls[i], extWalls[j]);
                if (intersect.HasValue) total -= surfaceArea(intersect.Value);
            }
        }
        return total;
    }*/

    private Vector3Int toVector3Int(Vector2Int vector){
        return new Vector3Int(vector.x,vector.y,0);
    }

    private int surfaceArea(Walls value){
        return (Mathf.Abs(value.up-value.down)+1)*(Mathf.Abs(value.right-value.left)+1);
    }
    private Walls intersection(Walls w1, Walls w2){
        Walls result = new Walls();
        int count = 0;

        if(isBetween(w1.left,w2.left,w2.right)){
            result.left = w1.left;
            count++;
        }
        else if(isBetween(w2.left,w1.left,w1.right)){
            result.left = w2.left;
            count++;
        }

        if(isBetween(w1.right,w2.left,w2.right)){
            result.right = w1.right;
            count++;
        }
        else if(isBetween(w2.right,w1.left,w1.right)){
            result.right = w2.right;
            count++;
        }
        
        if(isBetween(w1.up,w2.up,w2.down)){
            result.up = w1.up;
            count++;
        }
        else if(isBetween(w2.up,w1.up,w1.down)){
            result.up = w2.up;
            count++;
        }

        if(isBetween(w1.down,w2.up,w2.down)){
            result.down = w1.down;
            count++;
        }
        else if(isBetween(w2.down,w1.up,w1.down)){
            result.down = w2.down;
            count++;
        }

        if(count == 4) return result;
        return null;
    }

    private bool isBetween(int value, int limit1, int limit2){
        
        //Corrects the input if necessary
        if(limit1 > limit2){
            int aux = limit2;
            limit2 = limit1;
            limit1 = aux;
        }

        if(value >= limit1 && value <= limit2) return true;
        return false;
    }

    
}
