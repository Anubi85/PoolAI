using System;

namespace PoolAI.SDK.LinearAlgebra
{
    internal struct Vector
    {
        #region Fields

        private double[] m_Values;

        #endregion

        #region Properties

        public double this[int idx]
        {
            get { return m_Values[idx]; }
            set { m_Values[idx] = value; }
        }

        public int Size { get; private set; }

        #endregion

        #region Constructor

        public Vector(int size)
        {
            Size = size;
            m_Values = new double[size];
        }

        #endregion

        #region Methods

        public static double Dot(Vector first, Vector second)
        {
            double res = 0;
            for (int i = 0; i < Math.Max(first.Size, second.Size); i++)
            {
                res += first[i] * second[i];
            }
            return res;
        }

        #endregion
    }
}
