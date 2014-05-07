using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Dxf;
using Dxf.Core;
using Dxf.Enums;
using Dxf.Classes;
using Dxf.Tables;
using Dxf.Blocks;
using Dxf.Entities;
using Dxf.Objects;

namespace Sheet
{
    public class BlockDxfWriter
    {
        #region Fields

        /// <summary>
        /// Convert user X coordinates to PDF coordinates in 72 dpi.
        /// </summary>
        //private Func<double, double> X;

        /// <summary>
        /// Convert user Y coordinates to PDF coordinates in 72 dpi.
        /// </summary>
        //private Func<double, double> Y;

        #endregion

        #region Properties

        /// <summary>
        /// Default XPen stroke thickness in millimeters.
        /// </summary>
        public double defaultStrokeThickness = 1.20;
        public double DefaultStrokeThickness
        {
            get { return defaultStrokeThickness; }
            set
            {
                defaultStrokeThickness = value;
            }
        }

        #endregion

        #region Create

        public void Create(string fileName, double sourceWidth, double sourceHeight, BlockItem blockItem)
        {
            //using (var document = new PdfDocument())
            //{
            //    CreatePage(document, sourceWidth, sourceHeight, blockItem);
            //    document.Save(fileName);
            //}
        }

        public void Create(string fileName, double sourceWidth, double sourceHeight, IEnumerable<BlockItem> blockItems)
        {
            //using (var document = new PdfDocument())
            //{
            //    foreach (var blockItem in blockItems)
            //    {
            //        CreatePage(document, sourceWidth, sourceHeight, blockItem);
            //    }

            //    document.Save(fileName);
            //}
        }

        //private void CreatePage(PdfDocument document, double sourceWidth, double sourceHeight, BlockItem blockItem)
        //{
        //    // create A4 page with landscape orientation
        //    PdfPage page = document.AddPage();
        //    page.Size = PageSize.A4;
        //    page.Orientation = PageOrientation.Landscape;

        //    using (XGraphics gfx = XGraphics.FromPdfPage(page))
        //    {
        //        // calculate x and y page scale factors
        //        double scaleX = page.Width.Value / sourceWidth;
        //        double scaleY = page.Height.Value / sourceHeight;
        //        X = (x) => x * scaleX;
        //        Y = (y) => y * scaleY;

        //        // draw block contents to pdf graphics
        //        DrawBlock(gfx, blockItem);
        //    }
        //}

        #endregion

        #region Draw

        //private void DrawLine(XGraphics gfx, LineItem line)
        //{
        //    var pen = new XPen(XColor.FromArgb(line.Stroke.Alpha, line.Stroke.Red, line.Stroke.Green, line.Stroke.Blue),
        //        X(line.StrokeThickness == 0.0 ? XUnit.FromMillimeter(defaultStrokeThickness).Value : line.StrokeThickness)) { LineCap = XLineCap.Round };
        //    gfx.DrawLine(pen, X(line.X1), Y(line.Y1), X(line.X2), Y(line.Y2));
        //}

        //private void DrawRectangle(XGraphics gfx, RectangleItem rectangle)
        //{
        //    if (rectangle.IsFilled)
        //    {
        //        var pen = new XPen(XColor.FromArgb(rectangle.Stroke.Alpha, rectangle.Stroke.Red, rectangle.Stroke.Green, rectangle.Stroke.Blue),
        //            X(rectangle.StrokeThickness == 0.0 ? XUnit.FromMillimeter(defaultStrokeThickness).Value : rectangle.StrokeThickness)) { LineCap = XLineCap.Round };
        //        var brush = new XSolidBrush(XColor.FromArgb(rectangle.Fill.Alpha, rectangle.Fill.Red, rectangle.Fill.Green, rectangle.Fill.Blue));
        //        gfx.DrawRectangle(pen, brush, X(rectangle.X), Y(rectangle.Y), X(rectangle.Width), Y(rectangle.Height));
        //    }
        //    else
        //    {
        //        var pen = new XPen(XColor.FromArgb(rectangle.Stroke.Alpha, rectangle.Stroke.Red, rectangle.Stroke.Green, rectangle.Stroke.Blue),
        //            X(rectangle.StrokeThickness == 0.0 ? XUnit.FromMillimeter(defaultStrokeThickness).Value : rectangle.StrokeThickness)) { LineCap = XLineCap.Round };
        //        gfx.DrawRectangle(pen, X(rectangle.X), Y(rectangle.Y), X(rectangle.Width), Y(rectangle.Height));
        //    }
        //}

