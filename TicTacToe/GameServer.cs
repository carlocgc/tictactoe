using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace TicTacToe
{
    public class GameServer
    {
        private IPAddress _ServerAddress;

        private Int32 _Port;

        private TcpListener _ServerListener;

        private TcpClient _Client;

        private NetworkStream _MsgStream;

        private Boolean _Running = false;

        public GameServer()
        {
            _ServerAddress = IPAddress.Any;
            _Port = 6600;
            _ServerListener = new TcpListener(_ServerAddress, _Port);
        }

        public void Run()
        {
            // Wait for connection

            // if connected start game

            // Get client stream

            _Running = true;

            while (_Running)
            {
                // Prompt for move

                // Update board

                // Prompt client for move

                // Validate client move

                // Check for winner

                // Repeat
            }

            // Clean up game
        }
    }
}
