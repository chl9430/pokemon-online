using Google.Protobuf;
using Google.Protobuf.Protocol;
using System;
using System.Collections.Generic;
using System.Linq;
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
        public int RoomId { get; set; }

        Dictionary<int, Player> _players = new Dictionary<int, Player>();

        public Dictionary<int, Player> Players { get { return _players; } }

        public Map Map { get; private set; } = new Map();

        public void Init(int mapId)
        {
            Map.LoadMap(mapId);
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

            if (type == GameObjectType.Player)
            {
                Player player = gameObject as Player;
                _players.Add(gameObject.Id, player);
                player.Room = this;

                Map.ApplyMove(player, new Vector2Int(player.CellPos.x, player.CellPos.y));

                // 본인한테 정보 전송
                S_EnterRoom enterPacket = new S_EnterRoom();

                PlayerInfo playerInfo = player.MakePlayerInfo();
                
                enterPacket.PlayerInfo = playerInfo;

                player.Session.Send(enterPacket);

                // 본인한테 타인 정보 전송
                S_Spawn spawnPacket = new S_Spawn();
                foreach (Player p in _players.Values)
                {
                    if (player != p)
                    {
                        spawnPacket.Players.Add(p.MakePlayerInfo());
                    }
                }

                player.Session.Send(spawnPacket);
            }

            // 타인한테 본인 정보 전송
            {
                Player player = gameObject as Player;

                S_Spawn spawnPacket = new S_Spawn();

                spawnPacket.Players.Add(player.MakePlayerInfo());

                foreach (Player p in _players.Values)
                {
                    if (p.Id != gameObject.Id)
                        p.Session.Send(spawnPacket);
                }
            }
        }

        public void LeaveRoom(int objectId)
        {
            GameObjectType type = ObjectManager.GetObjectTypeById(objectId);

            if (type == GameObjectType.Player)
            {
                Player player = null;
                if (_players.Remove(objectId, out player) == false)
                    return;

                Map.ApplyLeave(player);
                player.Room = null;

                // 본인한테 정보 전송
                S_LeaveRoom leavePacket = new S_LeaveRoom();
                player.Session.Send(leavePacket);
            }

            // 타인한테 정보 전송
            S_Despawn despawnPacket = new S_Despawn();
            despawnPacket.ObjectIds.Add(objectId);
            
            foreach (Player p in _players.Values)
            {
                if (p.Id != objectId)
                    p.Session.Send(despawnPacket);
            }
        }

        public void HandleMove(Player player, C_Move movePacket)
        {
            var a = ObjectManager.Instance.Players;

            if (player == null)
                return;

            PositionInfo movePosInfo = movePacket.PosInfo;
            ObjectInfo info = player.Info;

            // 다른 좌표로 갈 수 있는지 체크
            if (movePosInfo.PosX != info.PosInfo.PosX || movePosInfo.PosY != info.PosInfo.PosY)
            {
                if (Map.CanGo(new Vector2Int(movePosInfo.PosX, movePosInfo.PosY)) == false)
                    return;
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

        public void Broadcast(Player player, IMessage packet)
        {
            foreach (Player p in _players.Values)
            {
                if (p.Id != player.Id)
                    p.Session.Send(packet);
            }
        }
    }
}
