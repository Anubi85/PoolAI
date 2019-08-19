using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PoolAI.SDK.Data
{
    internal sealed class BallPhysicsData
    {
        #region Properties

        public int Radius { get; private set; }
        public int Diameter { get; private set; }
        public double Mass { get; private set; }
        public double Elasticity { get; private set; }
        public double FrictionCoefficient { get; private set; }

        #endregion

        #region Constructor

        public BallPhysicsData(int radius, double mass, double elasticity, double frictionCoefficient)
        {
            Radius = radius;
            Diameter = 2 * radius;
            Mass = mass;
            Elasticity = elasticity;
            FrictionCoefficient = frictionCoefficient;
        }

        #endregion
    }
}
