using PoolAI.SDK.Data;
using PoolAI.SDK.Geometry;
using PoolAI.SDK.Tables;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace PoolAI.SDK.Balls
{
    internal sealed class TriangleBallSet : BallSetBase
    {
        #region Constants

        private static readonly BallPhysicsData c_PhysicsData;
        private static readonly List<BallGraphicsData> c_GraphicsData;
        private static readonly List<Coordinates> c_BallPositionOffsets;

        #endregion

        #region Properties

        public static int MaxBallNum { get { return c_GraphicsData.Count; } }

        #endregion

        #region Constructor

        static TriangleBallSet()
        {
            c_PhysicsData = new BallPhysicsData(13, 0.5, 0.98, 0.016);
            c_GraphicsData = new List<BallGraphicsData>() {
                new BallGraphicsData("1",   Color.FromArgb(0xFF, 0xFF, 0xC9, 0x0E), false, 0.5),
                new BallGraphicsData("2",   Color.FromArgb(0xFF, 0x00, 0x40, 0x80), false, 0.5),
                new BallGraphicsData("3",   Color.FromArgb(0xFF, 0xED, 0x1C, 0x24), false, 0.5),
                new BallGraphicsData("4",   Color.FromArgb(0xFF, 0x80, 0x00, 0xFF), false, 0.5),
                new BallGraphicsData("5",   Color.FromArgb(0xFF, 0xFF, 0x7F, 0x27), false, 0.5),
                new BallGraphicsData("6",   Color.FromArgb(0xFF, 0x00, 0x80, 0x40), false, 0.5),
                new BallGraphicsData("7",   Color.FromArgb(0xFF, 0x88, 0x00, 0x15), false, 0.5),
                new BallGraphicsData("9",   Color.FromArgb(0xFF, 0xFF, 0xC9, 0x0E), true,  0.5),
                new BallGraphicsData("10",  Color.FromArgb(0xFF, 0x00, 0x40, 0x80), true,  0.5),
                new BallGraphicsData("11",  Color.FromArgb(0xFF, 0xED, 0x1C, 0x24), true,  0.5),
                new BallGraphicsData("8",   Color.FromArgb(0xFF, 0x00, 0x00, 0x00), true,  0.5),
                new BallGraphicsData("12",  Color.FromArgb(0xFF, 0x80, 0x00, 0xFF), true,  0.5),
                new BallGraphicsData("13",  Color.FromArgb(0xFF, 0xFF, 0x7F, 0x27), true,  0.5),
                new BallGraphicsData("14",  Color.FromArgb(0xFF, 0x00, 0x80, 0x40), true,  0.5),
                new BallGraphicsData("15",  Color.FromArgb(0xFF, 0x88, 0x00, 0x15), true,  0.5),
            };
            c_BallPositionOffsets = new List<Coordinates>() {
                new Coordinates(0 * Math.Cos(Math.PI / 6) * c_PhysicsData.Radius * 2,  0 * Math.Sin(Math.PI / 6) * c_PhysicsData.Radius * 2),
                new Coordinates(1 * Math.Cos(Math.PI / 6) * c_PhysicsData.Radius * 2, -1 * Math.Sin(Math.PI / 6) * c_PhysicsData.Radius * 2),
                new Coordinates(1 * Math.Cos(Math.PI / 6) * c_PhysicsData.Radius * 2,  1 * Math.Sin(Math.PI / 6) * c_PhysicsData.Radius * 2),
                new Coordinates(2 * Math.Cos(Math.PI / 6) * c_PhysicsData.Radius * 2, -2 * Math.Sin(Math.PI / 6) * c_PhysicsData.Radius * 2),
                new Coordinates(2 * Math.Cos(Math.PI / 6) * c_PhysicsData.Radius * 2,  0 * Math.Sin(Math.PI / 6) * c_PhysicsData.Radius * 2),
                new Coordinates(2 * Math.Cos(Math.PI / 6) * c_PhysicsData.Radius * 2,  2 * Math.Sin(Math.PI / 6) * c_PhysicsData.Radius * 2),
                new Coordinates(3 * Math.Cos(Math.PI / 6) * c_PhysicsData.Radius * 2, -3 * Math.Sin(Math.PI / 6) * c_PhysicsData.Radius * 2),
                new Coordinates(3 * Math.Cos(Math.PI / 6) * c_PhysicsData.Radius * 2, -1 * Math.Sin(Math.PI / 6) * c_PhysicsData.Radius * 2),
                new Coordinates(3 * Math.Cos(Math.PI / 6) * c_PhysicsData.Radius * 2,  1 * Math.Sin(Math.PI / 6) * c_PhysicsData.Radius * 2),
                new Coordinates(3 * Math.Cos(Math.PI / 6) * c_PhysicsData.Radius * 2,  3 * Math.Sin(Math.PI / 6) * c_PhysicsData.Radius * 2),
                new Coordinates(4 * Math.Cos(Math.PI / 6) * c_PhysicsData.Radius * 2,  4 * Math.Sin(Math.PI / 6) * c_PhysicsData.Radius * 2),
                new Coordinates(4 * Math.Cos(Math.PI / 6) * c_PhysicsData.Radius * 2, -2 * Math.Sin(Math.PI / 6) * c_PhysicsData.Radius * 2),
                new Coordinates(4 * Math.Cos(Math.PI / 6) * c_PhysicsData.Radius * 2,  0 * Math.Sin(Math.PI / 6) * c_PhysicsData.Radius * 2),
                new Coordinates(4 * Math.Cos(Math.PI / 6) * c_PhysicsData.Radius * 2,  2 * Math.Sin(Math.PI / 6) * c_PhysicsData.Radius * 2),
                new Coordinates(4 * Math.Cos(Math.PI / 6) * c_PhysicsData.Radius * 2, -4 * Math.Sin(Math.PI / 6) * c_PhysicsData.Radius * 2),
            };
        }
        public TriangleBallSet(ITableInternal table) : base(table)
        {
            //add cue ball
            var cueBallLocation = m_Table.CueBallLocations.FirstOrDefault() ?? new Coordinates(-m_Table.Width / 4.0, 0);
            var cueBall = AddBall(new BallGraphicsData(null, Color.White, false, 0.5), c_PhysicsData, cueBallLocation);
            m_CueBalls.Add(cueBall as IBallInternal);
            //add other balls
            var ballLocation = m_Table.BallLocations.FirstOrDefault() ?? new Coordinates(m_Table.Width / 4.0, 0);
            foreach (var data in c_GraphicsData.Zip(c_BallPositionOffsets, (gd, off) => new { GraphicsData = gd, Position = Coordinates.Add(ballLocation, off) }))
            {
                AddBall(data.GraphicsData, c_PhysicsData, data.Position);
            }
        }

        #endregion
    }
}
