using System;

namespace TicTacToe.Data
{
    /// <summary> Global static game data </summary>
    public static class GameData
    {
        /// <summary> Whether the game is to launch in debug mode, used to streamline connection logic by not asking for ip to connect to </summary>
        public const Boolean DEBUG = true;
        /// <summary> Whether the default debug ip is the local or remote endpoint </summary>
        public const Boolean LOCAL_DEBUG_CONNECTION = true;
        /// <summary> Port the game communicates on </summary>
        public const Int32 GAME_PORT = 6600;
        /// <summary> Symbol that represents the master </summary>
        public const Char MASTER_CHAR = 'X';
        /// <summary> Symbol that represents the slave </summary>
        public const Char SLAVE_CHAR = 'O';

        public enum Command
        {
            MESSAGE,
            PACKET_RECEIVED,
            MOVE_REQUEST,
            MOVE_CONFIRM,
            MOVE_DENY,
            BOARD_STATE,
            GAME_WON,
            EXIT,
        }
    }
}
