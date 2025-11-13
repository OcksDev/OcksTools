
using System;
using System.Collections.Generic;
using UnityEngine;

public class RoomLol : MonoBehaviour
{
    public int RoomDensity = 6;
    public float DistanceScaler = 1f;
    public Vector3 CenterSpawn = new Vector3(0, 0, 0);
    public Room[] RoomNerds;
    public List<GameObject> SpawnedRooms = new List<GameObject>();
    public int[,] RoomColliders = new int[200, 200];
    private List<Room> AllRooms= new List<Room>();
    private List<Room> LeftRooms = new List<Room>();
    private List<Room> RightRooms = new List<Room>();
    private List<Room> UpRooms = new List<Room>();
    private List<Room> DownRooms = new List<Room>();
    private List<Room> EndLeftRooms = new List<Room>();
    private List<Room> EndRightRooms = new List<Room>();
    private List<Room> EndUpRooms = new List<Room>();
    private List<Room> EndDownRooms = new List<Room>();
    public void ClearRooms()
    {
        RoomColliders = new int[200, 200];
        foreach(var r in SpawnedRooms)
        {
            Destroy(r);
        }
        SpawnedRooms.Clear();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            GenerateRandomLayout();
        }
    }

    public int runs = 0;
    public int cycles = 0;
    public void GenerateRandomLayout()
    {
        runs = 0;
        cycles = 0;
        ClearRooms();
        PopulateRooms();
        int sz = RoomColliders.GetLength(0)/2;
        var crs = GenerateFromRooms(RoomDensity, RoomColliders, RoomDirecton.None, new Vector2(sz,sz));
        PlaceFromCoolRoom(crs, gameObject);

        Debug.Log($"Build Stats: [{runs}], [{cycles}]" );

    }
    public void PopulateRooms()
    {
        //change rooms to use here
        AllRooms = new List<Room>();

        for(int i = 0; i < RoomNerds.Length;i++)
        {
            RoomNerds[i].SetData(i);
            AllRooms.Add(RoomNerds[i]);
        }
        LeftRooms.Clear();
        RightRooms.Clear();
        UpRooms.Clear();
        DownRooms.Clear();
        EndLeftRooms.Clear();
        EndRightRooms.Clear();
        EndUpRooms.Clear();
        EndDownRooms.Clear();
        //whichs rooms are chosen for certain directions
        foreach (var room in AllRooms)
        {
            if (room.LeftDoors.Count > 0 && !room.IsEndpoint) LeftRooms.Add(room);
            if (room.RightDoors.Count > 0 && !room.IsEndpoint) RightRooms.Add(room);
            if (room.TopDoors.Count > 0 && !room.IsEndpoint) UpRooms.Add(room);
            if (room.BottomDoors.Count > 0 && !room.IsEndpoint) DownRooms.Add(room);
            if (room.LeftDoors.Count > 0 && room.IsEndpoint) EndLeftRooms.Add(room);
            if (room.RightDoors.Count > 0 && room.IsEndpoint) EndRightRooms.Add(room);
            if (room.TopDoors.Count > 0 && room.IsEndpoint) EndUpRooms.Add(room);
            if (room.BottomDoors.Count > 0 && room.IsEndpoint) EndDownRooms.Add(room);
        }
    }
    public CoolRoom GenerateFromRooms(int lvl, int[,] roomcol, RoomDirecton dir, Vector2 pos)
    {
        CoolRoom ret = new CoolRoom();

        runs++;
        if (lvl < 1) 
        {
            ret.WasChill = true;
            return ret;
        }
        /* dir:
         * -1 = any direction
         * 0 = top
         * 1 = bottom
         * 2 = left
         * 3 = right
         * 
         * dir is the direction the room is trying to be generated from the previous room.
         * Lists
         * 
         */
        List<Room> available_rooms;
        switch (dir)
        {
            default:
                available_rooms = new List<Room>(AllRooms);
                break;
            case RoomDirecton.Top:
                available_rooms = new List<Room>(lvl ==  1? EndDownRooms : DownRooms);
                break;
            case RoomDirecton.Bottom:
                available_rooms = new List<Room>(lvl == 1 ? EndUpRooms : UpRooms);
                break;
            case RoomDirecton.Left:
                available_rooms = new List<Room>(lvl == 1 ? EndRightRooms : RightRooms);
                break;
            case RoomDirecton.Right:
                available_rooms = new List<Room>(lvl == 1 ? EndLeftRooms : LeftRooms);
                break;
        }
        bool found_place = false;
        //bool found_ever = false;
        //Debug.Log("Fuck dawg " + lvl + ", " + dir);
        while (available_rooms.Count > 0)
        {
            int index = UnityEngine.Random.Range(0, available_rooms.Count);
            Room rom = available_rooms[index];
            Func<RoomDirecton, int> getamnt = (x) =>
            {

                switch (x)
                {
                    case RoomDirecton.Top:
                        return rom.BottomDoors.Count;
                    case RoomDirecton.Bottom:
                        return rom.TopDoors.Count;
                    case RoomDirecton.Left:
                        return rom.RightDoors.Count;
                    case RoomDirecton.Right:
                        return rom.LeftDoors.Count;
                    default:
                        return 1;
                }

            };
            var aaa = getamnt(dir);
            for(int doorindexlol = 0; doorindexlol < aaa; doorindexlol++)
            {
                cycles++;
                bool keepgoing = true;
                ret.room = rom;
                var pos2 = ModPos(dir, pos, rom, doorindexlol);

                ret.pos = pos2;

                for (int i = 0; i < rom.RoomSize.x && keepgoing; i++)
                {
                    for (int j = 0; j < rom.RoomSize.y && keepgoing; j++)
                    {
                        if (roomcol[i + (int)pos2.x, j + (int)pos2.y] > 0)
                        {
                            keepgoing = false;
                        }
                    }
                }
                if (keepgoing)
                {

                    if (rom.TopDoors.Count > 0)
                    {
                        for (int i = 0; i < rom.TopDoors.Count; i++)
                        {
                            if (dir == RoomDirecton.Bottom && i == doorindexlol) continue;
                            var v = pos2 + rom.TopDoors[i] - new Vector2(0, 1);
                            if (roomcol[(int)v.x, (int)v.y] > 0) keepgoing = false;
                        }
                    }
                    if (rom.BottomDoors.Count > 0 && keepgoing)
                    {
                        for (int i = 0; i < rom.BottomDoors.Count; i++)
                        {
                            if (dir == RoomDirecton.Top && i == doorindexlol) continue;
                            var v = pos2 + rom.BottomDoors[i] + new Vector2(0, 1);
                            if (roomcol[(int)v.x, (int)v.y] > 0) keepgoing = false;
                        }
                    }
                    if (rom.LeftDoors.Count > 0 && keepgoing)
                    {
                        for (int i = 0; i < rom.LeftDoors.Count; i++)
                        {
                            if (dir == RoomDirecton.Right && i == doorindexlol) continue;
                            var v = pos2 + rom.LeftDoors[i] - new Vector2(1, 0);
                            if (roomcol[(int)v.x, (int)v.y] > 0) keepgoing = false;
                        }
                    }
                    if (rom.RightDoors.Count > 0 && keepgoing)
                    {
                        for (int i = 0; i < rom.RightDoors.Count; i++)
                        {
                            if (dir == RoomDirecton.Left && i == doorindexlol) continue;
                            var v = pos2 + rom.RightDoors[i] + new Vector2(1, 0);
                            if (roomcol[(int)v.x, (int)v.y] > 0) keepgoing = false;
                        }
                    }
                }

                if (keepgoing)
                {
                    for (int i = 0; i < rom.RoomSize.x && keepgoing; i++)
                    {
                        for (int j = 0; j < rom.RoomSize.y && keepgoing; j++)
                        {
                            roomcol[i + (int)pos2.x, j + (int)pos2.y]++;
                        }
                    }
                    bool good = true;
                    if (good && rom.TopDoors.Count > 0)
                    {
                        for (int i = 0; i < rom.TopDoors.Count && good; i++)
                        {
                            if (dir == RoomDirecton.Bottom && i == doorindexlol) continue;
                            var a = GenerateFromRooms(lvl - 1, roomcol, RoomDirecton.Top, pos2 + rom.TopDoors[i]);
                            if (a.room != null && a.WasChill)
                            {
                                ret.comlpetedRooms.Add(a);
                            }
                            good = a.WasChill;
                        }
                    }
                    if (good && rom.BottomDoors.Count > 0)
                    {
                        for (int i = 0; i < rom.BottomDoors.Count && good; i++)
                        {
                            if (dir == RoomDirecton.Top && i == doorindexlol) continue;
                            var a = GenerateFromRooms(lvl - 1, roomcol, RoomDirecton.Bottom, pos2 + rom.BottomDoors[i]);
                            if (a.room != null && a.WasChill)
                            {
                                ret.comlpetedRooms.Add(a);
                            }
                            good = a.WasChill;
                        }

                    }
                    if (good && rom.LeftDoors.Count > 0)
                    {
                        for (int i = 0; i < rom.LeftDoors.Count && good; i++)
                        {
                            if (dir == RoomDirecton.Right && i == doorindexlol) continue;
                            var a = GenerateFromRooms(lvl - 1, roomcol, RoomDirecton.Left, pos2 + rom.LeftDoors[i]);
                            if (a.room != null && a.WasChill)
                            {
                                ret.comlpetedRooms.Add(a);
                            }
                            good = a.WasChill;
                        }
                    }
                    if (good && rom.RightDoors.Count > 0)
                    {
                        for (int i = 0; i < rom.RightDoors.Count && good; i++)
                        {
                            if (dir == RoomDirecton.Left && i == doorindexlol) continue;
                            var a = GenerateFromRooms(lvl - 1, roomcol, RoomDirecton.Right, pos2 + rom.RightDoors[i]);
                            if (a.room != null && a.WasChill)
                            {
                                ret.comlpetedRooms.Add(a);
                            }
                            good = a.WasChill;
                        }
                    }
                    //Debug.Log("Child Is Chill: " + lvl + ", " + good + ", " + dir);
                    if (!good)
                    {
                        ClearFromCoolRoom(ret);
                    }
                    else
                    {
                        found_place = true;
                        goto exitee;
                    }
                }
            }

            if (!found_place)
            {
                available_rooms.RemoveAt(index);
            }


        }
        exitee:
        //Debug.Log("Shit dawg " + lvl + ", " + found_place + ", " + dir);
        ret.WasChill = found_place;
        return ret;
    }

    public static Vector2 ModPos(RoomDirecton dir, Vector2 pos2, Room rom, int doorindexlol)
    {
        switch (dir)
        {
            case RoomDirecton.Top:
                pos2 -= rom.BottomDoors[doorindexlol];
                pos2 -= new Vector2(0, 1);
                break;
            case RoomDirecton.Bottom:
                pos2 -= rom.TopDoors[doorindexlol];
                pos2 += new Vector2(0, 1);
                break;
            case RoomDirecton.Left:
                pos2 -= rom.RightDoors[doorindexlol];
                pos2 -= new Vector2(1, 0);
                break;
            case RoomDirecton.Right:
                pos2 -= rom.LeftDoors[doorindexlol];
                pos2 += new Vector2(1, 0);
                break;
        }
        return pos2;
    }
    public void ClearFromCoolRoom(CoolRoom cr)
    {

        for (int i = 0; i < cr.room.RoomSize.x; i++)
        {
            for (int j = 0; j < cr.room.RoomSize.y; j++)
            {
                RoomColliders[i + (int)cr.pos.x, j + (int)cr.pos.y]--;
            }
        }
        foreach (var c in cr.comlpetedRooms)
        {
            ClearFromCoolRoom(c);
        }
        cr.comlpetedRooms.Clear();
    }
    public void PlaceFromCoolRoom(CoolRoom cr, GameObject parent)
    {
        float z = 0;
        int sz = RoomColliders.GetLength(0) / 2;
        var sp = cr.room.RoomObject;
        var aaaaaaaaa = ((new Vector3(cr.pos.x, cr.pos.y, z) - new Vector3(sz, sz, 0)) * DistanceScaler);
        aaaaaaaaa.y *= -1;
        var gm = Instantiate(sp, CenterSpawn + aaaaaaaaa + new Vector3(((cr.room.RoomSize.x*DistanceScaler)/2) - 0.5f, ((cr.room.RoomSize.y * -DistanceScaler) / 2) - 0.5f, 0), parent.transform.rotation, parent.transform);
        SpawnedRooms.Add(gm);
        cr.iroom = gm.GetComponent<I_Room>();
        if (cr.daddy != null) cr.iroom.RelatedRooms.Add(cr.daddy.iroom);
        foreach (var c in cr.comlpetedRooms)
        {
            c.daddy = cr;
            PlaceFromCoolRoom(c, parent);
            cr.iroom.RelatedRooms.Add(c.iroom);
        }
    }

    public enum RoomDirecton
    {
        None,
        Top,
        Bottom,
        Left,
        Right,
    }
}


