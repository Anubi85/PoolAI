using PoolAI.SDK.Balls;
using PoolAI.SDK.Geometry;
using System.Collections.Generic;

namespace PoolAI.SDK.Data
{
    internal  sealed class BallStepInfo : StepInfoBase
    {
        #region Fields

        private Dictionary<IBallInternal, Coordinates> m_CollidedBalls;

        #endregion

        #region Properties

        public IReadOnlyCollection<IBallInternal> CollidedBalls { get { return m_CollidedBalls.Keys; } }

        #endregion

        #region Constructor

        public BallStepInfo(double step) : base(step)
        {
            m_CollidedBalls = new Dictionary<IBallInternal, Coordinates>();
        }

        #endregion

        #region Methods

        public void AddCollidedBall(IBallInternal ball, Coordinates normalVersor)
        {
            m_CollidedBalls[ball] = normalVersor;
        }

        public Coordinates GetNormalVersor(IBallInternal ball)
        {
            return m_CollidedBalls[ball];
        }

        #endregion
    }
}
