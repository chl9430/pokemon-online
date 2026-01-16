using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace ServerCore
{
    public class Connector
    {
        Func<Session> _sessionFactory;

        void RegisterConnect(SocketAsyncEventArgs args)
        {
            //Socket socket = args.UserToken as Socket;
            //if (socket == null)
            //    return;

            //bool pending = socket.ConnectAsync(args);
            //if (pending == false)
            //    OnConnectedCompleted(null, args);
        }

        void OnConnectedCompleted(object sender, SocketAsyncEventArgs args)
        {
            //if (args.SocketError == SocketError.Success)
            //{
            //    Session session = _sessionFactory.Invoke();
            //    session.Start(args.ConnectSocket);
            //    session.OnConnected(args.RemoteEndPoint);
            //}
            //else
            //{
            //    Console.WriteLine($"OnConnectedCompleted Fail : {args.SocketError}");
            //}
        }
    }
}
