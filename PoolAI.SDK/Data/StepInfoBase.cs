namespace PoolAI.SDK.Data
{
    internal abstract class StepInfoBase
    {
        #region Properties

        public double Step { get; private set; }

        #endregion

        #region Constructor

        public StepInfoBase(double step)
        {
            Step = step;
        }

        #endregion

        #region Methods

        public void Update(double performed)
        {
            Step -= performed;
        }

        #endregion
    }
}
