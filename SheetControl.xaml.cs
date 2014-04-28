using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Packaging;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Xps;
using System.Windows.Xps.Packaging;

namespace Sheet
{
    #region Item Model

    public abstract class Item
    {
        public int Id { get; set; }
    }

    public class ItemColor
    {
        public int Alpha { get; set; }
        public int Red { get; set; }
        public int Green { get; set; }
        public int Blue { get; set; }
    }

    public class LineItem : Item
    {
        public double X1 { get; set; }
        public double Y1 { get; set; }
        public double X2 { get; set; }
        public double Y2 { get; set; }
        public ItemColor Stroke { get; set; }
        public double StrokeThickness { get; set; }
    }

    public class RectangleItem : Item
    {
        public double X { get; set; }
        public double Y { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }
        public bool IsFilled { get; set; }
        public ItemColor Stroke { get; set; }
        public ItemColor Fill { get; set; }
        public double StrokeThickness { get; set; }
    }

    public class EllipseItem : Item
    {
        public double X { get; set; }
        public double Y { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }
        public bool IsFilled { get; set; }
        public ItemColor Stroke { get; set; }
        public ItemColor Fill { get; set; }
        public double StrokeThickness { get; set; }
    } 

    public class TextItem : Item
    {
        public double X { get; set; }
        public double Y { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }
        public int HAlign { get; set; }
        public int VAlign { get; set; }
        public double Size { get; set; }
        public string Text { get; set; }
        public ItemColor Foreground { get; set; }
        public ItemColor Backgroud { get; set; }
    }

    public class BlockItem : Item
    {
        public string Name { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }
        public ItemColor Backgroud { get; set; }
        public List<LineItem> Lines { get; set; }
        public List<RectangleItem> Rectangles { get; set; }
        public List<EllipseItem> Ellipses { get; set; }
        public List<TextItem> Texts { get; set; }
        public List<BlockItem> Blocks { get; set; }
        public void Init(int id, string name)
        {
            Id = id;
            Name = name;
            Width = 0.0;
            Height = 0.0;
            Backgroud = new ItemColor() { Alpha = 0, Red = 0, Green = 0, Blue = 0 };
            Lines = new List<LineItem>();
            Rectangles = new List<RectangleItem>();
            Ellipses = new List<EllipseItem>();
            Texts = new List<TextItem>();
            Blocks = new List<BlockItem>();
        }
    }

    #endregion

    #region Item Editor

    public static class ItemEditor
    {
        #region Fields

        private static string lineSeparator = "\r\n";
        private static string modelSeparator = ";";
        private static char[] lineSeparators = { '\r', '\n' };
        private static char[] modelSeparators = { ';' };
        private static char[] whiteSpace = { ' ', '\t' };
        private static string indentWhiteSpace = "    ";

        #endregion

        #region Serialize

        public static void Serialize(StringBuilder sb, ItemColor color)
        {
            sb.Append(color.Alpha);
            sb.Append(modelSeparator);
            sb.Append(color.Red);
            sb.Append(modelSeparator);
            sb.Append(color.Green);
            sb.Append(modelSeparator);
            sb.Append(color.Blue);
        }

        public static void Serialize(StringBuilder sb, LineItem line, string indent)
        {
            sb.Append(indent);
            sb.Append("LINE");
            sb.Append(modelSeparator);
            sb.Append(line.Id);
            sb.Append(modelSeparator);
            sb.Append(line.X1);
            sb.Append(modelSeparator);
            sb.Append(line.Y1);
            sb.Append(modelSeparator);
            sb.Append(line.X2);
            sb.Append(modelSeparator);
            sb.Append(line.Y2);
            sb.Append(modelSeparator);
            Serialize(sb, line.Stroke);
            sb.Append(lineSeparator);
        }

        public static void Serialize(StringBuilder sb, RectangleItem rectangle, string indent)
        {
            sb.Append(indent);
            sb.Append("RECTANGLE");
            sb.Append(modelSeparator);
            sb.Append(rectangle.Id);
            sb.Append(modelSeparator);
            sb.Append(rectangle.X);
            sb.Append(modelSeparator);
            sb.Append(rectangle.Y);
            sb.Append(modelSeparator);
            sb.Append(rectangle.Width);
            sb.Append(modelSeparator);
            sb.Append(rectangle.Height);
            sb.Append(modelSeparator);
            sb.Append(rectangle.IsFilled);
            sb.Append(modelSeparator);
            Serialize(sb, rectangle.Stroke);
            sb.Append(modelSeparator);
            Serialize(sb, rectangle.Fill);
            sb.Append(lineSeparator);
        }

        public static void Serialize(StringBuilder sb, EllipseItem ellipse, string indent)
        {
            sb.Append(indent);
            sb.Append("ELLIPSE");
            sb.Append(modelSeparator);
            sb.Append(ellipse.Id);
            sb.Append(modelSeparator);
            sb.Append(ellipse.X);
            sb.Append(modelSeparator);
            sb.Append(ellipse.Y);
            sb.Append(modelSeparator);
            sb.Append(ellipse.Width);
            sb.Append(modelSeparator);
            sb.Append(ellipse.Height);
            sb.Append(modelSeparator);
            sb.Append(ellipse.IsFilled);
            sb.Append(modelSeparator);
            Serialize(sb, ellipse.Stroke);
            sb.Append(modelSeparator);
            Serialize(sb, ellipse.Fill);
            sb.Append(lineSeparator);
        }

        public static void Serialize(StringBuilder sb, TextItem text, string indent)
        {
            sb.Append(indent);
            sb.Append("TEXT");
            sb.Append(modelSeparator);
            sb.Append(text.Id);
            sb.Append(modelSeparator);
            sb.Append(text.X);
            sb.Append(modelSeparator);
            sb.Append(text.Y);
            sb.Append(modelSeparator);
            sb.Append(text.Width);
            sb.Append(modelSeparator);
            sb.Append(text.Height);
            sb.Append(modelSeparator);
            sb.Append(text.HAlign);
            sb.Append(modelSeparator);
            sb.Append(text.VAlign);
            sb.Append(modelSeparator);
            sb.Append(text.Size);
            sb.Append(modelSeparator);
            Serialize(sb, text.Foreground);
            sb.Append(modelSeparator);
            Serialize(sb, text.Backgroud);
            sb.Append(modelSeparator);
            sb.Append(text.Text);
            sb.Append(lineSeparator);
        }

        public static void Serialize(StringBuilder sb, BlockItem block, string indent)
        {
            sb.Append(indent);
            sb.Append("BLOCK");
            sb.Append(modelSeparator);
            sb.Append(block.Id);
            sb.Append(modelSeparator);
            sb.Append(block.Name);
            sb.Append(modelSeparator);
            sb.Append(block.Width);
            sb.Append(modelSeparator);
            sb.Append(block.Height);
            sb.Append(modelSeparator);
            Serialize(sb, block.Backgroud);
            sb.Append(lineSeparator);

            Serialize(sb, block.Lines, indent + indentWhiteSpace);
            Serialize(sb, block.Rectangles, indent + indentWhiteSpace);
            Serialize(sb, block.Ellipses, indent + indentWhiteSpace);
            Serialize(sb, block.Texts, indent + indentWhiteSpace);
            Serialize(sb, block.Blocks, indent + indentWhiteSpace);

            sb.Append(indent);
            sb.Append("END");
            sb.Append(lineSeparator);
        }

        public static void Serialize(StringBuilder sb, List<LineItem> lines, string indent)
        {
            foreach (var line in lines)
            {
                Serialize(sb, line, indent);
            }
        }

        public static void Serialize(StringBuilder sb, List<RectangleItem> rectangles, string indent)
        {
            foreach (var rectangle in rectangles)
            {
                Serialize(sb, rectangle, indent);
            }
        }

        public static void Serialize(StringBuilder sb, List<EllipseItem> ellipses, string indent)
        {
            foreach (var ellipse in ellipses)
            {
                Serialize(sb, ellipse, indent);
            }
        }

        public static void Serialize(StringBuilder sb, List<TextItem> texts, string indent)
        {
            foreach (var text in texts)
            {
                Serialize(sb, text, indent);
            }
        }

        public static void Serialize(StringBuilder sb, List<BlockItem> blocks, string indent)
        {
            foreach (var block in blocks)
            {
                Serialize(sb, block, indent);
            }
        }

        public static string Serialize(BlockItem block)
        {
            var sb = new StringBuilder();

            Serialize(sb, block.Lines, "");
            Serialize(sb, block.Rectangles, "");
            Serialize(sb, block.Ellipses, "");
            Serialize(sb, block.Texts, "");
            Serialize(sb, block.Blocks, "");

            return sb.ToString();
        }

        #endregion

        #region Deserialize

