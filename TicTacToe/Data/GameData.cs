using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TicTacToe.Data
{
    public class GameData
    {
        public enum Command
        {
            MESSAGE,
            MOVE_REQUEST,
            MOVE_CONFIRM,
            MOVE_DENY,
            BOARD_STATE,
            EXIT,
        }
    }
}
