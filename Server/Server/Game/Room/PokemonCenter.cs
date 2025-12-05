using Google.Protobuf.Protocol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    public class PokemonCenter : GameRoom
    {
        public PokemonCenter(RoomType roomType, int roomId) : base(roomType, roomId)
        {
        }
    }
}
