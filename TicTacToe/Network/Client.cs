using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using TicTacToe.Interfaces;

namespace TicTacToe.Network
{
    public class Client : INotifier<IClientListener>
    {
        private readonly List<IClientListener> _Listeners = new List<IClientListener>();

        private TcpClient _Client;

        private readonly IPAddress _IpAddress;

        private readonly Int32 _Port;

        public Client(IPAddress serverIp, Int32 port)
        {
            _IpAddress = serverIp;
            _Port = port;
            _Client = new TcpClient(_IpAddress.ToString(), _Port);
        }

        public void SendMessage(String message)
        {
            try
            {
                Byte[] data = System.Text.Encoding.ASCII.GetBytes(message);

                NetworkStream stream = _Client.GetStream();

                stream.Write(data, 0, data.Length);

                stream.Close();
            }
            catch (ArgumentNullException e)
            {
                Console.WriteLine(e);
                throw;
            }
            catch (SocketException e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        public void ReceiveMessage()
        {
            try
            {
                using (NetworkStream stream = _Client.GetStream())
                {
                    Byte[] data = new Byte[1024];
                    Int32 bytes = stream.Read(data, 0, data.Length);
                    var response = System.Text.Encoding.ASCII.GetString(data, 0, bytes);

                    foreach (var listener in _Listeners)
                    {
                        listener.OnClientMessage(response);
                    }
                }
            }
            catch (ArgumentNullException e)
            {
                Console.WriteLine(e);
                throw;
            }
            catch (SocketException e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        public void Disconnect()
        {
            _Client.Close();
        }

        public void AddListener(IClientListener listener)
        {
            if (!_Listeners.Contains(listener)) _Listeners.Add(listener);
        }

        public void RemoveListener(IClientListener listener)
        {
            if (_Listeners.Contains(listener)) _Listeners.Remove(listener);
        }
    }
}
