using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TicTacToe.Data
{
    public struct Move
    {
        public Int32 X { get; }

        public Int32 Y { get; }

        public Move(Int32 x, Int32 y)
        {
            X = x;
            Y = y;
        }
    }
}
