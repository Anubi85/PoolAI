using PoolAI.SDK.Balls;
using PoolAI.SDK.Tables;
using System;
using System.ComponentModel.Composition;

namespace PoolAI.SDK.Game
{
    [MetadataAttribute]
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public sealed class ExportGameAttribute : ExportAttribute, IGameMetadata
    {
        #region Properties

        public string Name { get; private set; }
        public TableType Table { get; private set; }
        public BallSetType BallSet { get; private set; }
        public int PlayersNum { get; private set; }

        #endregion

        #region Constructor

        public ExportGameAttribute(string name, TableType table, BallSetType ballSet, int playersNum) : base(typeof(IGame))
        {
            Name = name;
            Table = table;
            BallSet = ballSet;
            PlayersNum = playersNum;
        }

        #endregion
    }
}
