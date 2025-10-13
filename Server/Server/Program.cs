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
    public class Program
    {
        static Listener _listener = new Listener();

        static void Main(string[] args)
        {
            ConfigManager.LoadConfig();
            DataManager.LoadData();

            GameRoom mapRoom = RoomManager.Instance.Add(1, RoomType.Map);
            GameRoom pokemonCenterRoom = RoomManager.Instance.Add(1, RoomType.PokemonCenter);
            GameRoom friendlyShopRoom = RoomManager.Instance.Add(1, RoomType.FriendlyShop);
            mapRoom.TickRoom(50);
            pokemonCenterRoom.TickRoom(50);
            friendlyShopRoom.TickRoom(50);

            // DNS (Domain Name System)
            string host = Dns.GetHostName();
            IPHostEntry ipHost = Dns.GetHostEntry(host);
            IPAddress ipAddr = ipHost.AddressList[0];
            IPEndPoint endPoint = new IPEndPoint(ipAddr, 7777);

            _listener.Init(endPoint, () => { return SessionManager.Instance.Generate(); });
            Console.WriteLine("Listening...");

            while (true)
            {
                Thread.Sleep(100);
            }
        }
    }
}
