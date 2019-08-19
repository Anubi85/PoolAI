using PoolAI.SDK.Brain;
using System;
using System.Collections.Generic;
using System.Drawing;
using PoolAI.SDK.Data;
using PoolAI.SDK.Balls;
using System.Linq;

namespace Brain.AIBasic
{
    [ExportBrain("Basic AI", typeof(Brain), Population = c_Population)]
    class Brain : IBrain
    {
        #region Constants

        private const int c_Population = 50;
        private const double c_MaxForce = 2000.0;
        private const int c_ShotStep = 1;
        private const int c_GenerationStep = 4;
        private const int c_SelectedForNextGeneration = 7;
        private const double c_SingleMutationProbability = 0.75;
        private const double c_MutationRateMin = 0.05;
        private const double c_MutationRateMax = 0.85;
        private const int c_AvailableForMutation = 4;
        private const double c_BestBrainSelectionProbabilityFactor = 2.0;
        private const double c_SmallMutationFactor = 0.01;
        private const double c_LargeMutationFactor = 0.1;
        private const double c_kParam = 0.5;
        private const double c_aCoeff = c_GenerationStep > 2 ? c_kParam / ((c_GenerationStep - 2) * (c_GenerationStep - 2)) : 0.0;
        private const double c_bCoeff = c_GenerationStep > 2 ? -c_aCoeff * c_GenerationStep - c_kParam / (c_GenerationStep - 2) : -c_kParam;
        private const double c_cCoeff = c_GenerationStep > 2 ? c_kParam - c_aCoeff - c_bCoeff : 2 * c_kParam;
        //drawing
        private static readonly Font c_GenerationFont = new Font(SystemFonts.DefaultFont.FontFamily, 100.0f, FontStyle.Regular);
        private static readonly Font c_InfoFont = new Font(SystemFonts.DefaultFont.FontFamily, 10.0f, FontStyle.Regular);
        private static readonly Brush c_GenerationBrush = new SolidBrush(Color.FromArgb(64, Color.White));

        #endregion

        #region Fields

        private static int s_GenerationCounter;
        private static int s_MaxShots;
        private static readonly Random s_RandomGenerator;
        private static readonly List<Brain> s_Brains;
        private List<ShotData> m_Shots;
        private double m_ScoreFactor;
        private bool m_ScoreUpdated;

        #endregion

        #region Properties

        public double Score { get; private set; }
        public int ShotCount { get; private set; }

        #endregion

        #region Constructor

        static Brain()
        {
            s_GenerationCounter = 0;
            s_MaxShots = c_ShotStep;
            s_RandomGenerator = new Random();
            s_Brains = new List<Brain>();
        }
        public Brain() : this(new List<ShotData>())
        {
        }
        private Brain(IEnumerable<ShotData> shots)
        {
            m_Shots = new List<ShotData>(shots);
            ShotCount = 0;
            Score = 0;
            m_ScoreFactor = 1;
            m_ScoreUpdated = false;
        }

        #endregion

        #region Methods

