using PoolAI.SDK.Data;
using PoolAI.SDK.Geometry;
using PoolAI.SDK.LinearAlgebra;
using PoolAI.SDK.Tables;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace PoolAI.SDK.Balls
{
    internal abstract class BallSetBase : IBallSetInternal
    {
        #region Data

        private class NewVelocity
        {
            private Coordinates m_Value;

            public bool IsValid { get; private set; }
            public Coordinates Value
            {
                get { return m_Value; }
                set
                {
                    m_Value = value;
                    IsValid = true;
                }
            }

            public NewVelocity(Coordinates initialValue)
            {
                m_Value = initialValue;
            }
        }

        #endregion

        #region Fields

        protected ITableInternal m_Table;
        protected List<IBallInternal> m_Balls;
        private Dictionary<IBallInternal, BallStepInfo> m_BallSteps;
        protected List<IBallInternal> m_CueBalls;

        #endregion

        #region Properties

        public IReadOnlyList<IBall> CueBalls { get { return m_CueBalls; } }
        public IReadOnlyList<IBall> Balls { get { return m_Balls; } }

        #endregion

        #region Constructor

        protected BallSetBase(ITableInternal table)
        {
            m_Table = table;
            m_Balls = new List<IBallInternal>();
            m_CueBalls = new List<IBallInternal>();
            m_BallSteps = new Dictionary<IBallInternal, BallStepInfo>();
        }

        #endregion

        #region Methods

        public void Draw(Graphics g)
        {
            foreach (IBallInternal b in m_Balls)
            {
                b.Draw(g);
            }
        }
        protected IBall AddBall(BallGraphicsData graphicsData, BallPhysicsData physicsData, Coordinates startPosition)
        {
            Ball b = new Ball(graphicsData, physicsData, m_Table, startPosition);
            m_Balls.Add(b);
            ComputeSteps();
            return b;
        }
        public void Update()
        {
            if (m_Balls.Any(b => b.InGame && b.Velocity.Module > 0))
            {
                var newVelocities = new Dictionary<IBallInternal, NewVelocity>();
                double performed = 0;
                while (performed < 1)
                {
                    newVelocities.Clear();
                    var validBalls = m_Balls.Where(b => b.InGame);
                    double step = Math.Min(1 - performed, validBalls.Where(b => b.Velocity.Module > 0).Select(b => b?.BankDistance).Min() ?? double.PositiveInfinity);
                    step = Math.Min(step, m_BallSteps.Where(kvp => kvp.Key.InGame).Min(kvp => kvp.Value.Step));
                    foreach (IBallInternal b in validBalls)
                    {
                        if (m_BallSteps.ContainsKey(b))
                        {
                            m_BallSteps[b].Update(step);
                        }
                        b.UpdatePosition(step);
                        newVelocities[b] = new NewVelocity(b.Velocity);
                    }
                    performed += step;
                    foreach (var info in m_BallSteps.Where(kvp => kvp.Key.InGame).Where(kvp => kvp.Value.Step.ToleranceEqual(0)))
                    {
                        HandleBallCollision(info.Key, info.Value, newVelocities);
                    }
                    foreach (var data in newVelocities.Where(kvp => kvp.Value.IsValid))
                    {
                        data.Key.SetVelocity(data.Value.Value);
                    }
                    if (newVelocities.Any(kvp => kvp.Value.IsValid) || validBalls.Any(b => b.DirectionUpdated))
                    {
                        ComputeSteps();
                    }
                }
            }
        }
        public void HitBall(ShotData shot)
        {
            (shot.Ball as IBallInternal).Hit(shot.Force, shot.Direction);
            ComputeSteps();
        }
        private void ComputeSteps()
        {
            var validBalls = m_Balls.Where(b => b.InGame).ToList();
            m_BallSteps.Clear();
            for (int i = 0; i < validBalls.Count; i++)
            {
                var info = new BallStepInfo(double.PositiveInfinity);
                for (int j = i + 1; j < validBalls.Count; j++)
                {
                    CheckBallCollision(ref info, validBalls[i], validBalls[j]);
                }
                m_BallSteps[validBalls[i]] = info;
            }
        }
        private void HandleBallCollision(IBallInternal ball, BallStepInfo info, Dictionary<IBallInternal, NewVelocity> newVelocities)
        {
            var validBalls = info.CollidedBalls.Where(b => b.InGame);
            //prepare linear system wich solutions gives the impulses
            Matrix A = new Matrix(validBalls.Count());
            Vector B = new Vector(validBalls.Count());
            foreach (var iBall in validBalls.Select((Value, Idx) => new { Value, Idx }))
            {
                var iNorm = info.GetNormalVersor(iBall.Value);
                //build matrix A
                foreach (var jBall in Enumerable.Where(validBalls.Select((Value, Idx) => new { Value, Idx }), e => e.Idx <= iBall.Idx))
                {
                    if (iBall.Idx == jBall.Idx)
                    {
                        A[iBall.Idx, iBall.Idx] = (1 / ball.Mass + 1 / iBall.Value.Mass);
                    }
                    else
                    {
                        var jNorm = info.GetNormalVersor(jBall.Value);
                        A[iBall.Idx, jBall.Idx] = Coordinates.Dot(iNorm, jNorm) / ball.Mass;
                        A[jBall.Idx, iBall.Idx] = A[iBall.Idx, jBall.Idx];
                    }
                }
                //build known terms vector
                B[iBall.Idx] = (1 + Math.Min(iBall.Value.Elasticity, ball.Elasticity)) * Coordinates.Dot(Coordinates.Sub(iBall.Value.Velocity, ball.Velocity), iNorm);
            }
            //solve the system
            var x = LinearSystem.Solve(A, B);
            //apply the impulses
            foreach (var iBall in validBalls.Select((Value, Idx) => new { Value, Idx }))
            {
                var iNorm = info.GetNormalVersor(iBall.Value);
                newVelocities[ball].Value = Coordinates.Add(newVelocities[ball].Value, x[iBall.Idx] / ball.Mass * iNorm);
                newVelocities[iBall.Value].Value = Coordinates.Sub(newVelocities[iBall.Value].Value, x[iBall.Idx] / iBall.Value.Mass * iNorm);
                iBall.Value.SetHitSomethingFlag(true);
            }
            ball.SetHitSomethingFlag(validBalls.Count() != 0);
        }
        private void CheckBallCollision(ref BallStepInfo info, IBallInternal first, IBallInternal second)
        {
            if (first.Velocity.Module != 0 || second.Velocity.Module != 0)
            {
                int N = 0;
                double distance;
                double nextDistance;
                double step;
                Coordinates normal;
                var tmp = ComponentManager.Cache.Get(first.Position, first.Velocity, first.Friction, second.Position, second.Velocity, second.Friction);
                if (tmp != null)
                {
                    int iSteps = (int)Math.Floor(tmp.Step);
                    distance = GeometryUtils.Distance(first.ForeseenPosition(iSteps + 1), second.ForeseenPosition(iSteps + 1));
                    nextDistance = GeometryUtils.Distance(first.ForeseenPosition(iSteps + 2), second.ForeseenPosition(iSteps + 2));
                    step = tmp.Step;
                    normal = tmp.Normal;
                }
                else
                {
                    nextDistance = GeometryUtils.Distance(first.ForeseenPosition(N), second.ForeseenPosition(N));
                    do
                    {
                        distance = nextDistance;
                        nextDistance = GeometryUtils.Distance(first.ForeseenPosition(N + 1), second.ForeseenPosition(N + 1));
                        N++;
                    }
                    while (distance > nextDistance && distance.ToleranceGreater(first.Radius + second.Radius));
                    N -= 2;
                    if (N < 0)
                    {
                        step = 0;
                        normal = Coordinates.Normalize(Coordinates.Sub(second.Position, first.Position));
                    }
                    else
                    {
                        var p1 = first.ForeseenPosition(N);
                        var p2 = second.ForeseenPosition(N);
                        var v1 = first.ForeseenVelocity(N);
                        var v2 = second.ForeseenVelocity(N);
                        var deltaP = Coordinates.Sub(p1, p2);
                        var deltaV = Coordinates.Sub(v1, v2);
                        var a = Coordinates.Dot(deltaV, deltaV);
                        var b = 2 * Coordinates.Dot(deltaP, deltaV);
                        var c = Coordinates.Dot(deltaP, deltaP) - Math.Pow(first.Radius + second.Radius, 2);
                        var t1 = (-b + Math.Sqrt(b * b - 4 * a * c)) / (2 * a);
                        var t2 = (-b - Math.Sqrt(b * b - 4 * a * c)) / (2 * a);
                        double t = (t1.ToleranceGreaterEqual(0) && t2.ToleranceGreaterEqual(0)) ? Math.Min(t1, t2) : (t1.ToleranceGreaterEqual(0) ? t1 : (t2.ToleranceGreaterEqual(0) ? t2 : 0));
                        step = N + t;
                        normal = Coordinates.Normalize(Coordinates.Sub(Coordinates.Add(p2, t * v2), (Coordinates.Add(p1, t * v1))));
                    }
                    ComponentManager.Cache.Add(step, normal, Coordinates.Zero, first.Position, first.Velocity, first.Friction, second.Position, second.Velocity, second.Friction);
                }
                if (distance.ToleranceLessEqual(first.Radius + second.Radius) && ((step.ToleranceGreater(0) && step.ToleranceLessEqual(info.Step)) || (step.ToleranceEqual(0) && distance > nextDistance)))
                {
                    if (!step.ToleranceEqual(info.Step))
                    {
                        info = new BallStepInfo(step);
                    }
                    info.AddCollidedBall(second, normal);
                }
            }
        }

        #endregion
    }
}
