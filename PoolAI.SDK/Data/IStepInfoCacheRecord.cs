using PoolAI.SDK.Geometry;

namespace PoolAI.SDK.Data
{
    internal interface IStepInfoCacheRecord
    {
        #region Properties

        double Step { get; }
        Coordinates Normal { get; }
        Coordinates Tangent { get; }

        #endregion
    }
}
