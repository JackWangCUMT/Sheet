using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sheet
{
    #region PointController

    public static class PointController
    {
        #region Fields

        private static IBlockHelper blockHelper = new WpfBlockHelper();

        #endregion

        #region Get

        public static IEnumerable<KeyValuePair<int, XPoint>> GetAllPoints(List<XBlock> blocks)
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

        public static IEnumerable<XLine> GetAllLines(List<XBlock> blocks)
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

        public static void ConnectStart(XPoint point, XLine line)
        {
            Action<XElement, XPoint> update = (element, p) =>
            {
                blockHelper.SetX1(element as XLine, p.X);
                blockHelper.SetY1(element as XLine, p.Y);
            };
            var dependecy = new XDependency(line, update);
            point.Connected.Add(dependecy);
        }

        public static void ConnectEnd(XPoint point, XLine line)
        {
            Action<XElement, XPoint> update = (element, p) =>
            {
                blockHelper.SetX2(element as XLine, p.X);
                blockHelper.SetY2(element as XLine, p.Y);
            };
            var dependecy = new XDependency(line, update);
            point.Connected.Add(dependecy);
        }

        #endregion

        #region Dependencies

        public static void UpdateDependencies(List<XBlock> blocks, List<XPoint> points, List<XLine> lines)
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
