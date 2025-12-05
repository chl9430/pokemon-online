using Google.Protobuf.Protocol;
using Server;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Sockets;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

public struct ExchangeInfo
{
    public Player player;
    public int cursorX;
    public int cursorY;
    public int selectedPokemonOrder;
    public bool finalAnswer;
}

namespace Server
{
    public class PokemonExchangeRoom : JobSerializer
    {
        int _roomId;
        int _maxPlayerCount = 2;
        List<ExchangeInfo> _exchangePlayers = new List<ExchangeInfo>();

        public int RoomId { get { return _roomId; } set { _roomId = value; } }
        public int MaxPlayerCount { get { return _maxPlayerCount; } }
        public List<ExchangeInfo> ExchangePlayers { get { return _exchangePlayers; } }

        public void EnterRoom(Player player)
        {
            ExchangeInfo info = new ExchangeInfo()
            {
                player = player,
                cursorX = 0,
                cursorY = 0,
                selectedPokemonOrder = -1,
                finalAnswer = false
            };

            _exchangePlayers.Add(info);

            if (_exchangePlayers.Count == 2)
                BeginExchange();
        }

        void BeginExchange()
        {
            foreach (ExchangeInfo info in _exchangePlayers)
            {
                S_EnterPokemonExchangeScene s_EnterScenePacket = new S_EnterPokemonExchangeScene();
                s_EnterScenePacket.PlayerInfo = info.player.MakePlayerInfo();

                foreach (Pokemon pokemon in info.player.Pokemons)
                    s_EnterScenePacket.MyPokemonSums.Add(pokemon.MakePokemonSummary());

                s_EnterScenePacket.MyCursorPos = new ExchangeCursorPos()
                {
                    X = info.cursorX,
                    Y = info.cursorY,
                };

                foreach (ExchangeInfo otherInfo in _exchangePlayers)
                {
                    if (otherInfo.player.Id == info.player.Id)
                        continue;

                    s_EnterScenePacket.OtherPlayerInfo = otherInfo.player.MakeOtherPlayerInfo();

                    foreach (Pokemon pokemon in otherInfo.player.Pokemons)
                    {
                        s_EnterScenePacket.OtherPokemonSums.Add(pokemon.MakePokemonSummary());
                    }

                    s_EnterScenePacket.OtherCursorPos = new ExchangeCursorPos()
                    {
                        X = otherInfo.cursorX,
                        Y = otherInfo.cursorY,
                    };
                }

                info.player.Session.Send(s_EnterScenePacket);
            }
        }

        public void ReturnRoom(Player player)
        {
            S_EnterPokemonExchangeScene s_EnterScenePacket = new S_EnterPokemonExchangeScene();
            s_EnterScenePacket.PlayerInfo = player.MakePlayerInfo();

            ExchangeInfo myInfo = new ExchangeInfo();
            ExchangeInfo otherInfo = new ExchangeInfo();

            foreach (Pokemon pokemon in player.Pokemons)
                s_EnterScenePacket.MyPokemonSums.Add(pokemon.MakePokemonSummary());

            foreach (ExchangeInfo info in _exchangePlayers)
            {
                if (player.Id != info.player.Id)
                {
                    s_EnterScenePacket.OtherPlayerInfo = info.player.MakeOtherPlayerInfo();

                    foreach (Pokemon pokemon in info.player.Pokemons)
                        s_EnterScenePacket.OtherPokemonSums.Add(pokemon.MakePokemonSummary());

                    s_EnterScenePacket.OtherCursorPos = new ExchangeCursorPos();
                    s_EnterScenePacket.OtherCursorPos.X = info.cursorX;
                    s_EnterScenePacket.OtherCursorPos.Y = info.cursorY;

                    otherInfo = info;
                }
                else
                {
                    s_EnterScenePacket.MyCursorPos = new ExchangeCursorPos();
                    s_EnterScenePacket.MyCursorPos.X = info.cursorX;
                    s_EnterScenePacket.MyCursorPos.Y = info.cursorY;

                    myInfo = info;
                }
            }


            if (myInfo.selectedPokemonOrder != -1 && otherInfo.selectedPokemonOrder != -1)
                s_EnterScenePacket.OtherPokemonSum = otherInfo.player.Pokemons[otherInfo.selectedPokemonOrder].MakePokemonSummary();

            player.Session.Send(s_EnterScenePacket);
        }

        public void SetSelectedPokemonOrder(Player player, int order)
        {
            for (int i = 0; i < _exchangePlayers.Count; i++)
            {
                ExchangeInfo info = _exchangePlayers[i];
                if (info.player.Id == player.Id)
                {
                    info.selectedPokemonOrder = order;

                    _exchangePlayers[i] = info;
                }
            }

            foreach (ExchangeInfo info in _exchangePlayers)
            {
                if (info.selectedPokemonOrder == -1)
                    return;
            }

            // 전부 교환 준비가 되었으면
            foreach (ExchangeInfo info in _exchangePlayers)
            {
                S_ChooseExchangePokemon exchangePacket = new S_ChooseExchangePokemon();

                foreach (ExchangeInfo otherInfo in _exchangePlayers)
                {
                    if (otherInfo.player.Id != info.player.Id)
                    {
                        Pokemon pokemon = otherInfo.player.Pokemons[otherInfo.selectedPokemonOrder];
                        exchangePacket.OtherPokemonSum = pokemon.MakePokemonSummary();
                    }
                }

                info.player.Session.Send(exchangePacket);
            }
        }

