//using System;
using System.Collections;
using System.Collections.Generic;
using Unity.AI.Navigation;
using Unity.VisualScripting;
using UnityEditor;
using UnityEditor.Overlays;
using UnityEngine;

public class DungeonGenerator : MonoBehaviour
{

    [SerializeField] private RectInt room = new RectInt(0, 0, 100, 50);

    [SerializeField] private List<RectInt> toDo = new();
    [SerializeField] private List<RectInt> generatedRooms = new();
    [SerializeField] private List<RectInt> doors = new();
    [SerializeField] private List<RectInt> roomsToRemove = new();

    [Header("Room generation")]
    [Range(10,100)]
    [SerializeField] private int minRoomSize;

    [Range(20, 120)]
    [SerializeField] private int MaxRoomSize;
    [SerializeField] private int roomOverlap;

    private bool widthSplit;

    [Header("Door generation")]
    //[SerializeField] private int doorOffset;
    [SerializeField] private int doorWidth;

    [Header("Removing doors")]
    [SerializeField] private int removeRoomAmount;

    [Header("Assets")]
    [SerializeField] private NavMeshSurface navMeshSurface;

    [SerializeField] private GameObject player;

    [SerializeField] private GameObject wall;
    [SerializeField] private GameObject floor;

    private HashSet<Vector3> spawnLocations;

    [SerializeField] private Transform wallParent;
    [SerializeField] private Transform floorParent;

    [SerializeField] float buffer = 0.5f;
    [SerializeField] float floorHeight = -0.5f;

    private Graph<Vector3> graph;

    private void Start()
    {                
        graph = new Graph<Vector3>();
        toDo.Add(room);
        StartGenerate();
    }

