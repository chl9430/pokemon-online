using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    public class PlayerTalkRoom
    {
        object _lock = new object();
        Player _talkSender;
        Player _talkReceiver;

        PokemonExchangeRoom _exchangeRoom;

        public PlayerTalkRoom(Player talkSender, Player talkReceiver)
        {
            _talkSender = talkSender;
            _talkReceiver = talkReceiver;
        }

        public Player GetAnotherPlayer(Player myPlayer)
        {
            if (myPlayer == _talkSender)
                return _talkReceiver;
            else
                return _talkSender;
        }

        public void CreatePokmeonExchangeRoom(Player myPlayer)
        {
            lock (_lock)
            {
                if (_exchangeRoom == null)
                {
                    _exchangeRoom = RoomManager.Instance.Add();
                    _exchangeRoom.TickRoom(50);
                }
            }

            myPlayer.ExchangeRoom = _exchangeRoom;

            if (myPlayer == _talkSender)
            {
                _exchangeRoom.Push(_exchangeRoom.EnterRoom, myPlayer);
            }
            else
            {
                _exchangeRoom.Push(_exchangeRoom.EnterRoom, myPlayer);
            }
        }
    }
}