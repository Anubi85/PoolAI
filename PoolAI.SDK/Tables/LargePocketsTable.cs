namespace PoolAI.SDK.Tables
{
    internal sealed class LargePocketsTable : TableWithPocketsBase
    {
        #region Properties

        public static LargePocketsTable Instance { get; private set; }

        #endregion

        #region Constructor

        static LargePocketsTable()
        {
            Instance = new LargePocketsTable();
        }
        private LargePocketsTable() : base(23) { }

        #endregion
    }
}
