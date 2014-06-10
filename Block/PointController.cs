using Sheet.Block;
using Sheet.Block.Core;
using Sheet.Block.Model;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sheet.Block
{
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
                foreach (var point in block.Points)
                {
                    yield return new KeyValuePair<int, IPoint>(point.Id, point);
                }

                foreach (var kvp in GetAllPoints(block.Blocks))
                {
                    yield return kvp;
                }
            }
        }

        private IEnumerable<ILine> GetAllLines(IList<IBlock> blocks)
        {
            foreach (var block in blocks)
            {
                foreach (var line in block.Lines)
                {
                    yield return line;
                }

                foreach (var line in GetAllLines(block.Blocks))
                {
                    yield return line;
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
            point.Connected.Add(new XDependency(line, update));
        }

        public void ConnectEnd(IPoint point, ILine line)
        {
            Action<IElement, IPoint> update = (element, p) =>
            {
                _blockHelper.SetX2(element as ILine, p.X);
                _blockHelper.SetY2(element as ILine, p.Y);
            };
            point.Connected.Add(new XDependency(line, update));
        }

        #endregion

        #region Dependencies

        public void UpdateDependencies(List<IBlock> blocks, List<IPoint> points, List<ILine> lines)
        {
            var ps = GetAllPoints(blocks).ToDictionary(x => x.Key, x => x.Value);

            foreach (var point in points)
            {
                ps.Add(point.Id, point);
            }

            var ls = GetAllLines(blocks).ToList();

            foreach (var line in lines)
            {
                ls.Add(line);
            }

            foreach (var line in ls)
            {
                if (line.StartId >= 0)
                {
                    IPoint point;
                    if (ps.TryGetValue(line.StartId, out point))
                    {
                        line.Start = point;
                        ConnectStart(line.Start, line);
                    }
                    else
                    {
                        Debug.Print("Invalid line Start point Id.");
                    }
                }

                if (line.EndId >= 0)
                {
                    IPoint point;
                    if (ps.TryGetValue(line.EndId, out point))
                    {
                        line.End = point;
                        ConnectEnd(line.End, line);
                    }
                    else
                    {
                        Debug.Print("Invalid line End point Id.");
                    }
                }
            }
        }

        #endregion
    }
}
