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
        static void Main(string[] args)
        {
            Game.Game game = new Game.Game();
            game.Run();
        }
    }
}
