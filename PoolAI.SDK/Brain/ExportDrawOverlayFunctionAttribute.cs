using System;
using System.ComponentModel.Composition;
using System.Drawing;

namespace PoolAI.SDK.Brain
{
    #region Delegates

    public delegate void DrawOverlayFunction(Graphics g);

    #endregion

    [MetadataAttribute]
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public sealed class ExportDrawOverlayFunctionAttribute : ExportAttribute
    {
        #region Constructor

        public ExportDrawOverlayFunctionAttribute() : base(typeof(DrawOverlayFunction)) { }

        #endregion
    }
}
