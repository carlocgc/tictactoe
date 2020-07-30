using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TicTacToe.Data;
using TicTacToe.Interfaces;
using TicTacToe.Models;

namespace TicTacToe.FrontEnd
{
    public class ScreenDrawer
    {
        private readonly IMessageService _MessageService;

        public ScreenDrawer(IMessageService messageService)
        {
            _MessageService = messageService;
        }

        public void Draw(GameProgressData model)
        {
            Int32 playerScore =  _MessageService.IsHost ? model.HostScore : model.ClientScore;
            Int32 opponentScore = _MessageService.IsHost ? model.ClientScore : model.HostScore;

            Console.Clear();
            Console.WriteLine($"_____________________________");
            Console.WriteLine($"          TicTacToe.");
            Console.WriteLine();
            Console.WriteLine($"     Your Symbol: \"{model.PlayerSymbol}\"");
            Console.WriteLine();
            Console.WriteLine($"        Your Score: {playerScore}");
            Console.WriteLine($"     Opponent Score: {opponentScore}");
            Console.WriteLine($"_____________________________");
            Console.WriteLine();
            Console.WriteLine($"      {model.GameBoard[0, 0]}   |   {model.GameBoard[0, 1]}   |   {model.GameBoard[0, 2]}   ");
            Console.WriteLine($"   -------|-------|-------");
            Console.WriteLine($"      {model.GameBoard[1, 0]}   |   {model.GameBoard[1, 1]}   |   {model.GameBoard[1, 2]}   ");
            Console.WriteLine($"   -------|-------|-------");
            Console.WriteLine($"      {model.GameBoard[2, 0]}   |   {model.GameBoard[2, 1]}   |   {model.GameBoard[2, 2]}   ");
            Console.WriteLine($"_____________________________");
        }
    }
}
