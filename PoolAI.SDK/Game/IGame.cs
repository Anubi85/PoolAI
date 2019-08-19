using PoolAI.SDK.Balls;
using PoolAI.SDK.Brain;
using PoolAI.SDK.Tables;
using System.Collections.Generic;

namespace PoolAI.SDK.Game
{
    public interface IGame
    {
        #region Methods

        void InitializeBrains(IEnumerable<IBrain> brains, IBallSet ballSet);
        double GetScore(ITable table, IBallSet ballSet);
        bool IsOver(IBallSet ballSet);
        bool HasWin(IBallSet ballSet);

        #endregion
    }
}
