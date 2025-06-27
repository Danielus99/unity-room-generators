using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

public class HallwayGenerator : MonoBehaviour
{
    public TileBase floorTile;
    public TileBase wallTile;
    public GameObject defaultDoor;
    public Tilemap floorTilemap;
    public Tilemap wallsTilemap;
    public int roomSize;
    public Vector3Int origin;
    public int maxFloors = 1;
    [Range(0.0f, 1.0f)]
    public float newBranchProbablility;
    [Range(0.0f, 1.0f)]
    public float wallBetweenRoomsProbablility;
    [Range(0.0f, 1.0f)]
    public float directionPersistanceProbability;
    [Range(0.0f, 1.0f)]
    public float irrelevantDoorProbability;
    public RoomStyle[] roomsStyleTemplates;
    public List<RoomConfigs> roomsConfigurationsTemplate;

    List<RoomClass> rooms = new List<RoomClass>();
    Dictionary<Vector3Int, CellClass> cells = new Dictionary<Vector3Int, CellClass>();
    Vector3Int[] cardinalPoints = { Vector3Int.up, Vector3Int.down, Vector3Int.left, Vector3Int.right };
    Vector3Int globalAux;
    RoomStyleUtil roomStyleManager;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        roomStyleManager = new RoomStyleUtil(roomsStyleTemplates,wallTile,floorTile);
        Explore(origin);
        ConfigWalls();
        ConfigRooms();
        ConfigDoors();
        Print();
        InstantiatePrefabs();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void ConfigDoors()
    {
        List<RoomClass> sRooms = rooms.OrderBy(r => r.acceses.Count).ToList();
        HashSet<int> visited = new HashSet<int>();
        while(sRooms.Count != 0) //sRooms[0].acceses.Count < 2
        {
            List<int> accessKeys = new List<int>();
            foreach (int k in sRooms[0].acceses.Keys)
            {
                accessKeys.Add(k);
            }
            while (accessKeys.Count != 0)
            {
                int r = Random.Range(0, accessKeys.Count);
                int k = accessKeys[r];
                accessKeys.RemoveAt(r);
                if (!visited.Contains(k)/* && isDoorNeeded(sRooms[0], irrelevantDoorProbability)*/)
                {
                    if (isDoorNeeded(sRooms[0],k, irrelevantDoorProbability))
                    {
                        (Vector3Int, Vector3Int) tuple = sRooms[0].acceses[k][Random.Range(0, sRooms[0].acceses[k].Count)];
                        cells[tuple.Item1].wallStates[Vector3IntCardinalToIntIndex(tuple.Item2)] = 2;
                        cells[GetNeightbour(tuple.Item1, tuple.Item2)].wallStates[Vector3IntCardinalToIntIndex(-tuple.Item2)] = 2;
                        sRooms[0].acceses[k].Clear();
                        sRooms[0].acceses[k].Add(tuple);
                        rooms[k].acceses[sRooms[0].id].Clear();
                        rooms[k].acceses[sRooms[0].id].Add((GetNeightbour(tuple.Item1, tuple.Item2), -tuple.Item2));
                    } else
                    {
                        sRooms[0].acceses.Remove(k);
                        rooms[k].acceses.Remove(sRooms[0].id);
                    }
                    
                }
            }
            visited.Add(sRooms[0].id);
            sRooms.RemoveAt(0);
            
        }
    }

    private bool isDoorNeeded(RoomClass roomClass, int access, float irrelevantDoorProbability)
    {
        if (roomClass.acceses.Keys.Count == 1)
        {
            return true;
        } else
        {
            HashSet<int> visited = new HashSet<int>();
            Queue<int> q = new Queue<int>();
            visited.Add(roomClass.id);
            foreach (int r in roomClass.acceses.Keys)
            {
                if (r != access)
                {
                    q.Enqueue(r);
                }
            }
            while (q.Count != 0)
            {
                int node = q.Dequeue();
                visited.Add(node);
                foreach (int r in rooms[node].acceses.Keys)
                {
                    if (!visited.Contains(r))
                    {
                        q.Enqueue(r);
                    }
                }
            }
            if (visited.Count == rooms.Count)
                return Random.value < irrelevantDoorProbability;
            else return true;
        }

    }

