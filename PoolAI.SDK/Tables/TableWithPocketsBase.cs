using PoolAI.SDK.Balls;
using PoolAI.SDK.Data;
using PoolAI.SDK.Geometry;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;

namespace PoolAI.SDK.Tables
{
    internal abstract class TableWithPocketsBase : ITable, ITableInternal
    {
        #region Constants

        private const int c_BorderWidth = 35;
        private const int c_Width = 970;
        private const int c_Height = 600;
        private static readonly Brush c_BorderBrush;
        private static readonly Region c_BorderRegion;
        private static readonly Brush c_BankBrush;
        private static readonly Region c_BankRegion;
        private static readonly Brush c_TableBrush;
        private static readonly Region c_TableRegion;
        private static readonly Brush c_PocketBrush;
        private static readonly Region c_PocketRegion;

        private readonly int c_PocketRadius;
        private readonly int c_PocketDiameter;
        private readonly IReadOnlyList<Line2> c_Banks;
        private readonly IReadOnlyList<ICoordinates> c_BankCorners;
        private readonly IReadOnlyList<Coordinates> c_Pockets;
        private readonly IReadOnlyList<Coordinates> c_CueBallLocations;
        private readonly IReadOnlyList<Coordinates> c_BallLocations;

        private const double c_BankBouncingFrictionCoefficient = 0.95;
        private const double c_BankSlidingFrictionCoefficient = 0.99;

        #endregion

        #region Properties

        public static int TableWidth { get; private set; }
        public static int TableHeight { get; private set; }
        public int Width
        {
            get { return TableWidth; }
        }
        public int Height
        {
            get { return TableHeight; }
        }
        public double BankBouncingFrictionCoefficient
        {
            get { return c_BankBouncingFrictionCoefficient; }
        }
        public double BankSlidingFrictionCoefficient
        {
            get { return c_BankSlidingFrictionCoefficient; }
        }
        public IReadOnlyList<Coordinates> CueBallLocations
        {
            get { return c_CueBallLocations; }
        }
        public IReadOnlyList<Coordinates> BallLocations
        {
            get { return c_BallLocations; }
        }
        public IReadOnlyList<ICoordinates> TargetLocations
        {
            get { return c_Pockets; }
        }

        #endregion

        #region Constructor

        static TableWithPocketsBase()
        {
            TableWidth = c_BorderWidth * 2 + c_Width;
            TableHeight = c_BorderWidth * 2 + c_Height;
            //prepare brushes
            c_BorderBrush = new SolidBrush(Color.FromArgb(255, 153, 82, 45));
            c_TableBrush = new SolidBrush(Color.FromArgb(255, 15, 157, 52));
            c_BankBrush = new SolidBrush(Color.FromArgb(255, 16, 171, 55));
            c_PocketBrush = new SolidBrush(Color.Black);
            //prepare regions
            c_BorderRegion = new Region();
            c_TableRegion = new Region();
            c_BankRegion = new Region();
            c_PocketRegion = new Region();
        }
        protected TableWithPocketsBase(int pocketRadius)
        {
            //initialize pocket data
            c_PocketRadius = pocketRadius;
            c_PocketDiameter = 2 * c_PocketRadius;
            //prepare border
            InitBorderRegion();
            //prepare table
            InitTableRegion();
            //prepare banks
            InitBanksRegion();
            //prepare pockets
            InitPocketsRegion();
            //store pockets positions
            c_Pockets = InitPocketsPositions();
            //store banks lines
            c_Banks = InitBanksLines();
            //store banks corners
            c_BankCorners = InitBankPoints();
            //prepare ball locations
            var tmp = new Coordinates(c_Width / 4.0, 0);
            c_CueBallLocations = new List<Coordinates>() { -tmp };
            c_BallLocations = new List<Coordinates>() { tmp };
        }

        #endregion

        #region Methods

