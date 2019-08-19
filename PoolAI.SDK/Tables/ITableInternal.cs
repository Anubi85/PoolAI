using PoolAI.SDK.Balls;
using PoolAI.SDK.Data;
using PoolAI.SDK.Geometry;
using System.Collections.Generic;
using System.Drawing;

namespace PoolAI.SDK.Tables
{
    internal interface ITableInternal : ITable
    {
        #region Properties

        int Width { get; }
        int Height { get; }
        double BankBouncingFrictionCoefficient { get; }
        double BankSlidingFrictionCoefficient { get; }
        IReadOnlyList<Coordinates> CueBallLocations { get; }
        IReadOnlyList<Coordinates> BallLocations { get; }

        #endregion

        #region Methods

        void Draw(Graphics g);
        BankStepInfo ComputeStep(IBallInternal ball);
        void InGameCheck(IBallInternal ball);

        #endregion
    }
}
