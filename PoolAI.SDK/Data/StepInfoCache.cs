using PoolAI.SDK.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PoolAI.SDK.Data
{
    internal sealed class StepInfoCache
    {
        #region Data

        private sealed class StepInfoCacheRecord : IStepInfoCacheRecord
        {
            #region Constants

            private const int c_StartAge = 3;

            #endregion

            #region Properties

            public object[] Keys { get; private set; }
            public double Step { get; private set; }
            public Coordinates Normal { get; private set; }
            public Coordinates Tangent { get; private set; }
            public int Age { get; private set; }
            public StepInfoCacheRecord Next { get; set; }
            public StepInfoCacheRecord Previous { get; set; }

            #endregion

            #region Constructor

            public StepInfoCacheRecord(object[] keys, double step, Coordinates normal, Coordinates tangent)
            {
                Keys = keys;
                Step = step;
                Normal = Coordinates.MakeReadonly(new Coordinates(normal.X, normal.Y));
                Tangent = Coordinates.MakeReadonly(new Coordinates(tangent.X, tangent.Y));
                Age = c_StartAge;
            }

            #endregion

            #region Methods

            public void Refresh()
            {
                Age = Math.Max(0, Age - 1);
            }
            public void Hit()
            {
                Age = c_StartAge;
            }

            #endregion
        }

        #endregion

        #region Constants

        private const int c_BucketsNum = 4096;

        #endregion

        #region Fields

        private readonly List<StepInfoCacheRecord>[] m_Buckets;
        private StepInfoCacheRecord m_LastRecord;
        private readonly object m_Lock;

        #endregion

        #region Constructor

        public StepInfoCache()
        {
            m_Buckets = new List<StepInfoCacheRecord>[c_BucketsNum];
            for (int i = 0; i < c_BucketsNum; i++)
            {
                m_Buckets[i] = new List<StepInfoCacheRecord>();
            }
            m_LastRecord = null;
            m_Lock = new object();
        }

        #endregion

        #region Methods

        public void Add(double step, Coordinates normal, Coordinates tangent, params object[] key)
        {
            lock (m_Lock)
            {
                uint idx = Utility.GetHashCode(key) % c_BucketsNum;
                var record = new StepInfoCacheRecord(key, step, normal, tangent);
                m_Buckets[idx].Add(record);
                if (m_LastRecord != null)
                {
                    m_LastRecord.Next = record;
                    record.Previous = m_LastRecord;
                }
                m_LastRecord = record;
            }
        }
        public IStepInfoCacheRecord Get(params object[] key)
        {
            lock (m_Lock)
            {
                uint idx = Utility.GetHashCode(key) % c_BucketsNum;
                var bucket = m_Buckets[idx];
                foreach (var record in bucket)
                {
                    if (record != null && key.Length == record.Keys.Length && key.Zip(record.Keys, (f, s) => f.Equals(s)).All(v => v))
                    {
                        record.Hit();
                        return record;
                    }
                }
                return null;
            }
        }
        public void Refresh()
        {
            lock (m_Lock)
            {
                var current = m_LastRecord;
                while (current != null)
                {
                    current.Refresh();
                    if (current.Age == 0)
                    {
                        uint idx = Utility.GetHashCode(current.Keys) % c_BucketsNum;
                        m_Buckets[idx].Remove(current);
                        if (current.Previous != null)
                        {
                            current.Previous.Next = current.Next;
                        }
                        if (current.Next != null)
                        {
                            current.Next.Previous = current.Previous;
                        }
                        else
                        {
                            m_LastRecord = current.Previous;
                        }
                    }
                    current = current.Previous;
                }
            }
        }

        #endregion
    }
}
