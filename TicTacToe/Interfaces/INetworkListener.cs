using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TicTacToe.Interfaces
{
    public interface INetworkListener
    {
        void OnWaiting();

        void OnTurnToMove();

        void OnMoveReceived(Int32 x, Int32 y);
    }
}
