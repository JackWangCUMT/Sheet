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

        /// <summary>
        /// Convert user X coordinates to PDF coordinates in 72 dpi.
        /// </summary>
        private Func<double, double> X;

        /// <summary>
        /// Convert user Y coordinates to PDF coordinates in 72 dpi.
        /// </summary>
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

        /// <summary>
        /// Default XPen stroke thickness in millimeters.
        /// </summary>
        private double DefaultStrokeThickness = 1.20;

        private void AddLine(XGraphics gfx, LineItem line)
        {
            var pen = new XPen(XColor.FromArgb(line.Stroke.Alpha, line.Stroke.Red, line.Stroke.Green, line.Stroke.Blue),
                X(line.StrokeThickness == 0.0 ? XUnit.FromMillimeter(DefaultStrokeThickness).Value : line.StrokeThickness)) { LineCap = XLineCap.Round };
            gfx.DrawLine(pen, X(line.X1), Y(line.Y1), X(line.X2), Y(line.Y2));
        }

        private void AddRectangle(XGraphics gfx, RectangleItem rectangle)
        {
            if (rectangle.IsFilled)
            {
                var pen = new XPen(XColor.FromArgb(rectangle.Stroke.Alpha, rectangle.Stroke.Red, rectangle.Stroke.Green, rectangle.Stroke.Blue),
                    X(rectangle.StrokeThickness == 0.0 ? XUnit.FromMillimeter(DefaultStrokeThickness).Value : rectangle.StrokeThickness)) { LineCap = XLineCap.Round };
                var brush = new XSolidBrush(XColor.FromArgb(rectangle.Fill.Alpha, rectangle.Fill.Red, rectangle.Fill.Green, rectangle.Fill.Blue));
                gfx.DrawRectangle(pen, brush, X(rectangle.X), Y(rectangle.Y), X(rectangle.Width), Y(rectangle.Height));
            }
            else
            {
                var pen = new XPen(XColor.FromArgb(rectangle.Stroke.Alpha, rectangle.Stroke.Red, rectangle.Stroke.Green, rectangle.Stroke.Blue),
                    X(rectangle.StrokeThickness == 0.0 ? XUnit.FromMillimeter(DefaultStrokeThickness).Value : rectangle.StrokeThickness)) { LineCap = XLineCap.Round };
                gfx.DrawRectangle(pen, X(rectangle.X), Y(rectangle.Y), X(rectangle.Width), Y(rectangle.Height));
            }
        }

        private void AddEllipse(XGraphics gfx, EllipseItem ellipse)
        {
            if (ellipse.IsFilled)
            {
                var pen = new XPen(XColor.FromArgb(ellipse.Stroke.Alpha, ellipse.Stroke.Red, ellipse.Stroke.Green, ellipse.Stroke.Blue),
                    X(ellipse.StrokeThickness == 0.0 ? XUnit.FromMillimeter(DefaultStrokeThickness).Value : ellipse.StrokeThickness)) { LineCap = XLineCap.Round };
                var brush = new XSolidBrush(XColor.FromArgb(ellipse.Fill.Alpha, ellipse.Fill.Red, ellipse.Fill.Green, ellipse.Fill.Blue));
                gfx.DrawEllipse(pen, brush, X(ellipse.X), Y(ellipse.Y), X(ellipse.Width), Y(ellipse.Height));
            }
            else
            {
                var pen = new XPen(XColor.FromArgb(ellipse.Stroke.Alpha, ellipse.Stroke.Red, ellipse.Stroke.Green, ellipse.Stroke.Blue),
                    X(ellipse.StrokeThickness == 0.0 ? XUnit.FromMillimeter(DefaultStrokeThickness).Value : ellipse.StrokeThickness)) { LineCap = XLineCap.Round };
                gfx.DrawEllipse(pen, X(ellipse.X), Y(ellipse.Y), X(ellipse.Width), Y(ellipse.Height));
            }
        }

        private void AddText(XGraphics gfx, TextItem text)
        {
            XPdfFontOptions options = new XPdfFontOptions(PdfFontEncoding.Unicode, PdfFontEmbedding.Always);
            XFont font = new XFont("Calibri", Y(text.Size), XFontStyle.Regular, options);

            XStringFormat format = new XStringFormat();
            XRect rect = new XRect(X(text.X), Y(text.Y), X(text.Width), Y(text.Height));

            switch (text.HAlign)
            {
                case 0: format.Alignment = XStringAlignment.Near; break;
                case 1: format.Alignment = XStringAlignment.Center; break;
                case 2: format.Alignment = XStringAlignment.Far; break;
            }

            switch (text.VAlign)
            {
                case 0: format.LineAlignment = XLineAlignment.Near; break;
                case 1: format.LineAlignment = XLineAlignment.Center; break;
                case 2: format.LineAlignment = XLineAlignment.Far; break;
            }

            //var brushBackground = new XSolidBrush(XColor.FromArgb(text.Backgroud.Alpha, text.Backgroud.Red, text.Backgroud.Green, text.Backgroud.Blue));
            //gfx.DrawRectangle(brushBackground, rect);

            var brushForeground = new XSolidBrush(XColor.FromArgb(text.Foreground.Alpha, text.Foreground.Red, text.Foreground.Green, text.Foreground.Blue));
            gfx.DrawString(text.Text, font, brushForeground, rect, format);
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
                AddLine(gfx, line);
            }
        }

        private void Export(XGraphics gfx, IEnumerable<RectangleItem> rectangles)
        {
            foreach (var rectangle in rectangles)
            {
                AddRectangle(gfx, rectangle);
            }
        }

        private void Export(XGraphics gfx, IEnumerable<EllipseItem> ellipses)
        {
            foreach (var ellipse in ellipses)
            {
                AddEllipse(gfx, ellipse);
            }
        }

        private void Export(XGraphics gfx, IEnumerable<TextItem> texts)
        {
            foreach (var text in texts)
            {
                AddText(gfx, text);
            }
        }

        #endregion
    }
}
