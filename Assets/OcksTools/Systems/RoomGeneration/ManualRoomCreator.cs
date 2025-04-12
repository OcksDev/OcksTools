using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ManualRoomCreator : MonoBehaviour
{
    public RoomLol RoomGen;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Z))
        {
            GenerateDefinedLayout("");
        }
        if (Input.GetKeyDown(KeyCode.B))
        {
            GenerateDefinedLayout("NoDoorsError");
        }
    }


    public void GenerateDefinedLayout(string type)
    {

        RoomGen.ClearRooms();
        CoolRoom main = null;
        switch (type)
        {
            case "NoDoorsError":
                main = InitialRoom(RoomGen.RoomNerds[0]);
                main.AppendRoom(RoomGen.RoomNerds[3], RoomLol.RoomDirecton.Right);
                var notenoughdoors = main.AppendRoom(RoomGen.RoomNerds[2], RoomLol.RoomDirecton.Left);
                notenoughdoors.AppendRoom(RoomGen.RoomNerds[3], RoomLol.RoomDirecton.Top);
                // "notenoughdoors" has no available top doors
                notenoughdoors.AppendRoom(RoomGen.RoomNerds[3], RoomLol.RoomDirecton.Top);
                break;
            default:
                main = InitialRoom(RoomGen.RoomNerds[0]);
                main.AppendRoom(RoomGen.RoomNerds[3], RoomLol.RoomDirecton.Right);
                var c = main.AppendRoom(RoomGen.RoomNerds[2], RoomLol.RoomDirecton.Left);
                c.AppendRoom(RoomGen.RoomNerds[3], RoomLol.RoomDirecton.Top);
                break;


        }
        RoomGen.PlaceFromCoolRoom(main, RoomGen.gameObject);
    }

    public CoolRoom InitialRoom(Room init)
    {
        CoolRoom room = new CoolRoom();
        room.room = init;
        room.CompileUsedDoors();

        int sz = RoomGen.RoomColliders.GetLength(0) / 2;
        room.pos = new Vector2 (sz, sz);
        return room;
    }
    
}
