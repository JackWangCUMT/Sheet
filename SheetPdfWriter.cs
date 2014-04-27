using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sheet
{
    using iTextSharp.text;
    using iTextSharp.text.pdf;
    using System.IO;

    public class SheetPdfWriter
    {
        #region Fields

        private Func<double, float> X;
        private Func<double, float> Y;
        private Func<double, float> DX;
        private Func<double, float> DY;
        private string ttf;
        private BaseFont bf;
        private PdfContentByte canvas; 

        #endregion

        #region Create

        public void Create(string fileName, BlockItem block)
        {
            Document doc = CreateDocument();

            using (var fs = new FileStream(fileName, FileMode.Create))
            {
                var writer = PdfWriter.GetInstance(doc, fs);

                doc.Open();

                canvas = writer.DirectContent;

                Export(block);

                doc.Close();
            }

            ttf = null;
            bf = null;
            canvas = null;
        }

        private string GetFontPath(string font)
        {
            DirectoryInfo windows = Directory.GetParent(Environment.GetFolderPath(Environment.SpecialFolder.System));
            string fonts = Path.Combine(windows.FullName, "Fonts");
            return Path.Combine(fonts, font);
        }

        private Document CreateDocument()
        {
            ttf = GetFontPath("calibri.ttf");
            bf = BaseFont.CreateFont(ttf, BaseFont.IDENTITY_H, BaseFont.EMBEDDED);

            var doc = new Document(PageSize.A4.Rotate(), 0, 0, 0, 0);

            float width = 1260;
            float height = 891;
            float scaleX = doc.PageSize.Width / width;
            float scaleY = doc.PageSize.Height / height;

            X = (x) => (float)(x * scaleX);
            Y = (y) => (float)((height - y) * scaleY);
            DX = (d) => (float)(d * scaleX);
            DY = (d) => (float)(d * scaleY);

            return doc;
        }

        #endregion

        #region Add

        private void AddLine(double x1, double y1, double x2, double y2)
        {
            canvas.MoveTo(X(x1), Y(y1));
            canvas.LineTo(X(x2), Y(y2));
            canvas.Stroke();
        }

        private void AddEllipse(double x, double y, double width, double height, bool isFilled)
        {
            canvas.Ellipse(X(x), Y(y), X(width + x), Y(height + y));
            if (isFilled)
            {
                canvas.Fill();
            }
            canvas.Stroke();
        }

        private void AddRectangle(double x, double y, double width, double height, bool isFilled)
        {
            canvas.Rectangle(X(x), Y(y + height), DX(width), DY(height));
            if (isFilled)
            {
                canvas.Fill();
            }
            canvas.Stroke();
        }

        private void AddText(string text, double x, double y, double width, double height, int halign, int valign, double size)
        {
            Font font = new Font(bf, DY(size), Font.NORMAL, BaseColor.BLACK);
            var col = new ColumnText(canvas);

            int alignment;
            switch (halign)
            {
                case 0: alignment = Element.ALIGN_LEFT; break;
                case 1: alignment = Element.ALIGN_CENTER; break;
                case 2: alignment = Element.ALIGN_RIGHT; break;
                default: alignment = Element.ALIGN_LEFT; break;
            }

            float leading;
            switch (valign)
            {
                case 0: leading = -(DY(height) - (font.CalculatedSize)); break;
                case 1: leading = -((DY(height) / 2f) - (font.CalculatedSize / 3f)); break;
                case 2: leading = -(font.CalculatedSize / 3f); break;
                default: leading = -(DY(height) - (font.CalculatedSize)); break;
            }

            col.SetSimpleColumn(X(x), Y(y + height), X(x + width), Y(y + height), leading, alignment);
            col.AddText(new Phrase(text, font));
            col.Go();
        }

        #endregion

        #region Export

        private void Export(BlockItem block)
        {
            Export(block.Lines);
            Export(block.Rectangles);
            Export(block.Ellipses);
            Export(block.Texts);
            Export(block.Blocks);
        }

        private void Export(IEnumerable<BlockItem> blocks)
        {
            foreach (var block in blocks)
            {
                Export(block);
            }
        }

        private void Export(IEnumerable<LineItem> lines)
        {
            foreach (var line in lines)
            {
                AddLine(line.X1, line.Y1, line.X2, line.Y2);
            }
        }

        private void Export(IEnumerable<RectangleItem> rectangles)
        {
            foreach (var rectangle in rectangles)
            {
                AddRectangle(rectangle.X, rectangle.Y, rectangle.Width, rectangle.Height, rectangle.IsFilled);
            }
        }

        private void Export(IEnumerable<EllipseItem> ellipses)
        {
            foreach (var ellipse in ellipses)
            {
                AddEllipse(ellipse.X, ellipse.Y, ellipse.Width, ellipse.Height, ellipse.IsFilled);
            }
        }

        private void Export(IEnumerable<TextItem> texts)
        {
            foreach (var text in texts)
            {
                AddText(text.Text, text.X, text.Y, text.Width, text.Height, text.HAlign, text.VAlign, text.Size);
            }
        }

        #endregion
    }
}
