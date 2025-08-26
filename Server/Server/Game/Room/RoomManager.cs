using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    public class RoomManager
    {
        public static RoomManager Instance { get; } = new RoomManager();

        object _lock = new object();

        Dictionary<int, GameRoom> _rooms = new Dictionary<int, GameRoom>();
        Dictionary<int, PokemonExchangeRoom> _exchangeRooms = new Dictionary<int, PokemonExchangeRoom>();

        int _roomId = 1;
        int _exchangeRoomId = 1;

        public GameRoom Add(int mapId)
        {
            GameRoom gameRoom = new GameRoom();
            gameRoom.Push(gameRoom.Init, mapId);

            lock (_lock)
            {
                gameRoom.RoomId = _roomId;
                _rooms.Add(_roomId, gameRoom);
                _roomId++;
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

        public bool Remove(int roomId)
        {
            lock (_lock)
            {
                return _rooms.Remove(roomId);
            }
        }

        public bool RemoveExchangeRoom(int exchangeRoomId)
        {
            lock (_lock)
            {
                return _exchangeRooms.Remove(exchangeRoomId);
            }
        }

        public GameRoom Find(int roomId)
        {
            lock (_lock)
            {
                GameRoom room = null;
                if (_rooms.TryGetValue(roomId, out room))
                    return room;

                return null;
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
