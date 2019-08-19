using PoolAI.SDK.Geometry;
using System;
using System.Drawing;

namespace PoolAI.SDK.Data
{
    internal class BallGraphicsData
    {
        #region Properties

        public string Label { get; private set; }
        public Coordinates LabelOffset { get; private set; }
        public Brush Brush { get; private set; }
        public bool Striped { get; private set; }
        public double StripedRadiusCoefficient { get; private set; }

        #endregion

        #region Constructor

        public BallGraphicsData(string label, Color color, bool striped, double stripedRadiusCoefficient)
        {
            Label = label;
            SizeF labelSize = Graphics.FromHwnd(IntPtr.Zero).MeasureString(label, SystemFonts.DefaultFont);
            LabelOffset = new Coordinates(x: labelSize.Width / 2, y: labelSize.Height / 2);
            Brush = new SolidBrush(color);
            Striped = striped;
            StripedRadiusCoefficient = stripedRadiusCoefficient;
        }

        #endregion
    }
}
