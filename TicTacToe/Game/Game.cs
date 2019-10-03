using System;
using System.Net;
using System.Net.Sockets;
using TicTacToe.Data;
using TicTacToe.Network;

namespace TicTacToe.Game
{
    public class Game
    {
        private NetworkComms _Network;

        public Boolean _IsHost;

        public void Run()
        {
            DetermineHost();

            SetUpConnection();

            SendTestMessages();

            Console.ReadKey();
        }

        private void SendTestMessages()
        {
            Boolean running = true;

            while (running)
            {
                Console.WriteLine($"Enter a test message");

                String message = Console.ReadLine();

                _Network.SendMessage("message", message);
                Console.WriteLine($"SENT : {message}");

                String resp = _Network.ReceiveMessages();
                Console.WriteLine($"RESPONSE : {resp}");
            }
        }

        private void DetermineHost()
        {
            Boolean valid = false;

            while (!valid)
            {
                Console.WriteLine("Welcome to TicTacToe");
                Console.WriteLine("Enter \"Host\" or \"Client\"....");
                String resp = Console.ReadLine();

                if (resp == null) continue;

                if (resp.ToLower() == "host")
                {
                    _IsHost = true;
                    valid = true;
                }
                else if (resp.ToLower() == "client")
                {
                    _IsHost = false;
                    valid = true;
                }
                else
                {
                    Console.WriteLine($"Invalid input...");
                }
            }
        }

        private void SetUpConnection()
        {
            _Network = new NetworkComms();

            if (_IsHost)
            {
                _Network.WaitForClient();
            }
            else
            {
                IPAddress ip = IPAddress.None;
                Int32 port = 0;
                Boolean addressValid = false;

                while (!addressValid)
                {
                    Console.WriteLine($"Enter host ip address...");
                    addressValid = IPAddress.TryParse(Console.ReadLine() ?? "", out ip);
                }

                Boolean portValid = false;

                while (!portValid)
                {
                    Console.WriteLine($"Enter host port...");
                    portValid = Int32.TryParse(Console.ReadLine() ?? "", out port);
                }

                _Network.ConnectToHost(ip, port);
            }
        }
    }
}
