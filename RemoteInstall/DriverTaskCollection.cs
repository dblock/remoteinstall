using System;
using System.Collections.Generic;
using System.Text;

namespace RemoteInstall.DriverTasks
{
    /// <summary>
    /// A collection of driver tasks.
    /// </summary>
    public class DriverTaskCollection : List<DriverTask>
    {
        public DriverTaskCollection()
        {

        }

        /// <summary>
        /// Returns true if the task collection overlaps another task collection.
        /// </summary>
        /// <param name="coll">Task collection.</param>
        /// <returns>True if there's an overlap.</returns>
        public bool Overlaps(DriverTaskCollection coll)
        {
            foreach (DriverTask collTask in coll)
            {
                foreach (DriverTask thisTask in this)
                {
                    if (thisTask.Overlaps(collTask))
                        return true;
                }
            }

            return false;
        }
    }
}
