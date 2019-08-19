using System;

namespace PoolAI.SDK.Brain
{
    public interface IBrainMetadata : IMetadataBase
    {
        #region Properties

        int Population { get; }
        Type BrainType { get; }

        #endregion
    }
}
