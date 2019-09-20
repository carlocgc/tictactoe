using System;

namespace TicTacToe.Game
{
    public class Drawer
    {
        public void Draw(Int32[,] board)
        {
            Console.Clear();
            Console.WriteLine($" {GetSymbol(board[0, 0])} | {GetSymbol(board[0, 1])} | {GetSymbol(board[0, 2])} ");
            Console.WriteLine($" --------- ");
            Console.WriteLine($" {GetSymbol(board[1, 0])} | {GetSymbol(board[1, 1])} | {GetSymbol(board[1, 2])} ");
            Console.WriteLine($" --------- ");
            Console.WriteLine($" {GetSymbol(board[2, 0])} | {GetSymbol(board[2, 1])} | {GetSymbol(board[2, 2])} ");
        }

        private String GetSymbol(Int32 value)
        {
            switch (value)
            {
                case 1:
                    return "0";
                case 2:
                    return "X";
                default:
                    return " ";
            }
        }
    }
}
