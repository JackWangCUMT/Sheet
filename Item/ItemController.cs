using Sheet.Item.Model;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sheet.Item
{
    public class ItemController : IItemController
    {
        #region Text

        public async Task<string> OpenText(string fileName)
        {
            try
            {
                using (var stream = System.IO.File.OpenText(fileName))
                {
                    return await stream.ReadToEndAsync();
                }
            }
            catch (Exception ex)
            {
                Debug.Print(ex.Message);
                Debug.Print(ex.StackTrace);
            }
            return null;
        }

        public async void SaveText(string fileName, string text)
        {
            try
            {
                if (text != null)
                {
                    using (var stream = System.IO.File.CreateText(fileName))
                    {
                        await stream.WriteAsync(text);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.Print(ex.Message);
                Debug.Print(ex.StackTrace);
            }
        }

        #endregion

        #region Position

        public void ResetPosition(BlockItem block, double originX, double originY, double width, double height)
        {
            double minX = width;
            double minY = height;
            double maxX = originX;
            double maxY = originY;
            MinMax(block, ref minX, ref minY, ref maxX, ref maxY);
            double x = -(maxX - (maxX - minX));
            double y = -(maxY - (maxY - minY));
            Move(block, x, y);
        }

        #endregion

        #region MinMax

        public void MinMax(BlockItem block, ref double minX, ref double minY, ref double maxX, ref double maxY)
        {
            MinMax(block.Points, ref minX, ref minY, ref maxX, ref maxY);
            MinMax(block.Lines, ref minX, ref minY, ref maxX, ref maxY);
            MinMax(block.Rectangles, ref minX, ref minY, ref maxX, ref maxY);
            MinMax(block.Ellipses, ref minX, ref minY, ref maxX, ref maxY);
            MinMax(block.Texts, ref minX, ref minY, ref maxX, ref maxY);
            MinMax(block.Images, ref minX, ref minY, ref maxX, ref maxY);
            MinMax(block.Blocks, ref minX, ref minY, ref maxX, ref maxY);
        }

        public void MinMax(IEnumerable<BlockItem> blocks, ref double minX, ref double minY, ref double maxX, ref double maxY)
        {
            foreach (var block in blocks)
            {
                MinMax(block, ref minX, ref minY, ref maxX, ref maxY);
            }
        }

        public void MinMax(IEnumerable<PointItem> points, ref double minX, ref double minY, ref double maxX, ref double maxY)
        {
            foreach (var point in points)
            {
                minX = Math.Min(minX, point.X);
                minY = Math.Min(minY, point.Y);
                maxX = Math.Max(maxX, point.X);
                maxY = Math.Max(maxY, point.Y);
            }
        }

        public void MinMax(IEnumerable<LineItem> lines, ref double minX, ref double minY, ref double maxX, ref double maxY)
        {
            foreach (var line in lines)
            {
                minX = Math.Min(minX, line.X1);
                minX = Math.Min(minX, line.X2);
                minY = Math.Min(minY, line.Y1);
                minY = Math.Min(minY, line.Y2);
                maxX = Math.Max(maxX, line.X1);
                maxX = Math.Max(maxX, line.X2);
                maxY = Math.Max(maxY, line.Y1);
                maxY = Math.Max(maxY, line.Y2);
            }
        }

        public void MinMax(IEnumerable<RectangleItem> rectangles, ref double minX, ref double minY, ref double maxX, ref double maxY)
        {
            foreach (var rectangle in rectangles)
            {
                minX = Math.Min(minX, rectangle.X);
                minY = Math.Min(minY, rectangle.Y);
                maxX = Math.Max(maxX, rectangle.X);
                maxY = Math.Max(maxY, rectangle.Y);
            }
        }

        public void MinMax(IEnumerable<EllipseItem> ellipses, ref double minX, ref double minY, ref double maxX, ref double maxY)
        {
            foreach (var ellipse in ellipses)
            {
                minX = Math.Min(minX, ellipse.X);
                minY = Math.Min(minY, ellipse.Y);
                maxX = Math.Max(maxX, ellipse.X);
                maxY = Math.Max(maxY, ellipse.Y);
            }
        }

        public void MinMax(IEnumerable<TextItem> texts, ref double minX, ref double minY, ref double maxX, ref double maxY)
        {
            foreach (var text in texts)
            {
                minX = Math.Min(minX, text.X);
                minY = Math.Min(minY, text.Y);
                maxX = Math.Max(maxX, text.X);
                maxY = Math.Max(maxY, text.Y);
            }
        }

        public void MinMax(IEnumerable<ImageItem> images, ref double minX, ref double minY, ref double maxX, ref double maxY)
        {
            foreach (var image in images)
            {
                minX = Math.Min(minX, image.X);
                minY = Math.Min(minY, image.Y);
                maxX = Math.Max(maxX, image.X);
                maxY = Math.Max(maxY, image.Y);
            }
        }

        #endregion

        #region Move

        public void Move(IEnumerable<BlockItem> blocks, double x, double y)
        {
            foreach (var block in blocks)
            {
                Move(block, x, y);
            }
        }

        public void Move(BlockItem block, double x, double y)
        {
            Move(block.Points, x, y);
            Move(block.Lines, x, y);
            Move(block.Rectangles, x, y);
            Move(block.Ellipses, x, y);
            Move(block.Texts, x, y);
            Move(block.Images, x, y);
            Move(block.Blocks, x, y);
        }

        public void Move(IEnumerable<PointItem> points, double x, double y)
        {
            foreach (var point in points)
            {
                Move(point, x, y);
            }
        }

        public void Move(IEnumerable<LineItem> lines, double x, double y)
        {
            foreach (var line in lines)
            {
                Move(line, x, y);
            }
        }

        public void Move(IEnumerable<RectangleItem> rectangles, double x, double y)
        {
            foreach (var rectangle in rectangles)
            {
                Move(rectangle, x, y);
            }
        }

        public void Move(IEnumerable<EllipseItem> ellipses, double x, double y)
        {
            foreach (var ellipse in ellipses)
            {
                Move(ellipse, x, y);
            }
        }

        public void Move(IEnumerable<TextItem> texts, double x, double y)
        {
            foreach (var text in texts)
            {
                Move(text, x, y);
            }
        }

        public void Move(IEnumerable<ImageItem> images, double x, double y)
        {
            foreach (var image in images)
            {
                Move(image, x, y);
            }
        }

        public void Move(PointItem point, double x, double y)
        {
            point.X += x;
            point.Y += y;
        }

        public void Move(LineItem line, double x, double y)
        {
            line.X1 += x;
            line.Y1 += y;
            line.X2 += x;
            line.Y2 += y;
        }

        public void Move(RectangleItem rectangle, double x, double y)
        {
            rectangle.X += x;
            rectangle.Y += y;
        }

        public void Move(EllipseItem ellipse, double x, double y)
        {
            ellipse.X += x;
            ellipse.Y += y;
        }

        public void Move(TextItem text, double x, double y)
        {
            text.X += x;
            text.Y += y;
        }

        public void Move(ImageItem image, double x, double y)
        {
            image.X += x;
            image.Y += y;
        }

        #endregion

        #region Snap

        public double Snap(double val, double snap)
        {
            double r = val % snap;
            return r >= snap / 2.0 ? val + snap - r : val - r;
        }

        #endregion
    }
}
