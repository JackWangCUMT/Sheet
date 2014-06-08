using Sheet.Block.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sheet.Block.Model
{
    public class XText : XElement, IText
    {
        public XText(object element)
        {
            Native = element;
        }
    }
}
