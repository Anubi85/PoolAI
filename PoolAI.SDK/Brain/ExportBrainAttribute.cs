using System;
using System.ComponentModel.Composition;

namespace PoolAI.SDK.Brain
{
    [MetadataAttribute]
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public sealed class ExportBrainAttribute : ExportAttribute, IBrainMetadata
    {
        #region Properties

        public string Name { get; private set; }
        public int Population { get; set; }
        public Type BrainType { get; private set; }

        #endregion

        #region Constructor

        public ExportBrainAttribute(string name, Type brainType) : base(typeof(IBrain))
        {
            Name = name;
            Population = 1;
            BrainType = brainType;
        }

        #endregion
    }
}
