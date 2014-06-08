using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sheet.Entry.Model
{
    public abstract class Entry
    {
        public string Name { get; set; }
        public bool IsNew { get; set; }
        public bool IsModified { get; set; }
    }
}
