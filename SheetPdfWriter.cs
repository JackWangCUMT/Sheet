using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sheet
{
    using PdfSharp;
    using PdfSharp.Drawing;
    using PdfSharp.Pdf;

    public class SheetPdfWriter
    {
        #region Fields

        private Func<double, double> X;
        private Func<double, double> Y;

        #endregion

        #region Create

        public void Create(string fileName, double sourceWidth, double sourceHeight, BlockItem block)
        {
            PdfDocument document = new PdfDocument();
            PdfPage page = document.AddPage();
            page.Size = PageSize.A4;
            page.Orientation = PageOrientation.Landscape;
            XGraphics gfx = XGraphics.FromPdfPage(page);

            double scaleX = page.Width.Value / sourceWidth;
            double scaleY = page.Height.Value / sourceHeight;

            X = (x) => x * scaleX;
            Y = (y) => y * scaleY;

            Export(gfx, block);

            document.Save(fileName);
        }

        #endregion

        #region Add

        private void AddLine(XGraphics gfx, double x1, double y1, double x2, double y2)
        {
            gfx.DrawLine(XPens.Black, X(x1), Y(y1), X(x2), Y(y2));
        }

        private void AddEllipse(XGraphics gfx, double x, double y, double width, double height, bool isFilled)
        {
            if (isFilled)
            {
                gfx.DrawEllipse(XPens.Black, XBrushes.Black, X(x), Y(y), X(width), Y(height));
            }
            else
            {
                gfx.DrawEllipse(XPens.Black, X(x), Y(y), X(width), Y(height));
            }
        }

        private void AddRectangle(XGraphics gfx, double x, double y, double width, double height, bool isFilled)
        {
            if (isFilled)
            {
                gfx.DrawRectangle(XPens.Black, XBrushes.Black, X(x), Y(y), X(width), Y(height));
            }
            else
            {
                gfx.DrawRectangle(XPens.Black, X(x), Y(y), X(width), Y(height));
            }
        }

        private void AddText(XGraphics gfx, string text, double x, double y, double width, double height, int halign, int valign, double size)
        {
            XPdfFontOptions options = new XPdfFontOptions(PdfFontEncoding.Unicode, PdfFontEmbedding.Always);
            XFont font = new XFont("Calibri", Y(size), XFontStyle.Regular, options);

            XStringFormat format = new XStringFormat();
            XRect rect = new XRect(X(x), Y(y), X(width), Y(height));

            switch (halign)
            {
                case 0: format.Alignment = XStringAlignment.Near; break;
                case 1: format.Alignment = XStringAlignment.Center; break;
                case 2: format.Alignment = XStringAlignment.Far; break;
            }

            switch (valign)
            {
                case 0: format.LineAlignment = XLineAlignment.Near; break;
                case 1: format.LineAlignment = XLineAlignment.Center; break;
                case 2: format.LineAlignment = XLineAlignment.Far; break;
            }

            gfx.DrawString(text, font, XBrushes.Black, rect, format);
        }

        #endregion

        #region Export

        private void Export(XGraphics gfx, BlockItem block)
        {
            Export(gfx, block.Lines);
            Export(gfx, block.Rectangles);
            Export(gfx, block.Ellipses);
            Export(gfx, block.Texts);
            Export(gfx, block.Blocks);
        }

        private void Export(XGraphics gfx, IEnumerable<BlockItem> blocks)
        {
            foreach (var block in blocks)
            {
                Export(gfx, block);
            }
        }

        private void Export(XGraphics gfx, IEnumerable<LineItem> lines)
        {
            foreach (var line in lines)
            {
                AddLine(gfx, line.X1, line.Y1, line.X2, line.Y2);
            }
        }

        private void Export(XGraphics gfx, IEnumerable<RectangleItem> rectangles)
        {
            foreach (var rectangle in rectangles)
            {
                AddRectangle(gfx, rectangle.X, rectangle.Y, rectangle.Width, rectangle.Height, rectangle.IsFilled);
            }
        }

        private void Export(XGraphics gfx, IEnumerable<EllipseItem> ellipses)
        {
            foreach (var ellipse in ellipses)
            {
                AddEllipse(gfx, ellipse.X, ellipse.Y, ellipse.Width, ellipse.Height, ellipse.IsFilled);
            }
        }

        private void Export(XGraphics gfx, IEnumerable<TextItem> texts)
        {
            foreach (var text in texts)
            {
                AddText(gfx, text.Text, text.X, text.Y, text.Width, text.Height, text.HAlign, text.VAlign, text.Size);
            }
        }

        #endregion
    }
}
