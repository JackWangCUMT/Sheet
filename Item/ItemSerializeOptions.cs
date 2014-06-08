using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sheet.Item
{
    public class ItemSerializeOptions
    {
        #region Properties

        public string LineSeparator { get; set; }
        public string ModelSeparator { get; set; }
        public char[] LineSeparators { get; set; }
        public char[] ModelSeparators { get; set; }
        public char[] WhiteSpace { get; set; }
        public string IndentWhiteSpace { get; set; }
        public static ItemSerializeOptions Default
        {
            get
            {
                return new ItemSerializeOptions()
                {
                    LineSeparator = "\r\n",
                    ModelSeparator = ";",
                    LineSeparators = new char[] { '\r', '\n' },
                    ModelSeparators = new char[] { ';' },
                    WhiteSpace = new char[] { ' ', '\t' },
                    IndentWhiteSpace = "    "
                };
            }
        }

        #endregion
    }
}