[Serializable]
public class Room
{
    public GameObject RoomObject;
    public bool IsEndpoint = false;
    public Vector2 RoomSize = new Vector2(1, 1);
    public List<Vector2> LeftDoors = new List<Vector2>();
    public List<Vector2> RightDoors = new List<Vector2>();
    public List<Vector2> TopDoors = new List<Vector2>();
    public List<Vector2> BottomDoors = new List<Vector2>();
    [HideInInspector]
    public int RoomID = -1;

    public void SetData(int index)
    {
        RoomID= index;
    }

    /*
    public Room(int roomid)
    {
        RoomID = roomid;
        switch (roomid)
        {
            case 0:
                RoomSize = new Vector2(2, 1);
                HasLeftDoor = true;
                HasRightDoor = true;
                LeftDoor = new Vector2(0, 0);
                RightDoor = new Vector2(1, 0);
                break;
            case 1:
                RoomSize = new Vector2(1, 2);
                HasTopDoor = true;
                HasBottomDoor = true;
                TopDoor = new Vector2(0, 0);
                BottomDoor = new Vector2(0, 1);
                break;
            case 2:
                RoomSize = new Vector2(1, 1);
                HasBottomDoor = true;
                HasTopDoor = true;
                HasLeftDoor = true;
                HasRightDoor = true;
                LeftDoor = new Vector2(0, 0);
                RightDoor = new Vector2(0, 0);
                TopDoor = new Vector2(0, 0);
                BottomDoor = new Vector2(0, 0);
                break;
            case 3:
                RoomSize = new Vector2(1, 1);
                //HasBottomDoor = true;
                HasTopDoor = true;
                HasLeftDoor = true;
                //HasRightDoor = true;
                LeftDoor = new Vector2(0, 0);
                //RightDoor = new Vector2(0, 0);
                TopDoor = new Vector2(0, 0);
                //BottomDoor = new Vector2(0, 0);
                break;
            case 4:
                RoomSize = new Vector2(1, 1);
                //HasBottomDoor = true;
                HasTopDoor = true;
                //HasLeftDoor = true;
                HasRightDoor = true;
                //LeftDoor = new Vector2(0, 0);
                RightDoor = new Vector2(0, 0);
                TopDoor = new Vector2(0, 0);
                //BottomDoor = new Vector2(0, 0);
                break;
            case 5:
                RoomSize = new Vector2(1, 1);
                HasBottomDoor = true;
                //HasTopDoor = true;
                //HasLeftDoor = true;
                HasRightDoor = true;
                //LeftDoor = new Vector2(0, 0);
                RightDoor = new Vector2(0, 0);
                //TopDoor = new Vector2(0, 0);
                BottomDoor = new Vector2(0, 0);
                break;
            case 6:
                RoomSize = new Vector2(1, 1);
                HasBottomDoor = true;
                //HasTopDoor = true;
                HasLeftDoor = true;
                //HasRightDoor = true;
                LeftDoor = new Vector2(0, 0);
                //RightDoor = new Vector2(0, 0);
                //TopDoor = new Vector2(0, 0);
                BottomDoor = new Vector2(0, 0);
                break;
            case 7:
                RoomSize = new Vector2(1, 1);
                HasBottomDoor = true;
                HasTopDoor = true;
                HasLeftDoor = true;
                HasRightDoor = true;
                LeftDoor = new Vector2(0, 0);
                RightDoor = new Vector2(0, 0);
                TopDoor = new Vector2(0, 0);
                BottomDoor = new Vector2(0, 0);
                IsEndpoint = true;
                break;
            default:
                RoomSize = new Vector2(1, 1);
                HasBottomDoor = true;
                HasTopDoor = true;
                HasLeftDoor = true;
                HasRightDoor = true;
                IsEndpoint = true;
                LeftDoor = new Vector2(0,0);
                RightDoor = new Vector2(0,0);
                TopDoor = new Vector2(0,0);
                BottomDoor = new Vector2(0,0);
                break;
        }
    }*/
}
public class CoolRoom
{
    public bool WasChill;
    public Room room;
    public CoolRoom daddy;
    public I_Room iroom;
    public Vector2 pos;
    public List<CoolRoom> comlpetedRooms = new List<CoolRoom>();
    public Dictionary<RoomLol.RoomDirecton, List<bool>> complileduses = new Dictionary<RoomLol.RoomDirecton, List<bool>>();
    public void CompileUsedDoors()
    {
        complileduses.Clear();
        complileduses.Add(RoomLol.RoomDirecton.Top, new List<bool>(new bool[room.TopDoors.Count]));
        complileduses.Add(RoomLol.RoomDirecton.Bottom, new List<bool>(new bool[room.BottomDoors.Count]));
        complileduses.Add(RoomLol.RoomDirecton.Left, new List<bool>(new bool[room.LeftDoors.Count]));
        complileduses.Add(RoomLol.RoomDirecton.Right, new List<bool>(new bool[room.RightDoors.Count]));
    }

