using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sheet.Entry.Model
{
    public class DocumentEntry : Entry
    {
        public SolutionEntry Solution { get; set; }
        public ObservableCollection<PageEntry> Pages { get; set; }
    }
}
