using System;
using System.Net;
using System.Net.Sockets;

namespace TicTacToe.Network
{
    public class Network
    {
        private enum Mode { Undefined, Server, Client }

        private Mode _Mode;

        private TcpListener _Server;

        private TcpClient _Client;

        private IPAddress _IpAddress;

        private Int32 _Port;

        public Network()
        {
            
        }

        private void Configure()
        {
            while (_Mode == Mode.Undefined)
            {
                Console.Clear();
                Console.WriteLine("(1) Server or (2) Client? ");
                if (!Enum.TryParse(Console.ReadLine(), out _Mode))
                {
                    _Mode = Mode.Undefined;
                }
            }
            if (_Mode == Mode.Server)
            {
                Console.WriteLine("Enter port of the server");
                Int32.TryParse(Console.ReadLine(), out Int32 port);

                _Server = new TcpListener(port);
                _Server.Start();

                Console.WriteLine("Server Started...!");
            }
            else
            {
                Console.WriteLine("Enter server address...");
                IPAddress.TryParse(Console.ReadLine() ?? "", out IPAddress ip);
                Console.WriteLine("Enter port...");
                Int32.TryParse(Console.ReadLine(), out Int32 port);

                _Client = new TcpClient(ip.ToString(), port);
            }
        }

        private IPAddress GetLocalIpAddress()
        {
            IPAddress ip = IPAddress.None;
            using (Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, 0))
            {
                socket.Connect("8.8.8.8", 65530);
                IPEndPoint endPoint = socket.LocalEndPoint as IPEndPoint;
                if (endPoint != null) ip = IPAddress.Parse(endPoint.Address.ToString());
            }
            return ip;
        }
    }
}
