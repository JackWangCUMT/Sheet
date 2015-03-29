﻿// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using Sheet.Block;
using Sheet.Controller;
using Sheet.Item;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sheet.Plugins
{
    public interface ISelectedBlockPlugin
    {
        string Name { get; }
        bool CanProcess(ISheet contentSheet, IBlock contentBlock, IBlock selectedBlock, SheetOptions options);
        void Process(ISheet contentSheet, IBlock contentBlock, IBlock selectedBlock, SheetOptions options);
    }

    public class InvertLineStartPlugin : ISelectedBlockPlugin
    {
        #region IoC

        private readonly IServiceLocator _serviceLocator;
        private readonly IBlockController _blockController;
        private readonly IBlockFactory _blockFactory;
        private readonly IBlockHelper _blockHelper;

        public InvertLineStartPlugin(IServiceLocator serviceLocator)
        {
            _serviceLocator = serviceLocator;
            _blockController = serviceLocator.GetInstance<IBlockController>();
            _blockFactory = serviceLocator.GetInstance<IBlockFactory>();
            _blockHelper = serviceLocator.GetInstance<IBlockHelper>();
        }

        #endregion

        #region ISelectedBlockPlugin

        public string Name
        {
            get { return "Invert Line Start"; }
        }

        public bool CanProcess(ISheet contentSheet, IBlock contentBlock, IBlock selectedBlock, SheetOptions options)
        {
            return _blockController.HaveSelected(selectedBlock) && selectedBlock.Lines != null && selectedBlock.Lines.Count > 0;
        }

        public void Process(ISheet contentSheet, IBlock contentBlock, IBlock selectedBlock, SheetOptions options)
        {
            InvertSelectedLineStart(contentSheet, contentBlock, selectedBlock, options);
        }

        #endregion

        #region Plugin Code

        private double invertedEllipseWidth = 10.0;
        private double invertedEllipseHeight = 10.0;

        private void AddInvertedLineEllipse(ISheet contentSheet, IBlock contentBlock, IBlock selectedBlock, SheetOptions options, double x, double y, double width, double height)
        {
            // create ellipse
            var ellipse = _blockFactory.CreateEllipse(options.LineThickness / options.Zoom, x, y, width, height, false, ItemColors.Black, ItemColors.Transparent);
            contentBlock.Ellipses.Add(ellipse);
            contentSheet.Add(ellipse);
        }

        private void InvertSelectedLineStart(ISheet contentSheet, IBlock contentBlock, IBlock selectedBlock, SheetOptions options)
        {
            // add for horizontal or vertical line start ellipse and shorten line
            if (_blockController.HaveSelected(selectedBlock) && selectedBlock.Lines != null && selectedBlock.Lines.Count > 0)
            {
                foreach (var line in selectedBlock.Lines)
                {
                    double x1 = _blockHelper.GetX1(line);
                    double y1 = _blockHelper.GetY1(line);
                    double x2 = _blockHelper.GetX2(line);
                    double y2 = _blockHelper.GetY2(line);
                    bool sameX = Math.Round(x1, 1) == Math.Round(x2, 1);
                    bool sameY = Math.Round(y1, 1) == Math.Round(y2, 1);

                    // vertical line
                    if (sameX && !sameY)
                    {
                        // X1, Y1 is start position
                        if (y1 < y2)
                        {
                            AddInvertedLineEllipse(contentSheet, contentBlock, selectedBlock, options, x1 - invertedEllipseWidth / 2.0, y1, invertedEllipseWidth, invertedEllipseHeight);
                            _blockHelper.SetY1(line, y1 + invertedEllipseHeight);
                        }
                        // X2, Y2 is start position
                        else
                        {
                            AddInvertedLineEllipse(contentSheet, contentBlock, selectedBlock, options, x2 - invertedEllipseWidth / 2.0, y2, invertedEllipseWidth, invertedEllipseHeight);
                            _blockHelper.SetY2(line, y2 + invertedEllipseHeight);
                        }
                    }
                    // horizontal line
                    else if (!sameX && sameY)
                    {
                        // X1, Y1 is start position
                        if (x1 < x2)
                        {
                            AddInvertedLineEllipse(contentSheet, contentBlock, selectedBlock, options, x1, y1 - invertedEllipseHeight / 2.0, invertedEllipseWidth, invertedEllipseHeight);
                            _blockHelper.SetX1(line, x1 + invertedEllipseWidth);
                        }
                        // X2, Y2 is start position
                        else
                        {
                            AddInvertedLineEllipse(contentSheet, contentBlock, selectedBlock, options, x2, y2 - invertedEllipseHeight / 2.0, invertedEllipseWidth, invertedEllipseHeight);
                            _blockHelper.SetX2(line, x2 + invertedEllipseWidth);
                        }
                    }
                }
            }
        }

        #endregion
    }

    public class InvertLineEndPlugin : ISelectedBlockPlugin
    {
        #region IoC

        private readonly IServiceLocator _serviceLocator;
        private readonly IBlockController _blockController;
        private readonly IBlockFactory _blockFactory;
        private readonly IBlockHelper _blockHelper;

        public InvertLineEndPlugin(IServiceLocator serviceLocator)
        {
            _serviceLocator = serviceLocator;
            _blockController = serviceLocator.GetInstance<IBlockController>();
            _blockFactory = serviceLocator.GetInstance<IBlockFactory>();
            _blockHelper = serviceLocator.GetInstance<IBlockHelper>();
        }

        #endregion

        #region ISelectedBlockPlugin

        public string Name
        {
            get { return "Invert Line End"; }
        }

        public bool CanProcess(ISheet contentSheet, IBlock contentBlock, IBlock selectedBlock, SheetOptions options)
        {
            return _blockController.HaveSelected(selectedBlock) && selectedBlock.Lines != null && selectedBlock.Lines.Count > 0;
        }

        public void Process(ISheet contentSheet, IBlock contentBlock, IBlock selectedBlock, SheetOptions options)
        {
            InvertSelectedLineEnd(contentSheet, contentBlock, selectedBlock, options);
        }

        #endregion

        #region Plugin Code

        private double invertedEllipseWidth = 10.0;
        private double invertedEllipseHeight = 10.0;

        private void AddInvertedLineEllipse(ISheet contentSheet, IBlock contentBlock, IBlock selectedBlock, SheetOptions options, double x, double y, double width, double height)
        {
            // create ellipse
            var ellipse = _blockFactory.CreateEllipse(options.LineThickness / options.Zoom, x, y, width, height, false, ItemColors.Black, ItemColors.Transparent);
            contentBlock.Ellipses.Add(ellipse);
            contentSheet.Add(ellipse);
        }

        private void InvertSelectedLineEnd(ISheet contentSheet, IBlock contentBlock, IBlock selectedBlock, SheetOptions options)
        {
            // add for horizontal or vertical line end ellipse and shorten line
            if (_blockController.HaveSelected(selectedBlock) && selectedBlock.Lines != null && selectedBlock.Lines.Count > 0)
            {
                foreach (var line in selectedBlock.Lines)
                {
                    double x1 = _blockHelper.GetX1(line);
                    double y1 = _blockHelper.GetY1(line);
                    double x2 = _blockHelper.GetX2(line);
                    double y2 = _blockHelper.GetY2(line);
                    bool sameX = Math.Round(x1, 1) == Math.Round(x2, 1);
                    bool sameY = Math.Round(y1, 1) == Math.Round(y2, 1);

                    // vertical line
                    if (sameX && !sameY)
                    {
                        // X2, Y2 is end position
                        if (y2 > y1)
                        {
                            AddInvertedLineEllipse(contentSheet, contentBlock, selectedBlock, options, x2 - invertedEllipseWidth / 2.0, y2 - invertedEllipseHeight, invertedEllipseWidth, invertedEllipseHeight);
                            _blockHelper.SetY2(line, y2 - invertedEllipseHeight);
                        }
                        // X1, Y1 is end position
                        else
                        {
                            AddInvertedLineEllipse(contentSheet, contentBlock, selectedBlock, options, x1 - invertedEllipseWidth / 2.0, y1 - invertedEllipseHeight, invertedEllipseWidth, invertedEllipseHeight);
                            _blockHelper.SetY1(line, y1 - invertedEllipseHeight);
                        }
                    }
                    // horizontal line
                    else if (!sameX && sameY)
                    {
                        // X2, Y2 is end position
                        if (x2 > x1)
                        {
                            AddInvertedLineEllipse(contentSheet, contentBlock, selectedBlock, options, x2 - invertedEllipseWidth, y2 - invertedEllipseHeight / 2.0, invertedEllipseWidth, invertedEllipseHeight);
                            _blockHelper.SetX2(line, x2 - invertedEllipseWidth);
                        }
                        // X1, Y1 is end position
                        else
                        {
                            AddInvertedLineEllipse(contentSheet, contentBlock, selectedBlock, options, x1 - invertedEllipseWidth, y1 - invertedEllipseHeight / 2.0, invertedEllipseWidth, invertedEllipseHeight);
                            _blockHelper.SetX1(line, x1 - invertedEllipseWidth);
                        }
                    }
                }
            }
        }

        #endregion
    }
}
