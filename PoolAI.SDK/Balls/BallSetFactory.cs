using PoolAI.SDK.Tables;

namespace PoolAI.SDK.Balls
{
    internal static class BallSetFactory
    {
        #region Methods

        public static IBallSetInternal GetBallSet(ITable table, BallSetType type)
        {
            switch(type)
            {
                case BallSetType.Triangle:
                    return new TriangleBallSet(table as ITableInternal);
                default:
                    return null;
            }
        }
        public static int GetMaxBallNum(BallSetType type)
        {
            switch (type)
            {
                case BallSetType.Triangle:
                    return TriangleBallSet.MaxBallNum;
                default:
                    return -1;
            }
        }

        #endregion
    }
}
