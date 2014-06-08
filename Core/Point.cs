using Sheet.Block.Core;
using Sheet.Block.Model;
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

        private readonly IServiceLocator _serviceLocator;
        private readonly IBlockHelper _blockHelper;

        public PointController(IServiceLocator serviceLocator)
        {
            this._serviceLocator = serviceLocator;
            this._blockHelper = _serviceLocator.GetInstance<IBlockHelper>();
        }

        #endregion

        #region Get

        private IEnumerable<KeyValuePair<int, IPoint>> GetAllPoints(IList<IBlock> blocks)
        {
            foreach (var block in blocks)
            {
                if (block.Points != null)
                {
                    foreach (var point in block.Points)
                    {
                        yield return new KeyValuePair<int, IPoint>(point.Id, point);
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

        private IEnumerable<ILine> GetAllLines(IList<IBlock> blocks)
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

        public void ConnectStart(IPoint point, ILine line)
        {
            Action<IElement, IPoint> update = (element, p) =>
            {
                _blockHelper.SetX1(element as ILine, p.X);
                _blockHelper.SetY1(element as ILine, p.Y);
            };
            var dependecy = new XDependency(line, update);
            point.Connected.Add(dependecy);
        }

        public void ConnectEnd(IPoint point, ILine line)
        {
            Action<IElement, IPoint> update = (element, p) =>
            {
                _blockHelper.SetX2(element as ILine, p.X);
                _blockHelper.SetY2(element as ILine, p.Y);
            };
            var dependecy = new XDependency(line, update);
            point.Connected.Add(dependecy);
        }

        #endregion

        #region Dependencies

        public void UpdateDependencies(List<IBlock> blocks, List<IPoint> points, List<ILine> lines)
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
