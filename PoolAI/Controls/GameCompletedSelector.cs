using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PoolAI.Controls
{
    public partial class GameCompletedSelector : UserControl
    {
        #region Constructor

        public GameCompletedSelector()
        {
            InitializeComponent();
        }

        #endregion

        #region Event Handlers

        private void OnRestartClick(object sender, EventArgs e)
        {
            RestartClick?.Invoke(this);
        }
        private void OnPlayWinnerClick(object sender, EventArgs e)
        {
            PlayWinnerClick?.Invoke(this);
        }

        #endregion

        #region Events

        public event Action<GameCompletedSelector> RestartClick;
        public event Action<GameCompletedSelector> PlayWinnerClick;

        #endregion
    }
}
