using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Net;
using System.Threading;
using Newtonsoft.Json;
using TicTacToe.Data;
using TicTacToe.Network;
using static TicTacToe.Data.GameData;

namespace TicTacToe.Game
{
    public class Game
    {
        /// <summary> Message handler functions  </summary>
        private readonly Dictionary<Command, Action<String>> _MessageHandlers = new Dictionary<Command, Action<String>>();
        /// <summary> Sends and receives messages as packets </summary>
        private MessageService _MessageService;
        /// <summary> Whether or not this player is the game host </summary>
        private Boolean _IsHost;
        /// <summary> Whether the game is running </summary>
        private Boolean _Running = false;
        /// <summary> Whether its the players turn </summary>
        private Boolean _Moving;
        /// <summary> The game board data </summary>
        private Char[,] _GameBoard = { { '-','-','-' }, { '-','-','-' }, { '-','-','-' } };
        /// <summary> The character that represents the player on the game board X or O </summary>
        private Char _PlayerChar;

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


            // Game board send test

            if (_IsHost)
            {
                _MessageService.SendPacket(new Packet(Command.BOARD_STATE.ToString(), JsonConvert.SerializeObject(_GameBoard)));

                Packet packet = _MessageService.AwaitPacket();

                Console.ReadKey();
            }
            else
            {
                Packet packet = _MessageService.AwaitPacket();

                _GameBoard = JsonConvert.DeserializeObject<Char[,]>(packet.Message);

                Console.ReadKey();
            }


            _PlayerChar = _IsHost ? 'X' : 'O';

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
                        Move move = GetMove();

                        _GameBoard[move.X, move.Y] = _PlayerChar;

                        if (GameWon())
                        {
                            // TODO handle game won
                        }

                        _MessageService.SendPacket(new Packet(Command.BOARD_STATE.ToString(), JsonConvert.SerializeObject(_GameBoard)));

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

        /// <summary>
        /// Prompts for a valid move on the game board 
        /// </summary>
        /// <returns></returns>
        private Move GetMove()
        {
            Boolean valid = false;

            Int32 x = 0;
            Int32 y = 0;

            String lastError = String.Empty;

            while (!valid)
            {
                DrawGameBoard();
                Console.WriteLine();
                Console.WriteLine($"Enter a valid move in format \"X,X\" (1-3)");
                if (lastError != String.Empty)
                {
                    Console.WriteLine($"{lastError}");
                }

                String input = Console.ReadLine() ?? "";

                if (input.Length > 3)
                {
                    Console.WriteLine($"Too many characters, try again...");
                    continue;
                }

                if (input.Length < 3)
                {
                    Console.WriteLine($"Too few characters, try again...");
                    continue;
                }

                if (!input.Contains(","))
                {
                    Console.WriteLine($"Missing comma, try again...");
                    continue;
                }

                String[] parts = input.Split(',');

                if (!Int32.TryParse(parts[0], out Int32 tempX))
                {
                    Console.WriteLine($"{parts[0]} is not a number, try again...");
                    continue;
                }

                if (tempX <= 0 || tempX >= 4)
                {
                    Console.WriteLine($"{tempX} is out of bounds, must be between 1-3, try again...");
                    continue;
                }

                if (!Int32.TryParse(parts[1], out Int32 tempY))
                {
                    Console.WriteLine($"{parts[0]} is not a number, try again...");
                    continue;
                }

                if (tempY <= 0 || tempY >= 4)
                {
                    Console.WriteLine($"{tempY} is out of bounds, must be between 1-3, try again...");
                    continue;
                }

                if (_GameBoard[tempX, tempY] != '-')
                {
                    Console.WriteLine($"({tempX}, {tempY}) is already taken by \"{_GameBoard[tempX, tempY]}\", try again...");
                    continue;
                }

                x = tempX;
                y = tempY;
                valid = true;
            }
            return new Move(x, y);
        }

        private Boolean GameWon()
        {
            if (_GameBoard[0, 0] == _PlayerChar && _GameBoard[0, 1] == _PlayerChar && _GameBoard[0, 2] == _PlayerChar) return true;
            if (_GameBoard[1, 0] == _PlayerChar && _GameBoard[1, 1] == _PlayerChar && _GameBoard[1, 2] == _PlayerChar) return true;
            if (_GameBoard[2, 0] == _PlayerChar && _GameBoard[2, 1] == _PlayerChar && _GameBoard[2, 2] == _PlayerChar) return true;
            if (_GameBoard[0, 0] == _PlayerChar && _GameBoard[1, 1] == _PlayerChar && _GameBoard[2, 2] == _PlayerChar) return true;
            if (_GameBoard[0, 2] == _PlayerChar && _GameBoard[1, 1] == _PlayerChar && _GameBoard[2, 0] == _PlayerChar) return true;
            return false;
        }

        private void HandlePacket(Packet packet)
        {
            if (Enum.TryParse(packet.Command, true, out Command command))
            {
                _MessageHandlers[command].Invoke(packet.Message);
            }
        }

        private void HandleMessage(String message)
        {
            DrawGameBoard();
            Console.WriteLine($"{message}");
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
            _GameBoard = JsonConvert.DeserializeObject<Char[,]>(state);
            DrawGameBoard();
            Console.WriteLine($"Game board updated!");
        }

        private void DrawGameBoard()
        {
            Console.Clear();
            Console.WriteLine();
            Console.WriteLine($"          TicTacToe.");
            Console.WriteLine($"_____________________________");
            Console.WriteLine();
            Console.WriteLine($"      {_GameBoard[0,0]}   |   {_GameBoard[0,1]}   |   {_GameBoard[0,2]}   ");
            Console.WriteLine($"   -------|-------|-------");
            Console.WriteLine($"      {_GameBoard[1,0]}   |   {_GameBoard[1,1]}   |   {_GameBoard[1,2]}   ");
            Console.WriteLine($"   -------|-------|-------");
            Console.WriteLine($"      {_GameBoard[2,0]}   |   {_GameBoard[2,1]}   |   {_GameBoard[2,2]}   ");
            Console.WriteLine($"_____________________________");
        }
    }
}
