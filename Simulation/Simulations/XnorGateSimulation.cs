using Sheet.Simulation.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sheet.Simulation.Simulations
{
    public class XnorGateSimulation : ISimulation
    {
        #region Constructor

        public XnorGateSimulation()
            : base()
        {
            this.InitialState = null;
        }

        #endregion

        #region ISimulation

        public Element Element { get; set; }

        public IClock Clock { get; set; }

        public IBoolState State { get; set; }
        public bool? InitialState { get; set; }
        public Tuple<IBoolState, bool>[] StatesCache { get; set; }
        public bool HaveCache { get; set; }

        public Element[] DependsOn { get; set; }

        public void Compile()
        {
            throw new NotImplementedException();
        }

        public void Calculate()
        {
            throw new NotImplementedException();
        }

        public void Reset()
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