    void ConfigRooms()
    {
        HashSet<Vector3Int> visited = new HashSet<Vector3Int>();
        Queue<Vector3Int> possibleRooms = new Queue<Vector3Int>();
        possibleRooms.Enqueue(origin);
        int roomCount = 0;
        while(visited.Count != cells.Count)
        {
            Vector3Int node = possibleRooms.Dequeue();
            if (!visited.Contains(node))
            {
                RoomClass room = new RoomClass();
                Queue<Vector3Int> roomQ = new Queue<Vector3Int>();
                roomQ.Enqueue(node);
                while (roomQ.Count != 0)
                {
                    node = roomQ.Dequeue();
                    visited.Add(node);
                    cells[node].roomIndex = roomCount;
                    room.cells.Add(node);
                    foreach (Vector3Int dir in NextDirections(node, false))
                    {
                        Vector3Int nextNode = GetNeightbour(node, dir);
                        if (!visited.Contains(nextNode))
                        {
                            if (cells[node].wallStates[Vector3IntCardinalToIntIndex(dir)] == 0)
                            {
                                roomQ.Enqueue(nextNode);
                            }
                            else
                            {
                                possibleRooms.Enqueue(nextNode);
                                room.acceses[-1].Add((node,dir));
                            }
                        }
                    }
                }
                room.id = roomCount;
                room.style = roomStyleManager.GetRandomStyle();
                rooms.Add(room);
                roomCount++;
            }
            
        }

        for(int j = 0; j < rooms.Count; j++)
        {
            RoomClass r = rooms[j];
            while(r.acceses[-1].Count > 0)
            {
                (Vector3Int, Vector3Int) acess = r.acceses[-1][0];
                r.acceses[-1].RemoveAt(0);

                Vector3Int nPosition = GetNeightbour(acess.Item1, acess.Item2);

                if (cells[nPosition].roomIndex != r.id)
                {
                    if (!r.acceses.ContainsKey(cells[nPosition].roomIndex))
                    {
                        r.acceses.Add(cells[nPosition].roomIndex, new List<(Vector3Int, Vector3Int)>());
                    }
                    r.acceses[cells[nPosition].roomIndex].Add(acess);
                    RoomClass nRoom = rooms[cells[nPosition].roomIndex];

                    if (!nRoom.acceses.ContainsKey(j))
                    {
                        nRoom.acceses.Add(j, new List<(Vector3Int, Vector3Int)>());
                    }
                    nRoom.acceses[j].Add((nPosition, -acess.Item2));
                }
            }
            r.acceses.Remove(-1);
        }

        //Put Configs in Rooms
        int aux = 0;
        List<int> roomsAvariable = new List<int>();
        foreach(RoomClass r in rooms)
        {
            roomsAvariable.Add(aux);
            aux++;
        }
        aux = 0;
        while (roomsConfigurationsTemplate.Count != 0 && roomsAvariable.Count != 0)
        {
            
  
            int index = Random.Range(0, roomsAvariable.Count);
            rooms[roomsAvariable[index]].config = roomsConfigurationsTemplate[aux];
            if (roomsConfigurationsTemplate[aux].style.walls != null && roomsConfigurationsTemplate[aux].style.floor != null)
                rooms[roomsAvariable[index]].style = roomsConfigurationsTemplate[aux].style;

            roomsAvariable.RemoveAt(index);
            roomsConfigurationsTemplate[aux].instances--;
            if (roomsConfigurationsTemplate[aux].instances == 0)
            {
                roomsConfigurationsTemplate.RemoveAt(aux);
            }
            else
            {
                aux++;
            }
            if (aux >= roomsConfigurationsTemplate.Count)
                aux = 0;
            
        }
        
    }

