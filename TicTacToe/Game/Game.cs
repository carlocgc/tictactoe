using System;
using TicTacToe.Interfaces;

namespace TicTacToe.Game
{
    public class Game : INetworkListener
    {
        private Drawer _ScreenDrawer;

        private Network.Network _Network;

        private Int32[,] _GameBoard;

        private Boolean _ServerWon;

        private Boolean _ClientWon;

        private Int32 _PlayerSymbol;

        public void Run()
        {
            _GameBoard = new Int32[3, 3];
            _ScreenDrawer = new Drawer();
            _Network = new Network.Network();

            _Network.Initialise();
            _PlayerSymbol = _Network.GetPlayerSymbol();


        }

        public void OnWaiting()
        {
            _ScreenDrawer.Draw(_GameBoard);

            Console.WriteLine("Opponent is thinking...");
        }

        public void OnTurnToMove()
        {
            Boolean inputValid = false;

            while (!inputValid)
            {
                _ScreenDrawer.Draw(_GameBoard);
                Console.WriteLine("Enter move: X,X");

                String input = Console.ReadLine() ?? " ";
                String[] parts = input.Split(',');

                if (parts.Length != 3) continue;
                if (!Int32.TryParse(parts[0], out Int32 x) || x < 0 || x > 2) continue;
                if (!Int32.TryParse(parts[0], out Int32 y) || y < 0 || y > 2) continue;
                if (!String.IsNullOrEmpty(_GameBoard[x, y].ToString())) continue;

                inputValid = true;
                _GameBoard[x, y] = _PlayerSymbol;
            }

            String move = Console.ReadLine();
        }

        public void OnMoveReceived(int x, int y)
        {
            _GameBoard[x, y] = _PlayerSymbol == 1 ? 2 : 1;
        }

        private void CheckIfGameComplete()
        {
            // TODO
        }
    }
}
