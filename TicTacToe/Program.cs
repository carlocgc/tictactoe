using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TicTacToe
{
    class Program
    {
        private static Game.Game _Game;

        static void Main(string[] args)
        {
            _Game = new Game.Game();
            _Game.Run();
        }
    }
}
