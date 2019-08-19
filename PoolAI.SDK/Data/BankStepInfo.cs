using PoolAI.SDK.Geometry;

namespace PoolAI.SDK.Data
{
    internal sealed class BankStepInfo : StepInfoBase
    {
        #region Properties

        public Coordinates NormalVersor { get; private set; } 
        public Coordinates TangentVersor { get; private set; }

        #endregion

        #region Constructor

        public BankStepInfo(double step, Coordinates normal, Coordinates tangent) : base(step)
        {
            NormalVersor = Coordinates.MakeReadonly(Coordinates.Normalize(normal));
            TangentVersor = Coordinates.MakeReadonly(Coordinates.Normalize(tangent));
        }

        #endregion

        #region Method

        public static BankStepInfo Clone(BankStepInfo toClone)
        {
            return new BankStepInfo(toClone.Step, toClone.NormalVersor, toClone.TangentVersor);
        }

        #endregion
    }
}
