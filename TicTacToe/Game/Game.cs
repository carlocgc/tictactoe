using System;
using System.Collections.Generic;
using System.Net;
using TicTacToe.Data;
using TicTacToe.Network;
using static TicTacToe.Data.GameData;

namespace TicTacToe.Game
{
    public class Game
    {
        private const Char HOST_CHAR = 'X';
        private const Char CLIENT_CHAR = 'O';
        /// <summary> The symbol that the player is using </summary>
        private Char _PlayerChar;
        /// <summary> Whether the current game is won </summary>
        private Boolean _GameWon;
        /// <summary> How many games the host has won </summary>
        private Int32 _HostScore = 0;
        /// <summary> How many games the client has won </summary>
        private Int32 _ClientScore = 0;
        /// <summary> The players score </summary>
        private Int32 _PlayerScore => _IsHost ? _HostScore : _ClientScore;
        /// <summary> The opponents score </summary>
        private Int32 _OpponentScore => _IsHost ? _ClientScore : _HostScore;
        /// <summary> Message handler functions  </summary>
        private readonly Dictionary<Command, Action<String>> _MessageHandlers = new Dictionary<Command, Action<String>>();
        /// <summary> Sends and receives messages as packets </summary>
        private MessageService _MessageService;
        /// <summary> Whether or not this player is the game host </summary>
        private Boolean _IsHost;
        /// <summary> Whether the game is running </summary>
        private Boolean _Running = false;
        /// <summary> Whether the game is initialised </summary>
        private Boolean _Initialised = false;
        /// <summary> Whether its the players turn </summary>
        private Boolean _Moving;
        /// <summary> The game board data </summary>
        private Char[,] _GameBoard = { { '-','-','-' }, { '-','-','-' }, { '-','-','-' } };
        /// <summary> Whether host game is waiting for a valid move from the client </summary>
        private Boolean _WaitingValidMoveFromClient = false;
        /// <summary> Whether client game is waiting for the host to validate the clients move request </summary>
        private Boolean _WaitingMoveConfirmationFromHost = false;

        /// <summary> Sets up the message handlers, called once at game start </summary>
        private void Initialise()
        {
            _MessageHandlers.Add(Command.MESSAGE, HandleMessage);
            _MessageHandlers.Add(Command.MOVE_REQUEST, HandleMoveRequest);
            _MessageHandlers.Add(Command.MOVE_CONFIRM, HandleMoveConfirm);
            _MessageHandlers.Add(Command.MOVE_DENY, HandleMoveDeny);
            _MessageHandlers.Add(Command.BOARD_STATE, HandleBoardState);
            _MessageHandlers.Add(Command.GAME_WON, HandleGameWon);
            _MessageHandlers.Add(Command.EXIT, HandleExit);

            DetermineHost();
            SetUpConnection();

            _Initialised = true;
        }

        /// <summary> Main game loop </summary>
        public void Run()
        {
            if (!_Initialised)
            {
                Initialise();
            }

            _GameWon = false;
            _Moving = _IsHost;
            _PlayerChar = _IsHost ? HOST_CHAR : CLIENT_CHAR;

            _Running = true;

            while (_Running)
            {
                DrawGameBoard();

                if (_IsHost)
                {
                    if (_Moving)
                    {
                        Move move = GetMove();

                        _GameBoard[move.X, move.Y] = HOST_CHAR;

                        if (IsGameWon(HOST_CHAR))
                        {
                            HandleGameWon(HOST_CHAR.ToString());
                        }
                        else
                        {
                            _MessageService.SendPacket(GameBoardAsPacket());
                            _Moving = false;
                        }
                    }
                    else
                    {
                        _WaitingValidMoveFromClient = true;

                        Console.WriteLine("Opponent is thinking....");

                        while (_WaitingValidMoveFromClient)
                        {
                            Packet moveRequest = _MessageService.AwaitPacket();
                            HandlePacket(moveRequest);
                        }

                        _Moving = true;
                    }
                }
                else
                {
                    if (_Moving)
                    {
                        _WaitingMoveConfirmationFromHost = true;

                        while (_WaitingMoveConfirmationFromHost)
                        {
                            DrawGameBoard();
                            Move move = GetMove();
                            _MessageService.SendPacket(new Packet(Command.MOVE_REQUEST.ToString(), move.ToString()));
                            Packet resp = _MessageService.AwaitPacket();
                            HandlePacket(resp);
                        }

                        _MessageService.SendPacket(new Packet(Command.PACKET_RECEIVED.ToString()));
                        _Moving = false;
                    }
                    else
                    {
                        Console.WriteLine($"Opponent is thinking...");

                        Packet packet = _MessageService.AwaitPacket();
                        HandlePacket(packet);

                        _Moving = true;
                    }
                }

                _Running = !_GameWon;
                // TODO Play again logic?
            }

            if (_IsHost)
            {
                String rematch = String.Empty;

                while (rematch.ToLower() != "y" && rematch.ToLower() != "n")
                {
                    Console.WriteLine($"Rematch?... y/n");
                    rematch = Console.ReadLine() ?? "";
                }

                if (rematch == "y")
                {
                    _MessageService.SendPacket(new Packet(Command.MESSAGE.ToString(), rematch));
                    Packet resp = _MessageService.AwaitPacket();

                    if (resp.Message == rematch)
                    {
                        ResetGame();
                        Run();
                    }
                }
            }
            else
            {
                Console.WriteLine($"Awaiting rematch instruction from host...");
                Packet packet = _MessageService.AwaitPacket();
                if (packet.Message == "y")
                {
                    ResetGame();
                    Run();
                }
                else
                {
                    Console.WriteLine($"Host declined a rematch...");
                }
            }

            Console.WriteLine($"Game Exiting...");
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
                Console.Clear();
                Console.WriteLine("Welcome to TicTacToe");
                Console.WriteLine($"----------------------");
                Console.WriteLine($"1. Host");
                Console.WriteLine($"2. Client");
                Console.WriteLine($"----------------------");
                Console.WriteLine($"Enter selection...");
                String resp = Console.ReadLine();

                if (resp == null) continue;

                if (resp.ToLower() == "host" || resp.ToLower() == "1")
                {
                    _IsHost = true;
                    valid = true;
                }
                else if (resp.ToLower() == "client" || resp.ToLower() == "2")
                {
                    _IsHost = false;
                    valid = true;
                }
                else
                {
                    Console.WriteLine($"Invalid option...");
                    Console.WriteLine($"Press enter to try again.");
                    Console.ReadKey();
                }
            }
        }

