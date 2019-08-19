namespace PoolAI.SDK.Geometry
{
    class Line2
    {
        #region Fields

        private double m_A;
        private double m_B;
        private double m_C;
        private double m_M;
        private double m_Q;
        private bool m_InvertNormal;
        private Coordinates m_Versor;
        private Coordinates m_NormalVersor;

        #endregion

        #region Properties

        public double A
        {
            get { return m_A; }
            set { SetImplicit(ref m_A, value); }
        }
        public double B
        {
            get { return m_B; }
            set { SetImplicit(ref m_B, value); }
        }
        public double C
        {
            get { return m_C; }
            set { SetImplicit(ref m_C, value); }
        }
        public double M
        {
            get { return m_M; }
            set { SetExplicit(ref m_M, value); }
        }
        public double Q
        {
            get { return m_Q; }
            set { SetExplicit(ref m_Q, value); }
        }
        public Coordinates Versor { get { return m_Versor; } }
        public Coordinates NormalVersor { get { return m_NormalVersor; } }

        #endregion

        #region Constructor

        public Line2(double a, double b, double c, bool invertNormal = false)
        {
            m_InvertNormal = invertNormal;
            A = a;
            B = b;
            C = c;
        }

        public Line2(double m, double q, bool invertNormal = false)
        {
            m_InvertNormal = invertNormal;
            M = m;
            Q = q;
        }

        #endregion

        #region Methods

        private void SetImplicit(ref double field, double value)
        {
            if (field != value)
            {
                field = value;
                m_M = m_B == 0 ? double.PositiveInfinity : -m_A / m_B;
                m_Q = m_B == 0 ? -m_C : -m_C / m_B;
                m_Versor = Coordinates.Normalize(new Coordinates(-m_B, m_A));
                m_NormalVersor = new Coordinates(module: 1, phase: m_Versor.Phase + (m_InvertNormal ? -90 : 90));
            }
        }

        private void SetExplicit(ref double field, double value)
        {
            if (field != value)
            {
                field = value;
                m_A = double.IsInfinity(m_M) ? 1 : -m_M;
                m_B = double.IsInfinity(m_M) ? 0 : 1;
                m_C = -m_Q;
                m_Versor = Coordinates.Normalize(new Coordinates(-m_B, m_A));
                m_NormalVersor = new Coordinates(module: 1, phase: m_Versor.Phase + (m_InvertNormal ? -90 : 90));
            }
        }

        public static Line2 FromPoints(Coordinates point1, Coordinates point2)
        {
            if (point1.X == point2.X)
            {
                return new Line2(1, 0, -point1.X);
            }
            else if (point1.Y == point2.Y)
            {
                return new Line2(0, 1, -point1.Y);
            }
            else
            {
                double m = (point2.Y - point1.Y) / (point2.X - point1.X);
                return new Line2(m, point1.Y - m * point1.X);
            }
        }

        public static Line2 FromPointDirection(Coordinates point, Coordinates direction)
        {
            return new Line2(-direction.Y, direction.X, point.X * direction.Y - point.Y * direction.X);
        }

        public bool CheckPoint(Coordinates point)
        {
            double check = A * point.X + B * point.Y + C;
            return check.ToleranceEqual(0);
        }

        #endregion

        #region Operators

        public static Line2 ReflectX(Line2 line)
        {
            return new Line2(line.A, -line.B, line.C);
        }

        public static Line2 ReflectY(Line2 line)
        {
            return new Line2(-line.A, line.B, line.C);
        }

        #endregion
    }
}
