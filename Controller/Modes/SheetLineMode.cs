using Sheet.Block;
using Sheet.Block.Core;
using Sheet.Controller.Core;
using Sheet.Item;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sheet.Controller.Modes
{
    public class SheetLineMode
    {
        #region IoC

        private readonly ISheetController _sheetController;
        private readonly IServiceLocator _serviceLocator;
        private readonly IBlockController _blockController;
        private readonly IBlockFactory _blockFactory;
        private readonly IBlockHelper _blockHelper;
        private readonly IItemController _itemController;
        private readonly IPointController _pointController;

        public SheetLineMode(ISheetController sheetController, IServiceLocator serviceLocator)
        {
            this._serviceLocator = serviceLocator;
            this._sheetController = sheetController;
            this._blockController = serviceLocator.GetInstance<IBlockController>();
            this._blockFactory = serviceLocator.GetInstance<IBlockFactory>();
            this._blockHelper = serviceLocator.GetInstance<IBlockHelper>();
            this._itemController = serviceLocator.GetInstance<IItemController>();
            this._pointController = serviceLocator.GetInstance<IPointController>();
        }

        #endregion

        #region Fields

        private ILine TempLine;
        private IEllipse TempStartEllipse;
        private IEllipse TempEndEllipse;

        #endregion

        #region Methods

        public void Init(ImmutablePoint p, IPoint start)
        {
            double x = _itemController.Snap(p.X, _sheetController.Options.SnapSize);
            double y = _itemController.Snap(p.Y, _sheetController.Options.SnapSize);

            TempLine = _blockFactory.CreateLine(_sheetController.Options.LineThickness / _sheetController.ZoomController.Zoom, x, y, x, y, ItemColors.Black);

            if (start != null)
            {
                TempLine.Start = start;
            }

            TempStartEllipse = _blockFactory.CreateEllipse(_sheetController.Options.LineThickness / _sheetController.ZoomController.Zoom, x - 4.0, y - 4.0, 8.0, 8.0, true, ItemColors.Black, ItemColors.Black);
            TempEndEllipse = _blockFactory.CreateEllipse(_sheetController.Options.LineThickness / _sheetController.ZoomController.Zoom, x - 4.0, y - 4.0, 8.0, 8.0, true, ItemColors.Black, ItemColors.Black);

            _sheetController.OverlaySheet.Add(TempLine);
            _sheetController.OverlaySheet.Add(TempStartEllipse);
            _sheetController.OverlaySheet.Add(TempEndEllipse);
            _sheetController.OverlaySheet.Capture();
        }

        public void Move(ImmutablePoint p)
        {
            double x = _itemController.Snap(p.X, _sheetController.Options.SnapSize);
            double y = _itemController.Snap(p.Y, _sheetController.Options.SnapSize);
            double x2 = _blockHelper.GetX2(TempLine);
            double y2 = _blockHelper.GetY2(TempLine);
            if (Math.Round(x, 1) != Math.Round(x2, 1)
                || Math.Round(y, 1) != Math.Round(y2, 1))
            {
                _blockHelper.SetX2(TempLine, x);
                _blockHelper.SetY2(TempLine, y);
                _blockHelper.SetLeft(TempEndEllipse, x - 4.0);
                _blockHelper.SetTop(TempEndEllipse, y - 4.0);
            }
        }

        public void Finish(IPoint end)
        {
            double x1 = _blockHelper.GetX1(TempLine);
            double y1 = _blockHelper.GetY1(TempLine);
            double x2 = _blockHelper.GetX2(TempLine);
            double y2 = _blockHelper.GetY2(TempLine);

            if (Math.Round(x1, 1) == Math.Round(x2, 1) && Math.Round(y1, 1) == Math.Round(y2, 1))
            {
                Cancel();
            }
            else
            {
                if (end != null)
                {
                    TempLine.End = end;
                }

                _sheetController.OverlaySheet.ReleaseCapture();
                _sheetController.OverlaySheet.Remove(TempLine);
                _sheetController.OverlaySheet.Remove(TempStartEllipse);
                _sheetController.OverlaySheet.Remove(TempEndEllipse);

                _sheetController.HistoryController.Register("Create Line");

                if (TempLine.Start != null)
                {
                    _pointController.ConnectStart(TempLine.Start, TempLine);
                }

                if (TempLine.End != null)
                {
                    _pointController.ConnectEnd(TempLine.End, TempLine);
                }

                _sheetController.GetContent().Lines.Add(TempLine);
                _sheetController.ContentSheet.Add(TempLine);

                TempLine = null;
                TempStartEllipse = null;
                TempEndEllipse = null;
            }
        }

        public void Cancel()
        {
            _sheetController.OverlaySheet.ReleaseCapture();
            _sheetController.OverlaySheet.Remove(TempLine);
            _sheetController.OverlaySheet.Remove(TempStartEllipse);
            _sheetController.OverlaySheet.Remove(TempEndEllipse);
            TempLine = null;
            TempStartEllipse = null;
            TempEndEllipse = null;
        }

        public void Reset()
        {
            if (TempLine != null)
            {
                _sheetController.OverlaySheet.Remove(TempLine);
                TempLine = null;
            }

            if (TempStartEllipse != null)
            {
                _sheetController.OverlaySheet.Remove(TempStartEllipse);
                TempLine = null;
            }

            if (TempEndEllipse != null)
            {
                _sheetController.OverlaySheet.Remove(TempEndEllipse);
                TempEndEllipse = null;
            }
        }

        public void Adjust(double zoom)
        {
            double lineThicknessZoomed = _sheetController.Options.LineThickness / zoom;

            if (TempLine != null)
            {
                _blockHelper.SetStrokeThickness(TempLine, lineThicknessZoomed);
            }

            if (TempStartEllipse != null)
            {
                _blockHelper.SetStrokeThickness(TempStartEllipse, lineThicknessZoomed);
            }

            if (TempEndEllipse != null)
            {
                _blockHelper.SetStrokeThickness(TempEndEllipse, lineThicknessZoomed);
            }
        }

        public void ToggleFill()
        {
        }

        #endregion
    }
}
