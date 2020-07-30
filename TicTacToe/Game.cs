using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using TicTacToe.Data;
using TicTacToe.Network;
using static TicTacToe.Data.GameData;

namespace TicTacToe
{
    public class Game
    {
        /// <summary> Message handler functions  </summary>
        private readonly Dictionary<Command, Action<String>> _MessageHandlers = new Dictionary<Command, Action<String>>();
        /// <summary> The game board data </summary>
        private Char[,] _GameBoard = { { '-','-','-' }, { '-','-','-' }, { '-','-','-' } };
        /// <summary> Sends and receives messages as packets </summary>
        private MessageService _MessageService;
        /// <summary> The symbol that the player is using </summary>
        private Char _PlayerChar;
        /// <summary> How many games the host has won </summary>
        private Int32 _HostScore;
        /// <summary> How many games the client has won </summary>
        private Int32 _ClientScore;
        /// <summary> Whether the current game is won </summary>
        private Boolean _GameWon;
        /// <summary> Whether the game is running </summary>
        private Boolean _Running;
        /// <summary> Whether the game is initialised </summary>
        private Boolean _Initialised;
        /// <summary> Whether its the players turn </summary>
        private Boolean _Moving;
        /// <summary> Whether host game is waiting for a valid move from the client </summary>
        private Boolean _WaitingValidMoveFromClient;
        /// <summary> Whether client game is waiting for the host to validate the clients move request </summary>
        private Boolean _WaitingMoveConfirmationFromHost;

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
            _Moving = _MessageService.Master;
            _PlayerChar = _MessageService.Master ? MASTER_CHAR : SLAVE_CHAR;

            _Running = true;

            while (_Running)
            {
                DrawGameBoard();

                if (_MessageService.Master)
                {
                    if (_Moving)
                    {
                        Move move = GetMove();

                        _GameBoard[move.X, move.Y] = MASTER_CHAR;

                        if (IsGameWon(MASTER_CHAR))
                        {
                            HandleGameWon(MASTER_CHAR.ToString());
                        }
                        else if (IsGameDrawn())
                        {
                            HandleDrawnGame("");
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
            }

            // Game is complete, ask for rematch
            if (_MessageService.Master)
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

                    if (resp.Message == "rematch")
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
                    _MessageService.SendPacket(new Packet(Command.MESSAGE.ToString(), "rematch"));
                    ResetGame();
                    Run();
                }
                else
                {
                    Console.WriteLine($"Host declined a rematch...");
                }
            }

            // No rematch, game will end
            Console.WriteLine($"Game Exiting...");
            Console.ReadKey();
        }

