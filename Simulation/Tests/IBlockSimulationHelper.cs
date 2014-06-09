using Sheet.Block.Core;
using Sheet.Simulation.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sheet.Simulation.Tests
{
    public interface IBlockSimulationHelper
    {
        void SeState(ILine line, IBoolState state);
        void SeState(IRectangle rectangle, IBoolState state);
        void SeState(IEllipse ellipse, IBoolState state);
        void SeState(IText text, IBoolState state);
        void SeState(IImage image, IBoolState state);
        void SeState(IBlock parent, IBoolState state);
    }
}
