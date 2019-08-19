using PoolAI.SDK.Balls;
using PoolAI.SDK.Brain;
using PoolAI.SDK.Data;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace Brain.Human
{
    [ExportBrain("Human", typeof(Brain))]
    class Brain : IBrain, IDisposable
    {
        #region Fields

        private int m_ShotCounter;
        private bool m_ShotEnabled;
        private bool m_IsShooting;
        private int m_MouseDownX;
        private int m_MouseDownY;
        private MouseHook m_MouseHook;
        private ShotData m_ShotData;

        #endregion

        #region Properties

        public double Score { get; private set; }
        public int ShotCount { get; private set; }

        #endregion

        #region Constructor

        public Brain()
        {
            m_ShotCounter = 0;
            m_ShotEnabled = true;
            m_IsShooting = false;
            m_MouseDownX = 0;
            m_MouseDownY = 0;
            m_MouseHook = null;
            Score = 0;
            ShotCount = 0;
        }

        #endregion

        #region Methods

        private void OnMouseDown(int x, int y)
        {
            if (m_ShotEnabled)
            {
                m_ShotEnabled = false;
                m_MouseDownX = x;
                m_MouseDownY = y;
                m_IsShooting = true;
            }
        }
        private void OnMouseUp(int x, int y)
        {
            if (m_IsShooting)
            {
                m_IsShooting = false;
                int deltaX = x - m_MouseDownX;
                int deltaY = y - m_MouseDownY;
                m_ShotData.Force = Math.Sqrt(deltaX * deltaX + deltaY * deltaY) * 5;
                m_ShotData.Direction = Math.Atan2(-deltaY, -deltaX) / Math.PI * 180.0;
                ShotCount++;
                ShotDataReady?.Invoke(m_ShotData);
            }
        }
        [ExportDrawOverlayFunction]
        public void DrawOverlay(Graphics g)
        {
            g.DrawString("Score: " + Score, SystemFonts.DefaultFont, Brushes.Black, Point.Empty);
            g.FillEllipse(m_ShotEnabled ? Brushes.LimeGreen : (m_IsShooting ? Brushes.Gold : Brushes.Red), g.ClipBounds.Left, g.ClipBounds.Bottom - 16, 15, 15);
            g.DrawString(m_ShotEnabled ? "Can Shot" : (m_IsShooting ? "Charging Shoot" : "Shot In Progress..."), SystemFonts.DefaultFont, Brushes.Black, g.ClipBounds.Left + 17, g.ClipBounds.Bottom - 14);
        }
        public void Initialize(IBall cueBall)
        {
            ShotCount = 0;
            m_ShotData = new ShotData(cueBall);
            //Add mouse hook to application
            m_MouseHook = new MouseHook();
            m_MouseHook.MouseDown += OnMouseDown;
            m_MouseHook.MouseUp += OnMouseUp;
            Application.AddMessageFilter(m_MouseHook);
        }
        public void RequestShotData()
        {
            m_ShotEnabled = true;
        }
        public void UpdateScore(double score)
        {
            Score += score;
        }
        public void Dispose()
        {
            m_MouseHook.MouseDown -= OnMouseDown;
            m_MouseHook.MouseUp -= OnMouseUp;
            Application.RemoveMessageFilter(m_MouseHook);
        }

        #endregion

        #region Events

        public event Action<ShotData> ShotDataReady;
        public event Action EvolutionRequest;

        #endregion
    }
}
