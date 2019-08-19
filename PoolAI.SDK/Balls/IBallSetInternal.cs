using PoolAI.SDK.Data;
using System;
using System.Drawing;

namespace PoolAI.SDK.Balls
{
    internal interface IBallSetInternal : IBallSet
    {
        #region Methods

        void Draw(Graphics g);
        void Update();
        void HitBall(ShotData shot);

        #endregion
    }
}
