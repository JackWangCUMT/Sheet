using Sheet.Block.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sheet.Block.Model
{
    public class XLine : XElement, ILine
    {
        public int StartId { get; set; }
        public int EndId { get; set; }
        public IPoint Start { get; set; }
        public IPoint End { get; set; }
        public XLine(object element)
        {
            Native = element;
        }
        public XLine(object element, int startId, int endId)
        {
            StartId = startId;
            EndId = endId;
            Native = element;
        }
        public XLine(object element, IPoint start, IPoint end)
        {
            Start = start;
            End = end;
            Native = element;
        }
    }
}