    void ConfigWalls()
    {
        foreach (Vector3Int r in cells.Keys)
        {
            List<Vector3Int> nextRooms = NextDirections(r, true);

            foreach (Vector3Int dir in nextRooms)
            {

                cells[r].wallStates[Vector3IntCardinalToIntIndex(dir)] = 1;
            }

            nextRooms = NextDirections(r, false);
            foreach (Vector3Int dir in nextRooms)
            {
                Vector3Int n = GetNeightbour(r, dir);
                if(cells[n].wallStates[Vector3IntCardinalToIntIndex(-dir)] == 0)
                    if(Random.value < wallBetweenRoomsProbablility)
                    {
                        cells[n].wallStates[Vector3IntCardinalToIntIndex(-dir)] = 1;
                        cells[r].wallStates[Vector3IntCardinalToIntIndex(dir)] = 1;
                    }                    
            }

        }
    }

    /*
    void Explore(Vector3Int spawn)
    {
        Stack<Vector3Int> pile = new Stack<Vector3Int>();
        Vector3Int aux = spawn;
        bool newCell;
        for (int i = 0; i < maxFloors; i++)
        {
            if (!rooms.ContainsKey(aux))
            {
                rooms.Add(aux, new RoomClass());
                pile.Push(aux);
                newCell = false;
                while (newCell == false)
                {
                    aux = NextMove(aux);
                    if (aux == spawn) //spawn works as null value here
                    {
                        aux = pile.Pop();
                    }
                    else
                    {
                        newCell = true;
                    }
                }
            }
            
        }
    }
    */

    void Explore(Vector3Int spawn)
    {
        List<Vector3Int> pile = new List<Vector3Int>();
        Vector3Int aux = spawn;
        globalAux = cardinalPoints[Random.Range(0, cardinalPoints.Length)];
        bool newCell;
        for (int i = 0; i < maxFloors; i++)
        {
            if (!cells.ContainsKey(aux))
            {
                cells.Add(aux, new CellClass());
                cells[aux].position = aux;
                pile.Add(aux);
                newCell = false;
                while (newCell == false)
                {
                    aux = NextMove(pile,directionPersistanceProbability);
                    if (!pile.Contains(aux)) //spawn works as null value here
                    {
                        newCell = true;
                    }
                }
            }

        }
        Debug.Log(pile.Count);
    }

    Vector3Int NextMove(Vector3Int point, float directionPersistanceProbability)
    {
        Vector3Int next = point + (globalAux * roomSize);
        
        if (!cells.ContainsKey(next) && Random.value < directionPersistanceProbability)
        {
            return next;
        }

        List<Vector3Int> possibleDirection = NextDirections(point, true);

        if (possibleDirection.Count > 0)
        {
            Vector3Int dir = possibleDirection[Random.Range(0, possibleDirection.Count)];
            globalAux = dir;
            return point + (dir * roomSize);
        }
        else
            return point;
        
    }

    Vector3Int NextMove(List<Vector3Int> maze, float directionPersistanceProbability)
    {
        Vector3Int point;
        if (Random.value > newBranchProbablility)
            point = maze[maze.Count -1];
        else
        {
            point = maze[Random.Range(0, maze.Count)];
        }

        return NextMove(point, directionPersistanceProbability);

    }

    List<Vector3Int> NextDirections(Vector3Int point, bool getFreeDirs)
    {
        List<Vector3Int> possibleDirection = new List<Vector3Int>();
        foreach (Vector3Int dir in cardinalPoints)
        {
            Vector3Int aux = GetNeightbour(point,dir);
            if ((getFreeDirs && !cells.ContainsKey(aux)) || (!getFreeDirs && cells.ContainsKey(aux))) possibleDirection.Add(dir);
        }
        return possibleDirection;
    }

