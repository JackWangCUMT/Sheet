using Sheet.Simulation.Core;
using Sheet.Simulation.Elements;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sheet.Simulation.Elements
{
    public class Solution : Element
    {
        public Solution() : base() { Tags = new ObservableCollection<Tag>(); }
        public ObservableCollection<Tag> Tags { get; set; }
    }
}
