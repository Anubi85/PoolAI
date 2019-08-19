namespace PoolAI.SDK.LinearAlgebra
{
    internal static class LinearSystem
    {
        #region Methods

        public static Vector Solve(Matrix A, Vector B)
        {
            Matrix L = A.CholeskyDecomposition();
            Matrix Lt = L.Transpose();
            Vector y = new Vector(B.Size);
            for(int i = 0; i < B.Size; i++)
            {
                y[i] = (B[i] - Vector.Dot(y, L.GetRow(i))) / L[i, i];
            }
            Vector x = new Vector(B.Size);
            for (int i = B.Size - 1; i >= 0; i--)
            {
                x[i] = (y[i] - Vector.Dot(x, Lt.GetRow(i))) / Lt[i, i];
            }
            return x;
        }

        #endregion
    }
}
