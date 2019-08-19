using PoolAI.SDK.Balls;

namespace PoolAI.SDK.Data
{
    public sealed class ShotData
    {
        #region Properties

        public IBall Ball { get; private set; }
        public double Force { get; set; }
        public double Direction { get; set; }

        #endregion

        #region Constructor

        public ShotData(IBall ball)
        {
            Ball = ball;
        }

        #endregion
    }
}
