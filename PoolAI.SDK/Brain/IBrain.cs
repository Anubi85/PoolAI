using PoolAI.SDK.Balls;
using PoolAI.SDK.Data;
using System;

namespace PoolAI.SDK.Brain
{
    public interface IBrain
    {
        #region Properties

        double Score { get; }
        int ShotCount { get; }

        #endregion

        #region Methods

        void Initialize(IBall cueBall);
        void RequestShotData();
        void UpdateScore(double score);

        #endregion

        #region Events

        event Action<ShotData> ShotDataReady;
        event Action EvolutionRequest;

        #endregion
    }
}
