using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sheet.Simulation.Core
{
    public class Clock : IClock
    {
        #region Constructor

        public Clock()
            : base()
        {
        }

        public Clock(long cycle, int resolution)
            : this()
        {
            this.Cycle = cycle;
            this.Resolution = resolution;
        }

        #endregion

        #region Properties

        public long Cycle { get; set; }
        public int Resolution { get; set; }

        #endregion
    }
}
