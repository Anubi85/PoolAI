using PoolAI.SDK.Balls;
using PoolAI.SDK.Brain;
using PoolAI.SDK.Game;
using PoolAI.SDK.Geometry;
using PoolAI.SDK.Tables;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Game.American
{
    [ExportGame("American", TableType.LargePockets, BallSetType.Triangle, 1)]
    class Game : IGame
    {
        #region Constants

        private static readonly double s_MaximumMinimumTargetDistance;

        #endregion

        #region Constructor

        static Game()
        {
            s_MaximumMinimumTargetDistance = Math.Sqrt(
                Math.Pow(TableFactory.GetTableWidth(TableType.LargePockets) / 4.0, 2) + 
                Math.Pow(TableFactory.GetTableHeight(TableType.LargePockets) / 2.0, 2));
        }

        #endregion

        #region Methods

        public void InitializeBrains(IEnumerable<IBrain> brains, IBallSet ballSet)
        {
            foreach(var brain in brains)
            {
                brain.Initialize(ballSet.CueBalls.First());
            }
        }
        public double GetScore(ITable table, IBallSet ballSet)
        {
            if (ballSet.CueBalls.Any(b => !b.InGame))
            {
                return double.NegativeInfinity;
            }
            if (ballSet.CueBalls.Any(b => !b.HasHitSomething))
            {
                return 0;
            }
            //normalize target discances and sum
            double score = ballSet.Balls.Select(b => table.TargetLocations.Max(t => 1 - GeometryUtils.Distance(b.Position, t) / s_MaximumMinimumTargetDistance)).Average();
            //adjust score accordingly with the number of ball in game
            return score + ballSet.Balls.Count(b => !b.InGame);
        }
        public bool IsOver(IBallSet ballSet)
        {
            return ballSet.CueBalls.All(b => !b.InGame) || HasWin(ballSet);
        }
        public bool HasWin(IBallSet ballSet)
        {
            return ballSet.CueBalls.All(b => b.InGame) && ballSet.Balls.Except(ballSet.CueBalls).All(b => !b.InGame);
        }

        #endregion
    }
}
