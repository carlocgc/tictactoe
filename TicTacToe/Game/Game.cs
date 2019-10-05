using System;
using System.Collections.Generic;
using System.Net;
using TicTacToe.Network;
using static TicTacToe.Data.GameData;

namespace TicTacToe.Game
{
    public class Game
    {
        /// <summary> Message handler functions  </summary>
        private readonly Dictionary<Command, Func<String, Boolean>> _MessageHandlers = new Dictionary<Command, Func<String, Boolean>>();
        /// <summary> Sends and receives messages as packets </summary>
        private MessageService _MessageService;
        /// <summary> Whether or not this player is the game host </summary>
        private Boolean _IsHost;
        /// <summary> Whether the game is running </summary>
        private Boolean _Running = false;
        /// <summary> Whether its the players turn </summary>
        private Boolean _Moving;

        /// <summary> Sets up the message handlers, called once at game start </summary>
        private void Initialise()
        {
            _MessageHandlers.Add(Command.MESSAGE, HandleMessage);
        }

        /// <summary> Main game loop </summary>
        public void Run()
        {
            Initialise();

            DetermineHost();

            SetUpConnection();

            if (_IsHost)
            {
                _Moving = true;
            }

            _Running = true;

            while (_Running)
            {
                if (_IsHost)
                {
                    if (_Moving)
                    {
                        // Get input

                        // Check for win

                        // Send game state

                        _Moving = false;
                    }
                    else
                    {
                        // await move request

                        // validate move and reply

                        // update board

                        // check for win
                        
                        // send game state

                        _Moving = true;
                    }
                }
                else
                {
                    if (_Moving)
                    {
                        // get input

                        // validate move

                        // send input to host

                        // await reply

                        // handle reply

                        _Moving = false;
                    }
                    else
                    {
                        // await host move

                        _Moving = true;
                    }
                }
            }

            Console.ReadKey();
        }

        /// <summary>
        /// Asks the player if they are a host or a client and configures the network accordingly
        /// </summary>
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

        /// <summary>
        /// Configures the network connection between two players
        /// </summary>
        private void SetUpConnection()
        {
            _MessageService = new MessageService();

            if (_IsHost)
            {
                _MessageService.WaitForClient();
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

                _MessageService.ConnectToHost(ip, port);
            }
        }
        private Boolean HandleMessage(String message)
        {
            return false;
        }

        private void HandleMoveRequest(String move)
        {

        }

        private void HandleMoveConfirm(String message)
        {

        }

        private void HandleInputDeny(String message)
        {

        }

        private void HandleBoardState(String state)
        {

        }
    }
}