        private static BlockItem Deserialize(string[] lines, int length, ref int end, string name, int id)
        {
            var sheet = new BlockItem();
            sheet.Init(id, name);

            for (; end < length; end++)
            {
                string line = lines[end].TrimStart(whiteSpace);
                var m = line.Split(modelSeparators);
                if ((m.Length == 6 || m.Length == 10) && string.Compare(m[0], "LINE", true) == 0)
                {
                    var lineItem = new LineItem();
                    lineItem.Id = int.Parse(m[1]);
                    lineItem.X1 = double.Parse(m[2]);
                    lineItem.Y1 = double.Parse(m[3]);
                    lineItem.X2 = double.Parse(m[4]);
                    lineItem.Y2 = double.Parse(m[5]);
                    if (m.Length == 10)
                    {
                        lineItem.Stroke = new ItemColor()
                        {
                            Alpha = int.Parse(m[6]),
                            Red = int.Parse(m[7]),
                            Green = int.Parse(m[8]),
                            Blue = int.Parse(m[9])
                        };
                    }
                    else
                    {
                        lineItem.Stroke = new ItemColor()
                        {
                            Alpha = 255,
                            Red = 0,
                            Green = 0,
                            Blue = 0
                        };
                    }
                    sheet.Lines.Add(lineItem);
                }
                if ((m.Length == 7 || m.Length == 15) && string.Compare(m[0], "RECTANGLE", true) == 0)
                {
                    var rectangleItem = new RectangleItem();
                    rectangleItem.Id = int.Parse(m[1]);
                    rectangleItem.X = double.Parse(m[2]);
                    rectangleItem.Y = double.Parse(m[3]);
                    rectangleItem.Width = double.Parse(m[4]);
                    rectangleItem.Height = double.Parse(m[5]);
                    rectangleItem.IsFilled = bool.Parse(m[6]);
                    if (m.Length == 15)
                    {
                        rectangleItem.Stroke = new ItemColor()
                        {
                            Alpha = int.Parse(m[7]),
                            Red = int.Parse(m[8]),
                            Green = int.Parse(m[9]),
                            Blue = int.Parse(m[10])
                        };
                        rectangleItem.Fill = new ItemColor()
                        {
                            Alpha = int.Parse(m[11]),
                            Red = int.Parse(m[12]),
                            Green = int.Parse(m[13]),
                            Blue = int.Parse(m[14])
                        };
                    }
                    else
                    {
                        rectangleItem.Stroke = new ItemColor()
                        {
                            Alpha = 255,
                            Red = 0,
                            Green = 0,
                            Blue = 0
                        };
                        rectangleItem.Fill = new ItemColor()
                        {
                            Alpha = 255,
                            Red = 0,
                            Green = 0,
                            Blue = 0
                        };
                    }
                    sheet.Rectangles.Add(rectangleItem);
                }
                if ((m.Length == 7 || m.Length == 15) && string.Compare(m[0], "ELLIPSE", true) == 0)
                {
                    var ellipseItem = new EllipseItem();
                    ellipseItem.Id = int.Parse(m[1]);
                    ellipseItem.X = double.Parse(m[2]);
                    ellipseItem.Y = double.Parse(m[3]);
                    ellipseItem.Width = double.Parse(m[4]);
                    ellipseItem.Height = double.Parse(m[5]);
                    ellipseItem.IsFilled = bool.Parse(m[6]);
                    if (m.Length == 15)
                    {
                        ellipseItem.Stroke = new ItemColor()
                        {
                            Alpha = int.Parse(m[7]),
                            Red = int.Parse(m[8]),
                            Green = int.Parse(m[9]),
                            Blue = int.Parse(m[10])
                        };
                        ellipseItem.Fill = new ItemColor()
                        {
                            Alpha = int.Parse(m[11]),
                            Red = int.Parse(m[12]),
                            Green = int.Parse(m[13]),
                            Blue = int.Parse(m[14])
                        };
                    }
                    else
                    {
                        ellipseItem.Stroke = new ItemColor()
                        {
                            Alpha = 255,
                            Red = 0,
                            Green = 0,
                            Blue = 0
                        };
                        ellipseItem.Fill = new ItemColor()
                        {
                            Alpha = 255,
                            Red = 0,
                            Green = 0,
                            Blue = 0
                        };
                    }
                    sheet.Ellipses.Add(ellipseItem);
                }
                else if ((m.Length == 10 || m.Length == 18) && string.Compare(m[0], "TEXT", true) == 0)
                {
                    var textItem = new TextItem();
                    textItem.Id = int.Parse(m[1]);
                    textItem.X = double.Parse(m[2]);
                    textItem.Y = double.Parse(m[3]);
                    textItem.Width = double.Parse(m[4]);
                    textItem.Height = double.Parse(m[5]);
                    textItem.HAlign = int.Parse(m[6]);
                    textItem.VAlign = int.Parse(m[7]);
                    textItem.Size = double.Parse(m[8]);
                    if (m.Length == 18)
                    {
                        textItem.Foreground = new ItemColor()
                        {
                            Alpha = int.Parse(m[9]),
                            Red = int.Parse(m[10]),
                            Green = int.Parse(m[11]),
                            Blue = int.Parse(m[12])
                        };
                        textItem.Backgroud = new ItemColor()
                        {
                            Alpha = int.Parse(m[13]),
                            Red = int.Parse(m[14]),
                            Green = int.Parse(m[15]),
                            Blue = int.Parse(m[16])
                        };
                        textItem.Text = m[17];
                    }
                    else
                    {
                        textItem.Foreground = new ItemColor()
                        {
                            Alpha = 255,
                            Red = 0,
                            Green = 0,
                            Blue = 0
                        };
                        textItem.Backgroud = new ItemColor()
                        {
                            Alpha = 255,
                            Red = 0,
                            Green = 0,
                            Blue = 0
                        };
                        textItem.Text = m[9];
                    }
                    sheet.Texts.Add(textItem);
                }
                else if ((m.Length == 3 || m.Length == 9) && string.Compare(m[0], "BLOCK", true) == 0)
                {
                    end++;
                    var blockItem = Deserialize(lines, length, ref end, m[2], int.Parse(m[1]));
                    if (m.Length == 9)
                    {
                        blockItem.Width = double.Parse(m[3]);
                        blockItem.Width = double.Parse(m[4]);
                        blockItem.Backgroud = new ItemColor()
                        {
                            Alpha = int.Parse(m[5]),
                            Red = int.Parse(m[6]),
                            Green = int.Parse(m[7]),
                            Blue = int.Parse(m[8])
                        };
                    }
                    else
                    {
                        blockItem.Width = 0.0;
                        blockItem.Width = 0.0;
                        blockItem.Backgroud = new ItemColor()
                        {
                            Alpha = 0,
                            Red = 0,
                            Green = 0,
                            Blue = 0
                        };
                    }
                    sheet.Blocks.Add(blockItem);
                    continue;
                }
                else if (m.Length == 1 && string.Compare(m[0], "END", true) == 0)
                {
                    return sheet;
                }
            }

            return sheet;
        }

        public static BlockItem Deserialize(string model)
        {
            string[] lines = model.Split(lineSeparators, StringSplitOptions.RemoveEmptyEntries);
            int length = lines.Length;
            int end = 0;
            return Deserialize(lines, length, ref end, "LOGIC", 0);
        }

        #endregion

        #region I/O

        public static string OpenText(string fileName)
        {
            try
            {
                using (var stream = System.IO.File.OpenText(fileName))
                {
                    var text = stream.ReadToEnd();
                    return text;
                }
            }
            catch (Exception ex)
            {
                Debug.Print(ex.Message);
            }
            return null;
        }

