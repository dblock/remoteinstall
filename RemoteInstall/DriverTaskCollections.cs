using System;
using System.Collections.Generic;
using System.Text;

namespace RemoteInstall.DriverTasks
{
    /// <summary>
    /// A set of collections of driver task collections.
    /// </summary>
    public class DriverTaskCollections : List<DriverTaskCollection>
    {        
        public DriverTaskCollections()
        {

        }

        /// <summary>
        /// Returns true if there's any overlap of dependencies in the collection.
        /// </summary>
        /// <param name="coll">Collection of tasks</param>
        /// <returns></returns>
        public bool Overlaps(DriverTaskCollection coll)
        {
            foreach (DriverTaskCollection thisColl in this)
            {
                if (thisColl.Overlaps(coll))
                    return true;
            }

            return false;
        }
    }
}
