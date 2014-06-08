using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sheet.Entry.Model
{
    public class PageEntry : Entry
    {
        public DocumentEntry Document { get; set; }
        public string Content { get; set; }
    }
}
