using System;
using System.Collections.Generic;
using System.IO.Pipes;
using System.Net;
using System.Net.Sockets;
using TicTacToe.Interfaces;

namespace TicTacToe.Network
{
    public class Server : INotifier<IServerListener>
    {
        private readonly List<IServerListener> _Listeners = new List<IServerListener>();

        private readonly TcpListener _Server;

        private Boolean _MessageToSend;

        private String _Message;

        public Server(Int32 port)
        {
            _Server = new TcpListener(GetLocalIpAddress(), port);
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

        public void Start()
        {
            try
            {
                _Server.Start();

                Byte[] bytes = new Byte[256];

                while (true)
                {
                    Console.WriteLine("Waiting for opponent to connect...");

                    using (TcpClient client = _Server.AcceptTcpClient())
                    {
                        Console.WriteLine("Opponent connected!");

                        using (NetworkStream stream = client.GetStream())
                        {
                            Int32 i;

                            while ((i = stream.Read(bytes, 0, bytes.Length)) != 0)
                            {
                                var data = System.Text.Encoding.ASCII.GetString(bytes, 0, i);

                                foreach (var connectionListener in _Listeners)
                                {
                                    connectionListener.OnServerMessage(data);
                                }
                            }

                            while (_MessageToSend)
                            {
                                Byte[] msg = System.Text.Encoding.ASCII.GetBytes(_Message);
                                stream.Write(msg, 0, msg.Length);
                                _Message = String.Empty;
                                _MessageToSend = false;
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
            finally
            {
                _Server.Stop();
            }
        }

        public void SendMessage(String message)
        {
            _Message = message;
            _MessageToSend = true;
        }

        public void AddListener(IServerListener listener)
        {
            if (!_Listeners.Contains(listener)) _Listeners.Add(listener);
        }

        public void RemoveListener(IServerListener listener)
        {
            if (_Listeners.Contains(listener)) _Listeners.Remove(listener);
        }
    }
}