        //private void DrawEllipse(XGraphics gfx, EllipseItem ellipse)
        //{
        //    if (ellipse.IsFilled)
        //    {
        //        var pen = new XPen(XColor.FromArgb(ellipse.Stroke.Alpha, ellipse.Stroke.Red, ellipse.Stroke.Green, ellipse.Stroke.Blue),
        //            X(ellipse.StrokeThickness == 0.0 ? XUnit.FromMillimeter(defaultStrokeThickness).Value : ellipse.StrokeThickness)) { LineCap = XLineCap.Round };
        //        var brush = new XSolidBrush(XColor.FromArgb(ellipse.Fill.Alpha, ellipse.Fill.Red, ellipse.Fill.Green, ellipse.Fill.Blue));
        //        gfx.DrawEllipse(pen, brush, X(ellipse.X), Y(ellipse.Y), X(ellipse.Width), Y(ellipse.Height));
        //    }
        //    else
        //    {
        //        var pen = new XPen(XColor.FromArgb(ellipse.Stroke.Alpha, ellipse.Stroke.Red, ellipse.Stroke.Green, ellipse.Stroke.Blue),
        //            X(ellipse.StrokeThickness == 0.0 ? XUnit.FromMillimeter(defaultStrokeThickness).Value : ellipse.StrokeThickness)) { LineCap = XLineCap.Round };
        //        gfx.DrawEllipse(pen, X(ellipse.X), Y(ellipse.Y), X(ellipse.Width), Y(ellipse.Height));
        //    }
        //}

        //private void DrawText(XGraphics gfx, TextItem text)
        //{
        //    XPdfFontOptions options = new XPdfFontOptions(PdfFontEncoding.Unicode, PdfFontEmbedding.Always);
        //    XFont font = new XFont("Calibri", Y(text.Size), XFontStyle.Regular, options);

        //    XStringFormat format = new XStringFormat();
        //    XRect rect = new XRect(X(text.X), Y(text.Y), X(text.Width), Y(text.Height));

        //    switch (text.HAlign)
        //    {
        //        case 0: format.Alignment = XStringAlignment.Near; break;
        //        case 1: format.Alignment = XStringAlignment.Center; break;
        //        case 2: format.Alignment = XStringAlignment.Far; break;
        //    }

        //    switch (text.VAlign)
        //    {
        //        case 0: format.LineAlignment = XLineAlignment.Near; break;
        //        case 1: format.LineAlignment = XLineAlignment.Center; break;
        //        case 2: format.LineAlignment = XLineAlignment.Far; break;
        //    }

        //    //var brushBackground = new XSolidBrush(XColor.FromArgb(text.Backgroud.Alpha, text.Backgroud.Red, text.Backgroud.Green, text.Backgroud.Blue));
        //    //gfx.DrawRectangle(brushBackground, rect);

        //    var brushForeground = new XSolidBrush(XColor.FromArgb(text.Foreground.Alpha, text.Foreground.Red, text.Foreground.Green, text.Foreground.Blue));
        //    gfx.DrawString(text.Text, font, brushForeground, rect, format);
        //}

        //private void DrawLines(XGraphics gfx, IEnumerable<LineItem> lines)
        //{
        //    foreach (var line in lines)
        //    {
        //        DrawLine(gfx, line);
        //    }
        //}

        //private void DrawRectangles(XGraphics gfx, IEnumerable<RectangleItem> rectangles)
        //{
        //    foreach (var rectangle in rectangles)
        //    {
        //        DrawRectangle(gfx, rectangle);
        //    }
        //}

        //private void DrawEllipses(XGraphics gfx, IEnumerable<EllipseItem> ellipses)
        //{
        //    foreach (var ellipse in ellipses)
        //    {
        //        DrawEllipse(gfx, ellipse);
        //    }
        //}

        //private void DrawTexts(XGraphics gfx, IEnumerable<TextItem> texts)
        //{
        //    foreach (var text in texts)
        //    {
        //        DrawText(gfx, text);
        //    }
        //}

        //private void DrawBlocks(XGraphics gfx, IEnumerable<BlockItem> blocks)
        //{
        //    foreach (var block in blocks)
        //    {
        //        DrawBlock(gfx, block);
        //    }
        //}

        //private void DrawBlock(XGraphics gfx, BlockItem block)
        //{
        //    DrawLines(gfx, block.Lines);
        //    DrawRectangles(gfx, block.Rectangles);
        //    DrawEllipses(gfx, block.Ellipses);
        //    DrawTexts(gfx, block.Texts);
        //    DrawBlocks(gfx, block.Blocks);
        //}

        #endregion
    }
}
