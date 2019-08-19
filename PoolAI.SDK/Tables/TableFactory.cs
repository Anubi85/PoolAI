namespace PoolAI.SDK.Tables
{
    public static class TableFactory
    {
        #region Methods

        internal static ITableInternal GetTable(TableType type)
        {
            switch (type)
            {
                case TableType.LargePockets:
                    return LargePocketsTable.Instance;
                case TableType.SmallPockets:
                    return SmallPocketsTable.Instance;
                default:
                    return null;
            }
        }
        public static int GetTableWidth(TableType type)
        {
            switch(type)
            {
                case TableType.LargePockets:
                    return LargePocketsTable.TableWidth;
                case TableType.SmallPockets:
                    return SmallPocketsTable.TableWidth;
                default:
                    return -1;
            }
        }
        public static int GetTableHeight(TableType type)
        {
            switch (type)
            {
                case TableType.LargePockets:
                    return LargePocketsTable.TableWidth;
                case TableType.SmallPockets:
                    return SmallPocketsTable.TableWidth;
                default:
                    return -1;
            }
        }

        #endregion
    }
}
