using PoolAI.SDK.Data;
using PoolAI.SDK.Geometry;
using PoolAI.SDK.Tables;
using System;
using System.Drawing;

namespace PoolAI.SDK.Balls
{
    internal sealed class Ball : IBallInternal
    {
        #region Constants
    
        private const double c_ImpulseTime = 0.005;
        private const int c_Prime = 73;

        #endregion

        #region Fields

        private IBallInternal m_BallInterface;
        private BallGraphicsData m_GraphicsData;
        private BallPhysicsData m_PhysicsData;
        private Coordinates m_Position;
        private Coordinates m_Velocity;
        private ITableInternal m_Table;
        private readonly Coordinates m_DrawingOffset;
        private readonly Coordinates m_BallDrawingOffset;
        private readonly Coordinates m_StripedDrawingOffset;
        private readonly int m_StripedRadius;
        private readonly int m_StripedDiameter;
        private BankStepInfo m_BankStep;
        private bool m_DirectionUpdated;
        private bool m_InGame;

        #endregion

        #region Properties

        public bool InGame
        {
            get { return m_InGame; }
            private set
            {
                if (m_InGame != value)
                {
                    m_InGame = value;
                    if (!m_InGame)
                    {
                        RemovedFromGame?.Invoke();
                    }
                }
            }
        }
        public bool HasHitSomething { get; private set; }
        int IBallInternal.Radius { get { return m_PhysicsData.Radius; } }
        public ICoordinates Position { get { return m_Position; } }
        Coordinates IBallInternal.Velocity { get { return m_Velocity; } }
        double IBallInternal.BankDistance { get { return m_BankStep?.Step ?? double.PositiveInfinity; } }
        double IBallInternal.Mass { get { return m_PhysicsData.Mass; } }
        double IBallInternal.Elasticity { get { return m_PhysicsData.Elasticity; } }
        bool IBallInternal.DirectionUpdated { get { return m_DirectionUpdated; } }
        double IBallInternal.Friction { get { return m_PhysicsData.FrictionCoefficient; } }

        #endregion

        #region Constructor

        public Ball(BallGraphicsData graphicsData, BallPhysicsData physicsData, ITableInternal table, Coordinates startPosition)
        {
            m_GraphicsData = graphicsData;
            m_PhysicsData = physicsData;
            m_Table = table;
            m_BallInterface = this;
            m_InGame = true;
            m_Position = startPosition;
            m_Velocity = Coordinates.Zero;
            m_DrawingOffset = new Coordinates(m_Table.Width / 2, m_Table.Height / 2);
            Coordinates.MakeReadonly(m_DrawingOffset);            
            m_BallDrawingOffset = new Coordinates(m_PhysicsData.Radius, m_PhysicsData.Radius);
            Coordinates.MakeReadonly(m_BallDrawingOffset);
            m_StripedRadius = (int)Math.Ceiling(m_PhysicsData.Radius * m_GraphicsData.StripedRadiusCoefficient);
            m_StripedDiameter = 2 * m_StripedRadius;
            m_StripedDrawingOffset = new Coordinates(m_StripedRadius, m_StripedRadius);
            Coordinates.MakeReadonly(m_StripedDrawingOffset);
            m_DirectionUpdated = false;
        }

        #endregion

        #region Methods

        void IBallInternal.Draw(Graphics g)
        {
            if (m_InGame)
            {
                Coordinates tmp = Coordinates.Add(m_Position, m_DrawingOffset);
                Coordinates position = Coordinates.Sub(tmp, m_BallDrawingOffset);
                Coordinates stripedPosition = Coordinates.Sub(tmp, m_StripedDrawingOffset);
                Coordinates labelPosition = Coordinates.Sub(tmp, m_GraphicsData.LabelOffset);
                g.FillEllipse(m_GraphicsData.Brush, (float)position.X, (float)position.Y, m_PhysicsData.Diameter, m_PhysicsData.Diameter);
                if (m_GraphicsData.Striped)
                {
                    g.FillEllipse(Brushes.White, (float)stripedPosition.X, (float)stripedPosition.Y, m_StripedDiameter, m_StripedDiameter);
                }
                if (!string.IsNullOrEmpty(m_GraphicsData.Label))
                {
                    g.DrawString(m_GraphicsData.Label, SystemFonts.DefaultFont, Brushes.Black, (float)labelPosition.X, (float)labelPosition.Y);
                }
            }
        }
        void IBallInternal.Hit(double force, double direction)
        {
            SetVelocity(new Coordinates(module: force * c_ImpulseTime, phase: direction), true);
            HasHitSomething = false;
        }
        void IBallInternal.UpdatePosition(double step)
        {
            if (m_InGame && m_Velocity.Module > 0)
            {
                //update position
                m_Position = Coordinates.Add(m_Position, m_Velocity * step);
                //update bank distance
                m_BankStep.Update(step);
                //update velocity
                SetVelocity(Coordinates.Sub(m_Velocity, Coordinates.Normalize(m_Velocity) * m_PhysicsData.FrictionCoefficient * step), false);
                //handle collisions
                if (m_BankStep.Step.ToleranceEqual(0))
                {
                    HandleBankCollision();
                }
                m_Table.InGameCheck(this);
            }
        }
        private void HandleBankCollision()
        {
            //compute normal and tangent velocity components
            double nVel = Coordinates.Dot(m_Velocity, m_BankStep.NormalVersor) * m_Table.BankBouncingFrictionCoefficient;
            double tVel = Coordinates.Dot(m_Velocity, m_BankStep.TangentVersor) * m_Table.BankSlidingFrictionCoefficient;
            //compute final velocity
            SetVelocity(Coordinates.Add(m_BankStep.NormalVersor * (-nVel), m_BankStep.TangentVersor * tVel), true);
        }
        private void SetVelocity(Coordinates newVelocity, bool calcStep)
        {
            if (newVelocity.Module < m_PhysicsData.FrictionCoefficient)
            {
                m_Velocity = Coordinates.Zero;
            }
            else
            {
                m_DirectionUpdated = !(m_Velocity.Phase % 360.0).ToleranceEqual(newVelocity.Phase % 360.0);
                m_Velocity = newVelocity;
            }
            if (calcStep && m_Velocity.Module > 0)
            {
                m_BankStep = m_Table.ComputeStep(this);
            }
        }
        void IBallInternal.SetVelocity(Coordinates newVelocity)
        {
            SetVelocity(newVelocity, true);
        }
        Coordinates IBallInternal.ForeseenPosition(int N)
        {
            N = Math.Min(N, (int)Math.Ceiling(m_BallInterface.Velocity.Module / m_PhysicsData.FrictionCoefficient));
            return Coordinates.Add(m_BallInterface.Position, Coordinates.Sub(N * m_BallInterface.Velocity, N * (N - 1) / 2 * m_PhysicsData.FrictionCoefficient * Coordinates.Normalize(m_BallInterface.Velocity)));
        }
        Coordinates IBallInternal.ForeseenVelocity(int N)
        {
            N = Math.Min(N, (int)Math.Ceiling(m_BallInterface.Velocity.Module / m_PhysicsData.FrictionCoefficient));
            return Coordinates.Sub(m_BallInterface.Velocity, N * m_PhysicsData.FrictionCoefficient * Coordinates.Normalize(m_BallInterface.Velocity));
        }
        void IBallInternal.SetInGameFlag(bool value)
        {
            InGame = value;
        }
        void IBallInternal.SetHitSomethingFlag(bool value)
        {
            HasHitSomething = value;
        }

        #endregion

        #region Events

        public event Action RemovedFromGame;

        #endregion
    }
}
