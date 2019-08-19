using PoolAI.SDK.Brain;
using System.Drawing;
using PoolAI.SDK.Balls;
using PoolAI.SDK.Data;
using System;
using System.Collections.Generic;

namespace Brain.Test
{
    [ExportBrain("Test", typeof(Brain), Population = 1)]
    public class Brain : IBrain
    {
        #region Fields

        private ShotData m_ShotData;
        private bool m_Evolve;

        #endregion

        #region Properties

        public double Score { get; private set; }
        public int ShotCount { get; private set; }

        #endregion

        #region Constructor

        public Brain()
        {
            Score = 0;
            ShotCount = 0;
        }

        #endregion

        #region Methods

        [ExportDrawOverlayFunction]
        private static void DrawOverlay(Graphics g)
        {

        }
        [ExportEvolveFunction]
        private static IEnumerable<IBrain> Evolve(IEnumerable<IBrain> brains)
        {
            return brains;
        }
        public void Initialize(IBall cueBall)
        {
            ShotCount = 0;
            m_ShotData = new ShotData(cueBall);
            m_ShotData.Force = 2000;
            m_ShotData.Direction = 0;
            m_Evolve = false;
        }
        public void RequestShotData()
        {
            ShotCount++;
            if (m_Evolve)
            {
                m_Evolve = !m_Evolve;
                EvolutionRequest?.Invoke();
            }
            else
            {
                m_Evolve = !m_Evolve;
                ShotDataReady?.Invoke(m_ShotData);
            }
        }
        public void UpdateScore(double score)
        {
            Score = score;
        }

        #endregion

        #region Events

        public event Action<ShotData> ShotDataReady;
        public event Action EvolutionRequest;

        #endregion
    }
}
