using Sheet.Block.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sheet.Block
{
    public interface IPointController
    {
        void ConnectStart(IPoint point, ILine line);
        void ConnectEnd(IPoint point, ILine line);
        void UpdateDependencies(List<IBlock> blocks, List<IPoint> points, List<ILine> lines);
    }
}
