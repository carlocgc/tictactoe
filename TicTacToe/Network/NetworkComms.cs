using System;
using System.Net;
using System.Net.Sockets;

namespace TicTacToe.Network
{
    public class NetworkComms
    {
        private readonly IPAddress _LocalAddress;

        private readonly Int32 _LocalPort;

        private readonly TcpListener _ServerListener;

        private TcpClient _Client;

        private NetworkStream _MsgStream;

        private Boolean _Connected;

        public NetworkComms()
        {
            _LocalAddress = IPAddress.Any;
            _LocalPort = 6600;
            _ServerListener = new TcpListener(_LocalAddress, _LocalPort);
            _Connected = false;
        }

        /// <summary>
        /// Wait for an opponent, called by the host game
        /// </summary>
        public void WaitForClient()
        {
            Console.WriteLine($"Listening at {_LocalAddress} on port {_LocalPort}.");
            Console.WriteLine($"Waiting for opponent...");

            _ServerListener.Start();

            while (!_Connected)
            {
                if (!_ServerListener.Pending()) continue;

                _Client = _ServerListener.AcceptTcpClient();

                if (!_Client.Connected) continue;

                _Connected = true;
                Console.WriteLine($"{_Client.Client.RemoteEndPoint} connected...");
            }

            _MsgStream = _Client.GetStream();
        }

        /// <summary>
        /// Connect to a host, called by the client game
        /// </summary>
        /// <param name="hostAddress"></param>
        /// <param name="port"></param>
        /// <returns></returns>
        public Boolean ConnectToHost(IPAddress hostAddress, Int32 port)
        {
            _Client = new TcpClient();

            try
            {
                _Client.Connect(hostAddress, port);
            }
            catch (SocketException e)
            {
                Console.WriteLine($"Error connecting to host {hostAddress} : {e.Message}");
                return false;
            }

            if (_Client.Connected)
            {
                _Connected = true;
                _MsgStream = _Client.GetStream();
                return true;
            }

            Console.WriteLine($"Unable to connect to host {hostAddress}...");
            return false;
        }

        public void SendMove(String command, String message)
        {
            // TODO send a message and await 
        }
    }
}
