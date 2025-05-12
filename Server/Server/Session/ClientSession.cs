using Google.Protobuf;
using Google.Protobuf.Protocol;
using ServerCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    public class ClientSession : PacketSession
    {
        public Player MyPlayer { get; set; }
        public int SessionId { get; set; }

        public void Send(IMessage packet)
        {
            string msgName = packet.Descriptor.Name.Replace("_", string.Empty);
            MsgId msgId = (MsgId)Enum.Parse(typeof(MsgId), msgName);
            ushort size = (ushort)packet.CalculateSize();
            byte[] sendBuffer = new byte[size + 4];
            Array.Copy(BitConverter.GetBytes((ushort)(size + 4)), 0, sendBuffer, 0, sizeof(ushort));
            Array.Copy(BitConverter.GetBytes((ushort)msgId), 0, sendBuffer, 2, sizeof(ushort));
            Array.Copy(packet.ToByteArray(), 0, sendBuffer, 4, size);
            Send(new ArraySegment<byte>(sendBuffer));
        }

        public override void OnConnected(EndPoint endPoint)
        {
            Console.WriteLine($"OnConnected : {endPoint}");

            /*
            {
                PokemonInfo info = new PokemonInfo()
                {
                    DictionaryNum = 35,
                    NickName = "MESSI",
                    PokemonName = "Charmander",
                    Level = 10,
                    Gender = PokemonGender.Male,
                };
                PokemonSkill skill = new PokemonSkill()
                {
                    Stat = new PokemonStat()
                    {
                        Hp = 10,
                        MaxHp = 100,
                        Attack = 50,
                        Defense = 40,
                        SpecialAttack = 70,
                        SpecialDefense = 40,
                        Speed = 60
                    }
                };
                PokemonBattleMove battleMove = new PokemonBattleMove()
                {
                };
                Pokemon pokemon = new Pokemon(info, skill, battleMove);

                PokemonInfo info1 = new PokemonInfo()
                {
                    DictionaryNum = 35,
                    NickName = "PEDRO",
                    PokemonName = "Pikachu",
                    Level = 10,
                    Gender = PokemonGender.Male,
                };
                PokemonSkill skill1 = new PokemonSkill()
                {
                    Stat = new PokemonStat()
                    {
                        Hp = 10,
                        MaxHp = 100,
                        Attack = 50,
                        Defense = 40,
                        SpecialAttack = 70,
                        SpecialDefense = 40,
                        Speed = 60
                    }
                };
                PokemonBattleMove battleMove1 = new PokemonBattleMove()
                {
                };
                Pokemon pokemon1 = new Pokemon(info1, skill1, battleMove1);

                PokemonInfo info2 = new PokemonInfo()
                {
                    DictionaryNum = 35,
                    NickName = "VILLA",
                    PokemonName = "Squirtle",
                    Level = 10,
                    Gender = PokemonGender.Male,
                };
                PokemonSkill skill2 = new PokemonSkill()
                {
                    Stat = new PokemonStat()
                    {
                        Hp = 10,
                        MaxHp = 100,
                        Attack = 50,
                        Defense = 40,
                        SpecialAttack = 70,
                        SpecialDefense = 40,
                        Speed = 60
                    }
                };
                PokemonBattleMove battleMove2 = new PokemonBattleMove()
                {
                };
                Pokemon pokemon2 = new Pokemon(info2, skill2, battleMove2);

                MyPlayer.AddPokemon(pokemon);
                MyPlayer.AddPokemon(pokemon1);
                MyPlayer.AddPokemon(pokemon2);
            }
            */
        }

        public override void OnRecvPacket(ArraySegment<byte> buffer)
        {
            PacketManager.Instance.OnRecvPacket(this, buffer);
        }

        public override void OnDisconnected(EndPoint endPoint)
        {
            GameRoom room = RoomManager.Instance.Find(1);
            room.Push(room.LeaveRoom, MyPlayer.Info.ObjectId);

            SessionManager.Instance.Remove(this);

            Console.WriteLine($"OnDisconnected : {endPoint}");
        }

        public override void OnSend(int numOfBytes)
        {
            // Console.WriteLine($"Transferred bytes : {numOfBytes}");
        }
    }
}
