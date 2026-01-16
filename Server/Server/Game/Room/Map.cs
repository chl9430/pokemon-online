using Google.Protobuf.Protocol;
using ServerCore;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Server
{
    public struct Pos
    {
        public Pos(int y, int x) { Y = y; X = x; }
        public int Y;
        public int X;
    }

    public struct PQNode : IComparable<PQNode>
    {
        public int F;
        public int G;
        public int Y;
        public int X;

        public int CompareTo(PQNode other)
        {
            if (F == other.F)
                return 0;
            return F < other.F ? 1 : -1;
        }
    }

    public struct Vector2Int
    {
        public int x;
        public int y;

        public Vector2Int(int x, int y) { this.x = x; this.y = y; }

        public static Vector2Int up { get { return new Vector2Int(0, 1); } }
        public static Vector2Int down { get { return new Vector2Int(0, -1); } }
        public static Vector2Int left { get { return new Vector2Int(-1, 0); } }
        public static Vector2Int right { get { return new Vector2Int(1, 0); } }

        public static Vector2Int operator +(Vector2Int a, Vector2Int b)
        {
            return new Vector2Int(a.x + b.x, a.y + b.y);
        }

        public static Vector2Int operator -(Vector2Int a, Vector2Int b)
        {
            return new Vector2Int(a.x - b.x, a.y - b.y);
        }

        public float magnitude { get { return (float)Math.Sqrt(sqrMagnitude); } }
        public int sqrMagnitude { get { return (x * x + y * y); } }
        public int cellDistFromZero { get { return Math.Abs(x) + Math.Abs(y); } }
    }

    public enum TileType
    {
        NONE = 0,
        PATH = 1,
        COLLISION = 2,
        BUSH = 3,
        DOOR = 4,
    }

    public class Map
    {
        public int MinX { get; set; }
        public int MaxX { get; set; }
        public int MinY { get; set; }
        public int MaxY { get; set; }

        public int SizeX { get { return MaxX - MinX + 1; } }
        public int SizeY { get { return MaxY - MinY + 1; } }

        TileType[,] _collision;
        int[,] _bushTileGrid;
        int[,] _doorTileGrid;
        GameObject[,] _objects;
        GameRoom _gameRoom;

        public GameRoom GameRoom { set { _gameRoom = value; } }

        public TileType GetTileType(Vector2Int cellPos)
        {
            if (cellPos.x < MinX || cellPos.x > MaxX)
                return TileType.COLLISION;
            if (cellPos.y < MinY || cellPos.y > MaxY)
                return TileType.COLLISION;

            int x = cellPos.x - MinX;
            int y = MaxY - cellPos.y;

            return _collision[y, x];
        }

        public GameObject Find(Vector2Int cellPos)
        {
            if (cellPos.x < MinX || cellPos.x > MaxX)
                return null;
            if (cellPos.y < MinY || cellPos.y > MaxY)
                return null;

            int x = cellPos.x - MinX;
            int y = MaxY - cellPos.y;

            return _objects[y, x];
        }

        public int GetBushId(Vector2Int cellPos)
        {
            if (cellPos.x < MinX || cellPos.x > MaxX)
                return 0;
            if (cellPos.y < MinY || cellPos.y > MaxY)
                return 0;

            int x = cellPos.x - MinX;
            int y = MaxY - cellPos.y;

            return _bushTileGrid[y, x];
        }

        public int GetDoorId(Vector2Int cellPos)
        {
            if (cellPos.x < MinX || cellPos.x > MaxX)
                return 0;
            if (cellPos.y < MinY || cellPos.y > MaxY)
                return 0;

            int x = cellPos.x - MinX;
            int y = MaxY - cellPos.y;

            return _doorTileGrid[y, x];
        }

        public Vector2Int GetDoorPos(int doorId)
        {
            Vector2Int doorPos = new Vector2Int();

            for (int i = 0; i < _doorTileGrid.GetLength(0); i++)
            {
                for (int j = 0; j < _doorTileGrid.GetLength(1); j++)
                {
                    if (_doorTileGrid[i, j] == doorId)
                    {
                        doorPos.x = j + MinX;
                        doorPos.y = MaxY - i;

                        return doorPos;
                    }
                }
            }

            return doorPos;
        }

        public void ApplyMove(GameObject gameObject, Vector2Int dest)
        {
            ApplyLeave(gameObject);

            if (gameObject.Room == null)
                return;
            if (gameObject.Room.Map != this)
                return;

            PositionInfo posInfo = gameObject.PosInfo;
            TileType tile = GetTileType(dest);
            if (tile == TileType.COLLISION)
            {
                return;
            }

            int x = dest.x - MinX;
            int y = MaxY - dest.y;
            _objects[y, x] = gameObject;

            posInfo.PosX = dest.x;
            posInfo.PosY = dest.y;
            return;
        }

        public bool ApplyLeave(GameObject gameObject)
        {
            if (gameObject.Room == null)
                return false;
            if (gameObject.Room.Map != this)
                return false;

            PositionInfo posInfo = gameObject.PosInfo;
            if (posInfo.PosX < MinX || posInfo.PosX > MaxX)
                return false;
            if (posInfo.PosY < MinY || posInfo.PosY > MaxY)
                return false;

            int x = posInfo.PosX - MinX;
            int y = MaxY - posInfo.PosY;
            if (_objects[y, x] == gameObject)
                _objects[y, x] = null;

            return true;
        }

        public void LoadMap(int mapId, RoomType roomType, string pathPrefix = "../../../../../Common/MapData")
        {
            string mapName = $"{roomType.ToString()}_" + mapId.ToString();

            string text = File.ReadAllText($"{pathPrefix}/{mapName}.txt");
            StringReader reader = new StringReader(text);

            MinX = int.Parse(reader.ReadLine());
            MaxX = int.Parse(reader.ReadLine());
            MinY = int.Parse(reader.ReadLine());
            MaxY = int.Parse(reader.ReadLine());

            int xCount = MaxX - MinX + 1;
            int yCount = MaxY - MinY + 1;

            _collision = new TileType[yCount, xCount];
            _bushTileGrid = new int[yCount, xCount];
            _doorTileGrid = new int[yCount, xCount];
            _objects = new GameObject[yCount, xCount];

            // 타일맵의 정보를 불러온다.
            for (int y = 0; y < yCount; y++)
            {
                string line = reader.ReadLine();
                for (int x = 0; x < xCount; x++)
                {
                    if (line[x] == '1')
                    {
                        _collision[y, x] = TileType.COLLISION;
                    }
                    else if (line[x] == '0')
                    {
                        _collision[y, x] = TileType.PATH;
                    }
                }
            }

            // 문 타일맵의 정보를 불러온다.
            string doorMapName = mapName + "_DoorMap";

            text = File.ReadAllText($"{pathPrefix}/{doorMapName}.txt");
            reader = new StringReader(text);

            MinX = int.Parse(reader.ReadLine());
            MaxX = int.Parse(reader.ReadLine());
            MinY = int.Parse(reader.ReadLine());
            MaxY = int.Parse(reader.ReadLine());

            for (int y = 0; y < yCount; y++)
            {
                string line = reader.ReadLine();
                for (int x = 0; x < xCount; x++)
                {
                    _doorTileGrid[y, x] = line[x] - '0';

                    if (line[x] != '0')
                        _collision[y, x] = TileType.DOOR;
                }
            }

            // 부쉬 타일맵의 정보를 불러온다.
            string bushMapName = mapName + "_BushMap";

            text = File.ReadAllText($"{pathPrefix}/{bushMapName}.txt");
            reader = new StringReader(text);

            MinX = int.Parse(reader.ReadLine());
            MaxX = int.Parse(reader.ReadLine());
            MinY = int.Parse(reader.ReadLine());
            MaxY = int.Parse(reader.ReadLine());

            for (int y = 0; y < yCount; y++)
            {
                string line = reader.ReadLine();
                for (int x = 0; x < xCount; x++)
                {
                    _bushTileGrid[y, x] = line[x] - '0';

                    if (line[x] != '0')
                        _collision[y, x] = TileType.BUSH;
                }
            }

            // NPC 타일맵의 정보를 불러온다.
            string npcMapName = mapName + "_NPCMap";

            text = File.ReadAllText($"{pathPrefix}/{npcMapName}.txt");
            reader = new StringReader(text);

            int npcCount = int.Parse(reader.ReadLine());

            for (int i = 0; i < npcCount; i++)
            {
                string line = reader.ReadLine();

                string[] parts = line.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                string name = parts[0];

                int x = int.Parse(parts[1].TrimEnd(',').Trim());
                int y = int.Parse(parts[2].TrimEnd(',').Trim());
                int z = int.Parse(parts[3].TrimEnd(',').Trim());

                string contentType = parts[4].TrimEnd(',').Trim();
                string npcType = parts[5].TrimEnd(',').Trim();
                MoveDir dir = (MoveDir)Enum.Parse(typeof(MoveDir), parts[6].TrimEnd(',').Trim());

                int cellPosX = int.Parse(parts[1].TrimEnd(','));
                int cellPosY = int.Parse(parts[2].TrimEnd(','));

                if (name == "Staff")
                    cellPosX += 1;
                else if (name == "Nurse")
                    cellPosY -= 1;

                int posX = cellPosX - MinX;
                int posY = MaxY - cellPosY;

                NPC npc;
                if (contentType == "Trainer")
                {
                    npc = ObjectManager.Instance.Add<TrainerNPC>();
                    {
                        npc.Info.PosInfo.State = CreatureState.Idle;
                        npc.Info.PosInfo.MoveDir = dir;
                        npc.Info.PosInfo.PosX = cellPosX;
                        npc.Info.PosInfo.PosY = cellPosY;
                        npc.Name = name;
                        npc.Room = _gameRoom;
                    }
                }
                else
                {
                    npc = ObjectManager.Instance.Add<NPC>();
                    {
                        npc.Info.PosInfo.State = CreatureState.Idle;
                        npc.Info.PosInfo.MoveDir = dir;
                        npc.Info.PosInfo.PosX = cellPosX;
                        npc.Info.PosInfo.PosY = cellPosY;
                        npc.Name = name;
                        npc.Room = _gameRoom;
                    }
                }

                npc.NPCType = (NPCType)Enum.Parse(typeof(NPCType), npcType);

                //if (npc.Name == "Staff")
                //    posX += 1;
                //else if (npc.Name == "Nurse")
                //    posY -= 1;

                _objects[posY, posX] = npc;

                if (_gameRoom.Objs.ContainsKey(GameObjectType.Npc) == false)
                    _gameRoom.Objs.Add(GameObjectType.Npc, new Dictionary<int, GameObject>());

                _gameRoom.NPCs.Add(npc.Id, npc);
            }
        }

        #region A* PathFinding

        // U D L R
        int[] _deltaY = new int[] { 1, -1, 0, 0 };
        int[] _deltaX = new int[] { 0, 0, -1, 1 };
        int[] _cost = new int[] { 10, 10, 10, 10 };

        public List<Vector2Int> FindPath(Vector2Int startCellPos, Vector2Int destCellPos, bool checkObjects = true)
        {
            List<Pos> path = new List<Pos>();

            // 점수 매기기
            // F = G + H
            // F = 최종 점수 (작을 수록 좋음, 경로에 따라 달라짐)
            // G = 시작점에서 해당 좌표까지 이동하는데 드는 비용 (작을 수록 좋음, 경로에 따라 달라짐)
            // H = 목적지에서 얼마나 가까운지 (작을 수록 좋음, 고정)

            // (y, x) 이미 방문했는지 여부 (방문 = closed 상태)
            bool[,] closed = new bool[SizeY, SizeX]; // CloseList

            // (y, x) 가는 길을 한 번이라도 발견했는지
            // 발견X => MaxValue
            // 발견O => F = G + H
            int[,] open = new int[SizeY, SizeX]; // OpenList
            for (int y = 0; y < SizeY; y++)
                for (int x = 0; x < SizeX; x++)
                    open[y, x] = Int32.MaxValue;

            Pos[,] parent = new Pos[SizeY, SizeX];

            // 오픈리스트에 있는 정보들 중에서, 가장 좋은 후보를 빠르게 뽑아오기 위한 도구
            PriorityQueue<PQNode> pq = new PriorityQueue<PQNode>();

            // CellPos -> ArrayPos
            Pos pos = Cell2Pos(startCellPos);
            Pos dest = Cell2Pos(destCellPos);

            // 시작점 발견 (예약 진행)
            open[pos.Y, pos.X] = 10 * (Math.Abs(dest.Y - pos.Y) + Math.Abs(dest.X - pos.X));
            pq.Push(new PQNode() { F = 10 * (Math.Abs(dest.Y - pos.Y) + Math.Abs(dest.X - pos.X)), G = 0, Y = pos.Y, X = pos.X });
            parent[pos.Y, pos.X] = new Pos(pos.Y, pos.X);

            while (pq.Count > 0)
            {
                // 제일 좋은 후보를 찾는다
                PQNode node = pq.Pop();
                // 동일한 좌표를 여러 경로로 찾아서, 더 빠른 경로로 인해서 이미 방문(closed)된 경우 스킵
                if (closed[node.Y, node.X])
                    continue;

                // 방문한다
                closed[node.Y, node.X] = true;
                // 목적지 도착했으면 바로 종료
                if (node.Y == dest.Y && node.X == dest.X)
                    break;

                // 상하좌우 등 이동할 수 있는 좌표인지 확인해서 예약(open)한다
                for (int i = 0; i < _deltaY.Length; i++)
                {
                    Pos next = new Pos(node.Y + _deltaY[i], node.X + _deltaX[i]);

                    // 유효 범위를 벗어났으면 스킵
                    // 벽으로 막혀서 갈 수 없으면 스킵
                    if (next.Y != dest.Y || next.X != dest.X)
                    {
                        TileType tile = GetTileType(Pos2Cell(next));

                        if (tile == TileType.COLLISION)
                            continue;
                    }

                    // 이미 방문한 곳이면 스킵
                    if (closed[next.Y, next.X])
                        continue;

                    // 비용 계산
                    int g = 0;// node.G + _cost[i];
                    int h = 10 * ((dest.Y - next.Y) * (dest.Y - next.Y) + (dest.X - next.X) * (dest.X - next.X));
                    // 다른 경로에서 더 빠른 길 이미 찾았으면 스킵
                    if (open[next.Y, next.X] < g + h)
                        continue;

                    // 예약 진행
                    open[dest.Y, dest.X] = g + h;
                    pq.Push(new PQNode() { F = g + h, G = g, Y = next.Y, X = next.X });
                    parent[next.Y, next.X] = new Pos(node.Y, node.X);
                }
            }

            return CalcCellPathFromParent(parent, dest);
        }

        List<Vector2Int> CalcCellPathFromParent(Pos[,] parent, Pos dest)
        {
            List<Vector2Int> cells = new List<Vector2Int>();

            int y = dest.Y;
            int x = dest.X;
            while (parent[y, x].Y != y || parent[y, x].X != x)
            {
                cells.Add(Pos2Cell(new Pos(y, x)));
                Pos pos = parent[y, x];
                y = pos.Y;
                x = pos.X;
            }
            cells.Add(Pos2Cell(new Pos(y, x)));
            cells.Reverse();

            return cells;
        }

        Pos Cell2Pos(Vector2Int cell)
        {
            // CellPos -> ArrayPos
            return new Pos(MaxY - cell.y, cell.x - MinX);
        }

        Vector2Int Pos2Cell(Pos pos)
        {
            // ArrayPos -> CellPos
            return new Vector2Int(pos.X + MinX, MaxY - pos.Y);
        }

        #endregion
    }
}
