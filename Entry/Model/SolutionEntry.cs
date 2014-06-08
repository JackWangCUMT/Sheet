using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sheet.Entry.Model
{
    public class SolutionEntry : Entry
    {
        public ObservableCollection<DocumentEntry> Documents { get; set; }
    }
}
