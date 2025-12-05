using Google.Protobuf;
using Google.Protobuf.Protocol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using System.Xml.Serialization;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Server
{
    public class GameRoom : JobSerializer
    {
        int _roomId;
        RoomType _roomType;
        RoomInfo _roomInfo;
        protected Dictionary<GameObjectType, Dictionary<int, GameObject>> _objs = new Dictionary<GameObjectType, Dictionary<int, GameObject>>();
        Random _ran;

        public int RoomId { get { return _roomId; } }

        public Dictionary<GameObjectType, Dictionary<int, GameObject>> Objs { get { return _objs; } }

        public Dictionary<int, GameObject> Players { get { return _objs[GameObjectType.Player]; } }

        public Dictionary<int, GameObject> NPCs { get { return _objs[GameObjectType.Npc]; } }

        public Map Map { get; private set; } = new Map();

        public RoomType RoomType { get { return _roomType; } }

        public GameRoom(RoomType roomType, int roomId)
        {
            _roomId = roomId;
            _roomType = roomType;

            if (DataManager.RoomDoorPathDict.TryGetValue(roomType, out RoomInfo[] value))
            {
                _roomInfo = value[roomId - 1];
            }

            Map.GameRoom = this;
            Map.LoadMap(roomId, roomType);
        }

        public void Update()
        {
            Flush();
        }

        public void EnterRoom(GameObject gameObject)
        {
            if (gameObject == null)
                return;

            GameObjectType type = ObjectManager.GetObjectTypeById(gameObject.Id);
            bool isNew = false;

            if (type == GameObjectType.Player)
            {
                Player player = gameObject as Player;

                if (_objs.ContainsKey(GameObjectType.Player) == false)
                    _objs.Add(GameObjectType.Player, new Dictionary<int, GameObject>());

                if (_objs[GameObjectType.Player].ContainsKey(gameObject.Id) == false)
                {
                    _objs[GameObjectType.Player].Add(gameObject.Id, player);
                    isNew = true;
                }

                player.Room = this;

                Vector2Int playerInitPos = new Vector2Int(player.CellPos.x, player.CellPos.y);

                Map.ApplyMove(player, playerInitPos);

                // 테스트 환경에서 게임 룸에 없는 경우 패킷 전송 불필요
                if (IsActiveInGaemRoom(player))
                {
                    // 본인한테 정보 전송
                    S_EnterRoom enterPacket = new S_EnterRoom();

                    PlayerInfo playerInfo = player.MakePlayerInfo();

                    enterPacket.PlayerInfo = playerInfo;
                    enterPacket.RoomId = _roomInfo.roomId;
                    enterPacket.RoomType = _roomType;

                    if (_objs.ContainsKey(GameObjectType.Npc) == true)
                    {
                        foreach (NPC npc in _objs[GameObjectType.Npc].Values)
                        {
                            enterPacket.NpcInfos.Add(npc.MakeNPCInfo());
                        }
                    }

                    player.Session.Send(enterPacket);

                    // 본인한테 타인 정보 전송
                    S_Spawn spawnPacket = new S_Spawn();
                    foreach (Player p in _objs[GameObjectType.Player].Values)
                    {
                        if (player != p)
                        {
                            spawnPacket.OtherPlayers.Add(p.MakeOtherPlayerInfo());
                        }
                    }

                    player.Session.Send(spawnPacket);
                }

                TileType tileType = Map.GetTileType(new Vector2Int(player.PosInfo.PosX, player.PosInfo.PosY));
                if (tileType == TileType.DOOR)
                {
                    S_GetDoorDestDir destDirPacket = new S_GetDoorDestDir();

                    int doorId = Map.GetDoorId(new Vector2Int(player.PosInfo.PosX, player.PosInfo.PosY));

                    if (DataManager.RoomDoorPathDict.TryGetValue(_roomType, out RoomInfo[] value))
                    {
                        destDirPacket.DestDir = value[RoomId - 1].doors[doorId - 1].destDir;
                    }

                    player.Session.Send(destDirPacket);
                }
            }

            {
                Player player = gameObject as Player;

                if (IsActiveInGaemRoom(player) && isNew)
                {
                    // 타인한테 본인 정보 전송
                    S_Spawn spawnPacket = new S_Spawn();
                    spawnPacket.OtherPlayers.Add(player.MakeOtherPlayerInfo());

                    Broadcast(player, spawnPacket);
                }
            }
        }

        public void LeaveRoom(GameObject gameObject)
        {
            GameObjectType type = ObjectManager.GetObjectTypeById(gameObject.Id);

            if (type == GameObjectType.Player)
            {
                GameObject player = null;
                if (_objs[GameObjectType.Player].Remove(gameObject.Id, out player) == false)
                    return;

                Map.ApplyLeave(player);
                player.Room = null;

                // 본인한테 정보 전송
                S_LeaveRoom leavePacket = new S_LeaveRoom();
                ((Player)player).Session.Send(leavePacket);
            }

            {
                Player player = gameObject as Player;

                // 타인한테 정보 전송
                S_Despawn despawnPacket = new S_Despawn();
                despawnPacket.ObjectIds.Add(player.Id);

                Broadcast(player, despawnPacket);
            }
        }

        public void MoveAnotherRoom(Player player)
        {
            if (player == null)
                return;

            ObjectInfo info = player.Info;

            int doorId = Map.GetDoorId(new Vector2Int(info.PosInfo.PosX, info.PosInfo.PosY));

            DoorDestInfo door = _roomInfo.doors[doorId - 1];

            RoomType destRoomType = door.destRoomType;
            int destRoomId = door.destRoomId;

            LeaveRoom(player);

            GameRoom destRoom = RoomManager.Instance.Find(destRoomId, destRoomType);
            int enterDoorId = destRoom.FindDoorId(_roomInfo.roomId, _roomType);
            Vector2Int playerInitPos = destRoom.Map.GetDoorPos(enterDoorId);

            player.Info.PosInfo.PosX = playerInitPos.x;
            player.Info.PosInfo.PosY = playerInitPos.y;
            player.Info.PosInfo.State = CreatureState.Idle;

            destRoom.EnterRoom(player);
        }

        public int FindDoorId(int destRoomId, RoomType destRoomType)
        {
            int enterDoorId = 0;
            for (int i = 0; i < _roomInfo.doors.Length; i++)
            {
                if (_roomInfo.doors[i].destRoomId == destRoomId && _roomInfo.doors[i].destRoomType == destRoomType)
                {
                    enterDoorId = _roomInfo.doors[i].doorId;
                    break;
                }
            }

            return enterDoorId;
        }

        public void HandleMove(Player player, C_Move movePacket)
        {
            if (player == null)
                return;

            PositionInfo movePosInfo = movePacket.PosInfo;
            ObjectInfo info = player.Info;

            // 방향만 바꾼게 아니라면(실제로 이동하였다면) 
            if (movePosInfo.PosX != info.PosInfo.PosX || movePosInfo.PosY != info.PosInfo.PosY)
            {
                TileType nextTile = Map.GetTileType(new Vector2Int(movePosInfo.PosX, movePosInfo.PosY));
                if (nextTile == TileType.COLLISION)
                {
                    return;
                }
                else if (nextTile == TileType.BUSH)
                {
                    if (_ran == null)
                        _ran = new Random();

                    int metPokemonRate = _ran.Next(0, 100);

                    if (metPokemonRate < 20)
                    {
                        int bushNum = Map.GetBushId(new Vector2Int(movePosInfo.PosX, movePosInfo.PosY));

                        S_MeetWildPokemon meetPokemonPacket = new S_MeetWildPokemon();
                        meetPokemonPacket.RoomId = RoomId;
                        meetPokemonPacket.BushNum = bushNum;

                        player.Session.Send(meetPokemonPacket);
                    }
                }
                else if (nextTile == TileType.DOOR)
                {
                    S_GetDoorDestDir destDirPacket = new S_GetDoorDestDir();

                    int doorId = Map.GetDoorId(new Vector2Int(movePosInfo.PosX, movePosInfo.PosY));

                    if (DataManager.RoomDoorPathDict.TryGetValue(_roomType, out RoomInfo[] value))
                    {
                        destDirPacket.DestDir = value[RoomId - 1].doors[doorId - 1].destDir;
                    }

                    player.Session.Send(destDirPacket);
                }
            }

            info.PosInfo.State = movePosInfo.State;
            info.PosInfo.MoveDir = movePosInfo.MoveDir;
            Map.ApplyMove(player, new Vector2Int(movePosInfo.PosX, movePosInfo.PosY));

            PositionInfo posInfo = player.PosInfo;
            posInfo.PosX = movePosInfo.PosX;
            posInfo.PosY = movePosInfo.PosY;

            // 타인한테 정보 전송
            S_Move resMovePacket = new S_Move();
            resMovePacket.ObjectId = player.Info.ObjectId;
            resMovePacket.PosInfo = movePacket.PosInfo;

            Broadcast(player, resMovePacket);
        }

        public void Broadcast(Player myPlayer, IMessage packet)
        {
            foreach (Player player in _objs[GameObjectType.Player].Values)
            {
                if (player.Id != myPlayer.Id)
                {
                    if (IsActiveInGaemRoom(player))
                    {
                        player.Session.Send(packet);
                    }
                }
            }
        }

        public void GetAnotherPlayerData(Player myPlayer)
        {
            S_ReturnGame returnGame = new S_ReturnGame();

            foreach (Player player in _objs[GameObjectType.Player].Values)
            {
                if (player.Id != myPlayer.Id)
                {
                    returnGame.OtherPlayers.Add(player.Info);
                }
            }

            myPlayer.Session.Send(returnGame);
        }

        bool IsActiveInGaemRoom(Player player)
        {
            // 게임 맵에 있지 않은 플레이어들에겐 브로드캐스트를 할 필요가 없다.
            if (player.Info.PosInfo.State == CreatureState.Shopping ||
                player.Info.PosInfo.State == CreatureState.WatchMenu ||
                player.Info.PosInfo.State == CreatureState.Fight ||
                player.Info.PosInfo.State == CreatureState.Exchanging ||
                player.Info.PosInfo.State == CreatureState.PokemonEvolving)
            {
                return false;
            }
            else
                return true;
        }
    }
}
