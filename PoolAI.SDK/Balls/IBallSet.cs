using System.Collections.Generic;

namespace PoolAI.SDK.Balls
{
    public interface IBallSet
    {
        #region Properties

        IReadOnlyList<IBall> CueBalls { get; }
        IReadOnlyList<IBall> Balls { get; }

        #endregion
    }
}
