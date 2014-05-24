using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sheet
{
    #region Page History

    public class PageHistory : IHistoryController
    {
        #region IoC

        private IPageController _pageController;

        public PageHistory(IPageController pageController)
        {
            this._pageController = pageController;
        }

        #endregion

        #region Fields

        private Stack<ChangeItem> undos = new Stack<ChangeItem>();
        private Stack<ChangeItem> redos = new Stack<ChangeItem>();

        #endregion

        #region Factory

        private async Task<ChangeItem> CreateChange(string message)
        {
            var block = _pageController.SerializePage();
            var text = await Task.Run(() => ItemSerializer.SerializeContents(block));
            var change = new ChangeItem()
            {
                Message = message,
                Model = text
            };
            return change;
        }

        #endregion

        #region IHistoryController

        public async void Register(string message)
        {
            var change = await CreateChange(message);
            undos.Push(change);
            redos.Clear();
        }

        public void Reset()
        {
            undos.Clear();
            redos.Clear();
        }

        public async void Undo()
        {
            if (undos.Count > 0)
            {
                try
                {
                    var change = await CreateChange("Redo");
                    redos.Push(change);
                    var undo = undos.Pop();
                    var block = await Task.Run(() => ItemSerializer.DeserializeContents(undo.Model));
                    _pageController.ResetPage();
                    _pageController.DeserializePage(block);
                }
                catch(Exception ex)
                {
                    Debug.Print(ex.Message);
                    Debug.Print(ex.StackTrace);
                }
            }
        }

        public async void Redo()
        {
            if (redos.Count > 0)
            {
                try
                {
                    var change = await CreateChange("Undo");
                    undos.Push(change);
                    var redo = redos.Pop();
                    var block = await Task.Run(() => ItemSerializer.DeserializeContents(redo.Model));
                    _pageController.ResetPage();
                    _pageController.DeserializePage(block);
                }
                catch(Exception ex)
                {
                    Debug.Print(ex.Message);
                    Debug.Print(ex.StackTrace);
                }
            }
        }
  
        #endregion
    }

    #endregion

    #region Page Factory

    public class LogicContentPageFactory : IPageFactory
    {
        #region IoC

        private IBlockFactory _blockFactory;

        public LogicContentPageFactory(IBlockFactory blockFactory)
        {
            this._blockFactory = blockFactory;
        } 

        #endregion

        #region Create

        public void CreateLine(ISheet sheet, List<XLine> lines, double thickness, double x1, double y1, double x2, double y2, ItemColor stroke)
        {
            var line = _blockFactory.CreateLine(thickness, x1, y1, x2, y2, stroke);

            if (lines != null)
            {
                lines.Add(line);
            }

            if (sheet != null)
            {
                sheet.Add(line);
            }
        }

        public void CreateText(ISheet sheet, List<XText> texts, string content, double x, double y, double width, double height, int halign, int valign, double size, ItemColor foreground)
        {
            var text = _blockFactory.CreateText(content, x, y, width, height, halign, valign, size, ItemColors.Transparent, foreground);

            if (texts != null)
            {
                texts.Add(text);
            }

            if (sheet != null)
            {
                sheet.Add(text);
            }
        }

        public void CreateFrame(ISheet sheet, XBlock block, double size, double thickness, ItemColor stroke)
        {
            double padding = 6.0;
            double width = 1260.0;
            double height = 891.0;

            double startX = padding;
            double startY = padding;

            double rowsStart = 60;
            double rowsEnd = 780.0;

            double tableStartX = startX;
            double tableStartY = rowsEnd + 25.0;

            bool frameShowBorder = true;
            bool frameShowRows = true;
            bool frameShowTable = true;

            double row0 = 0.0;
            double row1 = 20.0;
            double row2 = 40.0;
            double row3 = 60.0;
            double row4 = 80.0;

            bool tableShowRevisions = true;
            bool tableShowLogos = true;
            bool tableShowInfo = true;

            if (frameShowRows)
            {
                // frame left rows
                int leftRowNumber = 1;
                for (double y = rowsStart; y < rowsEnd; y += size)
                {
                    CreateLine(sheet, block.Lines, thickness, startX, y, 330.0, y, stroke);
                    CreateText(sheet, block.Texts, leftRowNumber.ToString("00"), startX, y, 30.0 - padding, size, (int)XHorizontalAlignment.Center, (int)XVerticalAlignment.Center, 14.0, stroke);
                    leftRowNumber++;
                }

                // frame right rows
                int rightRowNumber = 1;
                for (double y = rowsStart; y < rowsEnd; y += size)
                {
                    CreateLine(sheet, block.Lines, thickness, 930.0, y, width - padding, y, stroke);
                    CreateText(sheet, block.Texts, rightRowNumber.ToString("00"), width - 30.0, y, 30.0 - padding, size, (int)XHorizontalAlignment.Center, (int)XVerticalAlignment.Center, 14.0, stroke);
                    rightRowNumber++;
                }

                // frame columns
                double[] columnWidth = { 30.0, 210.0, 90.0, 600.0, 210.0, 90.0 };
                double[] columnX = { 30.0, 30.0, startY, startY, 30.0, 30.0 };
                double[] columnY = { rowsEnd, rowsEnd, rowsEnd, rowsEnd, rowsEnd, rowsEnd };

                double start = 0.0;
                for (int i = 0; i < columnWidth.Length; i++)
                {
                    start += columnWidth[i];
                    CreateLine(sheet, block.Lines, thickness, start, columnX[i], start, columnY[i], stroke);
                }

                // frame header
                CreateLine(sheet, block.Lines, thickness, startX, 30.0, width - padding, 30.0, stroke);

                // frame footer
                CreateLine(sheet, block.Lines, thickness, startX, rowsEnd, width - padding, rowsEnd, stroke);
            }

            if (frameShowTable)
            {
                // table header
                CreateLine(sheet, block.Lines, thickness, tableStartX + 0, tableStartY + row0, tableStartX + 1248, tableStartY + row0, stroke);

                // table revisions
                if (tableShowRevisions)
                {
                    CreateLine(sheet, block.Lines, thickness, tableStartX + 24, tableStartY + row0, tableStartX + 24, tableStartY + row4, stroke);
                    CreateLine(sheet, block.Lines, thickness, tableStartX + 75, tableStartY + row0, tableStartX + 75, tableStartY + row4, stroke);
                    CreateLine(sheet, block.Lines, thickness, tableStartX + 0, tableStartY + row1, tableStartX + 175, tableStartY + row1, stroke);
                    CreateLine(sheet, block.Lines, thickness, tableStartX + 0, tableStartY + row2, tableStartX + 175, tableStartY + row2, stroke);
                    CreateLine(sheet, block.Lines, thickness, tableStartX + 0, tableStartY + row3, tableStartX + 175, tableStartY + row3, stroke);
                }

                // table logos
                if (tableShowLogos)
                {
                    CreateLine(sheet, block.Lines, thickness, tableStartX + 175, tableStartY + row0, tableStartX + 175, tableStartY + row4, stroke);
                    CreateLine(sheet, block.Lines, thickness, tableStartX + 290, tableStartY + row0, tableStartX + 290, tableStartY + row4, stroke);
                    CreateLine(sheet, block.Lines, thickness, tableStartX + 405, tableStartY + row0, tableStartX + 405, tableStartY + row4, stroke);
                }

                // table info
                if (tableShowInfo)
                {
                    CreateLine(sheet, block.Lines, thickness, tableStartX + 405, tableStartY + row1, tableStartX + 1248, tableStartY + row1, stroke);
                    CreateLine(sheet, block.Lines, thickness, tableStartX + 405, tableStartY + row2, tableStartX + 695, tableStartY + row2, stroke);
                    CreateLine(sheet, block.Lines, thickness, tableStartX + 965, tableStartY + row2, tableStartX + 1248, tableStartY + row2, stroke);
                    CreateLine(sheet, block.Lines, thickness, tableStartX + 405, tableStartY + row3, tableStartX + 695, tableStartY + row3, stroke);
                    CreateLine(sheet, block.Lines, thickness, tableStartX + 965, tableStartY + row3, tableStartX + 1248, tableStartY + row3, stroke);

                    CreateLine(sheet, block.Lines, thickness, tableStartX + 465, tableStartY + row0, tableStartX + 465, tableStartY + row4, stroke);
                    CreateLine(sheet, block.Lines, thickness, tableStartX + 595, tableStartY + row0, tableStartX + 595, tableStartY + row4, stroke);
                    CreateLine(sheet, block.Lines, thickness, tableStartX + 640, tableStartY + row0, tableStartX + 640, tableStartY + row4, stroke);
                    CreateLine(sheet, block.Lines, thickness, tableStartX + 695, tableStartY + row0, tableStartX + 695, tableStartY + row4, stroke);
                    CreateLine(sheet, block.Lines, thickness, tableStartX + 965, tableStartY + row0, tableStartX + 965, tableStartY + row4, stroke);

                    CreateLine(sheet, block.Lines, thickness, tableStartX + 1005, tableStartY + row0, tableStartX + 1005, tableStartY + row4, stroke);
                    CreateLine(sheet, block.Lines, thickness, tableStartX + 1045, tableStartY + row0, tableStartX + 1045, tableStartY + row4, stroke);
                    CreateLine(sheet, block.Lines, thickness, tableStartX + 1100, tableStartY + row0, tableStartX + 1100, tableStartY + row4, stroke);
                }
            }

            if (frameShowBorder)
            {
                // frame border
                CreateLine(sheet, block.Lines, thickness, startX, startY, width - padding, startY, stroke);
                CreateLine(sheet, block.Lines, thickness, startX, height - padding, width - padding, height - padding, stroke);
                CreateLine(sheet, block.Lines, thickness, startX, startY, startX, height - padding, stroke);
                CreateLine(sheet, block.Lines, thickness, width - padding, startY, width - padding, height - padding, stroke);
            }
        }

        public void CreateGrid(ISheet sheet, XBlock block, double startX, double startY, double width, double height, double size, double thickness, ItemColor stroke)
        {
            for (double y = startY + size; y < height + startY; y += size)
            {
                CreateLine(sheet, block.Lines, thickness, startX, y, width + startX, y, stroke);
            }

            for (double x = startX + size; x < startX + width; x += size)
            {
                CreateLine(sheet, block.Lines, thickness, x, startY, x, height + startY, stroke);
            }
        }

        #endregion
    }

    #endregion
}
