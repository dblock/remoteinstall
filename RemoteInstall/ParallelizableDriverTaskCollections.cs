using System;
using System.Collections.Generic;
using System.Text;

namespace RemoteInstall.DriverTasks
{
    /// <summary>
    /// A parallelizable list of driver task collections.
    /// Each entry contains dependent collections of tasks.
    /// </summary>
    public class ParallelizableRemoteInstallDriverTaskCollections : IEnumerable<DriverTaskCollections>
    {
        private List<DriverTaskCollections> _collections = new List<DriverTaskCollections>();
        /// <summary>
        /// Add a collection into the right task bucket.
        /// </summary>
        /// <param name="coll">Collection to add.</param>
        /// <returns>True if this was an independent collection.</returns>
        public bool Add(DriverTaskCollection coll)
        {
            foreach (DriverTaskCollections collections in _collections)
            {
                if (collections.Overlaps(coll))
                {
                    collections.Add(coll);
                    return false;
                }
            }

            DriverTaskCollections newCollection = new DriverTaskCollections();
            newCollection.Add(coll);
            _collections.Add(newCollection);
            return true;
        }

        /// <summary>
        /// Number of parallelizable tasks.
        /// </summary>
        /// <returns>Number of parallelizable tasks.</returns>
        public int Count
        {
            get
            {
                return _collections.Count;
            }
        }

        #region IEnumerable<RemoteInstallDriverTaskCollections> Members

        public IEnumerator<DriverTaskCollections> GetEnumerator()
        {
            return _collections.GetEnumerator();
        }

        #endregion

        #region IEnumerable Members

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return _collections.GetEnumerator();
        }

        #endregion
    }
}
