using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class RoomLol : MonoBehaviour
{
    public int RoomDensity = 6;
    public float DistanceScaler = 1f;
    public Vector3 CenterSpawn = new Vector3(0, 0, 0);
    public CompileableDictionaryAlt<string, Room> RoomNerds;
    public List<GameObject> SpawnedRooms = new List<GameObject>();

    public int runs = 0;
    public int cycles = 0;
    public const int GridSize = 200;
    private readonly bool[] grid = new bool[GridSize * GridSize];

    // Direction tables, indexed by (int)RoomDirecton: None, Top, Bottom, Left, Right
    private static readonly int[] Opposite = { 0, 2, 1, 4, 3 };
    private static readonly int[] StepX = { 0, 0, 0, -1, 1 };
    private static readonly int[] StepY = { 0, -1, 1, 0, 0 };

    /// <summary>
    /// Integer-only snapshot of a Room, built once per generation so the hot
    /// search loop never touches List&lt;Vector2&gt; or float math.
    /// </summary>
    private sealed class RoomInfo
    {
        public Room Source;
        public int W, H;
        public int[][] DoorX = new int[5][]; // indexed by (int)RoomDirecton, [0] unused
        public int[][] DoorY = new int[5][];
    }

    private RoomInfo[] allRooms = new RoomInfo[0];
    private readonly RoomInfo[][] normalByDoor = EmptyTable(); // non-endpoint rooms that have a door facing [dir]
    private readonly RoomInfo[][] endByDoor = EmptyTable();    // endpoint rooms that have a door facing [dir]

    // One scratch buffer per recursion depth, so no List copies are made while searching.
    private RoomInfo[][] bufferPool = new RoomInfo[0][];
    private int poolWidth = -1;

    private static RoomInfo[][] EmptyTable()
    {
        var t = new RoomInfo[5][];
        for (int i = 0; i < t.Length; i++) t[i] = new RoomInfo[0];
        return t;
    }

    public void ClearRooms()
    {
        foreach (var r in SpawnedRooms)
        {
            Destroy(r);
        }
        SpawnedRooms.Clear();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            GenerateRandomLayout(RoomDensity);
        }
        if (Input.GetKeyDown(KeyCode.Q))
        {
            _ = LongGen();
        }
    }
    public async Task LongGen()
    {
        Debug.Log("Begin");
        ClearRooms();
        CoolRoom room = null;
        await Task.Run(() =>
        {
            room = CalcRoomLayout(50);
        });

        PlaceFromCoolRoom(room, gameObject);

        // Back on main thread here, safe to interact with Unity objects
        Debug.Log("Done");
        Debug.Log($"Build Stats: [{runs}], [{cycles}]");
    }

    public CoolRoom CalcRoomLayout(int density, OXRandom rnd = null)
    {
        PopulateRooms();
        int sz = GridSize / 2;
        var p = GenerateFromRooms(density, RoomDirecton.None, new Vector2(sz, sz), rnd);
        Debug.Log($"Build Stats: [{runs}], [{cycles}]");
        return p;
    }


    public void GenerateRandomLayout(int dens, OXRandom r = null)
    {
        ClearRooms();
        PlaceFromCoolRoom(CalcRoomLayout(dens, r), gameObject);
    }

    public void PopulateRooms()
    {
        runs = 0;
        cycles = 0;

        Array.Clear(grid, 0, grid.Length);

        //update roomlist
        RoomNerds.Clear();
        RoomNerds.Compile(x => x.Name);

        //change rooms to use here
        var all = new List<RoomInfo>();
        foreach (var a in RoomNerds)
        {
            all.Add(CompileRoom(a.Value));
        }
        allRooms = all.ToArray();

        //which rooms are chosen for certain directions
        var normal = new List<RoomInfo>();
        var end = new List<RoomInfo>();
        for (int d = 1; d <= 4; d++)
        {
            normal.Clear();
            end.Clear();
            for (int i = 0; i < allRooms.Length; i++)
            {
                var r = allRooms[i];
                if (r.DoorX[d].Length == 0) continue;
                if (r.Source.IsEndpoint) end.Add(r); else normal.Add(r);
            }
            normalByDoor[d] = normal.ToArray();
            endByDoor[d] = end.ToArray();
        }

        if (poolWidth != allRooms.Length)
        {
            poolWidth = allRooms.Length;
            bufferPool = new RoomInfo[0][];
        }
    }

    private static RoomInfo CompileRoom(Room r)
    {
        var info = new RoomInfo { Source = r, W = (int)r.RoomSize.x, H = (int)r.RoomSize.y };
        FillDoors(info, RoomDirecton.Top, r.TopDoors);
        FillDoors(info, RoomDirecton.Bottom, r.BottomDoors);
        FillDoors(info, RoomDirecton.Left, r.LeftDoors);
        FillDoors(info, RoomDirecton.Right, r.RightDoors);
        return info;
    }

    private static void FillDoors(RoomInfo info, RoomDirecton dir, List<Vector2> doors)
    {
        int n = doors.Count;
        var xs = new int[n];
        var ys = new int[n];
        for (int i = 0; i < n; i++)
        {
            xs[i] = (int)doors[i].x;
            ys[i] = (int)doors[i].y;
        }
        info.DoorX[(int)dir] = xs;
        info.DoorY[(int)dir] = ys;
    }

    private RoomInfo[] GetBuffer(int depth)
    {
        if (depth >= bufferPool.Length)
        {
            Array.Resize(ref bufferPool, Math.Max(depth + 1, bufferPool.Length * 2));
        }
        var b = bufferPool[depth];
        if (b == null)
        {
            b = new RoomInfo[Math.Max(1, poolWidth)];
            bufferPool[depth] = b;
        }
        return b;
    }

    /* dir:
     * None = any direction
     * Top / Bottom / Left / Right = the direction the room is trying to be generated from the previous room.
     */
    public CoolRoom GenerateFromRooms(int lvl, RoomDirecton SearchDirection, Vector2 pos, OXRandom rnd = null)
    {
        if (rnd == null) rnd = new OXRandom(); // created once, not once per loop iteration
        return Gen(lvl, 0, (int)SearchDirection, (int)pos.x, (int)pos.y, rnd);
    }

    private CoolRoom Gen(int lvl, int depth, int dir, int px, int py, OXRandom rnd)
    {
        runs++;
        var ret = new CoolRoom();
        if (lvl < 1)
        {
            ret.WasChill = true;
            return ret;
        }

        int opp = Opposite[dir];
        RoomInfo[] source = dir == 0 ? allRooms : (lvl == 1 ? endByDoor[opp] : normalByDoor[opp]);
        int count = source.Length;
        if (count == 0) return ret;

        RoomInfo[] avail = GetBuffer(depth);
        Array.Copy(source, avail, count);

        while (count > 0)
        {
            int index = rnd.Next(0, count);
            RoomInfo rom = avail[index];
            int attempts = dir == 0 ? 1 : rom.DoorX[opp].Length;

            for (int door = 0; door < attempts; door++)
            {
                cycles++;

                int x = px, y = py;
                if (dir != 0)
                {
                    x = px - rom.DoorX[opp][door] + StepX[dir];
                    y = py - rom.DoorY[opp][door] + StepY[dir];
                }

                if (!Fits(rom, x, y, opp, door)) continue;

                SetCells(x, y, rom.W, rom.H, true);
                ret.room = rom.Source;
                ret.pos = new Vector2(x, y);

                bool good = true;
                for (int d = 1; d <= 4 && good; d++)
                {
                    var dx = rom.DoorX[d];
                    var dy = rom.DoorY[d];
                    int skipDoor = d == opp ? door : -1; // the door we entered through
                    for (int i = 0; i < dx.Length && good; i++)
                    {
                        if (i == skipDoor) continue;
                        if (lvl > 1)
                        {
                            var child = Gen(lvl - 1, depth + 1, d, x + dx[i], y + dy[i], rnd);
                            if (child.room != null && child.WasChill)
                            {
                                ret.comlpetedRooms.Add(child);
                            }
                            good = child.WasChill;
                        }
                        else
                        {
                            runs++; // a level-0 child is always "chill"; counted but never allocated
                        }
                    }
                }

                if (good)
                {
                    ret.WasChill = true;
                    return ret;
                }
                ClearFromCoolRoom(ret);
            }

            // remove the tried room, keeping the order of the rest
            count--;
            if (index < count) Array.Copy(avail, index + 1, avail, index, count - index);
        }

        ret.WasChill = false;
        return ret;
    }

    /// <summary>Is the room's footprint free, and is the cell outside each of its doors free?</summary>
    private bool Fits(RoomInfo rom, int x, int y, int opp, int enteredDoor)
    {
        int w = rom.W, h = rom.H;
        if (x < 0 || y < 0 || x + w > GridSize || y + h > GridSize) return false;

        var g = grid;
        for (int i = 0; i < w; i++)
        {
            int row = (x + i) * GridSize + y;
            for (int j = 0; j < h; j++)
            {
                if (g[row + j]) return false;
            }
        }

        for (int d = 1; d <= 4; d++)
        {
            var dx = rom.DoorX[d];
            var dy = rom.DoorY[d];
            int sx = StepX[d], sy = StepY[d];
            for (int i = 0; i < dx.Length; i++)
            {
                if (d == opp && i == enteredDoor) continue;
                int cx = x + dx[i] + sx;
                int cy = y + dy[i] + sy;
                if (cx < 0 || cy < 0 || cx >= GridSize || cy >= GridSize) return false;
                if (g[cx * GridSize + cy]) return false;
            }
        }
        return true;
    }

    private void SetCells(int x, int y, int w, int h, bool value)
    {
        var g = grid;
        for (int i = 0; i < w; i++)
        {
            int row = (x + i) * GridSize + y;
            for (int j = 0; j < h; j++)
            {
                g[row + j] = value;
            }
        }
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
        SetCells((int)cr.pos.x, (int)cr.pos.y, (int)cr.room.RoomSize.x, (int)cr.room.RoomSize.y, false);

        var kids = cr.completed;
        if (kids != null)
        {
            for (int i = 0; i < kids.Count; i++)
            {
                ClearFromCoolRoom(kids[i]);
            }
            kids.Clear();
        }
    }

    public void PlaceFromCoolRoom(CoolRoom cr, GameObject parent)
    {
        if (cr == null || cr.room == null)
        {
            Debug.LogWarning("Room generation failed to find a valid layout.");
            return;
        }
        float z = 0;
        int sz = GridSize / 2;
        var sp = cr.room.RoomObject;
        var aaaaaaaaa = ((new Vector3(cr.pos.x, cr.pos.y, z) - new Vector3(sz, sz, 0)) * DistanceScaler);
        aaaaaaaaa.y *= -1;
        var gm = Instantiate(sp, CenterSpawn + aaaaaaaaa + new Vector3(((cr.room.RoomSize.x * DistanceScaler) / 2) - 0.5f, ((cr.room.RoomSize.y * -DistanceScaler) / 2) - 0.5f, 0), parent.transform.rotation, parent.transform);
        SpawnedRooms.Add(gm);
        cr.iroom = gm.GetComponent<I_Room>();
        if (cr.daddy != null) cr.iroom.RelatedRooms.Add(cr.daddy.iroom);
        var kids = cr.completed;
        if (kids != null)
        {
            for (int i = 0; i < kids.Count; i++)
            {
                var c = kids[i];
                c.daddy = cr;
                PlaceFromCoolRoom(c, parent);
                cr.iroom.RelatedRooms.Add(c.iroom);
            }
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
    public string Name;
    public GameObject RoomObject;
    public bool IsEndpoint = false;
    public Vector2 RoomSize = new Vector2(1, 1);
    public List<Vector2> LeftDoors = new List<Vector2>();
    public List<Vector2> RightDoors = new List<Vector2>();
    public List<Vector2> TopDoors = new List<Vector2>();
    public List<Vector2> BottomDoors = new List<Vector2>();

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

    // Allocated on first use: most rooms in a failed search never need either of these.
    internal List<CoolRoom> completed;
    private Dictionary<RoomLol.RoomDirecton, List<bool>> uses;
    public List<CoolRoom> comlpetedRooms { get { return completed ?? (completed = new List<CoolRoom>()); } }
    public Dictionary<RoomLol.RoomDirecton, List<bool>> complileduses { get { return uses ?? (uses = new Dictionary<RoomLol.RoomDirecton, List<bool>>()); } }

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
        if (daddy.complileduses.Count == 0) daddy.CompileUsedDoors();

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
