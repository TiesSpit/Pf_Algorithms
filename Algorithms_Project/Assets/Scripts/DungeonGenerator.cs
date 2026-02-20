using System.Collections.Generic;
using System.Collections;
using UnityEngine;

public class DungeonGenerator : MonoBehaviour
{

    [SerializeField] private RectInt room = new RectInt(0, 0, 100, 50);

    [SerializeField] private List<RectInt> toDo = new();

    [SerializeField] private List<RectInt> generatedRooms = new();


    [SerializeField] private int minRoomSize = 10;
    private int expectedRoomSize = 20;

    [SerializeField] private bool widthSplit;

    private void Start()
    {
        toDo.Add(room);
    }

    private void Update()
    {

    }

    [ContextMenu("Generate Dungeon")]
    public void StartGenerate()
    {
        StartCoroutine(GenerateDungeon());
    }

    public IEnumerator GenerateDungeon()
    {
        //run your split code and add the result to generatedRooms
        //generatedRooms.Clear();
        //int randomRoomSizeheight = Random.Range(1, 5);


        while (toDo.Count > 0)
        {

            RectInt newRoom = toDo[0];
            RectInt newOtherRoom = toDo[0];

            if (toDo[0].width > expectedRoomSize && toDo[0].height < expectedRoomSize)
                widthSplit = true;
            else if (toDo[0].height > expectedRoomSize && toDo[0].width < expectedRoomSize)
                widthSplit = false;

            if (widthSplit)
            {
                int randomRoomSize = Random.Range(minRoomSize, newRoom.width - minRoomSize);

                // New room changes
                newRoom.width = randomRoomSize;
                //newRoom.height /= randomRoomSizeheight;


                // Other new room changes
                //Width
                newOtherRoom.xMin = newRoom.width + newRoom.xMin;
                newOtherRoom.width = toDo[0].width - newRoom.width;
                //Height
                //newOtherRoom.yMin = newRoom.height + newRoom.yMin;
                //newOtherRoom.height = toDo[0].height - newRoom.height;

                //Debug.Log(room);
                //Debug.Log(newRoom.width);
                //Debug.Log(newOtherRoom.width);

                newRoom.width += 1;
                newOtherRoom.xMin -= 1;
            }
            else
            {
                int randomRoomSize = Random.Range(minRoomSize, newRoom.height - minRoomSize);

                // New room changes
                newRoom.height = randomRoomSize;


                // Other new room changes
                newOtherRoom.yMin = newRoom.height + newRoom.yMin;
                newOtherRoom.height = toDo[0].height - newRoom.height;

                newRoom.height += 1;
                newOtherRoom.yMin -= 1;
            }

            generatedRooms.Add(newRoom);
            generatedRooms.Add(newOtherRoom);

            toDo.Remove(toDo[0]);
            foreach (var ro in generatedRooms)
            {
                //Make it so that it will also keep going if one of the value is more than expected but only in that direction(width/height)
                if (ro.width > expectedRoomSize || ro.height > expectedRoomSize)
                {
                    toDo.Add(ro);
                }
            }
            foreach (var ro in toDo)
            {
                generatedRooms.Remove(ro);
            }

            widthSplit = !widthSplit;

            yield return new WaitForSeconds(0.1f);
        }
    }


    private void OnDrawGizmos()
    {
        foreach (var ro in generatedRooms) AlgorithmsUtils.DebugRectInt(ro, Color.yellow);
        foreach (var ro in toDo) AlgorithmsUtils.DebugRectInt(ro, Color.green);
    }


}