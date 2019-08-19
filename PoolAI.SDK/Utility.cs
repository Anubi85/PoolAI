namespace PoolAI.SDK
{
    internal static class Utility
    {
        #region Constants

        private const int c_Prime = 73;

        #endregion

        #region Methods

        public static uint GetHashCode(params object[] values)
        {
            uint res = 1;
            foreach(var val in values)
            {
                res = res * c_Prime + (uint)val.GetHashCode();
            }
            return res;
        }

        #endregion
    }
}
