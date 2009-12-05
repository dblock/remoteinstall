using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Setup
{
    class Program
    {
        static int Main(string[] args)
        {
            try
            {
                int rc = 0;
                Console.Write("Sample setup: rc=");
                if (args != null && args.Length == 1)
                {
                    rc = int.Parse(args[0]);                    
                }
                else if (args != null && args.Length > 1)
                {
                    throw new ArgumentException("args");
                }
                Console.WriteLine(rc.ToString());
                return rc;
            }
            catch (Exception ex)
            {
                Console.WriteLine("ERROR: {0}", ex.Message);
                return -1;
            }
        }
    }
}