    public CoolRoom AppendRoom(Room newnerd, RoomLol.RoomDirecton dir, int daddydoorindex = -1, int doorindex = -1)
    {
        CoolRoom room = new CoolRoom();
        room.room = newnerd;
        room.CompileUsedDoors();
        var daddy = this;

        if (daddydoorindex == -1)
        {
            for (int i = 0; i < daddy.complileduses[dir].Count; i++)
            {
                if (!daddy.complileduses[dir][i])
                {
                    daddydoorindex = i;
                    break;
                }
            }
            if (daddydoorindex == -1)
            {
                Debug.LogError($"No available {dir.ToString()} Doors!");
                return null;
            }
        }
        if (doorindex == -1)
        {
            for (int i = 0; i < room.complileduses[dir].Count; i++)
            {
                if (!room.complileduses[dir][i])
                {
                    doorindex = i;
                    break;
                }
            }
            if (doorindex == -1)
            {

                RoomLol.RoomDirecton inverteddir = RoomLol.RoomDirecton.None;
                switch (dir)
                {
                    case RoomLol.RoomDirecton.Top: inverteddir = RoomLol.RoomDirecton.Bottom; break;
                    case RoomLol.RoomDirecton.Bottom: inverteddir = RoomLol.RoomDirecton.Top; break;
                    case RoomLol.RoomDirecton.Left: inverteddir = RoomLol.RoomDirecton.Right; break;
                    case RoomLol.RoomDirecton.Right: inverteddir = RoomLol.RoomDirecton.Left; break;
                }
                Debug.LogError($"No available {inverteddir.ToString()} Doors!");
                return null;
            }
        }
        var coolpos = daddy.pos;
        switch (dir)
        {
            case RoomLol.RoomDirecton.Top: coolpos += daddy.room.TopDoors[daddydoorindex]; break;
            case RoomLol.RoomDirecton.Bottom: coolpos += daddy.room.BottomDoors[daddydoorindex]; break;
            case RoomLol.RoomDirecton.Left: coolpos += daddy.room.LeftDoors[daddydoorindex]; break;
            case RoomLol.RoomDirecton.Right: coolpos += daddy.room.RightDoors[daddydoorindex]; break;
        }
        coolpos = RoomLol.ModPos(dir, coolpos, newnerd, doorindex);
        room.pos = coolpos;

        daddy.complileduses[dir][daddydoorindex] = true;
        room.complileduses[dir][doorindex] = true;
        daddy.comlpetedRooms.Add(room);

        return room;
    }
}
