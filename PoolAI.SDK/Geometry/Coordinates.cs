using System;

namespace PoolAI.SDK.Geometry
{
#pragma warning disable CS0660
    class Coordinates : ICoordinates
    {
        #region Fields

        private bool m_IsReadonly;
        private double m_X;
        private double m_Y;
        private double m_Module;
        private double m_Phase;

        #endregion

        #region Properties

        public double X
        {
            get { return m_X; }
            set { SetCartesian(ref m_X, value); }
        }
        public double Y
        {
            get { return m_Y; }
            set { SetCartesian(ref m_Y, value); }
        }
        public double Module
        {
            get { return m_Module; }
            set { SetPolar(ref m_Module, value); }
        }
        public double Phase
        {
            get { return Rad2Deg(m_Phase); }
            set { SetPolar(ref m_Phase, Deg2Rad(value)); }
        }
        public static Coordinates Zero { get { return new Coordinates(); } }

        #endregion

        #region Constructor

        private Coordinates(bool isReadonly, double? x = null, double? y = null, double? module = null, double? phase = null)
        {
            if (x.HasValue || y.HasValue)
            {
                X = x.GetValueOrDefault();
                Y = y.GetValueOrDefault();
            }
            else if (module.HasValue || phase.HasValue)
            {
                Phase = phase.GetValueOrDefault();
                Module = module.GetValueOrDefault();
            }
            m_IsReadonly = isReadonly;
        }
        public Coordinates(double? x = null, double? y = null, double? module = null, double? phase = null) :
            this(false, x, y, module, phase)
        {
        }

        #endregion

        #region Methods

        private double Deg2Rad(double deg)
        {
            if (deg < 0)
            {
                deg += 360.0;
            }
            return deg * Math.PI / 180.0;
        }
        private double Rad2Deg(double rad)
        {
            double deg = rad * 180.0 / Math.PI;
            if (deg < 0)
            {
                deg += 360.0;
            }
            return deg;
        }
        private void SetCartesian(ref double field, double value)
        {
            if (!m_IsReadonly)
            {
                field = value;
                m_Module = Math.Sqrt(Dot(this, this));
                m_Phase = Math.Atan2(m_Y, m_X);
            }
        }
        private void SetPolar(ref double field, double value)
        {
            if (!m_IsReadonly)
            {
                field = value;
                if (m_Module < 0)
                {
                    m_Module = -m_Module;
                    m_Phase = (m_Phase + Math.PI) % (2 * Math.PI);
                }
                m_X = m_Module * Math.Cos(m_Phase);
                m_Y = m_Module * Math.Sin(m_Phase);
            }
        }
        public override int GetHashCode()
        {
            return (int)Utility.GetHashCode(m_X, m_Y);
        }
        public override bool Equals(object obj)
        {
            if (obj is Coordinates)
            {
                return this == (Coordinates)obj;
            }
            else
            {
                return false;
            }
        }

        #endregion

        #region Operators

        public static Coordinates MakeReadonly(Coordinates vector)
        {
            vector.m_IsReadonly = true;
            return vector;
        }
        public static double Dot(Coordinates right, Coordinates left)
        {
            return right.X * left.X + right.Y * left.Y;
        }
        public static Coordinates Cross(Coordinates right, Coordinates left)
        {
            return new Coordinates(x: right.X * left.Y, y: right.Y * left.X);
        }
        public static Coordinates Add(ICoordinates right, ICoordinates left)
        {
            return new Coordinates(x: right.X + left.X, y: right.Y + left.Y);
        }
        public static Coordinates Sub(ICoordinates right, ICoordinates left)
        {
            return new Coordinates(x: right.X - left.X, y: right.Y - left.Y);
        }
        public static Coordinates Normalize(Coordinates vector)
        {
            return new Coordinates(module: 1, phase: vector.Phase);
        }
        public static Coordinates operator -(Coordinates vector)
        {
            return new Coordinates(x: -vector.X, y: -vector.Y);
        }
        public static Coordinates operator *(Coordinates right, double left)
        {
            return new Coordinates(module: right.Module * left, phase: right.Phase);
        }
        public static Coordinates operator *(double right, Coordinates left)
        {
            return new Coordinates(module: left.Module * right, phase: left.Phase);
        }
        public static Coordinates operator /(Coordinates right, int left)
        {
            return new Coordinates(module: right.Module / left, phase: right.Phase);
        }
        public static bool operator ==(Coordinates right, Coordinates left)
        {
            return right.X == left.X && right.Y == left.Y;
        }
        public static bool operator !=(Coordinates right, Coordinates left)
        {
            return right.X != left.X || right.Y != left.Y;
        }

        #endregion
    }
}