        public void SetFinalAnswer(Player player, bool finalAnswer)
        {
            if (finalAnswer == false)
            {
                foreach (ExchangeInfo info in _exchangePlayers)
                {
                    if (info.player.Id != player.Id)
                    {
                        S_FinalAnswerToExchange finalAnswerPacket = new S_FinalAnswerToExchange();
                        finalAnswerPacket.MyPokemonSum = null;
                        finalAnswerPacket.OtherPokemonSum = null;

                        info.player.Session.Send(finalAnswerPacket);
                    }
                }

                // 초기화 설정할 부분을 초기화한다.
                for (int i = 0; i < _exchangePlayers.Count; i++)
                {
                    ExchangeInfo info = _exchangePlayers[i];
                    info.selectedPokemonOrder = -1;
                    info.finalAnswer = false;

                    _exchangePlayers[i] = info;
                }
            }
            else
            {
                for (int i = 0; i < _exchangePlayers.Count; i++)
                {
                    ExchangeInfo info = _exchangePlayers[i];
                    if (info.player.Id == player.Id)
                    {
                        info.finalAnswer = finalAnswer;

                        _exchangePlayers[i] = info;
                    }
                }

                foreach (ExchangeInfo info in _exchangePlayers)
                {
                    if (info.finalAnswer == false)
                        return;
                }

                // 서버에서 교환처리
                Pokemon firstPokemon = _exchangePlayers[0].player.Pokemons[_exchangePlayers[0].selectedPokemonOrder];
                Pokemon secondPokemon = _exchangePlayers[1].player.Pokemons[_exchangePlayers[1].selectedPokemonOrder];

                _exchangePlayers[0].player.Pokemons[_exchangePlayers[0].selectedPokemonOrder] = secondPokemon;
                _exchangePlayers[1].player.Pokemons[_exchangePlayers[1].selectedPokemonOrder] = firstPokemon;

                // 전부 교환을 승낙했다면 교환처리를 한다.
                foreach (ExchangeInfo info in _exchangePlayers)
                {
                    S_FinalAnswerToExchange finalAnswerPacket = new S_FinalAnswerToExchange();

                    foreach (ExchangeInfo otherInfo in _exchangePlayers)
                    {
                        if (otherInfo.player.Id == info.player.Id)
                        {
                            Pokemon otherPokemon = otherInfo.player.Pokemons[otherInfo.selectedPokemonOrder];

                            finalAnswerPacket.OtherPokemonSum = otherPokemon.MakePokemonSummary();
                        }
                        else
                        {
                            Pokemon myPokemon = otherInfo.player.Pokemons[otherInfo.selectedPokemonOrder];

                            finalAnswerPacket.MyPokemonSum = myPokemon.MakePokemonSummary();
                        }
                    }

                    info.player.Session.Send(finalAnswerPacket);
                }

                // 초기화 설정할 부분을 초기화한다.
                for (int i = 0; i < _exchangePlayers.Count; i++)
                {
                    ExchangeInfo info = _exchangePlayers[i];
                    info.selectedPokemonOrder = -1;
                    info.finalAnswer = false;

                    _exchangePlayers[i] = info;
                }
            }
        }

        public void HandlerCursorMove(Player player, int x, int y)
        {
            for (int i = 0; i < _exchangePlayers.Count; i++)
            {
                ExchangeInfo info = _exchangePlayers[i];
                if (info.player.Id == player.Id)
                {
                    info.cursorX = x;
                    info.cursorY = y;

                    _exchangePlayers[i] = info;
                }
            }

            foreach (ExchangeInfo otherinfo in _exchangePlayers)
            {
                S_MoveExchangeCursor s_MoveCursorPacket = new S_MoveExchangeCursor();
                s_MoveCursorPacket.X = x;
                s_MoveCursorPacket.Y = y;

                if (otherinfo.player.Id != player.Id)
                    otherinfo.player.Session.Send(s_MoveCursorPacket);
            }
        }

        public void ExitExchangeRoom(Player player)
        {
            foreach (ExchangeInfo info in _exchangePlayers)
            {
                if (info.player.Id != player.Id)
                {
                    S_ExitPokemonExchangeScene exitExchangePacket = new S_ExitPokemonExchangeScene();
                    exitExchangePacket.ExitPlayerInfo = player.MakePlayerInfo();

                    info.player.Session.Send(exitExchangePacket);
                }

                // 교환방을 나가고 교환방 삭제
                Player p = info.player;
                p.ExchangeRoom = null;
                RoomManager.Instance.RemoveExchangeRoom(_roomId);
            }
        }
    }
}
