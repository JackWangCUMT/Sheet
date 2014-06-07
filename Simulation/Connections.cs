using Sheet.Simulation.Core;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sheet.Simulation
{
    public class Connections
    {
        #region Find Connections by Id

        private Dictionary<UInt32, List<Tuple<Pin, bool>>> MapPinsToWires(Element[] elements)
        {
            int lenght = elements.Length;
            var dict = new Dictionary<UInt32, List<Tuple<Pin, bool>>>();

            for (int i = 0; i < lenght; i++)
            {
                var element = elements[i];
                if (element is Wire)
                {
                    var wire = element as Wire;
                    var start = wire.Start;
                    var end = wire.End;
                    bool inverted = wire.InvertStart | wire.InvertEnd;

                    var startId = start.ElementId;
                    var endId = end.ElementId;

                    if (!dict.ContainsKey(startId))
                    {
                        dict.Add(startId, new List<Tuple<Pin, bool>>());
                    }

                    dict[startId].Add(new Tuple<Pin, bool>(start, inverted));
                    dict[startId].Add(new Tuple<Pin, bool>(end, inverted));

                    if (!dict.ContainsKey(endId))
                    {
                        dict.Add(endId, new List<Tuple<Pin, bool>>());
                    }

                    dict[endId].Add(new Tuple<Pin, bool>(start, inverted));
                    dict[endId].Add(new Tuple<Pin, bool>(end, inverted));
                }
            }

            return dict;
        }

        private void Find(Pin root, Pin pin, Dictionary<UInt32, Tuple<Pin, bool>> connections, Dictionary<UInt32, List<Tuple<Pin, bool>>> pinToWireDict, int level)
        {
            var connectedPins = pinToWireDict[pin.ElementId].Where(x => x.Item1 != pin && x.Item1 != root && connections.ContainsKey(x.Item1.ElementId) == false);

            foreach (var p in connectedPins)
            {
                connections.Add(p.Item1.ElementId, p);

                if (SimulationSettings.EnableDebug)
                {
                    Debug.Print("{0}    Pin: {1} | Inverted: {2} | SimulationParent: {3} | Type: {4}",
                        new string(' ', level),
                        p.Item1.ElementId,
                        p.Item2,
                        p.Item1.SimulationParent.ElementId,
                        p.Item1.Type);
                }

                if (p.Item1.Type == PinType.Undefined && pinToWireDict.ContainsKey(pin.ElementId) == true)
                {
                    Find(root, p.Item1, connections, pinToWireDict, level + 4);
                }
            }
        }

        public void Find(Element[] elements)
        {
            if (SimulationSettings.EnableDebug)
            {
                Debug.Print("");
                Debug.Print("--- FindConnections(), elements.Count: {0}", elements.Count());
                Debug.Print("");
            }

            var pinToWireDict = MapPinsToWires(elements);

            var pins = elements.Where(x => x is IStateSimulation && x.Children != null)
                               .SelectMany(x => x.Children)
                               .Cast<Pin>()
                               .Where(p => (p.Type == PinType.Undefined || p.Type == PinType.Input) && pinToWireDict.ContainsKey(p.ElementId))
                               .ToArray();

            var lenght = pins.Length;

            for (int i = 0; i < lenght; i++)
            {
                var pin = pins[i];

                if (SimulationSettings.EnableDebug)
                {
                    Debug.Print("Pin  {0} | SimulationParent: {1} | Type: {2}",
                        pin.ElementId,
                        (pin.SimulationParent != null) ? pin.SimulationParent.ElementId : UInt32.MaxValue,
                        pin.Type);
                }

                var connections = new Dictionary<UInt32, Tuple<Pin, bool>>();

                Find(pin, pin, connections, pinToWireDict, 0);

                if (connections.Count > 0)
                {
                    pin.Connections = connections.Values.ToArray();
                }
                else
                {
                    pin.Connections = null;
                }
            }

            if (SimulationSettings.EnableDebug)
            {
                Debug.Print("");
            }

            pinToWireDict = null;
            pins = null;
        }

        #endregion

        #region Reset Connections

        public void Reset(IEnumerable<Pin> pins)
        {
            foreach (var pin in pins)
            {
                if (pin.IsPinTypeUndefined)
                {
                    pin.Connections = null;
                    pin.Type = PinType.Undefined;
                }
                else
                {
                    pin.Connections = null;
                }
            }
        }

        #endregion
    }
}
