using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;

namespace PoolAI.SDK.Brain
{
    #region Delegates

    public delegate IEnumerable<IBrain> EvolveFunction(IEnumerable<IBrain> brains);

    #endregion

    [MetadataAttribute]
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class ExportEvolveFunctionAttribute : ExportAttribute
    {
        #region Constructor

        public ExportEvolveFunctionAttribute() : base(typeof(EvolveFunction)) { }

        #endregion
    }
}
