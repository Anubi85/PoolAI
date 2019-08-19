using PoolAI.SDK.Geometry;
using System.Collections.Generic;

namespace PoolAI.SDK.Tables
{
    public interface ITable
    {
        #region Properties

        IReadOnlyList<ICoordinates> TargetLocations { get; }

        #endregion
    }
}