    Vector3Int GetNeightbour(Vector3Int point, Vector3Int cardinal)
    {
        return point + (cardinal * roomSize);
    }

    int Vector3IntCardinalToIntIndex(Vector3Int cardinal)
    {
        if (cardinal == Vector3Int.up)
            return 0;
        else if (cardinal == Vector3Int.down)
            return 1;
        else if (cardinal == Vector3Int.left)
            return 2;
        else if (cardinal == Vector3Int.right)
            return 3;
        else
            return -1;
    }

    void Print()
    {

        HashSet<Vector3> doors = new HashSet<Vector3>();
        foreach (RoomClass room in rooms)
        {
            foreach (Vector3Int cell in room.cells)
            {
                //Print floor
                for (int i = 0; i < roomSize; i++)
                {
                    for (int j = 0; j < roomSize; j++)
                    {

                        Vector3Int aux = cell + new Vector3Int(i, j, 0);
                        floorTilemap.SetTile(aux, room.style.floor);
                    }
                }

                //Print walls
                for (int card = 0; card < 4; card++)
                {
                    if (cells[cell].wallStates[card] != 0)
                    {
                        Vector3Int aux = cell;
                        if (card == 0)
                            aux = cell + new Vector3Int(0, roomSize - 1, 0);
                        else if (card == 3)
                            aux = cell + new Vector3Int(roomSize - 1, 0, 0);
                        for (int i = 0; i < roomSize; i++)
                        {
                            Vector3Int j = aux;
                            if (card == 0 || card == 1)
                                j += new Vector3Int(i, 0, 0);
                            else
                                j += new Vector3Int(0, i, 0);

                            //Custon patterns
                            if (cells[cell].wallStates[card] == 2 && (
                                (roomSize % 2 != 0 && i == roomSize / 2)
                                ||
                                (roomSize % 2 == 0 && ((i == roomSize / 2) || (i == (roomSize / 2) - 1)))
                                ))
                            {
                                if (defaultDoor != null)
                                {
                                    Vector3 doorPos = j + new Vector3(0.5f,0.5f,0) + new Vector3(0.5f * cardinalPoints[card].x, 0.5f * cardinalPoints[card].y, 0);
                                    if (!doors.Contains(doorPos))
                                    {
                                        Instantiate(defaultDoor, doorPos, Quaternion.identity);
                                        doors.Add(doorPos);
                                    }
                                }
                            }
                            else
                                wallsTilemap.SetTile(j, room.style.walls);
                        }
                    }


                }
            }

            
        }
    }
    
