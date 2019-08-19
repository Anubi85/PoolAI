using PoolAI.SDK.Geometry;

namespace PoolAI.SDK.Balls
{
    public interface IBall
    {
        #region Properties

        bool InGame { get; }
        bool HasHitSomething { get; }
        ICoordinates Position { get; }

        #endregion
    }
}
