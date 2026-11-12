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

    [Tooltip("Search steps allowed in the first attempt before restarting with fresh randomness. Doubles every restart. <= 0 disables restarts.")]
    public int RestartBudget = 20000;
    [Tooltip("After this many restarts the search runs unbounded (exhaustive), so a layout is never missed just because of the budget.")]
    public int MaxBoundedRestarts = 12;

    public int runs = 0;
    public int cycles = 0;
    public const int GridSize = 200;

    // Cell states
    private const byte Free = 0;
    private const byte Occupied = 1;
    private const byte Reserved = 2; // the cell just outside a placed room's door; only that door's child may take it
    private readonly byte[] grid = new byte[GridSize * GridSize];

    // Scratch for the dead-end flood fill (stamp trick: no clearing between floods)
    private readonly int[] floodMark = new int[GridSize * GridSize];
    private readonly int[] floodQueue = new int[GridSize * GridSize];
    private int floodStamp;

    // Restart bookkeeping
    private int attemptCycles;
    private int budget = int.MaxValue;
    private bool aborted;

    private int minArea = 1; // smallest room footprint, used by the dead-end check

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

        // Flat door list (same order as the nested d/i loops): direction, index within direction, x, y
        public int N;
        public int[] FD, FI, FX, FY;
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

        minArea = int.MaxValue;
        for (int i = 0; i < allRooms.Length; i++)
        {
            minArea = Math.Min(minArea, Math.Max(1, allRooms[i].W * allRooms[i].H));
        }
        if (allRooms.Length == 0) minArea = 1;

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

        int n = 0;
        for (int d = 1; d <= 4; d++) n += info.DoorX[d].Length;
        if (n > 64) throw new InvalidOperationException($"Room '{r.Name}' has {n} doors; at most 64 are supported.");
        info.N = n;
        info.FD = new int[n];
        info.FI = new int[n];
        info.FX = new int[n];
        info.FY = new int[n];
        int k = 0;
        for (int d = 1; d <= 4; d++)
        {
            for (int i = 0; i < info.DoorX[d].Length; i++, k++)
            {
                info.FD[k] = d;
                info.FI[k] = i;
                info.FX[k] = info.DoorX[d][i];
                info.FY[k] = info.DoorY[d][i];
            }
        }
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
        if (lvl < 1) return new CoolRoom { WasChill = true };

        // Randomized restarts: backtracking search has heavy-tailed run times (one unlucky early choice can
        // cost millions of steps while most seeds finish instantly). Cut an attempt off after 'budget' steps
        // and start over with fresh random choices. The budget doubles each time and after
        // MaxBoundedRestarts the search runs unbounded, so it still ends in an exhaustive answer.
        CoolRoom result = null;
        for (int attempt = 0; ; attempt++)
        {
            if (attempt > 0) Array.Clear(grid, 0, grid.Length);
            aborted = false;
            attemptCycles = 0;
            if (RestartBudget <= 0 || attempt >= MaxBoundedRestarts) budget = int.MaxValue;
            else budget = (int)Math.Min((long)RestartBudget << attempt, int.MaxValue);

            result = Gen(lvl, 0, (int)SearchDirection, (int)pos.x, (int)pos.y, rnd);
            if (result != null || !aborted) break; // success, or a genuinely exhausted search
        }

        // A failed search returns an empty CoolRoom (room == null) so PlaceFromCoolRoom logs a warning
        // instead of placing a half-built layout.
        return result ?? new CoolRoom();
    }

    /// <summary>
    /// Backtracking search. Returns the placed room (with its subtree) on success, or null on failure.
    /// Contract: on failure the grid is left exactly as it was found (the entry cell, if any, stays Reserved)
    /// unless the attempt was aborted for its budget, in which case the caller clears the grid.
    ///
    /// Pruning, in the order it fires:
    ///  1. Door cells are Reserved when a room is placed, so siblings can't steal a pending child's cell.
    ///  2. Dead-end check: every pending door must reach enough free space (flood fill) for the rooms its
    ///     branch still needs. This is what catches "walled-in pocket" mistakes at the room that created the
    ///     pocket, instead of after the chain has crawled 40 rooms into it.
    ///  3. Forward check + most-constrained-first: every pending door must have a room that fits now, and
    ///     the door with the fewest options is built first, so doomed branches fail before expensive ones are built.
    ///  4. Lazy CoolRoom allocation and swap-remove for candidates.
    /// </summary>
    private CoolRoom Gen(int lvl, int depth, int dir, int px, int py, OXRandom rnd)
    {
        runs++;

        int opp = Opposite[dir];
        RoomInfo[] source = dir == 0 ? allRooms : (lvl == 1 ? endByDoor[opp] : normalByDoor[opp]);
        int count = source.Length;
        if (count == 0) return null;

        int ex = -1, ey = -1; // the Reserved cell we are allowed to overlap
        if (dir != 0)
        {
            ex = px + StepX[dir];
            ey = py + StepY[dir];
        }
        bool grow = lvl > 1; // rooms at lvl 1 are leaves: their doors need free space but no children
        int childLvl = lvl - 1;
        int needCells = childLvl * minArea;

        RoomInfo[] avail = GetBuffer(depth);
        Array.Copy(source, avail, count);

        CoolRoom node = null;

        while (count > 0)
        {
            if (aborted) return null;

            int index = rnd.Next(0, count);
            RoomInfo rom = avail[index];
            avail[index] = avail[--count]; // swap-remove; order of the rest doesn't matter
            int attempts = dir == 0 ? 1 : rom.DoorX[opp].Length;

            for (int door = 0; door < attempts; door++)
            {
                cycles++;
                if (++attemptCycles > budget)
                {
                    aborted = true;
                    return null;
                }

                int x = px, y = py;
                if (dir != 0)
                {
                    x = px - rom.DoorX[opp][door] + StepX[dir];
                    y = py - rom.DoorY[opp][door] + StepY[dir];
                }

                if (!Fits(rom, x, y, opp, door, ex, ey)) continue;

                if (node == null) node = new CoolRoom();
                node.room = rom.Source;
                node.pos = new Vector2(x, y);
                SetCells(x, y, rom.W, rom.H, Occupied);

                bool good = true;
                if (grow)
                {
                    MarkDoorCells(rom, x, y, opp, door, Reserved);

                    ulong pending = 0;
                    for (int k = 0; k < rom.N; k++)
                    {
                        if (rom.FD[k] == opp && rom.FI[k] == door) continue; // the door we entered through
                        pending |= 1UL << k;
                    }

                    while (pending != 0)
                    {
                        // Re-evaluate every pending door against the current grid and take the most constrained one.
                        int best = -1, bestCount = int.MaxValue;
                        for (int k = 0; k < rom.N; k++)
                        {
                            if ((pending & (1UL << k)) == 0) continue;
                            int d = rom.FD[k];
                            int cx = x + rom.FX[k];
                            int cy = y + rom.FY[k];

                            int c = CountCandidates(childLvl, d, cx, cy, Math.Min(bestCount, 8));
                            if (c == 0 || !HasSpace(cx + StepX[d], cy + StepY[d], needCells))
                            {
                                good = false;
                                break;
                            }
                            if (c < bestCount)
                            {
                                bestCount = c;
                                best = k;
                            }
                        }
                        if (!good) break;

                        pending &= ~(1UL << best);
                        var child = Gen(childLvl, depth + 1, rom.FD[best], x + rom.FX[best], y + rom.FY[best], rnd);
                        if (child == null)
                        {
                            good = false;
                            break;
                        }
                        node.comlpetedRooms.Add(child);
                    }
                }
                else
                {
                    runs += rom.N - (dir != 0 ? 1 : 0); // level-0 children: counted, never allocated
                }

                if (good)
                {
                    node.WasChill = true;
                    return node;
                }

                // roll this attempt back
                ClearFromCoolRoom(node);                              // footprint + already-built children
                if (grow) MarkDoorCells(rom, x, y, opp, door, Free);   // release reservations
                if (ex >= 0) grid[ex * GridSize + ey] = Reserved;      // give the entry cell back to our parent

                if (aborted) return null;
            }
        }

        return null;
    }

    /// <summary>Sets the cell just outside every door (except the entered one) to the given state.</summary>
    private void MarkDoorCells(RoomInfo rom, int x, int y, int opp, int enteredDoor, byte value)
    {
        var g = grid;
        for (int k = 0; k < rom.N; k++)
        {
            int d = rom.FD[k];
            if (d == opp && rom.FI[k] == enteredDoor) continue;
            int cx = x + rom.FX[k] + StepX[d];
            int cy = y + rom.FY[k] + StepY[d];
            g[cx * GridSize + cy] = value;
        }
    }

    /// <summary>
    /// How many (room, door) placements fit for a child entered from (px,py) going 'dir', stopping at 'cap'.
    /// 0 means the branch is already dead.
    /// </summary>
    private int CountCandidates(int lvl, int dir, int px, int py, int cap)
    {
        int opp = Opposite[dir];
        RoomInfo[] src = lvl == 1 ? endByDoor[opp] : normalByDoor[opp];
        int sx = StepX[dir], sy = StepY[dir];
        int ex = px + sx;
        int ey = py + sy;

        int n = 0;
        for (int r = 0; r < src.Length; r++)
        {
            var rom = src[r];
            var dxs = rom.DoorX[opp];
            var dys = rom.DoorY[opp];
            for (int door = 0; door < dxs.Length; door++)
            {
                if (Fits(rom, px - dxs[door] + sx, py - dys[door] + sy, opp, door, ex, ey))
                {
                    if (++n >= cap) return n;
                }
            }
        }
        return n;
    }

    /// <summary>
    /// Necessary condition for a branch: every room of a branch is connected, so the branch lives inside the
    /// connected free region around its entry cell, and a branch of 'lvl' levels needs at least lvl rooms.
    /// Flood fill from the entry cell, stopping as soon as enough cells are known, so open space is cheap
    /// and only real pockets are explored to the end. Reserved cells of other doors count as blocked.
    /// </summary>
    private bool HasSpace(int sx, int sy, int need)
    {
        if (need <= 1) return true;

        if (++floodStamp == int.MaxValue)
        {
            Array.Clear(floodMark, 0, floodMark.Length);
            floodStamp = 1;
        }
        int stamp = floodStamp;

        var g = grid;
        var mark = floodMark;
        var q = floodQueue;
        int head = 0, tail = 0;

        int start = sx * GridSize + sy;
        mark[start] = stamp;
        q[tail++] = start;

        while (head < tail)
        {
            if (tail >= need) return true;

            int c = q[head++];
            int cx = c / GridSize;
            int cy = c - cx * GridSize;

            int n;
            if (cx > 0)
            {
                n = c - GridSize;
                if (g[n] == Free && mark[n] != stamp) { mark[n] = stamp; q[tail++] = n; }
            }
            if (cx < GridSize - 1)
            {
                n = c + GridSize;
                if (g[n] == Free && mark[n] != stamp) { mark[n] = stamp; q[tail++] = n; }
            }
            if (cy > 0)
            {
                n = c - 1;
                if (g[n] == Free && mark[n] != stamp) { mark[n] = stamp; q[tail++] = n; }
            }
            if (cy < GridSize - 1)
            {
                n = c + 1;
                if (g[n] == Free && mark[n] != stamp) { mark[n] = stamp; q[tail++] = n; }
            }
        }
        return tail >= need;
    }

    /// <summary>
    /// Is the room's footprint free (apart from the one Reserved entry cell at ex,ey),
    /// and is the cell outside each of its other doors free?
    /// </summary>
    private bool Fits(RoomInfo rom, int x, int y, int opp, int enteredDoor, int ex, int ey)
    {
        int w = rom.W, h = rom.H;
        if (x < 0 || y < 0 || x + w > GridSize || y + h > GridSize) return false;

        var g = grid;

        // door-outside cells first: only a handful of reads, and they reject most candidates
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
                if (cx >= x && cx < x + w && cy >= y && cy < y + h) return false; // door opens into itself
                if (g[cx * GridSize + cy] != Free) return false;
            }
        }

        for (int i = 0; i < w; i++)
        {
            int row = (x + i) * GridSize + y;
            for (int j = 0; j < h; j++)
            {
                byte v = g[row + j];
                if (v != Free && (v != Reserved || x + i != ex || y + j != ey)) return false;
            }
        }
        return true;
    }

    private void SetCells(int x, int y, int w, int h, byte value)
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
        SetCells((int)cr.pos.x, (int)cr.pos.y, (int)cr.room.RoomSize.x, (int)cr.room.RoomSize.y, Free);

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