    void InstantiatePrefabs()
    {

        foreach (RoomClass room in rooms)
        {
            //Print instances
            if (room.config != null)
            {
                List<Vector3Int> avariable = new List<Vector3Int>();
                foreach (Vector3Int cell in room.cells)
                {
                    for (int i = 1; i < roomSize - 1; i++)
                    {
                        for (int j = 1; j < roomSize - 1; j++)
                        {
                            avariable.Add(cell + new Vector3Int(i, j, 0));
                        }
                    }
                    /*
                    for (int i = 0; i < 4; i++)
                    {
                        Vector3Int init = cell;
                        int order = 1;
                        switch (i)
                        {
                            case 0:
                            case 3:
                                order = -1;
                                init += new Vector3Int(roomSize - 1, roomSize - 1, 0);
                                break;
                            default:
                                break;
                        }
                        if (cells[cell].wallStates[i] == 0)
                        {
                            for (int j = 0; j < roomSize; j++)
                            {
                                switch (i)
                                {
                                    case 0:
                                    case 1:
                                        if (wallsTilemap.GetTile(init + new Vector3Int(j * order, 0, 0)) == null)
                                            avariable.Add(init + new Vector3Int(j*order, 0, 0));
                                        break;
                                    default:
                                        if (wallsTilemap.GetTile(init + new Vector3Int(0, j * order, 0)) == null)
                                            avariable.Add(init + new Vector3Int(0, j * order, 0));
                                        break;
                                }
                            }
                        }

                    }*/
                }
                int usableSize = roomSize - 2;
                foreach (MazeObject obj in room.config.objects)
                {
                    int i = 0;
                    while (i < obj.instances && avariable.Count != 0)
                    {
                        int cellIndex = Random.Range(0, avariable.Count);
                        Vector3Int cell = avariable[cellIndex];
                        Instantiate(obj.prefab, cell + new Vector3(0.5f, 0.5f, 0), Quaternion.identity);
                        avariable.RemoveAt(cellIndex);
                        i++;

                    }
                }

            }
        }
            
        /*
        //Print instances
        if (room.config != null)
        {
            List<Vector3Int> avariable = new List<Vector3Int>();
            foreach (Vector3Int cell in room.cells)
            {
                for (int i = 1; i < roomSize - 1; i++)
                {
                    for (int j = 1; j < roomSize - 1; j++)
                    {
                        avariable.Add(cell + new Vector3Int(i, j, 0));
                    }
                }
            }
            int usableSize = roomSize - 2;
            foreach (MazeObject obj in room.config.objects)
            {
                int i = 0;
                while (i < obj.instances && avariable.Count != 0)
                {
                    bool validPos = false;
                    while (!validPos)
                    {
                        Vector3Int cell = room.cells[Random.Range(0, room.cells.Count)];

                        List<int> borders = new List<int>();
                        if (obj.inWall)
                        {
                            for (int j = 0; j < 4; j++)
                            {
                                if (cells[cell].wallStates[j] != 0)
                                {
                                    borders.Add(j);
                                }
                            }
                            if (borders.Count != 0)
                            {
                                Vector3Int dir = cardinalPoints[borders[Random.Range(0, borders.Count)]];
                                if (dir.x == 0)
                                {
                                    dir.x = Random.Range(1, roomSize - 1);
                                    if (dir.y > 0)
                                        dir.y = roomSize - 2;
                                    else
                                        dir.y = 1;
                                }
                                else
                                {
                                    dir.y = Random.Range(1, roomSize - 1);
                                    if (dir.x > 0)
                                        dir.x = roomSize - 2;
                                    else
                                        dir.x = 1;
                                }
                                cell = cell + dir;
                                if (!used.Contains(cell))
                                {
                                    validPos = true;
                                    Instantiate(obj.prefab, cell + new Vector3(0.5f,0.5f,0), Quaternion.identity);
                                    used.Add(cell);
                                }

                            }

                        }
                        else
                        {
                            cell = cell + new Vector3Int(Random.Range(1, roomSize - 1), Random.Range(1, roomSize - 1), 0);
                            if (!avariable.Contains(cell))
                            {
                                validPos = true;
                                Instantiate(obj.prefab, cell + new Vector3(0.5f, 0.5f, 0), Quaternion.identity);
                                avariable.Add(cell);
                            }*/
    }

    void ExternalWallsOld()
    {
        foreach (Vector3Int r in cells.Keys)
        {
            List<Vector3Int> emptyDirections = NextDirections(r, true);
            foreach (Vector3Int dir in emptyDirections)
            {
                Vector3Int aux = r;

                if (dir == cardinalPoints[0])
                    aux = r + new Vector3Int(0, roomSize - 1, 0);
                else if (dir == cardinalPoints[3])
                    aux = r + new Vector3Int(roomSize - 1, 0, 0);

                for (int i = 0; i < roomSize; i++)
                {
                    Vector3Int j = aux;
                    if (dir == Vector3Int.up || dir == Vector3Int.down)
                        j += new Vector3Int(i, 0, 0);
                    else
                        j += new Vector3Int(0, i, 0);
                    wallsTilemap.SetTile(j, wallTile);
                }

            }

        }
    }

}
