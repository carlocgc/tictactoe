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
                _Server.AddListener(this);
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
                _Client.AddListener(this);
            }
        }

        public Int32 GetPlayerSymbol()
        {
            return _Server != null ? 1 : 2;
        }

        public void StartGame()
        {
            if (_Server == null)
            {
                foreach (var listener in _Listeners)
                {
                    listener.OnWaiting();
                }
            }
            else
            {
                foreach (var listener in _Listeners)
                {
                    listener.OnTurnToMove();
                }
            }
        }

        public void SendMove(Int32 x, Int32 y)
        {
            if (_Server == null)
            {
                _Client.SendMessage($"{x},{y}");
            }
            else
            {
                _Server.SendMessage($"{x},{y}");
            }

            foreach (var listener in _Listeners)
            {
                listener.OnWaiting();
            }
        }

        public void OnServerMessage(string message)
        {
            Tuple<Int32, Int32> move = StringToMove(message);
            foreach (var listener in _Listeners)
            {
                listener.OnMoveReceived(move.Item1, move.Item2);
            }
        }

        public void OnClientMessage(string message)
        {
            Tuple<Int32, Int32> move = StringToMove(message);
            foreach (var listener in _Listeners)
            {
                listener.OnMoveReceived(move.Item1, move.Item2);
            }
        }

        private Tuple<Int32, Int32> StringToMove(String raw)
        {
            String[] parts = raw.Split(',');
            Int32 x = Int32.Parse(parts[0]);
            Int32 y = Int32.Parse(parts[1]);
            return new Tuple<Int32, Int32>(x, y);
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
