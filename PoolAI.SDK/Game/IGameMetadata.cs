using PoolAI.SDK.Balls;
using PoolAI.SDK.Tables;

namespace PoolAI.SDK.Game
{
    public interface IGameMetadata : IMetadataBase
    {
        #region Properties

        TableType Table { get; }
        BallSetType BallSet { get; }
        int PlayersNum { get; }

        #endregion
    }
}
