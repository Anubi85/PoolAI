using PoolAI.SDK.Balls;
using PoolAI.SDK.Brain;
using PoolAI.SDK.Data;
using PoolAI.SDK.Game;
using PoolAI.SDK.Tables;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;

namespace PoolAI.SDK
{
#pragma warning disable CS0649, IDE0044
    internal sealed class ComponentManager
    {
        #region Constants

        private static readonly Font c_InfoFont = new Font(SystemFonts.DefaultFont.FontFamily, 10.0f, FontStyle.Regular);
        private const int c_CountDown = 20;

        #endregion

        #region Fields

        private readonly object m_EvolutionLock;
        private ITableInternal m_Table;
        private IGame m_Game;
        private IGameMetadata m_GameMetadata;
        private List<GameManager> m_Games;
        private IEnumerable<GameManager> m_ValidGames;
        private int m_Population;
        private volatile Dictionary<GameManager, bool> m_EvolutionRequests;
        [Import(typeof(DrawOverlayFunction), AllowDefault = true)]
        private DrawOverlayFunction m_DrawOverlay;
        [Import(typeof(EvolveFunction), AllowDefault = true)]
        private EvolveFunction m_EvolveFunction;
        [Import(typeof(MaxShotFunction), AllowDefault = true)]
        private MaxShotFunction m_MaxShotFunction;
        private int? m_ShotRequestTimeout;
        //info
        private int m_MaxShot;
        private int m_ShotCount;
        private int m_MaxBalls;
        private int? m_BallCount;
        private int m_PlayingGamesCount;
        private double? m_BestScore;

        #endregion

        #region Properties

        internal static StepInfoCache Cache { get; private set; }
        public int TableWidth { get { return m_Table.Width; } }
        public int TableHeight { get { return m_Table.Height; } }
        public bool HasWinner { get { return m_Games.Any(g => g.HasWin); } }
        public bool PlayWinner { get; set; }

        #endregion

        #region Constructor

        static ComponentManager()
        {
            Cache = new StepInfoCache();
        }
        public ComponentManager(IGame game, IGameMetadata gameMetadata, ExportFactory<IBrain, IBrainMetadata> brainFactory)
        {
            m_EvolutionLock = new object();
            m_Game = game;
            m_GameMetadata = gameMetadata;
            m_Table = TableFactory.GetTable(gameMetadata.Table);
            m_ValidGames = new List<GameManager>();
            //load accessory functions from brain assembly
            new CompositionContainer(new TypeCatalog(brainFactory.Metadata.BrainType)).ComposeParts(this);
            m_Population = brainFactory.Metadata.Population;
            m_MaxShot = 0;
            m_MaxBalls = 0;
            m_ShotCount = 1;
            m_BallCount = null;
            m_PlayingGamesCount = m_Population;
            m_BestScore = null;
            PlayWinner = false;
            Initialize((pop, play) => brainFactory.CreateExport().Value);
        }

        #endregion

        #region Methods

