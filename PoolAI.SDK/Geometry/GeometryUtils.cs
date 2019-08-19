using System;
using System.Collections.Generic;

namespace PoolAI.SDK.Geometry
{
    public static class GeometryUtils
    {
        #region Methods

        public static double Distance(ICoordinates point1, ICoordinates point2)
        {
            return Coordinates.Sub(point2, point1).Module;
        }
        internal static double Distance(Coordinates point, Line2 line)
        {
            return Math.Abs(point.X * line.A + point.Y * line.B + line.C) / Math.Sqrt(Math.Pow(line.A, 2) + Math.Pow(line.B, 2));
        }
        internal static double Distance(Line2 line1, Line2 line2)
        {
            if (line1.M != line2.M)
            {
                return 0;
            }
            else
            {
                return Math.Abs(line1.Q - line2.Q);
            }
        }
        internal static bool IsParallel(Coordinates first, Coordinates second)
        {
            return first.Phase == second.Phase || first.Phase == (second.Phase + 180);
        }
        internal static bool IsParallel(Line2 first, Line2 second)
        {
            return first.M == second.M;
        }
        internal static PointsAtDistanceSolution GetPointsAtDistance(Line2 first, Coordinates second, double distance)
        {
            //check that line is not too far
            if (Distance(second, first) > distance)
            {
                return new PointsAtDistanceSolution(SolutionType.None);
            }
            else
            {
                //check if point belong to line
                if (first.CheckPoint(second))
                {
                    return new PointsAtDistanceSolution(SolutionType.Specific, new List<Tuple<Coordinates, Coordinates>>() { new Tuple<Coordinates, Coordinates>(Coordinates.Add(second, first.Versor * distance), second), new Tuple<Coordinates, Coordinates>(Coordinates.Sub(second, first.Versor * distance), second) });
                }
                else
                {
                    Coordinates p1 = Coordinates.Zero;
                    Coordinates p2 = Coordinates.Zero;
                    if (first.B == 0)
                    {
                        double T1 = -first.C / first.A - second.X;
                        double T2 = Math.Sqrt((distance - T1) * (distance + T1));
                        p1.X = -first.C / first.A;
                        p1.Y = second.Y + T2;
                        p2.X = -first.C / first.A;
                        p2.Y = second.Y - T2;
                    }
                    else
                    {
                        double T1 = second.X + first.M * (second.Y - first.Q);
                        double T2 = 1 + Math.Pow(first.M, 2);
                        double T3 = Math.Sqrt(T2 * Math.Pow(distance, 2) - Math.Pow(first.M * second.X - second.Y + first.Q, 2));
                        p1.X = (T1 + T3) / T2;
                        p1.Y = p1.X * first.M + first.Q;
                        p2.X = (T1 - T3) / T2;
                        p2.Y = p2.X * first.M + first.Q;
                    }
                    return new PointsAtDistanceSolution(SolutionType.Specific, new List<Tuple<Coordinates, Coordinates>>() { new Tuple<Coordinates, Coordinates>(p1, second), new Tuple<Coordinates, Coordinates>(p2, second) });
                }
            }
        }
        internal static PointsAtDistanceSolution GetPointsAtDistance(Line2 first, Line2 second, double distance)
        {
            //check if parallel lines
            if (IsParallel(first, second))
            {
                //only two possible solutions:
                //  - parallel lines are at the correct distance => any points is a solution
                //  - parallel lines are not at the correct distance => no solutions
                if (Distance(first, second) == distance)
                {
                    return new PointsAtDistanceSolution(SolutionType.Any);
                }
                else
                {
                    return new PointsAtDistanceSolution(SolutionType.None);
                }
            }
            else
            {
                var results = new List<Tuple<Coordinates, Coordinates>>();
                Coordinates p11 = Coordinates.Zero;
                Coordinates p12 = Coordinates.Zero;
                Coordinates p21 = Coordinates.Zero;
                Coordinates p22 = Coordinates.Zero;
                if (first.B == 0)
                {
                    double T1 = -first.C / first.A * second.M + second.Q;
                    double T2 = Math.Sqrt(1 + Math.Pow(second.M, 2));
                    p11.X = -first.C / first.A;
                    p11.Y = T1 - distance * T2;
                    p12.X = -first.C / first.A;
                    p12.Y = T1 + distance * T2;
                    p21.X = (-second.M * second.Q + second.M * p11.Y + p11.X) / Math.Pow(T2, 2);
                    p21.Y = second.M * p21.X + second.Q;
                    p22.X = (-second.M * second.Q + second.M * p12.Y + p12.X) / Math.Pow(T2, 2);
                    p22.Y = second.M * p22.X + second.Q;
                }
                else if (second.B == 0)
                {
                    p11.X = -second.C / second.A - distance;
                    p11.Y = first.M * p11.X + first.Q;
                    p12.X = -second.C / second.A + distance;
                    p12.Y = first.M * p12.X + first.Q;
                    p21.X = -second.C / second.A;
                    p21.Y = p11.Y;
                    p22.X = -second.C / second.A;
                    p22.Y = p12.Y;
                }
                else
                {
                    double T1 = second.Q - first.Q;
                    double T2 = second.M - first.M;
                    double T3 = Math.Sqrt(1 + Math.Pow(second.M, 2));
                    p11.X = (-T1 + distance * T3) / T2;
                    p11.Y = first.M * p11.X + first.Q;
                    p12.X = (-T1 - distance * T3) / T2;
                    p12.Y = first.M * p12.X + first.Q;
                    p21.X = (-second.M * second.Q + second.M * p11.Y + p11.X) / Math.Pow(T3, 2);
                    p21.Y = second.M * p21.X + second.Q;
                    p22.X = (-second.M * second.Q + second.M * p12.Y + p12.X) / Math.Pow(T3, 2);
                    p22.Y = second.M * p22.X + second.Q;
                }
                return new PointsAtDistanceSolution(SolutionType.Specific, new List<Tuple<Coordinates, Coordinates>>() { new Tuple<Coordinates, Coordinates>(p11, p21), new Tuple<Coordinates, Coordinates>(p12, p22) });
            }
        }

        #endregion
    }
}
