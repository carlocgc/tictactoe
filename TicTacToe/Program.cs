using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace TicTacToe
{
    class Program
    {
        private static GameServer _GameServer;

        private static GameClient _GameClient;

        private static Boolean _IsHost;

        static void Main(string[] args)
        {
            InitialiseGame();
        }

        
        private static void InitialiseGame()
        {
            Boolean valid = false;

            while (!valid)
            {
                Console.WriteLine("Welcome to TicTacToe");
                Console.WriteLine("Enter \"Host\" or \"Client\"....");
                String resp = Console.ReadLine();

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

            if (_IsHost)
            {
                _GameServer = new GameServer();
                _GameServer.Run();
            }
            else
            {
                _GameClient = new GameClient();
                _GameClient.Run();
            }
        }
    }
}
