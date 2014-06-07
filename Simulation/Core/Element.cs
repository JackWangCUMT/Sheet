using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sheet.Simulation.Core
{
    public abstract class Element
    {
        public Element()
        {
            Children = new ObservableCollection<Element>();
            Parent = null;
        }
        public string Id { get; set; }
        public double X { get; set; }
        public double Y { get; set; }
        public UInt32 ElementId { get; set; }
        public string Name { get; set; }
        public string FactoryName { get; set; }
        public Element Parent { get; set; }
        public ObservableCollection<Element> Children { get; set; }
        public Element SimulationParent { get; set; }
    }
}
