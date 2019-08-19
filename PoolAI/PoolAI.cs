using PoolAI.Controls;
using PoolAI.SDK;
using PoolAI.SDK.Brain;
using PoolAI.SDK.Game;
using System;
using System.ComponentModel.Composition;
using System.Windows.Forms;

namespace PoolAI
{
    public partial class PoolAI : Form
    {
        #region Fields

        private ComponentManager m_ComponentManager;
        private int m_OldWidth;
        private int m_OldHeight;

        #endregion

        #region Constructor

        public PoolAI()
        {
            InitializeComponent();
            Initialize();
        }

        #endregion

        #region Methods

        private void Initialize()
        {
            ComponentSelector<IGame, IGameMetadata> gameSelector = new ComponentSelector<IGame, IGameMetadata>("Games");
            gameSelector.ComponentSelected += OnGameSelected;
            gameSelector.Dock = DockStyle.Fill;
            Controls.Add(gameSelector);
        }
        private void StartGame()
        {
            var gameViewer = new GameViewer(m_ComponentManager);
            gameViewer.GameCompleted += OnGameCompleted;
            m_OldWidth = Width;
            m_OldHeight = Height;
            Controls.Add(gameViewer);
            CenterToScreen();
        }
        private void OnGameSelected(ComponentSelector<IGame, IGameMetadata> ctrl, ExportFactory<IGame, IGameMetadata> gameFactory)
        {
            Tag = gameFactory;
            ctrl.ComponentSelected -= OnGameSelected;
            Controls.Remove(ctrl);
            ComponentSelector<IBrain, IBrainMetadata> brainSelector = new ComponentSelector<IBrain, IBrainMetadata>("Brains");
            brainSelector.ComponentSelected += OnBrainSelected;
            brainSelector.Dock = DockStyle.Fill;
            Controls.Add(brainSelector);
        }
        private void OnBrainSelected(ComponentSelector<IBrain, IBrainMetadata> ctrl, ExportFactory<IBrain, IBrainMetadata> brainFactory)
        {
            ctrl.ComponentSelected -= OnBrainSelected;
            Controls.Remove(ctrl);
            var gameFactory = Tag as ExportFactory<IGame, IGameMetadata>;
            Tag = null;
            m_ComponentManager = new ComponentManager(gameFactory.CreateExport().Value, gameFactory.Metadata, brainFactory);
            StartGame();
        }
        private void OnGameCompleted(GameViewer ctrl)
        {
            ctrl.GameCompleted -= OnGameCompleted;
            Controls.Remove(ctrl);
            Width = m_OldWidth;
            Height = m_OldHeight;
            var gameCompletedSelector = new GameCompletedSelector();
            gameCompletedSelector.Dock = DockStyle.Fill;
            gameCompletedSelector.RestartClick += OnRestartClick;
            gameCompletedSelector.PlayWinnerClick += OnPlayWinnerClick;
            Controls.Add(gameCompletedSelector);
            CenterToScreen();
        }
        private void OnRestartClick(GameCompletedSelector ctrl)
        {
            ctrl.RestartClick -= OnRestartClick;
            ctrl.PlayWinnerClick -= OnPlayWinnerClick;
            Controls.Remove(ctrl);
            Initialize();
        }
        private void OnPlayWinnerClick(GameCompletedSelector ctrl)
        {
            ctrl.RestartClick -= OnRestartClick;
            ctrl.PlayWinnerClick -= OnPlayWinnerClick;
            Controls.Remove(ctrl);
            m_ComponentManager.PlayWinner = true;
            StartGame();
        }

        #endregion
    }
}
