using System;
using System.Collections.Generic;
using System.Linq;

namespace PoolAI.SDK.Geometry
{
    internal class PointsAtDistanceSolution
    {
        #region Properties

        public SolutionType Type { get; private set; }
        public IReadOnlyList<Tuple<Coordinates, Coordinates>> Points { get; private set; }

        #endregion

        #region Constructor

        public PointsAtDistanceSolution(SolutionType type, IEnumerable<Tuple<Coordinates, Coordinates>> points = null)
        {
            Type = type;
            if (type != SolutionType.Any && type != SolutionType.None)
            {
                Points = points?.Select(t => new Tuple<Coordinates, Coordinates>(Coordinates.MakeReadonly(t.Item1), Coordinates.MakeReadonly(t.Item2))).ToList();
            }
        }

        #endregion
    }
}