        private static double GetRandom(double min, double max)
        {
            return s_RandomGenerator.NextDouble() * (max - min) + min;
        }
        private static double GetMutationRate(int idx)
        {
            return (c_MutationRateMax - c_MutationRateMin) / (c_AvailableForMutation) * idx + c_MutationRateMin;
        }
        private static int GetMaxMutations()
        {
            return (int)Math.Ceiling(Math.Pow(c_AvailableForMutation, (s_RandomGenerator.NextDouble() - c_SingleMutationProbability) / (1 - c_SingleMutationProbability)));
        }
        private static int GetRandomBreedableBrain()
        {
            double step = 1 / (c_BestBrainSelectionProbabilityFactor + c_SelectedForNextGeneration - 1);
            double probability = step * c_BestBrainSelectionProbabilityFactor;
            double random = s_RandomGenerator.NextDouble();
            int idx = 0;
            while (idx < c_SelectedForNextGeneration)
            {
                if (random < probability)
                {
                    break;
                }
                idx++;
                probability += step;
            }
            return idx;
        }
        private static int GetNumberOfRandomShots()
        {
            int idx = s_GenerationCounter % c_GenerationStep;
            if (idx != 0 && idx <= (c_GenerationStep - 1))
            {
                return (int)Math.Round(Math.Round(c_aCoeff * Math.Pow(idx, 2) + c_bCoeff * idx + c_cCoeff, 2) * c_Population);
            }
            else
            {
                return 0;
            }
        }
        [ExportEvolveFunction]
        private static IEnumerable<IBrain> Evolve(IEnumerable<IBrain> brains)
        {
            s_Brains.Clear();
            s_GenerationCounter++;
            //select individual that can reproduce
            var breedable = SelectForReproduction(brains.Cast<Brain>());
            //regenerate population from selected individuals
            var newBrains = Reproduce(breedable);
            //apply mutations
            Mutate(newBrains, breedable.Count());
            //increase max shots if needed
            if (s_GenerationCounter % c_GenerationStep == 0)
            {
                s_MaxShots += c_ShotStep;
            }
            //return the new brains population
            return newBrains;
        }
        private static IEnumerable<Brain> SelectForReproduction(IEnumerable<Brain> brains)
        {
            List<Brain> breedableBrains = brains.Where(b => b.Score > 0).GroupBy(b => b.Score).OrderByDescending(bg => bg.Key).Select(bg => bg.First()).Take(c_SelectedForNextGeneration).ToList();
            return breedableBrains;
        }
        private static IEnumerable<Brain> Reproduce(IEnumerable<Brain> selectedBrains)
        {
            List<Brain> newBrains = selectedBrains.Select(b => new Brain(b.m_Shots.Select(s => new ShotData(null) { Force = s.Force, Direction = s.Direction }))).ToList();
            while (newBrains.Count < c_Population)
            {
                int brainIdx = GetRandomBreedableBrain();
                if (brainIdx < selectedBrains.Count())
                {
                    newBrains.Add(new Brain(selectedBrains.ElementAt(brainIdx).m_Shots.Select(s => new ShotData(null) { Force = s.Force, Direction = s.Direction })));
                }
                else
                {
                    newBrains.Add(new Brain());
                }
            }
            return newBrains;
        }
        private static void Mutate(IEnumerable<Brain> brains, int toSkip)
        {
            var randomShotBrains = brains.Skip(toSkip).OrderBy(i => s_RandomGenerator.NextDouble()).Take(GetNumberOfRandomShots());
            //do not mutate best of previous generation
            foreach (Brain b in brains.Skip(toSkip))
            {
                if (randomShotBrains.Contains(b) && b.m_Shots.Count != 0)
                {
                    var lastShotIdx = b.m_Shots.Count - 1;
                    b.m_Shots[lastShotIdx].Force = GetRandom(lastShotIdx == 0 ? c_MaxForce / 2 : 0, c_MaxForce);
                    b.m_Shots[lastShotIdx].Direction = GetRandom(lastShotIdx == 0 ? -45 : -180, lastShotIdx == 0 ? 45 : 180);
                }
                else
                {
                    int availableMutations = GetMaxMutations();
                    //for each shot check if mutation need to be applied
                    for (int shotIdx = s_MaxShots - c_AvailableForMutation; shotIdx < b.m_Shots.Count && availableMutations > 0; shotIdx++)
                    {
                        if (shotIdx >= 0 && s_RandomGenerator.NextDouble() < GetMutationRate(shotIdx))
                        {
                            availableMutations--;
                            //choose what to mutate
                            bool mutateForce = s_RandomGenerator.Next(2) == 0;
                            double step = s_RandomGenerator.Next(5) == 0 ? c_LargeMutationFactor : c_SmallMutationFactor;
                            //perform mutation
                            if (mutateForce)
                            {
                                step *= c_MaxForce;
                                b.m_Shots[shotIdx].Force += GetRandom(-step, step);
                                b.m_Shots[shotIdx].Force = Math.Min(c_MaxForce, Math.Max(0, b.m_Shots[shotIdx].Force));
                            }
                            else
                            {
                                step *= 360.0;
                                b.m_Shots[shotIdx].Direction += GetRandom(-step, step);
                                b.m_Shots[shotIdx].Direction %= 360.0;
                            }
                        }
                    }
                }
            }
        }
        [ExportDrawOverlayFunction]
        private static void DrawOverlay(Graphics g)
        {
            string gen = (s_GenerationCounter + 1).ToString();
            SizeF genStrSize = g.MeasureString(gen, c_GenerationFont);
            g.DrawString(gen, c_GenerationFont, c_GenerationBrush, (g.ClipBounds.Width - genStrSize.Width) / 2, (g.ClipBounds.Height - genStrSize.Height) / 2);
        }
        public void Initialize(IBall cueBall)
        {
            ShotCount = 0;
            for (int i = 0; i < s_MaxShots; i++)
            {
                if (i < m_Shots.Count)
                {
                    m_Shots[i] = new ShotData(cueBall) { Force = m_Shots[i].Force, Direction = m_Shots[i].Direction };
                }
                else
                {
                    m_Shots.Add(new ShotData(cueBall) { Force = GetRandom(i == 0 ? c_MaxForce / 2 : 0, c_MaxForce), Direction = GetRandom(i == 0 ? -45 : -180, i == 0 ? 45 : 180) });
                }
            }
            s_Brains.Add(this);
        }
        public void RequestShotData()
        {
            if (ShotCount < s_MaxShots)
            {
                ShotDataReady?.Invoke(m_Shots[ShotCount++]);
                m_ScoreUpdated = false;
            }
            else
            {
                EvolutionRequest?.Invoke();
            }
        }
        public void UpdateScore(double score)
        {
            if (!m_ScoreUpdated)
            {
                if (score == 0)
                {
                    m_ScoreFactor *= 0.75;
                    Score *= 0.75;
                }
                else
                {
                    Score = score * m_ScoreFactor;
                }
                m_ScoreUpdated = true;
            }
        }
        [ExportMaxShotFunction]
        private static int GetMaxShot()
        {
            return s_MaxShots;
        }

        #endregion

        #region Events

        public event Action<ShotData> ShotDataReady;
        public event Action EvolutionRequest;

        #endregion
    }
}
