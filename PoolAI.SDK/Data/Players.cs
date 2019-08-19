using PoolAI.SDK.Brain;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PoolAI.SDK.Data
{
    public sealed class Players
    {
        #region Data

        private class Player
        {
            #region Fields

            private readonly object m_Lock;

            #endregion

            #region Properties

            public Player Previous { get; set; }
            public Player Next { get; set; }
            public IBrain Brain { get; private set; }

            #endregion

            #region Constructor

            public Player(IBrain brain, Players players)
            {
                m_Lock = new object();
                Brain = brain;
                brain.EvolutionRequest += () =>
                    {
                        lock (m_Lock)
                        {
                            players.m_EvolutionRequests[brain] = true;
                            if (players.m_EvolutionRequests.Values.All(v => v))
                            {
                                players.EvolutionRequest?.Invoke();
                            }
                        }
                    };
                brain.ShotDataReady += (data) => players.ShotDataReady?.Invoke(data);
            }

            #endregion
        }

        #endregion

        #region Fields

        private Player m_CurrentPlayer;
        private List<Player> m_Players;
        private Dictionary<IBrain, bool> m_EvolutionRequests;

        #endregion

        #region Properties

        internal IBrain CurrentPlayerBrain { get { return m_CurrentPlayer.Brain; } }
        internal double BestScore { get { return m_Players.Max(p => p.Brain.Score); } }

        #endregion

        #region Constructor

        internal Players(IEnumerable<IBrain> brains)
        {
            m_Players = new List<Player>();
            m_EvolutionRequests = new Dictionary<IBrain, bool>();
            m_CurrentPlayer = new Player(brains.First(), this);
            Player tmp = m_CurrentPlayer;
            m_Players.Add(tmp);
            m_EvolutionRequests[tmp.Brain] = false;
            foreach (var item in brains.Skip(1))
            {
                tmp.Next = new Player(item, this)
                {
                    Previous = tmp
                };
                tmp = tmp.Next;
                m_Players.Add(tmp);
                m_EvolutionRequests[tmp.Brain] = false;
            }
            tmp.Next = m_CurrentPlayer;
            m_CurrentPlayer.Previous = tmp;
        }

        #endregion

        #region Methods

        public void NextPlayer(int count = 1)
        {
            for(int i = 0; i < count; i++)
            {
                m_CurrentPlayer = m_CurrentPlayer.Next;
            }
        }
        public void PreviousPlayer(int count = 1)
        {
            for (int i = 0; i < count; i++)
            {
                m_CurrentPlayer = m_CurrentPlayer.Previous;
            }
        }
        internal IBrain GetBrain(int playerID)
        {
            return m_Players[playerID].Brain;
        }

        #endregion

        #region Events

        internal event Action EvolutionRequest;
        internal event Action<ShotData> ShotDataReady;

        #endregion
    }
}
