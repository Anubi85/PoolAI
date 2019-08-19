using System;
using System.Windows.Forms;
using PoolAI.SDK;

namespace PoolAI.Controls
{
    partial class GameViewer : UserControl
    {
        #region Fields

        private ComponentManager m_ComponentManager;

        #endregion

        #region Constructor

        public GameViewer(ComponentManager componentManager)
        {
            InitializeComponent();
            DoubleBuffered = true;
            m_ComponentManager = componentManager;
            Width = m_ComponentManager.TableWidth;
            Height = m_ComponentManager.TableHeight;
            m_ComponentManager.StartGame();
            GameTimer.Start();
        }

        #endregion

        #region Draw

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            m_ComponentManager.Draw(e.Graphics);
        }

        #endregion

        #region Event Handler

        private void GameTick(object sender, EventArgs e)
        {
            if (!m_ComponentManager.HasWinner)
            {
                m_ComponentManager.Update();
                Refresh();
            }
            else
            {
                GameTimer.Stop();
                GameCompleted?.Invoke(this);
            }
        }

        #endregion

        #region Events

        public event Action<GameViewer> GameCompleted;

        #endregion
    }
}
