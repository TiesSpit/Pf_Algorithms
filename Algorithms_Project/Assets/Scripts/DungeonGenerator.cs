using System.Collections.Generic;
using System.Collections;
using UnityEngine;

public class DungeonGenerator : MonoBehaviour
{

    [SerializeField] private RectInt room = new RectInt(0, 0, 100, 50);

    [SerializeField] private List<RectInt> toDo = new();
    [SerializeField] private List<RectInt> generatedRooms = new();
    [SerializeField] private List<RectInt> doors = new();

    [Header("Room generation")]
    [SerializeField] private int minRoomSize = 10;
    [SerializeField] private int MaxRoomSize;
    [SerializeField] private int roomOverlap;

    private bool widthSplit;

    [Header("Door generation")]
    [SerializeField] private int doorOffset;
    [SerializeField] private int doorWidth;

    private void Start()
    {
        toDo.Add(room);
        StartGenerate();
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
        toDo.Clear();
        generatedRooms.Clear();
        doors.Clear();
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


            generatedRooms.Insert(0, newOtherRoom);
            generatedRooms.Insert(0, newRoom);

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

            yield return new WaitForSeconds(0.1f);
        }
        yield return new WaitUntil(SpacePress);
        StartCoroutine(GenerateDoors());
    }

    public IEnumerator GenerateDoors()
    {
        for (int i = 0; i < generatedRooms.Count; i++)
        {
            for (int j = i + 1; j < generatedRooms.Count; j++)
            {
                if (AlgorithmsUtils.Intersects(generatedRooms[i], generatedRooms[j]))
                {

                    RectInt door = AlgorithmsUtils.Intersect(generatedRooms[i], generatedRooms[j]);
                    if (door.width < minRoomSize && door.height < minRoomSize)    continue;     // Checks if the intersect is to small to be a room and there for is a corner

                    if (door.width == roomOverlap)
                    {
                        int randomDoorSpawn = Random.Range(door.yMin + doorOffset, door.yMax - doorOffset);
                        door.yMin = (randomDoorSpawn);
                        door.height = doorWidth;
                    }
                    if (door.height == roomOverlap)
                    {
                        int randomDoorSpawn = Random.Range(door.xMin + doorOffset, door.xMax - doorOffset);
                        door.xMin = (randomDoorSpawn);
                        door.width = doorWidth;
                    }
                    doors.Add(door);
                    yield return new WaitForSeconds(0.1f);
                }                
            }
        }
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