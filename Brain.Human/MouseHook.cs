using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace Brain.Human
{
    class MouseHook : IMessageFilter
    {
        #region Structs

        [StructLayout(LayoutKind.Explicit)]
        class MousePosition
        {
            #region Fields

            [FieldOffset(0)]
            private int m_Raw;
            [FieldOffset(0)]
            private short m_X;
            [FieldOffset(2)]
            private short m_Y;

            #endregion

            #region Properties

            public int X { get { return m_X; } }
            public int Y { get { return m_Y; } }

            #endregion

            #region Constructor

            public MousePosition(int raw)
            {
                m_Raw = raw;
            }

            #endregion
        }

        #endregion

        #region Constants

        private const int c_ButtonDownMsg = 0x0201;
        private const int c_ButtonUpMsg = 0x0202;
        private const int c_LeftMouseButton = 0x01;

        #endregion

        #region Methods

        public bool PreFilterMessage(ref Message m)
        {
            MousePosition pos;
            if (m.Msg == c_ButtonDownMsg)
            {
                pos = new MousePosition(m.LParam.ToInt32());
                MouseDown?.Invoke(pos.X, pos.Y);
                return true;
            }
            if (m.Msg == c_ButtonUpMsg)
            {
                pos = new MousePosition(m.LParam.ToInt32());
                MouseUp?.Invoke(pos.X, pos.Y);
                return true;
            }
            return false;
        }

        #endregion

        #region Events

        public event Action<int, int> MouseDown;
        public event Action<int, int> MouseUp;

        #endregion
    }
}