        /// <summary>
        /// Configures the network connection between two players
        /// </summary>
        private void SetUpConnection()
        {
            if (_MessageService != null && _MessageService.Connected) return;

            _MessageService = new MessageService();
            _MessageService.Initialise();

            if (_MessageService.Master)
            {
                _MessageService.WaitForClient();
            }
            else
            {
                IPAddress ip = null;

                Boolean addressValid = false;

                while (!addressValid)
                {
                    Console.WriteLine($"Enter host ip address...");
                    addressValid = IPAddress.TryParse(Console.ReadLine() ?? "", out ip);
                }

                if (ip == null)
                {
                    // TODO Handle no ip defined
                    Console.WriteLine("IP was not defined, exiting...");
                    Console.ReadKey();
                    Environment.Exit(0);
                }

                _MessageService.ConnectToHost(ip, GAME_PORT);
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

        /// <summary>
        /// Checks whether a move is valid
        /// </summary>
        /// <param name="move"></param>
        /// <returns></returns>
        private Boolean IsMoveValid(Move move)
        {
            return _GameBoard[move.X, move.Y] == '-';
        }

        /// <summary>
        /// Checks if the game has been won
        /// </summary>
        /// <param name="playerChar"></param>
        /// <returns></returns>
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

        /// <summary>
        /// returns whether all the spaces on the board are taken
        /// </summary>
        /// <returns></returns>
        private Boolean IsGameDrawn()
        {
            return _GameBoard.Cast<Char>().All(c => c != '-');
        }

        /// <summary>
        /// Creates a packet containing the game board information
        /// </summary>
        /// <returns></returns>
        private Packet GameBoardAsPacket()
        {
            String gameBoardString = $"{_GameBoard[0, 0]}:{_GameBoard[0, 1]}:{_GameBoard[0, 2]}:" +
                                     $"{_GameBoard[1, 0]}:{_GameBoard[1, 1]}:{_GameBoard[1, 2]}:" +
                                     $"{_GameBoard[2, 0]}:{_GameBoard[2, 1]}:{_GameBoard[2, 2]}";

            return new Packet(Command.BOARD_STATE.ToString(), gameBoardString);
        }

        /// <summary>
        /// Passes the packet to the handler that can handle it
        /// </summary>
        /// <param name="packet"></param>
        private void HandlePacket(Packet packet)
        {
            if (Enum.TryParse(packet.Command, true, out Command command))
            {
                _MessageHandlers[command].Invoke(packet.Message);
            }
        }

        /// <summary>
        /// Prints a message to the console
        /// </summary>
        /// <param name="message"></param>
        private void HandleMessage(String message)
        {
            DrawGameBoard();
            Console.WriteLine($"{message}");
        }

        /// <summary>
        /// Checks a move is valid and sends a response to the client with a result
        /// </summary>
        /// <param name="moveString"></param>
        private void HandleMoveRequest(String moveString)
        {
            Move move = Move.FromString(moveString);

            if (IsMoveValid(move))
            {
                _GameBoard[move.X, move.Y] = SLAVE_CHAR;

                if (IsGameWon(SLAVE_CHAR))
                {
                    _MessageService.SendPacket(new Packet(Command.GAME_WON.ToString(), SLAVE_CHAR.ToString()));
                    Packet wonResp = _MessageService.AwaitPacket();

                    if (!Enum.TryParse(wonResp.Command, out Command wonRespCommand)) return;

                    if (wonRespCommand == Command.PACKET_RECEIVED)
                    {
                        HandleGameWon(SLAVE_CHAR.ToString());
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

        /// <summary>
        /// Sets whether we are waiting for move confirmation from the host
        /// </summary>
        /// <param name="message"></param>
        private void HandleMoveConfirm(String message)
        {
            _WaitingMoveConfirmationFromHost = false;
        }

        /// <summary> Handles a movement deny, shouldn't occur both sides of the connection validate a given move </summary>
        /// <param name="message"></param>
        private void HandleMoveDeny(String message)
        {
            Console.WriteLine($"Move was denied by host...");

            // TODO Get another move from the client
        }

        /// <summary> Handles an exit command </summary>
        /// <param name="message"></param>
        private void HandleExit(String message)
        {
            Console.WriteLine($"Rematch denied, exiting...");
            Console.ReadKey();
            Environment.Exit(0);
        }

        /// <summary> Handles the game board as a packet </summary>
        /// <param name="state"></param>
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

        /// <summary> Sends the game complete message to the client and displays the game complete message </summary>
        /// <param name="message"></param>
        private void HandleGameWon(String message)
        {
            if (!Char.TryParse(message, out Char winner)) return;

            if (_MessageService.Master)
            {
                _MessageService.SendPacket(new Packet(Command.GAME_WON.ToString(), MASTER_CHAR.ToString()));
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

            if (message == MASTER_CHAR.ToString())
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

        /// <summary>
        /// Sends the drawn game message to the other player and awaits a response
        /// </summary>
        /// <param name="message"></param>
        private void HandleDrawnGame(String message)
        {
            _MessageService.SendPacket(new Packet(Command.GAME_DRAW.ToString()));

            Packet packet = _MessageService.AwaitPacket();
            HandlePacket(packet);

            DrawGameBoard();
            Console.WriteLine("Game is a draw!");

            _WaitingMoveConfirmationFromHost = false;
        }

        private void DrawGameBoard()
        {

            Int32 playerScore =  _MessageService.Master ? _HostScore : _ClientScore;
            Int32 opponentScore = _MessageService.Master ? _ClientScore : _HostScore;

            Console.Clear();
            Console.WriteLine($"_____________________________");
            Console.WriteLine($"          TicTacToe.");
            Console.WriteLine();
            Console.WriteLine($"     Your Symbol: \"{_PlayerChar}\"");
            Console.WriteLine();
            Console.WriteLine($"        Your Score: {playerScore}");
            Console.WriteLine($"     Opponent Score: {opponentScore}");
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
            _GameBoard = new Char[,] { { '-', '-', '-' }, { '-', '-', '-' }, { '-', '-', '-' } };
        }
    }
}