        public void Draw(Graphics g)
        {
            m_Table.Draw(g);
            if (m_DrawOverlay?.Method.IsStatic ?? false)
            {
                m_DrawOverlay.Invoke(g);
            }
            else
            {
                //assume that there is only one game
                m_Games.FirstOrDefault()?.DrawOVerlay(m_DrawOverlay?.Method, g);
            }
            string info1 = string.Format("Playing {0} games out of {1}", m_PlayingGamesCount, m_Population);
            string info2 = string.Format("Best game has {0} out of {1} ball still in game", m_BallCount?.ToString() ?? "??" , m_MaxBalls);
            SizeF info2StrSize = g.MeasureString(info2, c_InfoFont);
            string info3 = string.Format("Shot {0} of {1}", m_ShotCount, m_MaxShot);
            SizeF info3StrSize = g.MeasureString(info3, c_InfoFont);
            string info4 = string.Format("Best Score: {0}", m_BestScore?.ToString("0.000") ?? "---");
            SizeF info4StrSize = g.MeasureString(info4, c_InfoFont);
            g.DrawString(info1, c_InfoFont, Brushes.Black, 5, 5);
            g.DrawString(info2, c_InfoFont, Brushes.Black, g.ClipBounds.Right - info2StrSize.Width - 5, 5);
            g.DrawString(info3, c_InfoFont, Brushes.Black, g.ClipBounds.Left + 5, g.ClipBounds.Bottom - info3StrSize.Height - 5);
            g.DrawString(info4, c_InfoFont, Brushes.Black, g.ClipBounds.Right - info4StrSize.Width - 5, g.ClipBounds.Bottom - info4StrSize.Height - 5);
            foreach (var data in m_ValidGames)
            {
                data.Draw(g);
            }
        }
        public void Update()
        {
            if (m_ShotRequestTimeout.HasValue && --m_ShotRequestTimeout == 0)
            {
                RequestShot();
            }
            else if (!m_ShotRequestTimeout.HasValue)
            {
                Parallel.ForEach(m_ValidGames, g => g.Update());
                m_ValidGames = m_Games.Where(g => !g.IsOver);
                int playingGames = m_ValidGames.Count(gm => gm.IsPlaing);
                if (m_PlayingGamesCount != playingGames)
                {
                    m_PlayingGamesCount = playingGames;
                    m_Games.ForEach(gm => { if (!gm.IsPlaing) gm.UpdateScore(); });
                    UpdateGameInfo();
                    if (m_PlayingGamesCount == 0)
                    {
                        m_ShotRequestTimeout = c_CountDown;
                    }
                }
            }
        }
        public void StartGame()
        {
            Cache.Refresh();
            m_MaxShot = m_MaxShotFunction?.Invoke() ?? -1;
            m_MaxBalls = BallSetFactory.GetMaxBallNum(m_GameMetadata.BallSet);
            m_BestScore = null;
            m_ShotCount = 1;
            m_BallCount = null;
            m_PlayingGamesCount = m_Population;
            if (PlayWinner)
            {
                m_Population = 1;
                var bestGame = m_Games.Where(g => g.HasWin).OrderBy(g => g.BestScore).First();
                var brains = new List<IBrain>();
                for (int i = 0; i < m_GameMetadata.PlayersNum; i++)
                {
                    brains.Add(bestGame.GetBrain(i));
                }
                Initialize((pop, play) => brains[play]);
            }
            foreach (var g in m_Games)
            {
                g.RequestShot();
            }
            m_ShotRequestTimeout = null;
        }
        private void Initialize(Func<int, int, IBrain> brainFactory)
        {
            m_Games = new List<GameManager>();
            m_EvolutionRequests = new Dictionary<GameManager, bool>();
            for (int i = 0; i < m_Population; i++)
            {
                var brains = new List<IBrain>();
                for (int j = 0; j < m_GameMetadata.PlayersNum; j++)
                {
                    brains.Add(brainFactory.Invoke(i, j));
                }
                var players = new Players(brains);
                var ballSet = BallSetFactory.GetBallSet(m_Table, m_GameMetadata.BallSet);
                var gameManager = new GameManager(m_Game, m_Table, ballSet, players);
                m_EvolutionRequests[gameManager] = false;
                players.EvolutionRequest += () => HandleEvolutionRequests(gameManager);
                m_Games.Add(gameManager);
                m_Game.InitializeBrains(brains, ballSet);
            }
            m_ValidGames = m_Games;
        }
        private void HandleEvolutionRequests(GameManager gameManager)
        {
            lock (m_EvolutionLock)
            {
                m_EvolutionRequests[gameManager] = true;
                if (m_ValidGames.All(g => m_EvolutionRequests[g]))
                {
                    List<List<IBrain>> newBrains = new List<List<IBrain>>();
                    for (int i = 0; i < m_GameMetadata.PlayersNum; i++)
                    {
                        newBrains.Add(m_EvolveFunction?.Invoke(m_Games.Select(g => g.GetBrain(i))).ToList());
                    }
                    Initialize((pop, play) => { return newBrains[play][pop]; });
                    StartGame();
                }
            }
        }
        private void RequestShot()
        {
            m_ShotRequestTimeout = null;
            foreach (var gm in m_ValidGames)
            {
                gm.RequestShot();
            }
            UpdateGameInfo();
        }
        private void UpdateGameInfo()
        {
            if (m_ValidGames.Count() > 0)
            {
                m_ShotCount = m_ValidGames.Min(g => g.ShotCount);
                var bestGame = m_ValidGames.Where(g => !g.IsPlaing).OrderByDescending(g => g.BestScore).FirstOrDefault();
                if (m_ShotCount == 1 || m_ShotCount != 1 & bestGame != null)
                {
                    m_BestScore = bestGame?.BestScore;
                    m_BallCount = bestGame?.InGameBallCount;
                }
            }
        }

        #endregion
    }
}
