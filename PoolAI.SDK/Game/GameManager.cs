using PoolAI.SDK.Balls;
using PoolAI.SDK.Brain;
using PoolAI.SDK.Data;
using PoolAI.SDK.Tables;
using System;
using System.Drawing;
using System.Linq;
using System.Reflection;

namespace PoolAI.SDK.Game
{
    internal sealed class GameManager
    {
        #region Fields

        private readonly Func<double> m_GetScore;
        private readonly Func<bool> m_IsOver;
        private readonly Func<bool> m_HasWin;
        private IBallSetInternal m_BallSet;
        private readonly Players m_Players;

        #endregion

        #region Properties

        public bool IsPlaing { get { return m_BallSet.Balls.Where(b => b.InGame).Any(b => (b as IBallInternal).Velocity.Module > 0); } }
        public bool IsOver { get { return m_IsOver?.Invoke() ?? true; } }
        public bool HasWin { get { return m_HasWin?.Invoke() ?? false; } }
        public int InGameBallCount { get; private set; }
        public double BestScore { get { return m_Players.BestScore; } }
        public int ShotCount { get { return m_Players.CurrentPlayerBrain.ShotCount; } }

        #endregion

        #region Constructor

        public GameManager(IGame game, ITableInternal table, IBallSetInternal ballSet, Players players)
        {
            m_GetScore = () => game.GetScore(table, ballSet);
            m_IsOver = () => game.IsOver(ballSet);
            m_HasWin = () => game.HasWin(ballSet);
            m_BallSet = ballSet;
            m_Players = players;
            m_Players.ShotDataReady += m_BallSet.HitBall;
            var balls = m_BallSet.Balls.Except(m_BallSet.CueBalls);
            InGameBallCount = balls.Count();
            foreach (IBallInternal b in balls)
            {
                b.RemovedFromGame += () => InGameBallCount--;
            }
        }

        #endregion

        #region Methods

        public void Draw(Graphics g)
        {
            m_BallSet.Draw(g);
        }
        public void DrawOVerlay(MethodInfo method, Graphics g)
        {
            method?.Invoke(m_Players.CurrentPlayerBrain, new object[] { g });
        }
        public void Update()
        {
            m_BallSet.Update();
        }
        public void UpdateScore()
        {
            m_Players.CurrentPlayerBrain.UpdateScore(m_GetScore?.Invoke() ?? 0);
        }
        public void RequestShot()
        {
            m_Players.CurrentPlayerBrain.RequestShotData();
        }
        public IBrain GetBrain(int playerID)
        {
            return m_Players.GetBrain(playerID);
        }

        #endregion
    }
}
