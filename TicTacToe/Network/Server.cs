using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using TicTacToe.Interfaces;

namespace TicTacToe.Network
{
    public class Server : INotifier<IMessageListener>
    {
        private readonly List<IMessageListener> _Listeners = new List<IMessageListener>();

        private readonly TcpListener _Server;

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
                String data = null;

                while (true)
                {
                    foreach (var listener in _Listeners)
                    {
                        listener.OnMessage("Waiting for connection...");
                    }

                    using (TcpClient client = _Server.AcceptTcpClient())
                    {
                        foreach (var listener in _Listeners)
                        {
                            listener.OnMessage("Connected!");
                        }

                        using (NetworkStream stream = client.GetStream())
                        {
                            data = null;
                            Int32 i;

                            while ((i = stream.Read(bytes, 0, bytes.Length)) != 0)
                            {
                                data = System.Text.Encoding.ASCII.GetString(bytes, 0, i);

                                foreach (var connectionListener in _Listeners)
                                {
                                    connectionListener.OnMessage(data);
                                }

                                data = data.ToUpper();

                                Byte[] msg = System.Text.Encoding.ASCII.GetBytes(data);

                                stream.Write(msg, 0, msg.Length);

                                foreach (var listener in _Listeners)
                                {
                                    listener.OnMessage($"Sent: {data}");
                                }
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

        public void AddListener(IMessageListener listener)
        {
            if (!_Listeners.Contains(listener)) _Listeners.Add(listener);
        }

        public void RemoveListener(IMessageListener listener)
        {
            if (_Listeners.Contains(listener)) _Listeners.Remove(listener);
        }
    }
}
