using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using TicTacToe.Interfaces;

namespace TicTacToe.Network
{
    public class Client : INotifier<IMessageListener>
    {
        private readonly List<IMessageListener> _Listeners = new List<IMessageListener>();

        private TcpClient _Client;

        private readonly IPAddress _IpAddress;

        private readonly Int32 _Port;

        public Client(IPAddress serverIp, Int32 port)
        {
            _IpAddress = serverIp;
            _Port = port;
        }

        public void SendMessage(String message)
        {
            try
            {

                _Client = new TcpClient(_IpAddress.ToString(), _Port);
                Byte[] data = System.Text.Encoding.ASCII.GetBytes(message);

                NetworkStream stream = _Client.GetStream();

                stream.Write(data, 0, data.Length);

                String response = String.Empty;

                Int32 bytes = stream.Read(data, 0, data.Length);
                response = System.Text.Encoding.ASCII.GetString(data, 0, bytes);

                foreach (var listener in _Listeners)
                {
                    listener.OnMessage(response);
                }

                stream.Close();
                _Client.Close();

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