        /// <summary>
        /// Configures the network connection between two players
        /// </summary>
        private void SetUpConnection()
        {
            if (_MessageService != null && _MessageService.Connected) return;

            _MessageService = new MessageService();

            if (_IsHost)
            {
                _MessageService.WaitForClient();
            }
            else
            {
                //IPAddress ip = IPAddress.None;
                //Int32 port = 0;
                //Boolean addressValid = false;

                //while (!addressValid)
                //{
                //    Console.WriteLine($"Enter host ip address...");
                //    addressValid = IPAddress.TryParse(Console.ReadLine() ?? "", out ip);
                //}

                //Boolean portValid = false;

                //while (!portValid)
                //{
                //    Console.WriteLine($"Enter host port...");
                //    portValid = Int32.TryParse(Console.ReadLine() ?? "", out port);
                //}

                //_MessageService.ConnectToHost(ip, port);

                // TODO Remove this test code

                IPAddress ip = IPAddress.Parse("192.168.0.10");

                _MessageService.ConnectToHost(ip, 6600);

                Console.WriteLine($"Connected to host game.");
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
                Console.WriteLine();
                Console.WriteLine($"Enter a valid move in format \"X,X\" (0-2)");
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
                    Console.WriteLine($"{tempX} is not a number, try again...");
                    continue;
                }

                if (tempX < 0 || tempX > 2)
                {
                    Console.WriteLine($"{tempX} is out of bounds, must be between 0-2, try again...");
                    continue;
                }

                if (!Int32.TryParse(parts[1], out Int32 tempY))
                {
                    Console.WriteLine($"{tempY} is not a number, try again...");
                    continue;
                }

                if (tempY < 0 || tempY > 2)
                {
                    Console.WriteLine($"{tempY} is out of bounds, must be between 0-2, try again...");
                    continue;
                }

                if (!IsMoveValid(new Move(tempX, tempY)))
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

        private Boolean IsMoveValid(Move move)
        {
            return _GameBoard[move.X, move.Y] == '-';
        }

        private Boolean IsGameWon(Char playerChar)
        {
            if (_GameBoard[0, 0] == playerChar && _GameBoard[0, 1] == playerChar && _GameBoard[0, 2] == playerChar) return true;
            if (_GameBoard[1, 0] == playerChar && _GameBoard[1, 1] == playerChar && _GameBoard[1, 2] == playerChar) return true;
            if (_GameBoard[2, 0] == playerChar && _GameBoard[2, 1] == playerChar && _GameBoard[2, 2] == playerChar) return true;
            if (_GameBoard[0, 0] == playerChar && _GameBoard[1, 1] == playerChar && _GameBoard[2, 2] == playerChar) return true;
            if (_GameBoard[0, 2] == playerChar && _GameBoard[1, 1] == playerChar && _GameBoard[2, 0] == playerChar) return true;
            if (_GameBoard[0, 0] == playerChar && _GameBoard[1, 0] == playerChar && _GameBoard[2, 0] == playerChar) return true;
            if (_GameBoard[0, 1] == playerChar && _GameBoard[1, 1] == playerChar && _GameBoard[2, 1] == playerChar) return true;
            if (_GameBoard[0, 2] == playerChar && _GameBoard[1, 2] == playerChar && _GameBoard[2, 2] == playerChar) return true;
            return false;
        }

        private Packet GameBoardAsPacket()
        {
            String gameBoardString = $"{_GameBoard[0, 0]}:{_GameBoard[0, 1]}:{_GameBoard[0, 2]}:" +
                                     $"{_GameBoard[1, 0]}:{_GameBoard[1, 1]}:{_GameBoard[1, 2]}:" +
                                     $"{_GameBoard[2, 0]}:{_GameBoard[2, 1]}:{_GameBoard[2, 2]}";

            return new Packet(Command.BOARD_STATE.ToString(), gameBoardString);
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

        private void HandleMoveRequest(String moveString)
        {
            Move move = Move.FromString(moveString);

            if (IsMoveValid(move))
            {
                _GameBoard[move.X, move.Y] = CLIENT_CHAR;

                if (IsGameWon(CLIENT_CHAR))
                {
                    _MessageService.SendPacket(new Packet(Command.GAME_WON.ToString(), CLIENT_CHAR.ToString()));
                    Packet wonResp = _MessageService.AwaitPacket();

                    if (!Enum.TryParse(wonResp.Command, out Command wonRespCommand)) return;

                    if (wonRespCommand == Command.PACKET_RECEIVED)
                    {
                        HandleGameWon(CLIENT_CHAR.ToString());
                    }
                }
                else
                {
                    _MessageService.SendPacket(GameBoardAsPacket());
                }

                Packet resp = _MessageService.AwaitPacket();

                if (!Enum.TryParse(resp.Command, out Command command)) return;

                if (command == Command.PACKET_RECEIVED)
                {
                    _WaitingValidMoveFromClient = false;
                }
            }
            else
            {
                _MessageService.SendPacket(new Packet(Command.MOVE_DENY.ToString()));
            }
        }

        private void HandleMoveConfirm(String message)
        {
            _WaitingMoveConfirmationFromHost = false;
        }

        private void HandleMoveDeny(String message)
        {
            Console.WriteLine($"Move was denied by host...");
        }

        private void HandleExit(String message)
        {
            throw new NotImplementedException();
        }

        private void HandleBoardState(String state)
        {
            String[] parts = state.Split(':');
            Int32 count = 0;

            for (int x = 0; x < 3; x++)
            {
                for (int y = 0; y < 3; y++)
                {
                    _GameBoard[x, y] = Char.Parse(parts[count]);
                    count++;
                }
            }

            _WaitingMoveConfirmationFromHost = false;
        }

        private void HandleGameWon(String message)
        {
            if (!Char.TryParse(message, out Char winner)) return;

            if (_IsHost)
            {
                _MessageService.SendPacket(new Packet(Command.GAME_WON.ToString(), HOST_CHAR.ToString()));
                Packet packet = _MessageService.AwaitPacket();
                if (Enum.TryParse(packet.Command, out Command command))
                {
                    if (command == Command.PACKET_RECEIVED)
                    {
                        _MessageService.SendPacket(GameBoardAsPacket());
                    }
                }
            }
            else
            {
                _MessageService.SendPacket(new Packet(Command.PACKET_RECEIVED.ToString()));
                Packet packet = _MessageService.AwaitPacket();
                HandlePacket(packet);
            }

            DrawGameBoard();
            Console.WriteLine(winner == _PlayerChar ? $"Congratulations, you won!" : $"Unlucky, you lost!");

            if (message == HOST_CHAR.ToString())
            {
                _HostScore++;
            }
            else
            {
                _ClientScore++;
            }

            _GameWon = true;
            _WaitingMoveConfirmationFromHost = false;
        }

        private void DrawGameBoard()
        {
            Console.Clear();
            
            Console.WriteLine();
            Console.WriteLine($"_____________________________");
            Console.WriteLine($"          TicTacToe.");
            Console.WriteLine();
            Console.WriteLine($"     Your Symbol: \"{_PlayerChar}\"");
            Console.WriteLine();
            Console.WriteLine($"Your Score: {_PlayerScore} | Opponent Score: {_OpponentScore}");
            Console.WriteLine($"_____________________________");
            Console.WriteLine();
            Console.WriteLine($"      {_GameBoard[0, 0]}   |   {_GameBoard[0, 1]}   |   {_GameBoard[0, 2]}   ");
            Console.WriteLine($"   -------|-------|-------");
            Console.WriteLine($"      {_GameBoard[1, 0]}   |   {_GameBoard[1, 1]}   |   {_GameBoard[1, 2]}   ");
            Console.WriteLine($"   -------|-------|-------");
            Console.WriteLine($"      {_GameBoard[2, 0]}   |   {_GameBoard[2, 1]}   |   {_GameBoard[2, 2]}   ");
            Console.WriteLine($"_____________________________");
        }

        private void ResetGame()
        {
            _MessageHandlers.Clear();
            _GameBoard = new Char[,] { { '-', '-', '-' }, { '-', '-', '-' }, { '-', '-', '-' } };
        }
    }
}
