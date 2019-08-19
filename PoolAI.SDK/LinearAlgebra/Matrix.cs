using System;

namespace PoolAI.SDK.LinearAlgebra
{
    internal struct Matrix
    {
        #region Fields

        private Vector[] m_Columns;
        private Vector[] m_Rows;

        #endregion

        #region Properties

        public double this[int row, int col]
        {
            get { return m_Rows[row][col]; }
            set
            {
                m_Rows[row][col] = value;
                m_Columns[col][row] = value;
            }
        }

        public int Size { get; private set; }

        #endregion

        #region Constructor

        private Matrix(Vector[] data) : this(data.Length)
        {
            for (int i = 0; i < Size; i++)
            {
                for (int j = 0; j < Size; j++)
                {
                    m_Rows[i][j] = data[i][j];
                    m_Columns[i][j] = data[j][i];
                }
            }
        }

        public Matrix(int size)
        {
            Size = size;
            m_Columns = new Vector[size];
            m_Rows = new Vector[size];
            for (int i = 0; i < size; i++)
            {
                m_Columns[i] = new Vector(size);
                m_Rows[i] = new Vector(size);
            }
        }

        #endregion

        #region Methods

        public Vector GetRow(int row)
        {
            return m_Rows[row];
        }

        public Vector GetColumn(int col)
        {
            return m_Columns[col];
        }

        public Matrix Transpose()
        {
            return new Matrix(m_Columns);
        }

        public Matrix Clone()
        {
            return new Matrix(m_Columns);
        }

        public Matrix CholeskyDecomposition()
        {
            Matrix res = new Matrix(Size);
            for (int i = 0; i < Size; i++)
            {
                for (int j = 0; j <= i; j++)
                {
                    if (i == j)
                    {
                        //diagonal terms
                        Vector row = res.GetRow(i);
                        double tmp = m_Rows[i][i] - Vector.Dot(row, row);
                        if (tmp < 0)
                        {
                            throw new Exception("Matrix is not positive defined");
                        }
                        res[i, i] = Math.Sqrt(m_Rows[i][i] - Vector.Dot(row, row));
                    }
                    else
                    {
                        //non diagonal terms
                        Vector row = res.GetRow(i);
                        Vector col = res.GetRow(j);
                        res[i, j] = (m_Rows[i][j] - Vector.Dot(row, col)) / res[j, j];
                    }
                }
            }
            return res;
        }

        #endregion
    }
}
