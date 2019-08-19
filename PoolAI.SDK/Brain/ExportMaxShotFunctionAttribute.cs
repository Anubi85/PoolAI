using System;
using System.ComponentModel.Composition;

namespace PoolAI.SDK.Brain
{
    #region Delegates

    public delegate int MaxShotFunction();

    #endregion

    [MetadataAttribute]
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public sealed class ExportMaxShotFunctionAttribute : ExportAttribute
    {
        #region Constructor

        public ExportMaxShotFunctionAttribute() : base(typeof(MaxShotFunction)) { }

        #endregion
    }
}
