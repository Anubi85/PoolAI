namespace PoolAI.SDK.Tables
{
    internal sealed class SmallPocketsTable : TableWithPocketsBase
    {
        #region Properties

        public static SmallPocketsTable Instance { get; private set; }

        #endregion

        #region Constructor

        static SmallPocketsTable()
        {
            Instance = new SmallPocketsTable();
        }
        private SmallPocketsTable() : base(18) { }

        #endregion
    }
}
