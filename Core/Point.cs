using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sheet
{
    #region PointController

    public class PointController : IPointController
    {
        #region IoC

        private IInterfaceLocator _interfaceLocator;
        private IBlockHelper _blockHelper;

        public PointController(IInterfaceLocator interfaceLocator)
        {
            this._interfaceLocator = interfaceLocator;
            this._blockHelper = _interfaceLocator.GetInterface<IBlockHelper>();
        }

        #endregion

        #region Get

        private IEnumerable<KeyValuePair<int, XPoint>> GetAllPoints(List<XBlock> blocks)
        {
            foreach (var block in blocks)
            {
                if (block.Points != null)
                {
                    foreach (var point in block.Points)
                    {
                        yield return new KeyValuePair<int, XPoint>(point.Id, point);
                    }
                }

                if (block.Blocks != null)
                {
                    foreach (var kvp in GetAllPoints(block.Blocks))
                    {
                        yield return kvp;
                    }
                }
            }
        }

        private IEnumerable<XLine> GetAllLines(List<XBlock> blocks)
        {
            foreach (var block in blocks)
            {
                if (block.Lines != null)
                {
                    foreach (var line in block.Lines)
                    {
                        yield return line;
                    }
                }

                if (block.Blocks != null)
                {
                    foreach (var line in GetAllLines(block.Blocks))
                    {
                        yield return line;
                    }
                }
            }
        }

        #endregion

        #region Connect

        public void ConnectStart(XPoint point, XLine line)
        {
            Action<XElement, XPoint> update = (element, p) =>
            {
                _blockHelper.SetX1(element as XLine, p.X);
                _blockHelper.SetY1(element as XLine, p.Y);
            };
            var dependecy = new XDependency(line, update);
            point.Connected.Add(dependecy);
        }

        public void ConnectEnd(XPoint point, XLine line)
        {
            Action<XElement, XPoint> update = (element, p) =>
            {
                _blockHelper.SetX2(element as XLine, p.X);
                _blockHelper.SetY2(element as XLine, p.Y);
            };
            var dependecy = new XDependency(line, update);
            point.Connected.Add(dependecy);
        }

        #endregion

        #region Dependencies

        public void UpdateDependencies(List<XBlock> blocks, List<XPoint> points, List<XLine> lines)
        {
            // get all points
            var ps = GetAllPoints(blocks).ToDictionary(x => x.Key, x => x.Value);

            foreach (var point in points)
            {
                ps.Add(point.Id, point);
            }

            // get all lines
            var ls = GetAllLines(blocks).ToList();

            foreach (var line in lines)
            {
                ls.Add(line);
            }

            // update point dependencies
            foreach (var line in ls)
            {
                if (line.StartId >= 0)
                {
                    var point = ps[line.StartId];
                    line.Start = point;
                    ConnectStart(line.Start, line);
                }

                if (line.EndId >= 0)
                {
                    var point = ps[line.EndId];
                    line.End = point;
                    ConnectEnd(line.End, line);
                }
            }
        }

        #endregion
    }

    #endregion
}