        private void InitBorderRegion()
        {
            c_BorderRegion.MakeEmpty();
            c_BorderRegion.Union(new Rectangle(0, 0, c_BorderWidth * 2 + c_Width, c_BorderWidth * 2 + c_Height));
            c_BorderRegion.Exclude(new Rectangle(c_BorderWidth, c_BorderWidth, c_Width, c_Height));
        }
        private void InitTableRegion()
        {
            c_TableRegion.MakeEmpty();
            c_TableRegion.Union(new Rectangle(c_BorderWidth, c_BorderWidth, c_Width, c_Height));
        }
        private void InitBanksRegion()
        {
            c_BankRegion.MakeEmpty();
            c_BankRegion.Union(new Rectangle(c_BorderWidth, c_BorderWidth, c_Width, c_Height));
            c_BankRegion.Exclude(new Rectangle(c_BorderWidth + c_PocketRadius, c_BorderWidth + c_PocketRadius, c_Width - c_PocketDiameter, c_Height - c_PocketDiameter));
            ClearBankRegionAngles();
            ClearBankRegionSides();
        }
        private void InitPocketsRegion()
        {
            float sqrt2 = (float)Math.Sqrt(2);
            c_PocketRegion.MakeEmpty();
            //prepare path
            using (GraphicsPath pocketPath = new GraphicsPath())
            {
                pocketPath.AddEllipse(0, 0, c_PocketDiameter, c_PocketDiameter);
                //prepare region
                using (Region pocketRegion = new Region(pocketPath))
                {
                    Region tmp;
                    //top left
                    using (tmp = pocketRegion.Clone())
                    {
                        tmp.Translate(c_BorderWidth - c_PocketRadius * (sqrt2 - 1), c_BorderWidth - c_PocketRadius * (sqrt2 - 1));
                        c_PocketRegion.Union(tmp);
                    }
                    //top center
                    using (tmp = pocketRegion.Clone())
                    {
                        tmp.Translate(c_BorderWidth + c_Width / 2 - c_PocketRadius, c_BorderWidth - c_PocketRadius);
                        c_PocketRegion.Union(tmp);
                    }
                    //top right
                    using (tmp = pocketRegion.Clone())
                    {
                        tmp.Translate(c_BorderWidth + c_Width - c_PocketDiameter + c_PocketRadius * (sqrt2 - 1), c_BorderWidth - c_PocketRadius * (sqrt2 - 1));
                        c_PocketRegion.Union(tmp);
                    }
                    //bottom left
                    using (tmp = pocketRegion.Clone())
                    {
                        tmp.Translate(c_BorderWidth - c_PocketRadius * (sqrt2 - 1), c_BorderWidth + c_Height - c_PocketDiameter + c_PocketRadius * (sqrt2 - 1));
                        c_PocketRegion.Union(tmp);
                    }
                    //bottom center
                    using (tmp = pocketRegion.Clone())
                    {
                        tmp.Translate(c_BorderWidth + c_Width / 2 - c_PocketRadius, c_BorderWidth + c_Height - c_PocketRadius);
                        c_PocketRegion.Union(tmp);
                    }
                    //bottom right
                    using (tmp = pocketRegion.Clone())
                    {
                        tmp.Translate(c_BorderWidth + c_Width - c_PocketDiameter + c_PocketRadius * (sqrt2 - 1), c_BorderWidth + c_Height - c_PocketDiameter + c_PocketRadius * (sqrt2 - 1));
                        c_PocketRegion.Union(tmp);
                    }
                }
            }
        }
        private void ClearBankRegionAngles()
        {
            float sqrt2 = (float)Math.Sqrt(2);
            Matrix flipVertical = new Matrix(-1, 0, 0, 1, 0, 0);
            Matrix flipHorizontal = new Matrix(1, 0, 0, -1, 0, 0);
            //prepare path
            PointF P1 = new PointF(0, 0);
            PointF P2 = new PointF(P1.X + c_PocketRadius * sqrt2, P1.Y);
            PointF P3 = new PointF(P2.X + c_PocketRadius, P2.Y + c_PocketRadius);
            PointF P4 = new PointF(P1.X, P3.Y);
            using (GraphicsPath tmpPath = new GraphicsPath())
            {
                tmpPath.AddLines(new PointF[] { P1, P2, P3, P4 });
                //prepare region to exclude
                using (Region tmpRegion = new Region(tmpPath))
                {
                    Region tmp;
                    using (tmp = tmpRegion.Clone())
                    {
                        Matrix rotate = new Matrix();
                        rotate.Rotate(90);
                        tmp.Transform(rotate);
                        tmp.Transform(flipVertical);
                        tmpRegion.Union(tmp);
                    }
                    //top left corner
                    using (tmp = tmpRegion.Clone())
                    {
                        tmp.Translate(c_BorderWidth, c_BorderWidth);
                        c_BankRegion.Exclude(tmp);
                    }
                    //top right corner
                    using (tmp = tmpRegion.Clone())
                    {
                        tmp.Transform(flipVertical);
                        tmp.Translate(c_BorderWidth + c_Width, c_BorderWidth);
                        c_BankRegion.Exclude(tmp);
                    }
                    //bottom left corner
                    using (tmp = tmpRegion.Clone())
                    {
                        tmp.Transform(flipHorizontal);
                        tmp.Translate(c_BorderWidth, c_BorderWidth + c_Height);
                        c_BankRegion.Exclude(tmp);
                    }
                    //bottom right corner
                    using (tmp = tmpRegion.Clone())
                    {
                        tmp.Transform(flipHorizontal);
                        tmp.Transform(flipVertical);
                        tmp.Translate(c_BorderWidth + c_Width, c_BorderWidth + c_Height);
                        c_BankRegion.Exclude(tmp);
                    }
                }
            }
        }
        private void ClearBankRegionSides()
        {
            Matrix flipVertical = new Matrix(-1, 0, 0, 1, 0, 0);
            Matrix flipHorizontal = new Matrix(1, 0, 0, -1, 0, 0);
            float halfWidth = c_Width / 2f;
            //prepare path
            PointF P1 = new PointF(0, 0);
            PointF P2 = new PointF(P1.X - c_PocketRadius, P1.Y);
            PointF P3 = new PointF(P2.X - (float)Math.Tan(Math.PI / 8) * c_PocketRadius, P2.Y + c_PocketRadius);
            PointF P4 = new PointF(P1.X, P3.Y);
            using (GraphicsPath tmpPath = new GraphicsPath())
            {
                tmpPath.AddLines(new PointF[] { P1, P2, P3, P4 });
                //prepare region to exclude
                using (Region tmpRegion = new Region(tmpPath))
                {
                    Region tmp;
                    using (tmp = tmpRegion.Clone())
                    {
                        tmp.Transform(flipVertical);
                        tmpRegion.Union(tmp);
                    }
                    //top
                    using (tmp = tmpRegion.Clone())
                    {
                        tmp.Translate(c_BorderWidth + halfWidth, c_BorderWidth);
                        c_BankRegion.Exclude(tmp);
                    }
                    //bottom
                    using (tmp = tmpRegion.Clone())
                    {
                        tmp.Transform(flipHorizontal);
                        tmp.Translate(c_BorderWidth + halfWidth, c_BorderWidth + c_Height);
                        c_BankRegion.Exclude(tmp);
                    }
                }
            }
        }
        private IReadOnlyList<Coordinates> InitPocketsPositions()
        {
            List<Coordinates> pockets = new List<Coordinates>();
            double sqrt2 = Math.Sqrt(2);
            double halfWidth = c_Width / 2.0;
            double halfHeight = c_Height / 2.0;
            pockets.Add(new Coordinates(-halfWidth + c_PocketRadius * (2 - sqrt2), halfHeight - c_PocketRadius * (2 - sqrt2)));
            pockets.Add(new Coordinates(0, halfHeight));
            pockets.Add(new Coordinates(halfWidth - c_PocketRadius * (2 - sqrt2), halfHeight - c_PocketRadius * (2 - sqrt2)));
            pockets.Add(new Coordinates(-halfWidth + c_PocketRadius * (2 - sqrt2), -halfHeight + c_PocketRadius * (2 - sqrt2)));
            pockets.Add(new Coordinates(0, -halfHeight));
            pockets.Add(new Coordinates(halfWidth - c_PocketRadius * (2 - sqrt2), -halfHeight + c_PocketRadius * (2 - sqrt2)));
            return pockets;
        }
        private IReadOnlyList<Line2> InitBanksLines()
        {
            List<Line2> banks = new List<Line2>();
            Line2 tmp;
            //Vertical banks
            tmp = new Line2(1, 0, -c_Width / 2.0);
            banks.Add(tmp);
            banks.Add(Line2.ReflectY(tmp));
            tmp = new Line2(1, 0, -c_Width / 2.0 + c_PocketRadius);
            banks.Add(tmp);
            banks.Add(Line2.ReflectY(tmp));
            //Horizontal banks
            tmp = new Line2(0, 1, -c_Height / 2.0);
            banks.Add(tmp);
            banks.Add(Line2.ReflectX(tmp));
            tmp = new Line2(0, 1, -c_Height / 2.0 + c_PocketRadius);
            banks.Add(tmp);
            banks.Add(Line2.ReflectX(tmp));
            //Center pockets
            double a = -1 / Math.Tan(Math.PI / 8);
            double c = -c_Height / 2 + a * c_PocketRadius;
            banks.Add(new Line2(-a, 1, c));
            banks.Add(new Line2(a, 1, c));
            banks.Add(new Line2(a, -1, c));
            banks.Add(new Line2(-a, -1, c));
            //Corner pockets
            double c1 = c_Width / 2.0 - c_Height / 2.0 - c_PocketRadius * Math.Sqrt(2);
            banks.Add(new Line2(-1, 1, c1));
            banks.Add(new Line2(1, 1, c1));
            banks.Add(new Line2(1, -1, c1));
            banks.Add(new Line2(-1, -1, c1));
            double c2 = -c_Width / 2.0 + c_Height / 2.0 - c_PocketRadius * Math.Sqrt(2);
            banks.Add(new Line2(1, -1, c2));
            banks.Add(new Line2(-1, -1, c2));
            banks.Add(new Line2(-1, 1, c2));
            banks.Add(new Line2(1, 1, c2));
            return banks;
        }
        private IReadOnlyList<ICoordinates> InitBankPoints()
        {
            var bankPoints = new List<Coordinates>
            {
                new Coordinates(0, c_Height / 2.0),
                new Coordinates(c_PocketRadius, c_Height / 2.0),
                new Coordinates(c_PocketRadius * (1 + Math.Tan(Math.PI / 8)), c_Height / 2.0 - c_PocketRadius),
                new Coordinates(c_Width / 2.0 - c_PocketRadius * (1 + Math.Sqrt(2)), c_Height / 2.0 - c_PocketRadius),
                new Coordinates(c_Width / 2.0 - c_PocketRadius * Math.Sqrt(2), c_Height / 2.0),
                new Coordinates(c_Width / 2.0, c_Height / 2.0),
                new Coordinates(c_Width / 2.0, c_Height / 2.0 - c_PocketRadius * Math.Sqrt(2)),
                new Coordinates(c_Width / 2.0 - c_PocketRadius, c_Height / 2.0 - c_PocketRadius * (1 + Math.Sqrt(2))),
                new Coordinates(c_Width / 2.0 - c_PocketRadius, 0),
                new Coordinates(c_Width / 2.0 - c_PocketRadius, -c_Height / 2.0 + c_PocketRadius * (1 + Math.Sqrt(2))),
                new Coordinates(c_Width / 2.0, -c_Height / 2.0 + c_PocketRadius * Math.Sqrt(2)),
                new Coordinates(c_Width / 2.0, -c_Height / 2.0),
                new Coordinates(c_Width / 2.0 - c_PocketRadius * Math.Sqrt(2), -c_Height / 2.0),
                new Coordinates(c_Width / 2.0 - c_PocketRadius * (1 + Math.Sqrt(2)), -c_Height / 2.0 + c_PocketRadius),
                new Coordinates(c_PocketRadius * (1 + Math.Tan(Math.PI / 8)), -c_Height / 2.0 + c_PocketRadius),
                new Coordinates(c_PocketRadius, -c_Height / 2.0),
                new Coordinates(0, -c_Height / 2.0),
                new Coordinates(-c_PocketRadius, -c_Height / 2.0),
                new Coordinates(-c_PocketRadius * (1 + Math.Tan(Math.PI / 8)), -c_Height / 2.0 + c_PocketRadius),
                new Coordinates(-c_Width / 2.0 + c_PocketRadius * (1 + Math.Sqrt(2)), -c_Height / 2.0 + c_PocketRadius),
                new Coordinates(-c_Width / 2.0 + c_PocketRadius * Math.Sqrt(2), -c_Height / 2.0),
                new Coordinates(-c_Width / 2.0, -c_Height / 2.0),
                new Coordinates(-c_Width / 2.0, -c_Height / 2.0 + c_PocketRadius * Math.Sqrt(2)),
                new Coordinates(-c_Width / 2.0 + c_PocketRadius, -c_Height / 2.0 + c_PocketRadius * (1 + Math.Sqrt(2))),
                new Coordinates(-c_Width / 2.0 - c_PocketRadius, 0),
                new Coordinates(-c_Width / 2.0 + c_PocketRadius, c_Height / 2.0 - c_PocketRadius * (1 + Math.Sqrt(2))),
                new Coordinates(-c_Width / 2.0, c_Height / 2.0 - c_PocketRadius * Math.Sqrt(2)),
                new Coordinates(-c_Width / 2.0, c_Height / 2.0),
                new Coordinates(-c_Width / 2.0 + c_PocketRadius * Math.Sqrt(2), c_Height / 2.0),
                new Coordinates(-c_Width / 2.0 + c_PocketRadius * (1 + Math.Sqrt(2)), c_Height / 2.0 - c_PocketRadius),
                new Coordinates(-c_PocketRadius * (1 + Math.Tan(Math.PI / 8)), c_Height / 2.0 - c_PocketRadius),
                new Coordinates(-c_PocketRadius, c_Height / 2.0)
            };
            return bankPoints;
        }
        private bool IsCornerValidForCollision(int cornerIdx)
        {
            return
                cornerIdx == 02 || cornerIdx == 03 || cornerIdx == 07 ||
                cornerIdx == 09 || cornerIdx == 13 || cornerIdx == 14 ||
                cornerIdx == 18 || cornerIdx == 19 || cornerIdx == 23 ||
                cornerIdx == 25 || cornerIdx == 29 || cornerIdx == 30;
        }
        public void Draw(Graphics g)
        {
            //draw border
            g.FillRegion(c_BorderBrush, c_BorderRegion);
            //draw table
            g.FillRegion(c_TableBrush, c_TableRegion);
            //draw banks
            g.FillRegion(c_BankBrush, c_BankRegion);
            //draw pockets
            g.FillRegion(c_PocketBrush, c_PocketRegion);
        }
        public BankStepInfo ComputeStep(IBallInternal ball)
        {
            BankStepInfo res = new BankStepInfo(double.PositiveInfinity, Coordinates.Zero, Coordinates.Zero);
            foreach (var bank in c_Banks)
            {
                var tmp = ComponentManager.Cache.Get(bank, ball.Position, ball.Velocity, ball.Friction);
                if (tmp != null)
                {
                    res = new BankStepInfo(tmp.Step, tmp.Normal, tmp.Tangent);
                }
                else
                {
                    UpdateStep(ref res, bank, ball);
                    ComponentManager.Cache.Add(res.Step, res.NormalVersor, res.TangentVersor, bank, ball.Position, ball.Velocity, ball.Friction);
                }
            }
            foreach(var corner in c_BankCorners.Where((c,i) => IsCornerValidForCollision(i)))
            {
                var tmp = ComponentManager.Cache.Get(corner, ball.Position, ball.Velocity, ball.Friction);
                if (tmp != null)
                {
                    res = new BankStepInfo(tmp.Step, tmp.Normal, tmp.Tangent);
                }
                else
                {
                    UpdateStep(ref res, corner, ball);
                    ComponentManager.Cache.Add(res.Step, res.NormalVersor, res.TangentVersor, corner, ball.Position, ball.Velocity, ball.Friction);
                }
            }
            return res;
        }
        private void UpdateStep(ref BankStepInfo info, ICoordinates corner, IBallInternal ball)
        {
            double distance = double.PositiveInfinity;
            int N = -1;
            while (distance > ball.Radius)
            {
                N++;
                double newDistance = GeometryUtils.Distance(ball.ForeseenPosition(N), corner);
                if (newDistance >= distance)
                {
                    return;
                }
                else
                {
                    distance = newDistance;
                }
            }
            N--;
            var p = ball.ForeseenPosition(N);
            var v = ball.ForeseenVelocity(N);
            double a = v.X * (corner.X - p.X) + v.Y * (corner.Y - p.Y);
            double c = Math.Pow(v.X, 2) + Math.Pow(v.Y, 2);
            double b = Math.Sqrt(Math.Pow(a, 2) - c * (Math.Pow(corner.X - p.X, 2) + Math.Pow(corner.Y - p.Y, 2) - Math.Pow(ball.Radius, 2))); 
            double t1 = (a + b) / c;
            double t2 = (a - b) / c;
            double t = (t1.Between(0, 1) ? (t2.Between(0, 1) ? Math.Min(t1, t2) : t1) : (t2.Between(0, 1) ? t2 : double.PositiveInfinity));
            var ballCenter = Coordinates.Add(p, t * v);
            var contactPoint = Coordinates.Add(ballCenter, Coordinates.Sub(corner, ballCenter));
            if (!CheckCenterPoint(ballCenter, ball.Radius) || !CheckContactPoint(contactPoint))
            {
                return;
            }
            double step = N + t;
            if (!step.ToleranceEqual(0) && step.ToleranceLessEqual(info.Step))
            {
                if (step < info.Step)
                {
                    var normal = Coordinates.Normalize(Coordinates.Sub(p, corner));
                    var tangent = new Coordinates(module: 1, phase: normal.Phase + 90);
                    info = new BankStepInfo(step, normal, tangent);
                }
            }
        }
        private void UpdateStep(ref BankStepInfo info, Line2 bank, IBallInternal ball)
        {
            double distance = double.PositiveInfinity;
            int N = -1;
            while (distance > ball.Radius)
            {
                N++;
                double newDistance = GeometryUtils.Distance(ball.ForeseenPosition(N), bank);
                if (newDistance >= distance)
                {
                    return;
                }
                else
                {
                    distance = newDistance;
                }
            }
            N--;
            var p = ball.ForeseenPosition(N);
            var v = ball.ForeseenVelocity(N);
            double a = bank.A * p.X + bank.B * p.Y + bank.C;
            double b = ball.Radius * Math.Sqrt(Math.Pow(bank.A, 2) + Math.Pow(bank.B, 2));
            double c = bank.A * v.X + bank.B * v.Y;
            double t1 = (-a + b) / c;
            double t2 = (-a - b) / c;
            double t = (t1.Between(0, 1) ? (t2.Between(0, 1) ? Math.Min(t1, t2) : t1) : (t2.Between(0, 1) ? t2 : double.PositiveInfinity));
            var ballCenter = Coordinates.Add(p, t * v);
            var contactPoint = Coordinates.Add(ballCenter, -ball.Radius * bank.NormalVersor);
            if (!CheckCenterPoint(ballCenter, ball.Radius) || !CheckContactPoint(contactPoint))
            {
                return;
            }
            double step = N + t;
            if (!step.ToleranceEqual(0) && step.ToleranceLessEqual(info.Step))
            {
                if (step < info.Step)
                {
                    info = new BankStepInfo(step, bank.NormalVersor, bank.Versor);
                }
            }
        }
        private bool CheckCenterPoint(ICoordinates centerPoint, double radius)
        {
            return Math.Abs(centerPoint.X).Between(c_Width / 2.0 - c_PocketRadius - radius, c_Width / 2.0 - radius) || Math.Abs(centerPoint.Y).Between(c_Height / 2.0 - c_PocketRadius - radius, c_Height / 2.0 - radius);
        }
        private bool CheckContactPoint(ICoordinates contactPoint)
        {
            double absX = Math.Abs(contactPoint.X);
            double absY = Math.Abs(contactPoint.Y);
            if (absY.ToleranceEqual(c_Height / 2.0))
            {
                return absX.Between(c_BankCorners[0].X, c_BankCorners[1].X) || absX.Between(c_BankCorners[4].X, c_BankCorners[5].X);
            }
            else if (absY.ToleranceEqual(c_Height / 2.0 - c_PocketRadius))
            {
                return absX.Between(c_BankCorners[2].X, c_BankCorners[3].X);
            }
            else if (absX.ToleranceEqual(c_Width / 2.0))
            {
                return absY.Between(c_BankCorners[6].Y, c_BankCorners[5].Y);
            }
            else if (absX.ToleranceEqual(c_Width / 2.0 - c_PocketRadius))
            {
                return absY.Between(c_BankCorners[8].Y, c_BankCorners[7].Y);
            }
            else if (absY.Between(c_Height / 2.0 - c_PocketRadius, c_Height / 2.0))
            {
                return absX.Between(c_BankCorners[1].X, c_BankCorners[2].X) || absX.Between(c_BankCorners[3].X, c_BankCorners[4].X);
            }
            else if (absX.Between(c_Width / 2.0 - c_PocketRadius, c_Width / 2.0))
            {
                return absY.Between(c_BankCorners[7].Y, c_BankCorners[6].Y);
            }
            else
            {
                return false;
            }
        }
        public void InGameCheck(IBallInternal ball)
        {
            foreach (var pocket in c_Pockets)
            {
                ball.SetInGameFlag(ball.InGame & Coordinates.Sub(ball.Position, pocket).Module > c_PocketRadius);
            }
        }

        #endregion
    }
}
