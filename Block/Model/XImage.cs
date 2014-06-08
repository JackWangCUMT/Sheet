using Sheet.Block.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sheet.Block.Model
{
    public class XImage : XElement, IImage
    {
        public XImage(object element)
        {
            Native = element;
        }
    }
}