    private void OnValidate()
    {
        if (minRoomSize > MaxRoomSize - 10) MaxRoomSize = minRoomSize + 10;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            StopAllCoroutines();
            StartGenerate();
        }

    }

    //[ContextMenu("Generate Dungeon")]
    public void StartGenerate()
    {
        doorWidth = minRoomSize / 3;
        //doorOffset = minRoomSize / 5;

        toDo.Clear();
        generatedRooms.Clear();
        doors.Clear();

        graph.Clear();
        DebugDrawingBatcher.GetInstance().ClearAllBatchedCalls();

        toDo.Add(room);
        StartCoroutine(GenerateDungeon());
    }

    public IEnumerator GenerateDungeon()
    {
        yield return new WaitUntil(SpacePress);
        while (toDo.Count > 0)
        {

            RectInt newRoom = toDo[0];
            RectInt newOtherRoom = toDo[0];

            if (toDo[0].width > MaxRoomSize && toDo[0].height < MaxRoomSize)
                widthSplit = true;
            else if (toDo[0].height > MaxRoomSize && toDo[0].width < MaxRoomSize)
                widthSplit = false;

            if (widthSplit)
            {
                int randomRoomSize = Random.Range(minRoomSize, newRoom.width - minRoomSize);
                
                // New room changes
                newRoom.width = randomRoomSize;

                // Other new room changes
                newOtherRoom.xMin = newRoom.width + newRoom.xMin;
                newOtherRoom.width = toDo[0].width - newRoom.width;

                newRoom.width += roomOverlap;
                //newOtherRoom.xMin -= 1;
            }
            else
            {
                int randomRoomSize = Random.Range(minRoomSize, newRoom.height - minRoomSize);

                // New room changes
                newRoom.height = randomRoomSize;


                // Other new room changes
                newOtherRoom.yMin = newRoom.height + newRoom.yMin;
                newOtherRoom.height = toDo[0].height - newRoom.height;

                newRoom.height += roomOverlap;
                //newOtherRoom.yMin -= 1;
            }          

            generatedRooms.Add(newOtherRoom);
            generatedRooms.Add(newRoom);

            toDo.Remove(toDo[0]);
            foreach (var ro in generatedRooms)
            {
                if (ro.width > MaxRoomSize || ro.height > MaxRoomSize)
                {
                    toDo.Insert(0, ro);
                }
            }
            foreach (var ro in toDo)
            {
                generatedRooms.Remove(ro);
            }

            widthSplit = !widthSplit;

            yield return new WaitForSeconds(0.05f);
        }

        foreach (var ro in generatedRooms)
        {
            graph.AddNode(GetRoomCenter(ro));
        }

        Debug.Log("Rooms: " + generatedRooms.Count);

        yield return new WaitUntil(SpacePress);
        StartCoroutine(GenerateDoors());
    }

    private Vector3 GetRoomCenter(RectInt ro)
    {
        return new Vector3(
            ro.xMin + (ro.width / 2),
            0,
            ro.yMin + (ro.height / 2)
        );
    }

    public IEnumerator GenerateDoors()
    {
        //doorWidth = minRoomSize / 3;
        for (int i = 0; i < generatedRooms.Count; i++)
        {
            Vector3 iRoomCenter = GetRoomCenter(generatedRooms[i]);

            for (int j = i + 1; j < generatedRooms.Count; j++)
            {
                Vector3 jRoomCenter = GetRoomCenter(generatedRooms[j]);

                if (AlgorithmsUtils.Intersects(generatedRooms[i], generatedRooms[j]))
                {
                    RectInt door = AlgorithmsUtils.Intersect(generatedRooms[i], generatedRooms[j]);
                    if (door.width <= doorWidth + (roomOverlap * 2) && door.height <= doorWidth + (roomOverlap * 2))    continue;     // Checks if the intersect is to small to add a door
                    
                    //yield return new WaitUntil(SpacePress);
                    Vector3 doorCenter = new();
                    if (door.width == roomOverlap)
                    {
                        int min = door.yMin + roomOverlap;
                        int max = door.yMax - roomOverlap - doorWidth;
                        int randomDoorSpawn = Random.Range(min, max);     // Gets a valid random spawn not overlapping with any corners

                        door.yMin = (randomDoorSpawn);
                        door.height = doorWidth;

                        doorCenter = new Vector3(
                            door.xMin + (roomOverlap / 2),
                            0,
                            door.yMin + (doorWidth / 2)
                        );
                    }
                    if (door.height == roomOverlap)
                    {
                        int min = door.xMin + roomOverlap;
                        int max = door.xMax - roomOverlap - doorWidth;
                        int randomDoorSpawn = Random.Range(min, max);     // Gets a valid random spawn not overlapping with any corners

                        door.xMin = (randomDoorSpawn);
                        door.width = doorWidth;

                        doorCenter = new Vector3(
                            door.xMin + (doorWidth / 2),
                            0,
                            door.yMin + (roomOverlap / 2)
                        );
                    }
                    doors.Add(door);

                    graph.AddNode(doorCenter);

                    graph.AddEdge(iRoomCenter, doorCenter);
                    graph.AddEdge(jRoomCenter, doorCenter);

                    yield return new WaitForSeconds(0.05f);
                }                
            }
        }

        Debug.Log("Doors: " + doors.Count);

        yield return new WaitUntil(SpacePress);
        StartCoroutine(GenerateConections());
    }

    public IEnumerator GenerateConections()
    {
        int randomStartPoint = Random.Range(0, graph.GetNodes().Count);
        Vector3 startNode = graph.GetNodes().ToArray()[randomStartPoint];
        bool connected = graph.BFS(startNode);

        foreach (var node in graph.GetVisited())
        {
            foreach (var edge in graph.GetNeighbors(node))
            {
                DebugDrawingBatcher.GetInstance().BatchCall(
                    () => Debug.DrawLine(node, edge, color: Color.red)
                );
                yield return new WaitForSeconds(0.01f);
            }
        }         
        Debug.Log("Connected: " + connected);
        yield return new WaitUntil(SpacePress);
        SpawnDungeonAssets();
    }

    public IEnumerator RemoveSmallRooms()
    {
        foreach (var room in generatedRooms)
        {
            yield return new WaitUntil(SpacePress);

            int roomSize = room.x + room.y;
            if (roomsToRemove.Count == 0)
            {
                roomsToRemove.Add(room);
                continue;
            }
            else
            {
                for (int i = 0; i < roomsToRemove.Count; i++) 
                {
                    RectInt ro = roomsToRemove[i];
                    int roRoomSize = ro.x + ro.y;
                    if (roomSize < roRoomSize) 
                        roomsToRemove.Insert(i, room);
                }
            }            
        }

        for (int i = roomsToRemove.Count; i > removeRoomAmount; i--) 
            roomsToRemove.RemoveAt(i);

        // Remove the rooms that are left from generated rooms and remove the nodes and edges
    }

    public void SpawnDungeonAssets()
    {
        spawnLocations = new();

        foreach (var room in generatedRooms)
        {
            SpawnWallsForRoom(room);
        }
        foreach (var room in generatedRooms)
        {
            SpawnFloorsForRoom(room);
        }
        BakeNavMesh();

        player.transform.position = new Vector3(5, 2, 5);
    }

    private void SpawnWallsForRoom(RectInt room)
    {        
        foreach (var door in doors)
        {
            Vector3 doorPosition = new();
            for (int i = 0; i < doorWidth; i++)
            {
                if (door.width == doorWidth)
                {
                    doorPosition = new Vector3(door.position.x + buffer + i, buffer, door.position.y + buffer);
                }
                else
                {
                    doorPosition = new Vector3(door.position.x + buffer, buffer, door.position.y + buffer + i);
                }
                spawnLocations.Add(doorPosition);
            }
        }

        for (int i = room.xMin; i < room.xMax; i++)
        {
            Vector3 spawnPointTop = new Vector3(i + buffer, buffer, room.yMax - buffer);
            Vector3 spawnPointBot = new Vector3(i + buffer, buffer, room.yMin + buffer);
            SpawnWall(spawnPointTop);
            SpawnWall(spawnPointBot);
        }
        for (int i = room.yMin; i < room.yMax; i++)
        {
            Vector3 spawnPointRight = new Vector3(room.xMax - buffer, buffer, i + buffer);
            Vector3 spawnPointLeft = new Vector3(room.xMin + buffer, buffer, i + buffer);
            SpawnWall(spawnPointRight);
            SpawnWall(spawnPointLeft);
        }
    }

    private void SpawnFloorsForRoom(RectInt room)
    {
        for (int i = room.xMin + 1; i < room.xMax - 1; i++)
        {
            for (int j = room.yMin + 1; j < room.yMax - 1; j++)
            {
                Vector3 spawnPoint = new Vector3(i + buffer, floorHeight, j + buffer);
                SpawnFloor(spawnPoint);
            }
        }

        foreach (var door in doors)
        {
            Vector3 doorFloor = new();
            for (int i = 0; i < doorWidth; i++)
            {
                if (door.width == doorWidth)
                {
                    doorFloor = new Vector3(door.position.x + buffer + i, floorHeight, door.position.y + buffer);
                }
                else
                {
                    doorFloor = new Vector3(door.position.x + buffer, floorHeight, door.position.y + buffer + i);
                }
                SpawnFloor(doorFloor);
            }

        }

    }


    private void SpawnWall(Vector3 spawnPoint)
    {
        if (spawnLocations.Contains(spawnPoint)) return;

        spawnLocations.Add(spawnPoint);
        var newWall = Instantiate(wall, spawnPoint, Quaternion.identity, wallParent);        
    }

    private void SpawnFloor(Vector3 spawnPoint)
    {
        if (spawnLocations.Contains(spawnPoint)) return;

        spawnLocations.Add(spawnPoint);
        var newFloor = Instantiate(floor, spawnPoint, Quaternion.identity, floorParent);
    }

    private void BakeNavMesh()
    {
        navMeshSurface.BuildNavMesh();
    }

    private void OnDrawGizmos()
    {
        foreach (var ro in generatedRooms) AlgorithmsUtils.DebugRectInt(ro, Color.blue);
        foreach (var ro in toDo) AlgorithmsUtils.DebugRectInt(ro, Color.red);
        foreach (var door in doors) AlgorithmsUtils.DebugRectInt(door, Color.green);        
    }

    public bool SpacePress()
    {
        if (Input.GetKeyDown(KeyCode.Space))
            return true;
        else
            return false;
    }
}