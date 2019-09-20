using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using TicTacToe.Interfaces;

namespace TicTacToe.Network
{
    public class Network : INotifier<INetworkListener>, IServerListener, IClientListener
    {
        private enum Mode { Undefined, Server, Client }

        private readonly List<INetworkListener> _Listeners = new List<INetworkListener>();

        private Mode _Mode;

        private Server _Server;

        private Client _Client;

        public void Initialise()
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

                _Server = new Server(port);
                _Server.Start();

                Console.WriteLine("Server Started...!");
            }
            else
            {
                Console.WriteLine("Enter server address...");
                IPAddress.TryParse(Console.ReadLine() ?? "", out IPAddress ip);
                Console.WriteLine("Enter port...");
                Int32.TryParse(Console.ReadLine(), out Int32 port);

                _Client = new Client(ip, port);
            }
        }

        public Int32 GetPlayerSymbol()
        {
            return _Server != null ? 1 : 2;
        }

        public void OnServerMessage(string message)
        {

        }

        public void OnClientMessage(string message)
        {

        }

        public void AddListener(INetworkListener listener)
        {
            if (!_Listeners.Contains(listener)) _Listeners.Add(listener);
        }

        public void RemoveListener(INetworkListener listener)
        {
            if (_Listeners.Contains(listener)) _Listeners.Remove(listener);
        }

    }
}
