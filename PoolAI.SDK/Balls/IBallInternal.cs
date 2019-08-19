using PoolAI.SDK.Geometry;
using System;
using System.Drawing;

namespace PoolAI.SDK.Balls
{
    internal interface IBallInternal : IBall
    {
        #region Properties

        int Radius { get; }
        Coordinates Velocity { get; }
        bool DirectionUpdated { get; }
        double BankDistance { get; }
        double Mass { get; }
        double Elasticity { get; }
        double Friction { get; }

        #endregion

        #region Methods

        void Draw(Graphics g);
        void Hit(double force, double direction);
        void UpdatePosition(double step);
        void SetVelocity(Coordinates newVelocity);
        Coordinates ForeseenPosition(int N);
        Coordinates ForeseenVelocity(int N);
        void SetInGameFlag(bool value);
        void SetHitSomethingFlag(bool value);

        #endregion

        #region Events

        event Action RemovedFromGame;

        #endregion
    }
}