        public static void SaveText(string fileName, string text)
        {
            try
            {
                if (text != null)
                {
                    using (var stream = System.IO.File.CreateText(fileName))
                    {
                        stream.Write(text);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.Print(ex.Message);
            }
        }

        #endregion

        #region Normalize Position

        public static void NormalizePosition(BlockItem block, double originX, double originY, double width, double height)
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

        public static void MinMax(BlockItem block, ref double minX, ref double minY, ref double maxX, ref double maxY)
        {
            MinMax(block.Lines, ref minX, ref minY, ref maxX, ref maxY);
            MinMax(block.Rectangles, ref minX, ref minY, ref maxX, ref maxY);
            MinMax(block.Ellipses, ref minX, ref minY, ref maxX, ref maxY);
            MinMax(block.Texts, ref minX, ref minY, ref maxX, ref maxY);
            MinMax(block.Blocks, ref minX, ref minY, ref maxX, ref maxY);
        }

        public static void MinMax(IEnumerable<BlockItem> blocks, ref double minX, ref double minY, ref double maxX, ref double maxY)
        {
            foreach (var block in blocks)
            {
                MinMax(block, ref minX, ref minY, ref maxX, ref maxY);
            }
        }

        public static void MinMax(IEnumerable<LineItem> lines, ref double minX, ref double minY, ref double maxX, ref double maxY)
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

        public static void MinMax(IEnumerable<RectangleItem> rectangles, ref double minX, ref double minY, ref double maxX, ref double maxY)
        {
            foreach (var rectangle in rectangles)
            {
                minX = Math.Min(minX, rectangle.X);
                minY = Math.Min(minY, rectangle.Y);
                maxX = Math.Max(maxX, rectangle.X);
                maxY = Math.Max(maxY, rectangle.Y);
            }
        }

        public static void MinMax(IEnumerable<EllipseItem> ellipses, ref double minX, ref double minY, ref double maxX, ref double maxY)
        {
            foreach (var ellipse in ellipses)
            {
                minX = Math.Min(minX, ellipse.X);
                minY = Math.Min(minY, ellipse.Y);
                maxX = Math.Max(maxX, ellipse.X);
                maxY = Math.Max(maxY, ellipse.Y);
            }
        }

        public static void MinMax(IEnumerable<TextItem> texts, ref double minX, ref double minY, ref double maxX, ref double maxY)
        {
            foreach (var text in texts)
            {
                minX = Math.Min(minX, text.X);
                minY = Math.Min(minY, text.Y);
                maxX = Math.Max(maxX, text.X);
                maxY = Math.Max(maxY, text.Y);
            }
        }

        #endregion

        #region Move

        public static void Move(IEnumerable<BlockItem> blocks, double x, double y)
        {
            foreach (var block in blocks)
            {
                Move(block, x, y);
            }
        }

        public static void Move(BlockItem block, double x, double y)
        {
            Move(block.Lines, x, y);
            Move(block.Rectangles, x, y);
            Move(block.Ellipses, x, y);
            Move(block.Texts, x, y);
            Move(block.Blocks, x, y);
        }

        public static void Move(IEnumerable<LineItem> lines, double x, double y)
        {
            foreach (var line in lines)
            {
                Move(line, x, y);
            }
        }

        public static void Move(IEnumerable<RectangleItem> rectangles, double x, double y)
        {
            foreach (var rectangle in rectangles)
            {
                Move(rectangle, x, y);
            }
        }

        public static void Move(IEnumerable<EllipseItem> ellipses, double x, double y)
        {
            foreach (var ellipse in ellipses)
            {
                Move(ellipse, x, y);
            }
        }

        public static void Move(IEnumerable<TextItem> texts, double x, double y)
        {
            foreach (var text in texts)
            {
                Move(text, x, y);
            }
        }

        public static void Move(LineItem line, double x, double y)
        {
            line.X1 += x;
            line.Y1 += y;
            line.X2 += x;
            line.Y2 += y;
        }

        public static void Move(RectangleItem rectangle, double x, double y)
        {
            rectangle.X += x;
            rectangle.Y += y;
        }

        public static void Move(EllipseItem ellipse, double x, double y)
        {
            ellipse.X += x;
            ellipse.Y += y;
        }

        public static void Move(TextItem text, double x, double y)
        {
            text.X += x;
            text.Y += y;
        }

        #endregion
    }

    #endregion

    #region ILibrary

    public interface ILibrary
    {
        BlockItem GetSelected();
        void SetSelected(BlockItem block);
        IEnumerable<BlockItem> GetSource();
        void SetSource(IEnumerable<BlockItem> source);
    }

    #endregion

    #region ITextEditor

    public interface ITextEditor
    {
        void Show(Action<string> ok, Action cancel, string label, string text);
    }

    #endregion

    #region Block Model

    public class Block
    {
        public string Name { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }
        public Color Backgroud { get; set; }
        public List<Line> Lines { get; set; }
        public List<Rectangle> Rectangles { get; set; }
        public List<Ellipse> Ellipses { get; set; }
        public List<Grid> Texts { get; set; }
        public List<Block> Blocks { get; set; }
        public void Init()
        {
            Lines = new List<Line>();
            Rectangles = new List<Rectangle>();
            Ellipses = new List<Ellipse>();
            Texts = new List<Grid>();
            Blocks = new List<Block>();
        }
        public void ReInit()
        {
            if (Lines == null)
            {
                Lines = new List<Line>(); 
            }

            if (Rectangles == null)
            {
                Rectangles = new List<Rectangle>(); 
            }

            if (Ellipses == null)
            {
                Ellipses = new List<Ellipse>(); 
            }

            if (Texts == null)
            {
                Texts = new List<Grid>(); 
            }

            if (Blocks == null)
            {
                Blocks = new List<Block>(); 
            }
        }
    }

    #endregion

    #region Block Editor

    public static class BlockEditor
    {
        #region Serialize

        private static ItemColor ToItemColor(Brush brush)
        {
            var color = (brush as SolidColorBrush).Color;
            return ToItemColor(color);
        }

        private static ItemColor ToItemColor(Color color)
        {
            return new ItemColor()
            {
                Alpha = color.A,
                Red = color.R,
                Green = color.G,
                Blue = color.B
            };
        }

        public static LineItem SerializeLine(Line line)
        {
            var lineItem = new LineItem();

            lineItem.Id = 0;
            lineItem.X1 = line.X1;
            lineItem.Y1 = line.Y1;
            lineItem.X2 = line.X2;
            lineItem.Y2 = line.Y2;
            lineItem.Stroke = ToItemColor(line.Stroke);

            return lineItem;
        }

        public static RectangleItem SerializeRectangle(Rectangle rectangle)
        {
            var rectangleItem = new RectangleItem();

            rectangleItem.Id = 0;
            rectangleItem.X = Canvas.GetLeft(rectangle);
            rectangleItem.Y = Canvas.GetTop(rectangle);
            rectangleItem.Width = rectangle.Width;
            rectangleItem.Height = rectangle.Height;
            rectangleItem.IsFilled = rectangle.Fill == Brushes.Transparent ? false : true;
            rectangleItem.Stroke = ToItemColor(rectangle.Stroke);
            rectangleItem.Fill = ToItemColor(rectangle.Fill);

            return rectangleItem;
        }

        public static EllipseItem SerializeEllipse(Ellipse ellipse)
        {
            var ellipseItem = new EllipseItem();

            ellipseItem.Id = 0;
            ellipseItem.X = Canvas.GetLeft(ellipse);
            ellipseItem.Y = Canvas.GetTop(ellipse);
            ellipseItem.Width = ellipse.Width;
            ellipseItem.Height = ellipse.Height;
            ellipseItem.IsFilled = ellipse.Fill == Brushes.Transparent ? false : true;
            ellipseItem.Stroke = ToItemColor(ellipse.Stroke);
            ellipseItem.Fill = ToItemColor(ellipse.Fill);

            return ellipseItem;
        }

        public static TextItem SerializeText(Grid text)
        {
            var textItem = new TextItem();

            textItem.Id = 0;
            textItem.X = Canvas.GetLeft(text);
            textItem.Y = Canvas.GetTop(text);
            textItem.Width = text.Width;
            textItem.Height = text.Height;

            var tb = BlockEditor.GetTextBlock(text);
            textItem.Text = tb.Text;
            textItem.HAlign = (int)tb.HorizontalAlignment;
            textItem.VAlign = (int)tb.VerticalAlignment;
            textItem.Size = tb.FontSize;
            textItem.Foreground = ToItemColor(tb.Foreground);
            textItem.Backgroud = ToItemColor(tb.Background);

            return textItem;
        }

        public static BlockItem SerializeBlock(Block parent)
        {
            var blockItem = new BlockItem();
            blockItem.Init(0, parent.Name);
            blockItem.Width = 0;
            blockItem.Height = 0;
            blockItem.Backgroud = ToItemColor(parent.Backgroud);

            foreach (var line in parent.Lines)
            {
                blockItem.Lines.Add(SerializeLine(line));
            }

            foreach (var rectangle in parent.Rectangles)
            {
                blockItem.Rectangles.Add(SerializeRectangle(rectangle));
            }

            foreach (var ellipse in parent.Ellipses)
            {
                blockItem.Ellipses.Add(SerializeEllipse(ellipse));
            }

            foreach (var text in parent.Texts)
            {
                blockItem.Texts.Add(SerializeText(text));
            }

            foreach (var block in parent.Blocks)
            {
                blockItem.Blocks.Add(SerializeBlock(block));
            }

            return blockItem;
        }

        public static BlockItem SerializerBlockContents(Block parent, int id, string name)
        {
            var lines = parent.Lines;
            var rectangles = parent.Rectangles;
            var ellipses = parent.Ellipses;
            var texts = parent.Texts;
            var blocks = parent.Blocks;

            var sheet = new BlockItem() { Backgroud = new ItemColor() };
            sheet.Init(id, name);

            if (lines != null)
            {
                foreach (var line in lines)
                {
                    sheet.Lines.Add(SerializeLine(line));
                }
            }

            if (rectangles != null)
            {
                foreach (var rectangle in rectangles)
                {
                    sheet.Rectangles.Add(SerializeRectangle(rectangle));
                }
            }

            if (ellipses != null)
            {
                foreach (var ellipse in ellipses)
                {
                    sheet.Ellipses.Add(SerializeEllipse(ellipse));
                }
            }

            if (texts != null)
            {
                foreach (var text in texts)
                {
                    sheet.Texts.Add(SerializeText(text));
                }
            }

            if (blocks != null)
            {
                foreach (var block in blocks)
                {
                    sheet.Blocks.Add(SerializeBlock(block));
                }
            }

            return sheet;
        }

        #endregion

        #region Deserialize

        public static Line DeserializeLineItem(ISheet sheet, Block parent, LineItem lineItem, double thickness)
        {
            var line = BlockEditor.CreateLine(thickness, lineItem.X1, lineItem.Y1, lineItem.X2, lineItem.Y2);

            if (parent != null)
            {
                parent.Lines.Add(line);
            }

            if (sheet != null)
            {
                sheet.Add(line);
            }

            return line;
        }

        public static Rectangle DeserializeRectangleItem(ISheet sheet, Block parent, RectangleItem rectangleItem, double thickness)
        {
            var rectangle = BlockEditor.CreateRectangle(thickness, rectangleItem.X, rectangleItem.Y, rectangleItem.Width, rectangleItem.Height, rectangleItem.IsFilled);

            if (parent != null)
            {
                parent.Rectangles.Add(rectangle);
            }

            if (sheet != null)
            {
                sheet.Add(rectangle);
            }

            return rectangle;
        }

        public static Ellipse DeserializeEllipseItem(ISheet sheet, Block parent, EllipseItem ellipseItem, double thickness)
        {
            var ellipse = BlockEditor.CreateEllipse(thickness, ellipseItem.X, ellipseItem.Y, ellipseItem.Width, ellipseItem.Height, ellipseItem.IsFilled);

            if (parent != null)
            {
                parent.Ellipses.Add(ellipse);
            }

            if (sheet != null)
            {
                sheet.Add(ellipse); 
            }
            
            return ellipse;
        }

        public static Grid DeserializeTextItem(ISheet sheet, Block parent, TextItem textItem)
        {
            var text = BlockEditor.CreateText(textItem.Text,
                textItem.X, textItem.Y,
                textItem.Width, textItem.Height,
                (HorizontalAlignment)textItem.HAlign,
                (VerticalAlignment)textItem.VAlign,
                textItem.Size);
            
            if (parent != null)
            {
                parent.Texts.Add(text);
            }

            if (sheet != null)
            {
                sheet.Add(text);
            }

            return text;
        }

        public static Block DeserializeBlockItem(ISheet sheet, Block parent, BlockItem blockItem, bool select, double thickness)
        {
            var block = new Block() { Name = blockItem.Name };
            block.Init();

            foreach (var textItem in blockItem.Texts)
            {
                var text = DeserializeTextItem(sheet, block, textItem);

                if (select)
                {
                    BlockEditor.GetTextBlock(text).Foreground = Brushes.Red;
                }
            }

            foreach (var lineItem in blockItem.Lines)
            {
                var line = DeserializeLineItem(sheet, block, lineItem, thickness);

                if (select)
                {
                    line.Stroke = Brushes.Red;
                }
            }

            foreach (var rectangleItem in blockItem.Rectangles)
            {
                var rectangle = DeserializeRectangleItem(sheet, block, rectangleItem, thickness);

                if (select)
                {
                    rectangle.Stroke = Brushes.Red;
                    rectangle.Fill = rectangleItem.IsFilled ? Brushes.Red : Brushes.Transparent;
                }
            }

            foreach (var ellipseItem in blockItem.Ellipses)
            {
                var ellipse = DeserializeEllipseItem(sheet, block, ellipseItem, thickness);

                if (select)
                {
                    ellipse.Stroke = Brushes.Red;
                    ellipse.Fill = ellipseItem.IsFilled ? Brushes.Red : Brushes.Transparent;
                }
            }

            foreach (var childBlockItem in blockItem.Blocks)
            {
                DeserializeBlockItem(sheet, block, childBlockItem, select, thickness);
            }

            if (parent != null)
            {
                parent.Blocks.Add(block); 
            }

            return block;
        }

        #endregion

        #region Create

        public static TextBlock GetTextBlock(Grid text)
        {
            return text.Children[0] as TextBlock;
        }

        public static Grid CreateText(string text,
            double x, double y, double width, double height,
            HorizontalAlignment halign, VerticalAlignment valign,
            double fontSize)
        {
            var grid = new Grid();
            grid.Background = Brushes.Transparent;
            grid.Width = width;
            grid.Height = height;
            Canvas.SetLeft(grid, x);
            Canvas.SetTop(grid, y);

            var tb = new TextBlock();
            tb.HorizontalAlignment = halign;
            tb.VerticalAlignment = valign;
            tb.Background = Brushes.Transparent;
            tb.Foreground = Brushes.Black;
            tb.FontSize = fontSize;
            tb.FontFamily = new FontFamily("Calibri");
            tb.Text = text;

            grid.Children.Add(tb);

            return grid;
        }

        public static Line CreateLine(double thickness, double x1, double y1, double x2, double y2)
        {
            var line = new Line()
            {
                Stroke = Brushes.Black,
                StrokeThickness = thickness,
                StrokeStartLineCap = PenLineCap.Round,
                StrokeEndLineCap = PenLineCap.Round,
                X1 = x1,
                Y1 = y1,
                X2 = x2,
                Y2 = y2
            };

            return line;
        }

        public static Rectangle CreateRectangle(double thickness, double x, double y, double width, double height, bool isFilled)
        {
            var rectangle = new Rectangle()
            {
                Fill = isFilled ? Brushes.Black : Brushes.Transparent,
                Stroke = Brushes.Black,
                StrokeThickness = thickness,
                StrokeStartLineCap = PenLineCap.Round,
                StrokeEndLineCap = PenLineCap.Round,
                Width = width,
                Height = height
            };

            Canvas.SetLeft(rectangle, x);
            Canvas.SetTop(rectangle, y);

            return rectangle;
        }

        public static Ellipse CreateEllipse(double thickness, double x, double y, double width, double height, bool isFilled)
        {
            var ellipse = new Ellipse()
            {
                Fill = isFilled ? Brushes.Black : Brushes.Transparent,
                Stroke = Brushes.Black,
                StrokeThickness = thickness,
                StrokeStartLineCap = PenLineCap.Round,
                StrokeEndLineCap = PenLineCap.Round,
                Width = width,
                Height = height
            };

            Canvas.SetLeft(ellipse, x);
            Canvas.SetTop(ellipse, y);

            return ellipse;
        }

        public static Rectangle CreateSelectionRectangle(double thickness, double x, double y, double width, double height)
        {
            var rect = new Rectangle()
            {
                Fill = new SolidColorBrush(Color.FromArgb(0x3A, 0x00, 0x00, 0xFF)),
                Stroke = new SolidColorBrush(Color.FromArgb(0x7F, 0x00, 0x00, 0xFF)),
                StrokeThickness = thickness,
                StrokeStartLineCap = PenLineCap.Round,
                StrokeEndLineCap = PenLineCap.Round,
                Width = width,
                Height = height
            };

            Canvas.SetLeft(rect, x);
            Canvas.SetTop(rect, y);

            return rect;
        }

        #endregion

        #region Insert

        public static void InsertLines(ISheet sheet, IEnumerable<LineItem> lineItems, Block parent, Block selected, bool select, double thickness)
        {
            if (select)
            {
                selected.Lines = new List<Line>();
            }

            foreach (var lineItem in lineItems)
            {
                var line = DeserializeLineItem(sheet, parent, lineItem, thickness);

                if (select)
                {
                    line.Stroke = Brushes.Red;
                    selected.Lines.Add(line);
                }
            }
        }

        public static void InsertRectangles(ISheet sheet, IEnumerable<RectangleItem> rectangleItems, Block parent, Block selected, bool select, double thickness)
        {
            if (select)
            {
                selected.Rectangles = new List<Rectangle>();
            }

            foreach (var rectangleItem in rectangleItems)
            {
                var rectangle = DeserializeRectangleItem(sheet, parent, rectangleItem, thickness);

                if (select)
                {
                    rectangle.Stroke = Brushes.Red;
                    rectangle.Fill = rectangleItem.IsFilled ? Brushes.Red : Brushes.Transparent;
                    selected.Rectangles.Add(rectangle);
                }
            }
        }

        public static void InsertEllipses(ISheet sheet, IEnumerable<EllipseItem> ellipseItems, Block parent, Block selected, bool select, double thickness)
        {
            if (select)
            {
                selected.Ellipses = new List<Ellipse>();
            }

            foreach (var ellipseItem in ellipseItems)
            {
                var ellipse = DeserializeEllipseItem(sheet, parent, ellipseItem, thickness);

                if (select)
                {
                    ellipse.Stroke = Brushes.Red;
                    ellipse.Fill = ellipseItem.IsFilled ? Brushes.Red : Brushes.Transparent;
                    selected.Ellipses.Add(ellipse);
                }
            }
        }

        public static void InsertTexts(ISheet sheet, IEnumerable<TextItem> textItems, Block parent, Block selected, bool select, double thickness)
        {
            if (select)
            {
                selected.Texts = new List<Grid>();
            }

            foreach (var textItem in textItems)
            {
                var text = DeserializeTextItem(sheet, parent, textItem);

                if (select)
                {
                    BlockEditor.GetTextBlock(text).Foreground = Brushes.Red;
                    selected.Texts.Add(text);
                }
            }
        }

        public static void InsertBlocks(ISheet sheet, IEnumerable<BlockItem> blockItems, Block parent, Block selected, bool select, double thickness)
        {
            if (select)
            {
                selected.Blocks = new List<Block>();
            }

            foreach (var blockItem in blockItems)
            {
                var block = DeserializeBlockItem(sheet, parent, blockItem, select, thickness);

                if (select)
                {
                    selected.Blocks.Add(block);
                }
            }
        }

        public static void InsertBlockContents(ISheet sheet, BlockItem blockItem, Block logic, Block selected, bool select, double thickness)
        {
            InsertTexts(sheet, blockItem.Texts, logic, selected, select, thickness);
            InsertLines(sheet, blockItem.Lines, logic, selected, select, thickness);
            InsertRectangles(sheet, blockItem.Rectangles, logic, selected, select, thickness);
            InsertEllipses(sheet, blockItem.Ellipses, logic, selected, select, thickness);
            InsertBlocks(sheet, blockItem.Blocks, logic, selected, select, thickness);
        }

        public static void InsertBrokenBlock(ISheet sheet, BlockItem blockItem, Block logic, Block selected, bool select, double thickness)
        {
            InsertTexts(sheet, blockItem.Texts, logic, selected, select, thickness);
            InsertLines(sheet, blockItem.Lines, logic, selected, select, thickness);
            InsertRectangles(sheet, blockItem.Rectangles, logic, selected, select, thickness);
            InsertEllipses(sheet, blockItem.Ellipses, logic, selected, select, thickness);

            foreach (var block in blockItem.Blocks)
            {
                InsertTexts(sheet, block.Texts, logic, selected, select, thickness);
                InsertLines(sheet, block.Lines, logic, selected, select, thickness);
                InsertRectangles(sheet, block.Rectangles, logic, selected, select, thickness);
                InsertEllipses(sheet, block.Ellipses, logic, selected, select, thickness);
                InsertBlocks(sheet, block.Blocks, logic, selected, select, thickness);
            }
        }

        #endregion

        #region Remove

        public static void RemoveLines(ISheet sheet, IEnumerable<Line> lines)
        {
            foreach (var line in lines)
            {
                sheet.Remove(line);
            }
        }

        public static void RemoveRectangles(ISheet sheet, IEnumerable<Rectangle> rectangles)
        {
            foreach (var rectangle in rectangles)
            {
                sheet.Remove(rectangle);
            }
        }

        public static void RemoveEllipses(ISheet sheet, IEnumerable<Ellipse> ellipses)
        {
            foreach (var ellipse in ellipses)
            {
                sheet.Remove(ellipse);
            }
        }

        public static void RemoveTexts(ISheet sheet, IEnumerable<Grid> texts)
        {
            foreach (var text in texts)
            {
                sheet.Remove(text);
            }
        }

        public static void RemoveBlocks(ISheet sheet, IEnumerable<Block> blocks)
        {
            foreach (var block in blocks)
            {
                RemoveLines(sheet, block.Lines);
                RemoveRectangles(sheet, block.Rectangles);
                RemoveEllipses(sheet, block.Ellipses);
                RemoveTexts(sheet, block.Texts);
                RemoveBlocks(sheet, block.Blocks);
            }
        }

        public static void Remove(ISheet sheet, Block parent, Block selected)
        {
            if (selected.Lines != null)
            {
                RemoveLines(sheet, selected.Lines);

                foreach (var line in selected.Lines)
                {
                    parent.Lines.Remove(line);
                }

                selected.Lines = null;
            }

            if (selected.Rectangles != null)
            {
                RemoveRectangles(sheet, selected.Rectangles);

                foreach (var rectangle in selected.Rectangles)
                {
                    parent.Rectangles.Remove(rectangle);
                }

                selected.Rectangles = null;
            }

            if (selected.Ellipses != null)
            {
                RemoveEllipses(sheet, selected.Ellipses);

                foreach (var ellipse in selected.Ellipses)
                {
                    parent.Ellipses.Remove(ellipse);
                }

                selected.Ellipses = null;
            }

            if (selected.Texts != null)
            {
                RemoveTexts(sheet, selected.Texts);

                foreach (var text in selected.Texts)
                {
                    parent.Texts.Remove(text);
                }

                selected.Texts = null;
            }

            if (selected.Blocks != null)
            {
                foreach (var block in selected.Blocks)
                {
                    RemoveLines(sheet, block.Lines);
                    RemoveRectangles(sheet, block.Rectangles);
                    RemoveEllipses(sheet, block.Ellipses);
                    RemoveTexts(sheet, block.Texts);
                    RemoveBlocks(sheet, block.Blocks);

                    parent.Blocks.Remove(block);
                }

                selected.Blocks = null;
            }
        }

        #endregion

        #region Move

        public static void Move(double x, double y, Block block)
        {
            if (block.Lines != null)
            {
                MoveLines(x, y, block.Lines);
            }

            if (block.Rectangles != null)
            {
                MoveRectangles(x, y, block.Rectangles);
            }

            if (block.Ellipses != null)
            {
                MoveEllipses(x, y, block.Ellipses);
            }

            if (block.Texts != null)
            {
                MoveTexts(x, y, block.Texts);
            }

            if (block.Blocks != null)
            {
                MoveBlocks(x, y, block.Blocks);
            }
        }

        public static void MoveLines(double x, double y, IEnumerable<Line> lines)
        {
            foreach (var line in lines)
            {
                line.X1 += x;
                line.Y1 += y;
                line.X2 += x;
                line.Y2 += y;
            }
        }

        public static void MoveRectangles(double x, double y, IEnumerable<Rectangle> rectangles)
        {
            foreach (var rectangle in rectangles)
            {
                Canvas.SetLeft(rectangle, Canvas.GetLeft(rectangle) + x);
                Canvas.SetTop(rectangle, Canvas.GetTop(rectangle) + y);
            }
        }

        public static void MoveEllipses(double x, double y, IEnumerable<Ellipse> ellipses)
        {
            foreach (var ellipse in ellipses)
            {
                Canvas.SetLeft(ellipse, Canvas.GetLeft(ellipse) + x);
                Canvas.SetTop(ellipse, Canvas.GetTop(ellipse) + y);
            }
        }

        public static void MoveTexts(double x, double y, IEnumerable<Grid> texts)
        {
            foreach (var text in texts)
            {
                Canvas.SetLeft(text, Canvas.GetLeft(text) + x);
                Canvas.SetTop(text, Canvas.GetTop(text) + y);
            }
        }

        public static void MoveBlocks(double x, double y, IEnumerable<Block> blocks)
        {
            foreach (var block in blocks)
            {
                MoveLines(x, y, block.Lines);
                MoveRectangles(x, y, block.Rectangles);
                MoveEllipses(x, y, block.Ellipses);
                MoveTexts(x, y, block.Texts);
                MoveBlocks(x, y, block.Blocks);
            }
        }

        #endregion

        #region Selection

        public static bool HaveSelected(Block selected)
        {
            return (selected.Lines != null
                || selected.Rectangles != null
                || selected.Ellipses != null
                || selected.Texts != null
                || selected.Blocks != null);
        }

        public static bool HaveOneTextSelected(Block selected)
        {
            return (selected.Lines == null
                && selected.Rectangles == null
                && selected.Ellipses == null
                && selected.Blocks == null
                && selected.Texts != null
                && selected.Texts.Count == 1);
        }

        public static void Select(Block parent)
        {
            foreach (var line in parent.Lines)
            {
                line.Stroke = Brushes.Red;
            }

            foreach (var rectangle in parent.Rectangles)
            {
                rectangle.Stroke = Brushes.Red;
                rectangle.Fill = rectangle.Fill == Brushes.Transparent ? Brushes.Transparent : Brushes.Red;
            }

            foreach (var ellipse in parent.Ellipses)
            {
                ellipse.Stroke = Brushes.Red;
                ellipse.Fill = ellipse.Fill == Brushes.Transparent ? Brushes.Transparent : Brushes.Red;
            }

            foreach (var text in parent.Texts)
            {
                BlockEditor.GetTextBlock(text).Foreground = Brushes.Red;
            }

            foreach (var block in parent.Blocks)
            {
                Select(block);
            }
        }

        public static void Deselect(Block parent)
        {
            if (parent.Lines != null)
            {
                foreach (var line in parent.Lines)
                {
                    line.Stroke = Brushes.Black;
                }
            }

            if (parent.Rectangles != null)
            {
                foreach (var rectangle in parent.Rectangles)
                {
                    rectangle.Stroke = Brushes.Black;
                    rectangle.Fill = rectangle.Fill == Brushes.Transparent ? Brushes.Transparent : Brushes.Black;
                }
            }

            if (parent.Ellipses != null)
            {
                foreach (var ellipse in parent.Ellipses)
                {
                    ellipse.Stroke = Brushes.Black;
                    ellipse.Fill = ellipse.Fill == Brushes.Transparent ? Brushes.Transparent : Brushes.Black;
                }
            }

            if (parent.Texts != null)
            {
                foreach (var text in parent.Texts)
                {
                    BlockEditor.GetTextBlock(text).Foreground = Brushes.Black;
                }
            }

            if (parent.Blocks != null)
            {
                foreach (var block in parent.Blocks)
                {
                    Deselect(block);
                }
            }
        }

        public static void SelectAll(Block selected, Block logic)
        {
            selected.Init();

            foreach (var line in logic.Lines)
            {
                line.Stroke = Brushes.Red;
                selected.Lines.Add(line);
            }

            foreach (var rectangle in logic.Rectangles)
            {
                rectangle.Stroke = Brushes.Red;
                rectangle.Fill = rectangle.Fill == Brushes.Transparent ? Brushes.Transparent : Brushes.Red;
                selected.Rectangles.Add(rectangle);
            }

            foreach (var ellipse in logic.Ellipses)
            {
                ellipse.Stroke = Brushes.Red;
                ellipse.Fill = ellipse.Fill == Brushes.Transparent ? Brushes.Transparent : Brushes.Red;
                selected.Ellipses.Add(ellipse);
            }

            foreach (var text in logic.Texts)
            {
                BlockEditor.GetTextBlock(text).Foreground = Brushes.Red;
                selected.Texts.Add(text);
            }

            foreach (var parent in logic.Blocks)
            {
                foreach (var line in parent.Lines)
                {
                    line.Stroke = Brushes.Red;
                }

                foreach (var rectangle in parent.Rectangles)
                {
                    rectangle.Stroke = Brushes.Red;
                    rectangle.Fill = rectangle.Fill == Brushes.Transparent ? Brushes.Transparent : Brushes.Red;
                }

                foreach (var ellipse in parent.Ellipses)
                {
                    ellipse.Stroke = Brushes.Red;
                    ellipse.Fill = ellipse.Fill == Brushes.Transparent ? Brushes.Transparent : Brushes.Red;
                }

                foreach (var text in parent.Texts)
                {
                    BlockEditor.GetTextBlock(text).Foreground = Brushes.Red;
                }

                foreach (var block in parent.Blocks)
                {
                    Select(block);
                }

                selected.Blocks.Add(parent);
            }
        }

        public static void DeselectAll(Block selected)
        {
            if (selected.Lines != null)
            {
                foreach (var line in selected.Lines)
                {
                    line.Stroke = Brushes.Black;
                }

                selected.Lines = null;
            }

            if (selected.Rectangles != null)
            {
                foreach (var rectangle in selected.Rectangles)
                {
                    rectangle.Stroke = Brushes.Black;
                    rectangle.Fill = rectangle.Fill == Brushes.Transparent ? Brushes.Transparent : Brushes.Black;
                }

                selected.Rectangles = null;
            }

            if (selected.Ellipses != null)
            {
                foreach (var ellipse in selected.Ellipses)
                {
                    ellipse.Stroke = Brushes.Black;
                    ellipse.Fill = ellipse.Fill == Brushes.Transparent ? Brushes.Transparent : Brushes.Black;
                }

                selected.Ellipses = null;
            }

            if (selected.Texts != null)
            {
                foreach (var text in selected.Texts)
                {
                    BlockEditor.GetTextBlock(text).Foreground = Brushes.Black;
                }

                selected.Texts = null;
            }

            if (selected.Blocks != null)
            {
                foreach (var parent in selected.Blocks)
                {
                    foreach (var line in parent.Lines)
                    {
                        line.Stroke = Brushes.Black;
                    }

                    foreach (var rectangle in parent.Rectangles)
                    {
                        rectangle.Stroke = Brushes.Black;
                        rectangle.Fill = rectangle.Fill == Brushes.Transparent ? Brushes.Transparent : Brushes.Black;
                    }

                    foreach (var ellipse in parent.Ellipses)
                    {
                        ellipse.Stroke = Brushes.Black;
                        ellipse.Fill = ellipse.Fill == Brushes.Transparent ? Brushes.Transparent : Brushes.Black;
                    }

                    foreach (var text in parent.Texts)
                    {
                        BlockEditor.GetTextBlock(text).Foreground = Brushes.Black;
                    }

                    foreach (var block in parent.Blocks)
                    {
                        Deselect(block);
                    }
                }

                selected.Blocks = null;
            }
        }

        #endregion

        #region HitTest

        public static bool HitTestLines(IEnumerable<Line> lines, Block selected, Rect rect, bool onlyFirst, bool select)
        {
            foreach (var line in lines)
            {
                var bounds = VisualTreeHelper.GetContentBounds(line);
                if (rect.IntersectsWith(bounds))
                {
                    if (select)
                    {
                        if (line.Stroke != Brushes.Red)
                        {
                            line.Stroke = Brushes.Red;
                            selected.Lines.Add(line);
                        }
                        else
                        {
                            line.Stroke = Brushes.Black;
                            selected.Lines.Remove(line);
                        }
                    }

                    if (onlyFirst)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public static bool HitTestRectangles(IEnumerable<Rectangle> rectangles, Block selected, Rect rect, bool onlyFirst, bool select, UIElement relative)
        {
            foreach (var rectangle in rectangles)
            {
                var bounds = VisualTreeHelper.GetContentBounds(rectangle);
                var offset = rectangle.TranslatePoint(new Point(0, 0), relative);
                bounds.Offset(offset.X, offset.Y);
                if (rect.IntersectsWith(bounds))
                {
                    if (select)
                    {
                        if (rectangle.Stroke != Brushes.Red)
                        {
                            rectangle.Stroke = Brushes.Red;
                            rectangle.Fill = rectangle.Fill == Brushes.Transparent ? Brushes.Transparent : Brushes.Red;
                            selected.Rectangles.Add(rectangle);
                        }
                        else
                        {
                            rectangle.Stroke = Brushes.Black;
                            rectangle.Fill = rectangle.Fill == Brushes.Transparent ? Brushes.Transparent : Brushes.Black;
                            selected.Rectangles.Add(rectangle);
                        }
                    }

                    if (onlyFirst)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public static bool HitTestEllipses(IEnumerable<Ellipse> ellipses, Block selected, Rect rect, bool onlyFirst, bool select, UIElement relative)
        {
            foreach (var ellipse in ellipses)
            {
                var bounds = VisualTreeHelper.GetContentBounds(ellipse);
                var offset = ellipse.TranslatePoint(new Point(0, 0), relative);
                bounds.Offset(offset.X, offset.Y);
                if (rect.IntersectsWith(bounds))
                {
                    if (select)
                    {
                        if (ellipse.Stroke != Brushes.Red)
                        {
                            ellipse.Stroke = Brushes.Red;
                            ellipse.Fill = ellipse.Fill == Brushes.Transparent ? Brushes.Transparent : Brushes.Red;
                            selected.Ellipses.Add(ellipse); 
                        }
                        else
                        {
                            ellipse.Stroke = Brushes.Black;
                            ellipse.Fill = ellipse.Fill == Brushes.Transparent ? Brushes.Transparent : Brushes.Black;
                            selected.Ellipses.Remove(ellipse);
                        }
                    }

                    if (onlyFirst)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public static bool HitTestTexts(IEnumerable<Grid> texts, Block selected, Rect rect, bool onlyFirst, bool select, UIElement relative)
        {
            foreach (var text in texts)
            {
                var bounds = VisualTreeHelper.GetContentBounds(text);
                var offset = text.TranslatePoint(new Point(0, 0), relative);
                bounds.Offset(offset.X, offset.Y);
                if (rect.IntersectsWith(bounds))
                {
                    if (select)
                    {
                        var tb = BlockEditor.GetTextBlock(text);
                        if (tb.Foreground != Brushes.Red)
                        {
                            tb.Foreground = Brushes.Red;
                            selected.Texts.Add(text);
                        }
                        else
                        {
                            tb.Foreground = Brushes.Black;
                            selected.Texts.Remove(text);
                        }
                    }

                    if (onlyFirst)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public static bool HitTestBlocks(IEnumerable<Block> blocks, Block selected, Rect rect, bool onlyFirst, bool select, bool selectInsideBlock, UIElement relative)
        {
            foreach (var block in blocks)
            {
                bool result = HitTestBlock(block, selected, rect, true, selectInsideBlock, relative);

                if (result)
                {
                    if (select && !selectInsideBlock)
                    {
                        if (!selected.Blocks.Contains(block))
                        {
                            selected.Blocks.Add(block);
                            Select(block);
                        }
                        else
                        {
                            selected.Blocks.Remove(block);
                            Deselect(block);
                        }
                    }

                    if (onlyFirst)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public static bool HitTestBlock(Block parent, Block selected, Rect rect, bool onlyFirst, bool selectInsideBlock, UIElement relative)
        {
            bool result = false;

            result = HitTestTexts(parent.Texts, selected, rect, onlyFirst, selectInsideBlock, relative);
            if (result && onlyFirst)
            {
                return true;
            }

            result = HitTestLines(parent.Lines, selected, rect, onlyFirst, selectInsideBlock);
            if (result && onlyFirst)
            {
                return true;
            }

            result = HitTestRectangles(parent.Rectangles, selected, rect, onlyFirst, selectInsideBlock, relative);
            if (result && onlyFirst)
            {
                return true;
            }

            result = HitTestEllipses(parent.Ellipses, selected, rect, onlyFirst, selectInsideBlock, relative);
            if (result && onlyFirst)
            {
                return true;
            }

            result = HitTestBlocks(parent.Blocks, selected, rect, onlyFirst, false, selectInsideBlock, relative);
            if (result && onlyFirst)
            {
                return true;
            }

            return false;
        }

        public static bool HitTest(ISheet sheet, Block parent, Block selected, Point p, double size, bool selectInsideBlock, bool resetSelected)
        {
            if (resetSelected)
            {
                selected.Init();
            }
            else
            {
                selected.ReInit();
            }

            var rect = new Rect(p.X - size, p.Y - size, 2 * size, 2 * size);

            if (parent.Texts != null)
            {
                bool result = HitTestTexts(parent.Texts, selected, rect, true, true, sheet.GetParent());
                if (result)
                {
                    HitTestClean(selected);
                    return true;
                }
            }

            if (parent.Lines != null)
            {
                bool result = HitTestLines(parent.Lines, selected, rect, true, true);
                if (result)
                {
                    HitTestClean(selected);
                    return true;
                }
            }

            if (parent.Rectangles != null)
            {
                bool result = HitTestRectangles(parent.Rectangles, selected, rect, true, true, sheet.GetParent());
                if (result)
                {
                    HitTestClean(selected);
                    return true;
                }
            }

            if (parent.Ellipses != null)
            {
                bool result = HitTestEllipses(parent.Ellipses, selected, rect, true, true, sheet.GetParent());
                if (result)
                {
                    HitTestClean(selected);
                    return true;
                }
            }

            if (parent.Blocks != null)
            {
                bool result = HitTestBlocks(parent.Blocks, selected, rect, true, true, selectInsideBlock, sheet.GetParent());
                if (result)
                {
                    HitTestClean(selected);
                    return true;
                }
            }

            HitTestClean(selected);
            return false;
        }

        public static void HitTestSelectionRect(ISheet sheet, Block parent, Block selected, Rect rect, bool resetSelected)
        {
            if (resetSelected)
            {
                selected.Init();
            }
            else
            {
                selected.ReInit();
            }

            if (parent.Lines != null)
            {
                HitTestLines(parent.Lines, selected, rect, false, true);
            }

            if (parent.Rectangles != null)
            {
                HitTestRectangles(parent.Rectangles, selected, rect, false, true, sheet.GetParent());
            }

            if (parent.Ellipses != null)
            {
                HitTestEllipses(parent.Ellipses, selected, rect, false, true, sheet.GetParent());
            }

            if (parent.Texts != null)
            {
                HitTestTexts(parent.Texts, selected, rect, false, true, sheet.GetParent());
            }

            if (parent.Blocks != null)
            {
                HitTestBlocks(parent.Blocks, selected, rect, false, true, false, sheet.GetParent());
            }

            HitTestClean(selected);
        }

        private static void HitTestClean(Block selected)
        {
            if (selected.Lines != null && selected.Lines.Count == 0)
            {
                selected.Lines = null;
            }

            if (selected.Rectangles != null && selected.Rectangles.Count == 0)
            {
                selected.Rectangles = null;
            }

            if (selected.Ellipses != null && selected.Ellipses.Count == 0)
            {
                selected.Ellipses = null;
            }

            if (selected.Texts != null && selected.Texts.Count == 0)
            {
                selected.Texts = null;
            }

            if (selected.Blocks != null && selected.Blocks.Count == 0)
            {
                selected.Blocks = null;
            }
        }

        #endregion
    }

    #endregion

    #region ISheet Model

    public interface ISheet
    {
        FrameworkElement GetParent();
        void Add(UIElement element);
        void Remove(UIElement element);
        void Capture();
        void ReleaseCapture();
        bool IsCaptured { get; }
    }

    #endregion

    #region ISheet Canvas

    public class CanvasSheet : ISheet
    {
        #region Fields

        private Canvas canvas = null;

        #endregion

        #region Constructor

        public CanvasSheet(Canvas canvas)
        {
            this.canvas = canvas;
        }

        #endregion

        #region ISheet

        public FrameworkElement GetParent()
        {
            return canvas;
        }

        public void Add(UIElement element)
        {
            canvas.Children.Add(element);
        }

        public void Remove(UIElement element)
        {
            canvas.Children.Remove(element);
        }

        public void Capture()
        {
            canvas.CaptureMouse();
        }

        public void ReleaseCapture()
        {
            canvas.ReleaseMouseCapture();
        }

        public bool IsCaptured
        {
            get { return canvas.IsMouseCaptured; }
        }

        #endregion
    }

    #endregion

    public partial class SheetControl : UserControl
    {
        #region Mode

        public enum Mode
        {
            None,
            Selection,
            Insert,
            Pan,
            Move,
            Line,
            Rectangle,
            Ellipse,
            Text,
        }

        #endregion

        #region Action Item

        public class ActionItem : Item
        {
            public string Message { get; set; }
            public string Model { get; set; }
        }

        #endregion

        #region Fields

        private ISheet back = null;
        private ISheet sheet = null;
        private ISheet overlay = null;
        private Stack<ActionItem> undos = new Stack<ActionItem>();
        private Stack<ActionItem> redos = new Stack<ActionItem>();
        private Mode mode = Mode.Selection;
        private Mode tempMode = Mode.None;
        private double snapSize = 15;
        private double gridSize = 30;
        private double frameThickness = 1.0;
        private double gridThickness = 1.0;
        private double selectionThickness = 1.0;
        private double lineThickness = 2.0;
        private Point panStartPoint;
        private Line tempLine = null;
        private Ellipse tempStartEllipse = null;
        private Ellipse tempEndEllipse = null;
        private Rectangle tempRectangle = null;
        private Ellipse tempEllipse = null;
        private Point selectionStartPoint;
        private Rectangle selectionRect = null;
        private double hitSize = 3.5;
        private List<Line> gridLines = new List<Line>();
        private List<Line> frameLines = new List<Line>();

        #endregion

        #region Properties

        public ILibrary Library { get; set; }
        public ITextEditor TextEditor { get; set; }

        #endregion

        #region Pan & Zoom Properties

        private int zoomIndex = 9;
        private int defaultZoomIndex = 9;
        private int maxZoomIndex = 21;
        private double[] zoomFactors = { 0.01, 0.0625, 0.0833, 0.125, 0.25, 0.3333, 0.5, 0.6667, 0.75, 1, 1.25, 1.5, 2, 3, 4, 6, 8, 12, 16, 24, 32, 64 };

        public int ZoomIndex
        {
            get { return zoomIndex; }
            set
            {
                if (value >= 0 && value <= maxZoomIndex)
                {
                    zoomIndex = value;
                    Zoom = zoomFactors[zoomIndex];
                }
            }
        }

        public double Zoom
        {
            get { return zoom.ScaleX; }
            set
            {
                if (IsLoaded)
                    AdjustThickness(value);

                zoom.ScaleX = value;
                zoom.ScaleY = value;
            }
        }

        public double PanX
        {
            get { return pan.X; }
            set
            {
                pan.X = value;
            }
        }

        public double PanY
        {
            get { return pan.Y; }
            set
            {
                pan.Y = value;
            }
        }

        #endregion

        #region Block Properties

        private Block logic = null;
        private Block selected = null;

        public Block Logic
        {
            get { return logic; }
            set { logic = value; }
        }

        public Block Selected
        {
            get { return selected; }
            set { selected = value; }
        }

        #endregion

        #region Snap

        private double Snap(double val)
        {
            double r = val % snapSize;
            return r >= snapSize / 2.0 ? val + snapSize - r : val - r;
        }

        #endregion

        #region Constructor

        public SheetControl()
        {
            InitializeComponent();

            Init();
            Loaded += (s, e) => InitLoaded();
        }

        #endregion

        #region Init

        private void Init()
        {
            back = new CanvasSheet(Root.Back);
            sheet = new CanvasSheet(Root.Sheet);
            overlay = new CanvasSheet(Root.Overlay);

            Logic = new Block() { Name = "LOGIC" };
            Logic.Init();

            Selected = new Block() { Name = "SELECTED" };
        }

        private void InitLoaded()
        {
            CreateGrid(back, gridLines, 330.0, 30.0, 600.0, 720.0, gridSize, gridThickness, Brushes.LightGray);
            CreateFrame(back, frameLines, gridSize, gridThickness, Brushes.DarkGray);
            AdjustThickness(gridLines, gridThickness / zoomFactors[zoomIndex]);
            AdjustThickness(frameLines, frameThickness / zoomFactors[zoomIndex]);
            LoadStandardLibrary();
            Focus();
        }

        #endregion

        #region Mode

        public Mode GetMode()
        {
            return mode;
        }

        public void SetMode(Mode m)
        {
            mode = m;
        }

        private void StoreTempMode()
        {
            tempMode = GetMode();
        }

        private void RestoreTempMode()
        {
            SetMode(tempMode);
        }

        public void ModeNone()
        {
            SetMode(Mode.None);
        }

        public void ModeSelection()
        {
            SetMode(Mode.Selection);
        }

        public void ModeInsert()
        {
            SetMode(Mode.Insert);
        }

        public void ModePan()
        {
            SetMode(Mode.Pan);
        }

        public void ModeMove()
        {
            SetMode(Mode.Move);
        }

        public void ModeLine()
        {
            SetMode(Mode.Line);
        }

        public void ModeRectangle()
        {
            SetMode(Mode.Rectangle);
        }

        public void ModeEllipse()
        {
            SetMode(Mode.Ellipse);
        }

        public void ModeText()
        {
            SetMode(Mode.Text);
        }

        #endregion

        #region Text

        private void CreateText(Point p)
        {
            double x = Snap(p.X);
            double y = Snap(p.Y);
            PushUndo("Create Text");
            var text = BlockEditor.CreateText("Text", x, y, 30.0, 15.0, HorizontalAlignment.Center, VerticalAlignment.Center, 11.0);
            Logic.Texts.Add(text);
            sheet.Add(text);
        }

        #endregion

        #region Back

        private static void CreateLine(ISheet sheet, List<Line> lines, double thickness, double x1, double y1, double x2, double y2, Brush stroke)
        {
            var line = new Line()
            {
                Stroke = stroke,
                StrokeThickness = thickness,
                X1 = x1,
                Y1 = y1,
                X2 = x2,
                Y2 = y2
            };

            if (lines != null)
            {
                lines.Add(line);
            }

            if (sheet != null)
            {
                sheet.Add(line); 
            }
        }

        private static void CreateFrame(ISheet sheet, List<Line> lines, double size, double thickness, Brush stroke)
        {
            for (double y = 30.0 + 30.0; y < 750.0; y += size)
            {
                CreateLine(sheet, lines, thickness, 0.0, y, 30.0 + 300.0, y, stroke);
            }

            for (double y = 30.0 + 30.0; y < 750.0; y += size)
            {
                CreateLine(sheet, lines, thickness, 30.0 + 900.0, y, 30.0 + 30.0 + 1200.0, y, stroke);
            }

            CreateLine(sheet, lines, thickness, 30.0,                        30.0, 30.0,                        750.0, stroke);
            CreateLine(sheet, lines, thickness, 30.0 + 210.0,                30.0, 30.0 + 210.0,                750.0, stroke);
            CreateLine(sheet, lines, thickness, 30.0 + 210.0 + 90.0,          0.0, 30.0 + 210.0 + 90.0,         750.0, stroke);

            CreateLine(sheet, lines, thickness, 30.0 + 900.0,                 0.0, 30.0 + 900.0,                750.0, stroke);
            CreateLine(sheet, lines, thickness, 30.0 + 900.0 + 210.0,        30.0, 30.0 + 900.0 + 210.0,        750.0, stroke);
            CreateLine(sheet, lines, thickness, 30.0 + 900.0 + 210.0 + 90.0, 30.0, 30.0 + 900.0 + 210.0 + 90.0, 750.0, stroke);

            CreateLine(sheet, lines, thickness, 0.0, 30.0, 1260.0, 30.0, stroke);
        }

        private static void CreateGrid(ISheet sheet, List<Line> lines, double startX, double startY, double width, double height, double size, double thickness, Brush stroke)
        {
            for (double y = startY + size; y < height + startY; y += size)
            {
                CreateLine(sheet, lines, thickness, startX, y, width + startX, y, stroke);
            }

            for (double x = startX + size; x < startX + width; x += size)
            {
                CreateLine(sheet, lines, thickness, x, startY, x, height + startY, stroke);
            }
        }

        #endregion

        #region Block

        private void InsertBlock(BlockItem block, bool select)
        {
            BlockEditor.DeselectAll(Selected);
            BlockEditor.InsertBlockContents(sheet, block, Logic, Selected, select, lineThickness / Zoom);
        }

        private BlockItem SerializeLogicBlock()
        {
            return BlockEditor.SerializerBlockContents(Logic, 0, "LOGIC");
        }

        private static string SerializeBlockContents(int id, string name, Block parent)
        {
            var block = BlockEditor.SerializerBlockContents(parent, id, name);
            var sb = new StringBuilder();
            ItemEditor.Serialize(sb, block, "");
            return sb.ToString();
        }

        private BlockItem CreateBlockItem(string name)
        {
            var text = SerializeBlockContents(0, name, Selected);
            Delete();
            var block = ItemEditor.Deserialize(text);
            InsertBlock(block, true);
            return block.Blocks.FirstOrDefault();
        }

        public void CreateBlock()
        {
            if (BlockEditor.HaveSelected(Selected))
            {
                StoreTempMode();
                ModeNone();

                Action<string> ok = (name) =>
                {
                    PushUndo("Create Block");
                    var block = CreateBlockItem(name);
                    AddToLibrary(block);

                    Focus();
                    RestoreTempMode();
                };

                Action cancel = () => 
                {
                    Focus();
                    RestoreTempMode();
                };

                ShowTextEditor(ok, cancel, "Name:", "BLOCK0");
            }
        }

        public void BreakBlock()
        {
            if (BlockEditor.HaveSelected(Selected))
            {
                var text = ItemEditor.Serialize(BlockEditor.SerializerBlockContents(Selected, 0, "SELECTED"));
                var block = ItemEditor.Deserialize(text);
                PushUndo("Break Block");
                Delete();
                BlockEditor.InsertBrokenBlock(sheet, block, Logic, Selected, true, lineThickness / Zoom);
            }
        }

        #endregion

        #region Undo/Redo

        private ActionItem CreateActionItem(string message)
        {
            var item = new ActionItem()
            {
                Message = message,
                Model = ItemEditor.Serialize(SerializeLogicBlock())
            };
            return item;
        }

        public void PushUndo(string message)
        {
            //Debug.Print("Push Undo: " + message);
            undos.Push(CreateActionItem(message));
        }

        public void PushRedo(string message)
        {
            //Debug.Print("Push Redo: " + message);
            redos.Push(CreateActionItem(message));
        }

        public void Undo()
        {
            if (undos.Count > 0)
            {
                PushRedo("Redo");
                Reset();

                var undo = undos.Pop();
                //Debug.Print("Undo: " + undo.Message);
                InsertBlock(ItemEditor.Deserialize(undo.Model), false);
            }
        }

        public void Redo()
        {
            if (redos.Count > 0)
            {
                PushUndo("Undo");
                Reset();

                var redo = redos.Pop();
                //Debug.Print("Redo: " + redo.Message);
                InsertBlock(ItemEditor.Deserialize(redo.Model), false);
            }
        }

        #endregion

        #region Clipboard

        public void Cut()
        {
            Copy();
            PushUndo("Cut");

            if (BlockEditor.HaveSelected(Selected))
            {
                Delete();
            }
            else
            {
                Reset();
            }
        }

        public void Copy()
        {
            var text = ItemEditor.Serialize(BlockEditor.HaveSelected(Selected) ?
                BlockEditor.SerializerBlockContents(Selected, 0, "SELECTED") : SerializeLogicBlock());
            Clipboard.SetData(DataFormats.UnicodeText, text);
        }

        public void Paste()
        {
            var text = (string)Clipboard.GetData(DataFormats.UnicodeText);
            InsertBlock(ItemEditor.Deserialize(text), true);
        }

        #endregion

        #region Library

        private void Insert(Point p)
        {
            if (Library != null)
            {
                var blockItem = Library.GetSelected() as BlockItem;
                Insert(blockItem, p, true);
            }
        }

        private void Insert(BlockItem blockItem, Point p, bool select)
        {
            BlockEditor.DeselectAll(Selected);
            double thickness = lineThickness / Zoom;

            if (select)
            {
                Selected.Blocks = new List<Block>();
            }

            PushUndo("Insert Block");

            var block = BlockEditor.DeserializeBlockItem(sheet, Logic, blockItem, select, thickness);

            if (select)
            {
                Selected.Blocks.Add(block);
            }

            BlockEditor.Move(Snap(p.X), Snap(p.Y), block);
        }

        private void LoadStandardLibrary()
        {
            var assembly = System.Reflection.Assembly.GetExecutingAssembly();
            if (assembly == null)
            {
                return;
            }

            var name = "Sheet.Libraries.Digital.txt";

            using (var stream = assembly.GetManifestResourceStream(name))
            {
                using (var reader = new System.IO.StreamReader(stream))
                {
                    string text = reader.ReadToEnd();
                    if (text != null)
                    {
                        InitLibrary(text);
                    }
                }
            }
        }

        private void LoadLibrary(string fileName)
        {
            var text = ItemEditor.OpenText(fileName);
            if (text != null)
            {
                InitLibrary(text);
            }
        }

        private void InitLibrary(string text)
        {
            if (Library != null && text != null)
            {
                var blocks = ItemEditor.Deserialize(text).Blocks;
                Library.SetSource(blocks);
            }
        }

        private void AddToLibrary(BlockItem block)
        {
            if (Library != null && block != null)
            {
                var source = Library.GetSource() as IEnumerable<BlockItem>;
                var items = new List<BlockItem>(source);
                ItemEditor.NormalizePosition(block, 0.0, 0.0, 1200.0, 750.0);
                items.Add(block);
                Library.SetSource(items);
            }
        }

        #endregion

        #region Reset

        public void Delete()
        {
            if (BlockEditor.HaveSelected(Selected))
            {
                PushUndo("Delete");
                BlockEditor.Remove(sheet, Logic, Selected);
            }
        }

        public void Reset()
        {
            BlockEditor.RemoveLines(sheet, Logic.Lines);
            BlockEditor.RemoveRectangles(sheet, Logic.Rectangles);
            BlockEditor.RemoveEllipses(sheet, Logic.Ellipses);
            BlockEditor.RemoveTexts(sheet, Logic.Texts);
            BlockEditor.RemoveBlocks(sheet, Logic.Blocks);

            Logic.Lines.Clear();
            Logic.Rectangles.Clear();
            Logic.Ellipses.Clear();
            Logic.Texts.Clear();
            Logic.Blocks.Clear();

            Selected.Lines = null;
            Selected.Rectangles = null;
            Selected.Ellipses = null;
            Selected.Texts = null;
            Selected.Blocks = null;
        }

        #endregion

        #region Move

        private void Move(double x, double y)
        {
            PushUndo("Move");
            BlockEditor.Move(x, y, BlockEditor.HaveSelected(Selected) ? Selected : Logic);
        }

        public void MoveUp()
        {
            Move(0.0, -snapSize);
        }

        public void MoveDown()
        {
            Move(0.0, snapSize);
        }

        public void MoveLeft()
        {
            Move(-snapSize, 0.0);
        }

        public void MoveRight()
        {
            Move(snapSize, 0.0);
        }

        #endregion

        #region Selection

        public void SelecteAll()
        {
            BlockEditor.SelectAll(Selected, Logic);
        }

        #endregion

        #region Fill

        public void ToggleFill()
        {
            if (tempRectangle != null)
            {
                tempRectangle.Fill = tempRectangle.Fill == Brushes.Transparent ? Brushes.Black : Brushes.Transparent;
            }

            if (tempEllipse != null)
            {
                tempEllipse.Fill = tempEllipse.Fill == Brushes.Transparent ? Brushes.Black : Brushes.Transparent;
            }
        }

        #endregion

        #region Overlay

        private void ResetTempOverlayElements()
        {
            tempLine = null;
            tempStartEllipse = null;
            tempEndEllipse = null;
            tempRectangle = null;
            tempEllipse = null;
            selectionRect = null;
        }

        #endregion

        #region Move Mode

        private bool CanInitMove(Point p)
        {
            var temp = new Block();
            BlockEditor.HitTest(sheet, Selected, temp, p, hitSize, false, true);

            if (BlockEditor.HaveSelected(temp))
            {
                return true;
            }

            return false;
        }

        private void InitMove(Point p)
        {
            PushUndo("Move");
            StoreTempMode();
            ModeMove();
            p.X = Snap(p.X);
            p.Y = Snap(p.Y);
            panStartPoint = p;
            ResetTempOverlayElements();
            overlay.Capture();
        }

        private void Move(Point p)
        {
            p.X = Snap(p.X);
            p.Y = Snap(p.Y);

            double x = p.X - panStartPoint.X;
            double y = p.Y - panStartPoint.Y;
            double z = zoomFactors[zoomIndex];

            if (x != 0.0 || y != 0.0)
            {
                x = x / z;
                y = y / z;
                BlockEditor.Move(x, y, Selected);
                panStartPoint = p;
            }  
        }

        private void FinishMove()
        {
            RestoreTempMode();
            overlay.ReleaseCapture();
        }

        #endregion

        #region Pan & Zoom Mode

        private static void AdjustThickness(IEnumerable<Line> lines, double thickness)
        {
            foreach (var line in lines)
            {
                line.StrokeThickness = thickness;
            }
        }

        private static void AdjustThickness(IEnumerable<Rectangle> rectangles, double thickness)
        {
            foreach (var rectangle in rectangles)
            {
                rectangle.StrokeThickness = thickness;
            }
        }

        private static void AdjustThickness(IEnumerable<Ellipse> ellipses, double thickness)
        {
            foreach (var ellipse in ellipses)
            {
                ellipse.StrokeThickness = thickness;
            }
        }

        private static void AdjustThickness(Block parent, double thickness)
        {
            AdjustThickness(parent.Lines, thickness);
            AdjustThickness(parent.Rectangles, thickness);
            AdjustThickness(parent.Ellipses, thickness);

            foreach (var block in parent.Blocks)
            {
                AdjustThickness(block, thickness);
            }
        }

        private void AdjustThickness(double zoom)
        {
            double gridThicknessZoomed = gridThickness / zoom;
            double frameThicknessZoomed = frameThickness / zoom;
            double lineThicknessZoomed = lineThickness / zoom;
            double selectionThicknessZoomed = selectionThickness / zoom;

            AdjustThickness(gridLines, gridThicknessZoomed);
            AdjustThickness(frameLines, frameThicknessZoomed);
            AdjustThickness(Logic, lineThicknessZoomed);

            if (tempLine != null)
            {
                tempLine.StrokeThickness = lineThicknessZoomed;
            }

            if (tempStartEllipse != null)
            {
                tempStartEllipse.StrokeThickness = lineThicknessZoomed;
            }

            if (tempEndEllipse != null)
            {
                tempEndEllipse.StrokeThickness = lineThicknessZoomed;
            }

            if (tempRectangle != null)
            {
                tempRectangle.StrokeThickness = lineThicknessZoomed;
            }

            if (tempEllipse != null)
            {
                tempEllipse.StrokeThickness = lineThicknessZoomed;
            }

            if (selectionRect != null)
            {
                selectionRect.StrokeThickness = selectionThicknessZoomed;
            }
        }

        private void ZoomTo(double x, double y, int oldZoomIndex)
        {
            double oldZoom = zoomFactors[oldZoomIndex];
            double newZoom = zoomFactors[zoomIndex];
            Zoom = newZoom;
            PanX = (x * oldZoom + PanX) - x * newZoom;
            PanY = (y * oldZoom + PanY) - y * newZoom;
        }

        private void ZoomTo(int delta, Point p)
        {
            if (delta > 0)
            {
                if (zoomIndex < maxZoomIndex)
                {
                    ZoomTo(p.X, p.Y, zoomIndex++);
                }
            }
            else
            {
                if (zoomIndex > 0)
                {
                    ZoomTo(p.X, p.Y, zoomIndex--);
                }
            }
        }

        private void InitPan(Point p)
        {
            StoreTempMode();
            ModePan();
            panStartPoint = p;
            ResetTempOverlayElements();
            overlay.Capture();
        }

        private void Pan(Point p)
        {
            PanX = PanX + p.X - panStartPoint.X;
            PanY = PanY + p.Y - panStartPoint.Y;
            panStartPoint = p;
        }

        private void FinishPan()
        {
            RestoreTempMode();
            overlay.ReleaseCapture();
        }

        private void ResetPanAndZoom()
        {
            zoomIndex = defaultZoomIndex;
            Zoom = zoomFactors[zoomIndex];
            PanX = 0.0;
            PanY = 0.0;
        }

        #endregion

        #region Selection Mode

        private void InitSelectionRect(Point p)
        {
            selectionStartPoint = p;
            double x = p.X;
            double y = p.Y;
            selectionRect = BlockEditor.CreateSelectionRectangle(selectionThickness / Zoom, x, y, 0.0, 0.0);
            overlay.Add(selectionRect);
            overlay.Capture();
        }

        private void MoveSelectionRect(Point p)
        {
            double sx = selectionStartPoint.X;
            double sy = selectionStartPoint.Y;
            double x = p.X;
            double y = p.Y;
            double width = Math.Abs(sx - x);
            double height = Math.Abs(sy - y);
            Canvas.SetLeft(selectionRect, Math.Min(sx, x));
            Canvas.SetTop(selectionRect, Math.Min(sy, y));
            selectionRect.Width = width;
            selectionRect.Height = height;
        }

        private void FinishSelectionRect()
        {
            double x = Canvas.GetLeft(selectionRect);
            double y = Canvas.GetTop(selectionRect);
            double width = selectionRect.Width;
            double height = selectionRect.Height;

            CancelSelectionRect();

            bool ctrl = (Keyboard.Modifiers & ModifierKeys.Control) > 0;
            bool resetSelected = ctrl && BlockEditor.HaveSelected(Selected) ? false : true;
            BlockEditor.HitTestSelectionRect(sheet, Logic, Selected, new Rect(x, y, width, height), resetSelected);
        }

        private void CancelSelectionRect()
        {
            overlay.ReleaseCapture();
            overlay.Remove(selectionRect);
            selectionRect = null;
        }

        #endregion

        #region Line Mode

        private void InitTempLine(Point p)
        {
            double x = Snap(p.X);
            double y = Snap(p.Y);
            tempLine = BlockEditor.CreateLine(lineThickness / Zoom, x, y, x, y);
            tempStartEllipse = BlockEditor.CreateEllipse(lineThickness / Zoom, x - 4.0, y - 4.0, 8.0, 8.0, true);
            tempEndEllipse = BlockEditor.CreateEllipse(lineThickness / Zoom, x - 4.0, y - 4.0, 8.0, 8.0, true);
            overlay.Add(tempLine);
            overlay.Add(tempStartEllipse);
            overlay.Add(tempEndEllipse);
            overlay.Capture();
        }

        private void MoveTempLine(Point p)
        {
            double x = Snap(p.X);
            double y = Snap(p.Y);
            if (Math.Round(x, 1) != Math.Round(tempLine.X2, 1) 
                || Math.Round(y, 1) != Math.Round(tempLine.Y2, 1))
            {
                tempLine.X2 = x;
                tempLine.Y2 = y;
                Canvas.SetLeft(tempEndEllipse, x - 4.0);
                Canvas.SetTop(tempEndEllipse, y - 4.0);
            }
        }

        private void FinishTempLine()
        {
            if (Math.Round(tempLine.X1, 1) == Math.Round(tempLine.X2, 1) &&
                Math.Round(tempLine.Y1, 1) == Math.Round(tempLine.Y2, 1))
            {
                CancelTempLine();
            }
            else
            {
                overlay.ReleaseCapture();
                overlay.Remove(tempLine);
                overlay.Remove(tempStartEllipse);
                overlay.Remove(tempEndEllipse);
                PushUndo("Create Line");
                Logic.Lines.Add(tempLine);
                sheet.Add(tempLine);
                tempLine = null;
                tempStartEllipse = null;
                tempEndEllipse = null;
            }
        }

        private void CancelTempLine()
        {
            overlay.ReleaseCapture();
            overlay.Remove(tempLine);
            overlay.Remove(tempStartEllipse);
            overlay.Remove(tempEndEllipse);
            tempLine = null;
            tempStartEllipse = null;
            tempEndEllipse = null;
        }

        #endregion

        #region Rectangle Mode

        private void InitTempRect(Point p)
        {
            double x = Snap(p.X);
            double y = Snap(p.Y);
            selectionStartPoint = new Point(x, y);
            tempRectangle = BlockEditor.CreateRectangle(selectionThickness / Zoom, x, y, 0.0, 0.0, true);
            overlay.Add(tempRectangle);
            overlay.Capture();
        }

        private void MoveTempRect(Point p)
        {
            double sx = selectionStartPoint.X;
            double sy = selectionStartPoint.Y;
            double x = Snap(p.X);
            double y = Snap(p.Y);
            double width = Math.Abs(sx - x);
            double height = Math.Abs(sy - y);
            Canvas.SetLeft(tempRectangle, Math.Min(sx, x));
            Canvas.SetTop(tempRectangle, Math.Min(sy, y));
            tempRectangle.Width = width;
            tempRectangle.Height = height;
        }

        private void FinishTempRect()
        {
            double x = Canvas.GetLeft(tempRectangle);
            double y = Canvas.GetTop(tempRectangle);
            double width = tempRectangle.Width;
            double height = tempRectangle.Height;

            if (width == 0.0 || height == 0.0)
            {
                CancelTempRect();
            }
            else
            {
                overlay.ReleaseCapture();
                overlay.Remove(tempRectangle);
                PushUndo("Create Rectangle");
                Logic.Rectangles.Add(tempRectangle);
                sheet.Add(tempRectangle);
                tempRectangle = null;
            }
        }

        private void CancelTempRect()
        {
            overlay.ReleaseCapture();
            overlay.Remove(tempRectangle);
            tempRectangle = null;
        }

        #endregion

        #region Ellipse Mode

        private void InitTempEllipse(Point p)
        {
            double x = Snap(p.X);
            double y = Snap(p.Y);
            selectionStartPoint = new Point(x, y);
            tempEllipse = BlockEditor.CreateEllipse(selectionThickness / Zoom, x, y, 0.0, 0.0, true);
            overlay.Add(tempEllipse);
            overlay.Capture();
        }

        private void MoveTempEllipse(Point p)
        {
            double sx = selectionStartPoint.X;
            double sy = selectionStartPoint.Y;
            double x = Snap(p.X);
            double y = Snap(p.Y);
            double width = Math.Abs(sx - x);
            double height = Math.Abs(sy - y);
            Canvas.SetLeft(tempEllipse, Math.Min(sx, x));
            Canvas.SetTop(tempEllipse, Math.Min(sy, y));
            tempEllipse.Width = width;
            tempEllipse.Height = height;
        }

        private void FinishTempEllipse()
        {
            double x = Canvas.GetLeft(tempEllipse);
            double y = Canvas.GetTop(tempEllipse);
            double width = tempEllipse.Width;
            double height = tempEllipse.Height;

            if (width == 0.0 || height == 0.0)
            {
                CancelTempEllipse();
            }
            else
            {
                overlay.ReleaseCapture();
                overlay.Remove(tempEllipse);
                PushUndo("Create Ellipse");
                Logic.Ellipses.Add(tempEllipse);
                sheet.Add(tempEllipse);
                tempEllipse = null;
            }
        }

        private void CancelTempEllipse()
        {
            overlay.ReleaseCapture();
            overlay.Remove(tempEllipse);
            tempEllipse = null;
        }

        #endregion

        #region Edit Text

        private void ShowTextEditor(Action<string> ok, Action cancel, string label, string text)
        {
            if (TextEditor != null)
            {
                TextEditor.Show(ok, cancel, label, text);
            }
        }

        private bool TryToEditText(Point p)
        {
            var temp = new Block();
            BlockEditor.HitTest(sheet, Logic, temp, p, hitSize, true, true);

            if (BlockEditor.HaveOneTextSelected(temp))
            {
                var tb = BlockEditor.GetTextBlock(temp.Texts[0]);

                StoreTempMode();
                ModeNone();

                Action<string> ok = (text) =>
                {
                    PushUndo("Edit Text");
                    tb.Text = text;

                    Focus();
                    RestoreTempMode();
                };

                Action cancel = () => 
                {
                    Focus();
                    RestoreTempMode();
                };

                ShowTextEditor(ok, cancel, "Text:", tb.Text);

                BlockEditor.Deselect(temp);
                return true;
            }

            BlockEditor.Deselect(temp);
            return false;
        }

        #endregion

        #region Export Xps

        private CanvasControl CreateXpsRoot(Block parent)
        {
            var root = new CanvasControl();
            var sheet = new CanvasSheet(root.Sheet);

            //CreateGrid(sheet, null, 330.0, 30.0, 600.0, 720.0, gridSize, 0.013 * 96.0 / 2.54 /*gridThickness*/, Brushes.LightGray);
            CreateFrame(sheet, null, gridSize, 0.013 * 96.0 / 2.54 /*gridThickness*/, Brushes.Black);

            var block = BlockEditor.SerializerBlockContents(parent, 0, "LOGIC");
            BlockEditor.InsertBlockContents(sheet, block, null, null, false, 0.035 * 96.0 / 2.54 /*lineThickness*/);

            var vb = new Viewbox { Child = root };
            vb.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            vb.Arrange(new Rect(vb.DesiredSize));

            return root;
        }

        private void ExportToXps(string fileName)
        {
            using (var package = Package.Open(fileName, System.IO.FileMode.Create))
            {
                var doc = new XpsDocument(package);
                var writer = XpsDocument.CreateXpsDocumentWriter(doc);
                var root = CreateXpsRoot(Logic);
                writer.Write(root);
                doc.Close();
            }
        }

        #endregion

        #region Export Pdf

        private void ExportToPdf(string fileName)
        {
            var writer = new SheetPdfWriter();

            var page = new BlockItem();
            page.Init(0, "");

            //var grid = CreateGridBlock();
            var frame = CreateFrameBlock();
            var logic = BlockEditor.SerializerBlockContents(Logic, 0, "LOGIC");

            //page.Blocks.Add(grid);
            page.Blocks.Add(frame);
            page.Blocks.Add(logic);

            writer.Create(fileName, 1260.0, 891.0, page);
        }

        private BlockItem CreateGridBlock()
        {
            var grid = new BlockItem();
            grid.Init(0, "");

            foreach (var line in gridLines)
            {
                var lineItem = BlockEditor.SerializeLine(line);
                lineItem.StrokeThickness = 0.013 * 72.0 / 2.54;// 0.13mm
                //lineItem.Stroke = new ItemColor() { Alpha = 255, Red = 0, Green = 0, Blue = 0 };
                grid.Lines.Add(lineItem);
            }
            return grid;
        }

        private BlockItem CreateFrameBlock()
        {
            var frame = new BlockItem();
            frame.Init(0, "");

            foreach (var line in frameLines)
            {
                var lineItem = BlockEditor.SerializeLine(line);
                lineItem.StrokeThickness = 0.018 * 72.0 / 2.54; // 0.18mm
                lineItem.Stroke = new ItemColor() { Alpha = 255, Red = 0, Green = 0, Blue = 0 };
                frame.Lines.Add(lineItem);
            }
            return frame;
        }

        #endregion

        #region File Dialogs

        public void Open()
        {
            var dlg = new Microsoft.Win32.OpenFileDialog()
            {
                Filter = "TXT Files (*.txt)|*.txt|All Files (*.*)|*.*"
            };

            if (dlg.ShowDialog() == true)
            {
                var text = ItemEditor.OpenText(dlg.FileName);
                if (text != null)
                {
                    PushUndo("Open");
                    Reset();
                    InsertBlock(ItemEditor.Deserialize(text), false);
                }
            }
        }

        public void Save()
        {
            var dlg = new Microsoft.Win32.SaveFileDialog()
            {
                Filter = "TXT Files (*.txt)|*.txt|All Files (*.*)|*.*",
                FileName = "sheet"
            };

            if (dlg.ShowDialog() == true)
            {
                var text = ItemEditor.Serialize(SerializeLogicBlock());
                ItemEditor.SaveText(dlg.FileName, text);
            }
        }

        public void Export()
        {
            var dlg = new Microsoft.Win32.SaveFileDialog()
            {
                Filter = "PDF Documents (*.pdf)|*.pdf|XPS Documents (*.xps)|*.xps|All Files (*.*)|*.*",
                FileName = "sheet"
            };

            if (dlg.ShowDialog() == true)
            {
                switch(dlg.FilterIndex)
                {
                    case 1:
                        ExportToPdf(dlg.FileName);
                        break;
                    case 2:
                    case 3:
                    ExportToXps(dlg.FileName);
                        break;
                }
            }
        }

        public void Load()
        {
            var dlg = new Microsoft.Win32.OpenFileDialog()
            {
                Filter = "TXT Files (*.txt)|*.txt|All Files (*.*)|*.*"
            };

            if (dlg.ShowDialog() == true)
            {
                LoadLibrary(dlg.FileName);
            }
        }

        #endregion

        #region Events

        private void UserControl_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Focus();

            if (GetMode() == Mode.None)
            {
                return;
            }

            bool ctrl = (Keyboard.Modifiers & ModifierKeys.Control) > 0;

            if (!ctrl)
            {
                if (BlockEditor.HaveSelected(Selected) && CanInitMove(e.GetPosition(overlay.GetParent())))
                {
                    InitMove(e.GetPosition(this));
                    return;
                }

                BlockEditor.DeselectAll(Selected);
            }

            bool resetSelected = ctrl && BlockEditor.HaveSelected(Selected) ? false : true;

            if (GetMode() == Mode.Selection)
            {
                bool result = BlockEditor.HitTest(sheet, Logic, Selected, e.GetPosition(overlay.GetParent()), hitSize, false, resetSelected);
                if ((ctrl || !BlockEditor.HaveSelected(Selected)) && !result)
                {
                    InitSelectionRect(e.GetPosition(overlay.GetParent()));
                }
                else
                {
                    InitMove(e.GetPosition(this));
                }
            }
            else if (GetMode() == Mode.Insert && !overlay.IsCaptured)
            {
                Insert(e.GetPosition(overlay.GetParent()));
            }
            else if (GetMode() == Mode.Line && !overlay.IsCaptured)
            {
                InitTempLine(e.GetPosition(overlay.GetParent()));
            }
            else if (GetMode() == Mode.Line && overlay.IsCaptured)
            {
                FinishTempLine();
            }
            else if (GetMode() == Mode.Rectangle && !overlay.IsCaptured)
            {
                InitTempRect(e.GetPosition(overlay.GetParent()));
            }
            else if (GetMode() == Mode.Rectangle && overlay.IsCaptured)
            {
                FinishTempRect();
            }
            else if (GetMode() == Mode.Ellipse && !overlay.IsCaptured)
            {
                InitTempEllipse(e.GetPosition(overlay.GetParent()));
            }
            else if (GetMode() == Mode.Ellipse && overlay.IsCaptured)
            {
                FinishTempEllipse();
            }
            else if (GetMode() == Mode.Pan && overlay.IsCaptured)
            {
                FinishPan();
            }
            else if (GetMode() == Mode.Text && !overlay.IsCaptured)
            {
                CreateText(e.GetPosition(overlay.GetParent()));
            }
        }

        private void UserControl_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (GetMode() == Mode.Selection && overlay.IsCaptured)
            {
                FinishSelectionRect();
            }
            else if (GetMode() == Mode.Move && overlay.IsCaptured)
            {
                FinishMove();
            }
        }

        private void UserControl_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (GetMode() == Mode.Selection && overlay.IsCaptured)
            {
                MoveSelectionRect(e.GetPosition(overlay.GetParent()));
            }
            else if (GetMode() == Mode.Line && overlay.IsCaptured)
            {
                MoveTempLine(e.GetPosition(overlay.GetParent()));
            }
            else if (GetMode() == Mode.Rectangle && overlay.IsCaptured)
            {
                MoveTempRect(e.GetPosition(overlay.GetParent()));
            }
            else if (GetMode() == Mode.Ellipse && overlay.IsCaptured)
            {
                MoveTempEllipse(e.GetPosition(overlay.GetParent()));
            }
            else if (GetMode() == Mode.Pan && overlay.IsCaptured)
            {
                Pan(e.GetPosition(this));
            }
            else if (GetMode() == Mode.Move && overlay.IsCaptured)
            {
                Move(e.GetPosition(this));
            }
        }

        private void UserControl_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            Focus();

            if (GetMode() == Mode.None)
            {
                return;
            }

            if (TryToEditText(e.GetPosition(overlay.GetParent())))
            {
                e.Handled = true;
                return;
            }

            BlockEditor.DeselectAll(Selected);

            if (GetMode() == Mode.Selection && overlay.IsCaptured)
            {
                CancelSelectionRect();
            }
            else if (GetMode() == Mode.Line && overlay.IsCaptured)
            {
                CancelTempLine();
            }
            else if (GetMode() == Mode.Rectangle && overlay.IsCaptured)
            {
                CancelTempRect();
            }
            else if (GetMode() == Mode.Ellipse && overlay.IsCaptured)
            {
                CancelTempEllipse();
            }
            else if (!overlay.IsCaptured)
            {
                InitPan(e.GetPosition(this));
            }
        }

        private void UserControl_PreviewMouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (GetMode() == Mode.Pan && overlay.IsCaptured)
            {
                FinishPan();
            }
        }

        private void UserControl_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            ZoomTo(e.Delta, e.GetPosition(Layout));
        }

        private void UserControl_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Middle && e.ClickCount == 2)
            {
                ResetPanAndZoom();
            }
        }

        #endregion

        #region Drop

        private void UserControl_DragEnter(object sender, DragEventArgs e)
        {
            if (!e.Data.GetDataPresent("Block") || sender == e.Source)
            {
                e.Effects = DragDropEffects.None;
            }
        }

        private void UserControl_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent("Block"))
            {
                var blockItem = e.Data.GetData("Block") as BlockItem;
                if (blockItem != null)
                {
                    Insert(blockItem, e.GetPosition(overlay.GetParent()), true);
                    e.Handled = true;
                }
            }
        } 

        #endregion
    }
}
