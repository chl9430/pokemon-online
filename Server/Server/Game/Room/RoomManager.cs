using Google.Protobuf.Protocol;
using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    public class RoomManager
    {
        public static RoomManager Instance { get; } = new RoomManager();

        object _lock = new object();

        Dictionary<RoomType, Dictionary<int, GameRoom>> _rooms = new Dictionary<RoomType, Dictionary<int, GameRoom>>();
        Dictionary<int, PokemonExchangeRoom> _exchangeRooms = new Dictionary<int, PokemonExchangeRoom>();

        int _exchangeRoomId = 1;

        public GameRoom Add(int mapId, RoomType roomType)
        {
            GameRoom gameRoom = null;

            lock (_lock)
            {
                if (_rooms.ContainsKey(roomType) == false)
                    _rooms.Add(roomType, new Dictionary<int, GameRoom>());

                if (roomType == RoomType.FriendlyShop)
                {
                    gameRoom = new FriendlyShop(roomType, _rooms[roomType].Count + 1);
                }
                else if (roomType == RoomType.PokemonCenter)
                {
                    gameRoom = new PokemonCenter(roomType, _rooms[roomType].Count + 1);
                }
                else
                {
                    gameRoom = new GameRoom(roomType, _rooms[roomType].Count + 1);
                }
                
                _rooms[roomType].Add(mapId, gameRoom);
            }

            return gameRoom;
        }

        public PokemonExchangeRoom Add()
        {
            PokemonExchangeRoom exchangeRoom = new PokemonExchangeRoom();

            lock (_lock)
            {
                exchangeRoom.RoomId = _exchangeRoomId;
                _exchangeRooms.Add(_exchangeRoomId, exchangeRoom);
                _exchangeRoomId++;
            }

            return exchangeRoom;
        }

        public bool Remove(RoomType roomType, int roomId)
        {
            lock (_lock)
            {
                return _rooms[roomType].Remove(roomId);
            }
        }

        public bool RemoveExchangeRoom(int exchangeRoomId)
        {
            lock (_lock)
            {
                return _exchangeRooms.Remove(exchangeRoomId);
            }
        }

        public GameRoom Find(int roomId, RoomType roomType)
        {
            lock (_lock)
            {
                GameRoom room = null;

                _rooms[roomType].TryGetValue(roomId, out room);

                return room;
            }
        }

        public PokemonExchangeRoom FindPokemonExchangeRoom(int roomId)
        {
            lock (_lock)
            {
                PokemonExchangeRoom room = null;
                if (_exchangeRooms.TryGetValue(roomId, out room))
                    return room;

                return null;
            }
        }
    }
}
