using Newtonsoft.Json;
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
        public int DataId { get; set; }
        public List<LineItem> Lines { get; set; }
        public List<RectangleItem> Rectangles { get; set; }
        public List<EllipseItem> Ellipses { get; set; }
        public List<TextItem> Texts { get; set; }
        public List<BlockItem> Blocks { get; set; }
        public void Init(int id, int dataId, string name)
        {
            Id = id;
            DataId = dataId;
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

    public interface ILibrary
    {
        BlockItem GetSelected();
        void SetSelected(BlockItem block);
        IEnumerable<BlockItem> GetSource();
        void SetSource(IEnumerable<BlockItem> source);
    }

    public class DataItem
    {
        public string[] Columns { get; set; }
        public string[] Data { get; set; }
    }

    public interface IDatabase
    {
        string[] Get(int index);
        bool Update(int index, string[] item);
        int Add(string[] item);
    }

    public interface ITextEditor
    {
        void Show(Action<string> ok, Action cancel, string label, string text);
    }

    #endregion

    #region Item Serializer

    public class ItemSerializeOptions
    {
        #region Properties

        public string LineSeparator { get; set; }
        public string ModelSeparator { get; set; }
        public char[] LineSeparators { get; set; }
        public char[] ModelSeparators { get; set; }
        public char[] WhiteSpace { get; set; }
        public string IndentWhiteSpace { get; set; }
        public static ItemSerializeOptions Default
        {
            get
            {
                return new ItemSerializeOptions()
                {
                    LineSeparator = "\r\n",
                    ModelSeparator = ";",
                    LineSeparators = new char[] { '\r', '\n' },
                    ModelSeparators = new char[] { ';' },
                    WhiteSpace = new char[] { ' ', '\t' },
                    IndentWhiteSpace = "    "
                };
            }
        } 

        #endregion
    }

    public static class ItemSerializer
    {
        #region Serialize

        public static void Serialize(StringBuilder sb, ItemColor color, ItemSerializeOptions options)
        {
            sb.Append(color.Alpha);
            sb.Append(options.ModelSeparator);
            sb.Append(color.Red);
            sb.Append(options.ModelSeparator);
            sb.Append(color.Green);
            sb.Append(options.ModelSeparator);
            sb.Append(color.Blue);
        }

        public static void Serialize(StringBuilder sb, LineItem line, string indent, ItemSerializeOptions options)
        {
            sb.Append(indent);
            sb.Append("LINE");
            sb.Append(options.ModelSeparator);
            sb.Append(line.Id);
            sb.Append(options.ModelSeparator);
            sb.Append(line.X1);
            sb.Append(options.ModelSeparator);
            sb.Append(line.Y1);
            sb.Append(options.ModelSeparator);
            sb.Append(line.X2);
            sb.Append(options.ModelSeparator);
            sb.Append(line.Y2);
            sb.Append(options.ModelSeparator);
            Serialize(sb, line.Stroke, options);
            sb.Append(options.LineSeparator);
        }

        public static void Serialize(StringBuilder sb, RectangleItem rectangle, string indent, ItemSerializeOptions options)
        {
            sb.Append(indent);
            sb.Append("RECTANGLE");
            sb.Append(options.ModelSeparator);
            sb.Append(rectangle.Id);
            sb.Append(options.ModelSeparator);
            sb.Append(rectangle.X);
            sb.Append(options.ModelSeparator);
            sb.Append(rectangle.Y);
            sb.Append(options.ModelSeparator);
            sb.Append(rectangle.Width);
            sb.Append(options.ModelSeparator);
            sb.Append(rectangle.Height);
            sb.Append(options.ModelSeparator);
            sb.Append(rectangle.IsFilled);
            sb.Append(options.ModelSeparator);
            Serialize(sb, rectangle.Stroke, options);
            sb.Append(options.ModelSeparator);
            Serialize(sb, rectangle.Fill, options);
            sb.Append(options.LineSeparator);
        }

        public static void Serialize(StringBuilder sb, EllipseItem ellipse, string indent, ItemSerializeOptions options)
        {
            sb.Append(indent);
            sb.Append("ELLIPSE");
            sb.Append(options.ModelSeparator);
            sb.Append(ellipse.Id);
            sb.Append(options.ModelSeparator);
            sb.Append(ellipse.X);
            sb.Append(options.ModelSeparator);
            sb.Append(ellipse.Y);
            sb.Append(options.ModelSeparator);
            sb.Append(ellipse.Width);
            sb.Append(options.ModelSeparator);
            sb.Append(ellipse.Height);
            sb.Append(options.ModelSeparator);
            sb.Append(ellipse.IsFilled);
            sb.Append(options.ModelSeparator);
            Serialize(sb, ellipse.Stroke, options);
            sb.Append(options.ModelSeparator);
            Serialize(sb, ellipse.Fill, options);
            sb.Append(options.LineSeparator);
        }

        public static void Serialize(StringBuilder sb, TextItem text, string indent, ItemSerializeOptions options)
        {
            sb.Append(indent);
            sb.Append("TEXT");
            sb.Append(options.ModelSeparator);
            sb.Append(text.Id);
            sb.Append(options.ModelSeparator);
            sb.Append(text.X);
            sb.Append(options.ModelSeparator);
            sb.Append(text.Y);
            sb.Append(options.ModelSeparator);
            sb.Append(text.Width);
            sb.Append(options.ModelSeparator);
            sb.Append(text.Height);
            sb.Append(options.ModelSeparator);
            sb.Append(text.HAlign);
            sb.Append(options.ModelSeparator);
            sb.Append(text.VAlign);
            sb.Append(options.ModelSeparator);
            sb.Append(text.Size);
            sb.Append(options.ModelSeparator);
            Serialize(sb, text.Foreground, options);
            sb.Append(options.ModelSeparator);
            Serialize(sb, text.Backgroud, options);
            sb.Append(options.ModelSeparator);
            sb.Append(text.Text);
            sb.Append(options.LineSeparator);
        }

        public static void Serialize(StringBuilder sb, BlockItem block, string indent, ItemSerializeOptions options)
        {
            sb.Append(indent);
            sb.Append("BLOCK");
            sb.Append(options.ModelSeparator);
            sb.Append(block.Id);
            sb.Append(options.ModelSeparator);
            sb.Append(block.Name);
            sb.Append(options.ModelSeparator);
            sb.Append(block.Width);
            sb.Append(options.ModelSeparator);
            sb.Append(block.Height);
            sb.Append(options.ModelSeparator);
            Serialize(sb, block.Backgroud, options);
            sb.Append(options.ModelSeparator);
            sb.Append(block.DataId);
            sb.Append(options.LineSeparator);

            Serialize(sb, block.Lines, indent + options.IndentWhiteSpace, options);
            Serialize(sb, block.Rectangles, indent + options.IndentWhiteSpace, options);
            Serialize(sb, block.Ellipses, indent + options.IndentWhiteSpace, options);
            Serialize(sb, block.Texts, indent + options.IndentWhiteSpace, options);
            Serialize(sb, block.Blocks, indent + options.IndentWhiteSpace, options);

            sb.Append(indent);
            sb.Append("END");
            sb.Append(options.LineSeparator);
        }

        public static void Serialize(StringBuilder sb, IEnumerable<LineItem> lines, string indent, ItemSerializeOptions options)
        {
            foreach (var line in lines)
            {
                Serialize(sb, line, indent, options);
            }
        }

        public static void Serialize(StringBuilder sb, IEnumerable<RectangleItem> rectangles, string indent, ItemSerializeOptions options)
        {
            foreach (var rectangle in rectangles)
            {
                Serialize(sb, rectangle, indent, options);
            }
        }

        public static void Serialize(StringBuilder sb, IEnumerable<EllipseItem> ellipses, string indent, ItemSerializeOptions options)
        {
            foreach (var ellipse in ellipses)
            {
                Serialize(sb, ellipse, indent, options);
            }
        }

        public static void Serialize(StringBuilder sb, IEnumerable<TextItem> texts, string indent, ItemSerializeOptions options)
        {
            foreach (var text in texts)
            {
                Serialize(sb, text, indent, options);
            }
        }

        public static void Serialize(StringBuilder sb, IEnumerable<BlockItem> blocks, string indent, ItemSerializeOptions options)
        {
            foreach (var block in blocks)
            {
                Serialize(sb, block, indent, options);
            }
        }

        public static string SerializeContents(BlockItem block, ItemSerializeOptions options)
        {
            var sb = new StringBuilder();

            Serialize(sb, block.Lines, "", options);
            Serialize(sb, block.Rectangles, "", options);
            Serialize(sb, block.Ellipses, "", options);
            Serialize(sb, block.Texts, "", options);
            Serialize(sb, block.Blocks, "", options);

            return sb.ToString();
        }

        public static string SerializeContents(BlockItem block)
        {
            return SerializeContents(block, ItemSerializeOptions.Default);
        }

        #endregion

        #region Deserialize

        private static LineItem DeserializeLine(string[] m)
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
            return lineItem;
        }
        
        private static RectangleItem DeserializeRectangle(string[] m)
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
            return rectangleItem;
        }
        
        private static EllipseItem DeserializeEllipse(string[] m)
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
            return ellipseItem;
        }
        
        private static TextItem DeserializeText(string[] m)
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
            return textItem;
        }

        private static BlockItem DeserializeBlock(string[] lines, int length, ref int end, string[] m, ItemSerializeOptions options)
        {
            var blockItem = DeserializeRootBlock(lines, length, ref end, m[2], int.Parse(m[1]), m.Length == 10 ? int.Parse(m[9]) : -1, options);
            if (m.Length == 9 || m.Length == 10)
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
                blockItem.DataId = m.Length == 10 ? int.Parse(m[9]) : -1;
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
                blockItem.DataId = -1;
            }
            return blockItem;
        }

        private static BlockItem DeserializeRootBlock(string[] lines, int length, ref int end, string name, int id, int dataId, ItemSerializeOptions options)
        {
            var root = new BlockItem();
            root.Init(id, dataId, name);

            for (; end < length; end++)
            {
                string line = lines[end].TrimStart(options.WhiteSpace);
                var m = line.Split(options.ModelSeparators);
                if ((m.Length == 6 || m.Length == 10) && string.Compare(m[0], "LINE", true) == 0)
                {
                    var lineItem = DeserializeLine(m);
                    root.Lines.Add(lineItem);
                }
                if ((m.Length == 7 || m.Length == 15) && string.Compare(m[0], "RECTANGLE", true) == 0)
                {
                    var rectangleItem = DeserializeRectangle(m);
                    root.Rectangles.Add(rectangleItem);
                }
                if ((m.Length == 7 || m.Length == 15) && string.Compare(m[0], "ELLIPSE", true) == 0)
                {
                    var ellipseItem = DeserializeEllipse(m);
                    root.Ellipses.Add(ellipseItem);
                }
                else if ((m.Length == 10 || m.Length == 18) && string.Compare(m[0], "TEXT", true) == 0)
                {
                    var textItem = DeserializeText(m);
                    root.Texts.Add(textItem);
                }
                else if ((m.Length == 3 || m.Length == 9 || m.Length == 10) && string.Compare(m[0], "BLOCK", true) == 0)
                {
                    end++;
                    var blockItem = DeserializeBlock(lines, length, ref end, m, options);
                    root.Blocks.Add(blockItem);
                    continue;
                }
                else if (m.Length == 1 && string.Compare(m[0], "END", true) == 0)
                {
                    return root;
                }
            }
            return root;
        }

        public static BlockItem DeserializeContents(string model, ItemSerializeOptions options)
        {
            string[] lines = model.Split(options.LineSeparators, StringSplitOptions.RemoveEmptyEntries);
            int length = lines.Length;
            int end = 0;
            return DeserializeRootBlock(lines, length, ref end, "", 0, -1, options);
        }

        public static BlockItem DeserializeContents(string model)
        {
            return DeserializeContents(model, ItemSerializeOptions.Default);
        }

        #endregion
    }

    #endregion

    #region Item Editor

    public static class ItemEditor
    {
        #region Text

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

        #region Position

        public static void ResetPosition(BlockItem block, double originX, double originY, double width, double height)
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

        #region Snap

        public static double Snap(double val, double snap)
        {
            double r = val % snap;
            return r >= snap / 2.0 ? val + snap - r : val - r;
        }

        #endregion
    }

    #endregion

    #region Block Model

    public class Block
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }
        public Color Backgroud { get; set; }
        public int DataId { get; set; }
        public List<Line> Lines { get; set; }
        public List<Rectangle> Rectangles { get; set; }
        public List<Ellipse> Ellipses { get; set; }
        public List<Grid> Texts { get; set; }
        public List<Block> Blocks { get; set; }
        public Block(int id, int dataId, string name)
        {
            Id = id;
            DataId = dataId;
            Name = name;
        }
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

    public class SheetOptions
    {
        public double PageOriginX { get; set; }
        public double PageOriginY { get; set; }
        public double PageWidth { get; set; }
        public double PageHeight { get; set; }
        public double SnapSize { get; set; }
        public double GridSize { get; set; }
        public double FrameThickness { get; set; }
        public double GridThickness { get; set; }
        public double SelectionThickness { get; set; }
        public double LineThickness { get; set; }
        public double HitTestSize { get; set; }
        public int DefaultZoomIndex { get; set; }
        public int MaxZoomIndex { get; set; }
        public double[] ZoomFactors { get; set; }

    }

    public class ChangeMessage
    {
        public string Message { get; set; }
        public string Model { get; set; }
    }

    public interface ISheet
    {
        FrameworkElement GetParent();
        void Add(UIElement element);
        void Remove(UIElement element);
        void Capture();
        void ReleaseCapture();
        bool IsCaptured { get; }
    }

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

    #region Block Serializer

    public static class BlockSerializer
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
            rectangleItem.IsFilled = rectangle.Fill == BlockFactory.TransparentBrush ? false : true;
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
            ellipseItem.IsFilled = ellipse.Fill == BlockFactory.TransparentBrush ? false : true;
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

            var tb = BlockFactory.GetTextBlock(text);
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
            blockItem.Init(0, parent.DataId, parent.Name);
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

        public static BlockItem SerializerBlockContents(Block parent, int id, int dataId, string name)
        {
            var lines = parent.Lines;
            var rectangles = parent.Rectangles;
            var ellipses = parent.Ellipses;
            var texts = parent.Texts;
            var blocks = parent.Blocks;

            var sheet = new BlockItem() { Backgroud = new ItemColor() };
            sheet.Init(id, dataId, name);

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
            var line = BlockFactory.CreateLine(thickness, lineItem.X1, lineItem.Y1, lineItem.X2, lineItem.Y2);

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
            var rectangle = BlockFactory.CreateRectangle(thickness, rectangleItem.X, rectangleItem.Y, rectangleItem.Width, rectangleItem.Height, rectangleItem.IsFilled);

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
            var ellipse = BlockFactory.CreateEllipse(thickness, ellipseItem.X, ellipseItem.Y, ellipseItem.Width, ellipseItem.Height, ellipseItem.IsFilled);

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
            var text = BlockFactory.CreateText(textItem.Text,
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
            var block = new Block(blockItem.Id, blockItem.DataId, blockItem.Name);
            block.Init();

            foreach (var textItem in blockItem.Texts)
            {
                var text = DeserializeTextItem(sheet, block, textItem);

                if (select)
                {
                    BlockEditor.MarkSelected(text);
                }
            }

            foreach (var lineItem in blockItem.Lines)
            {
                var line = DeserializeLineItem(sheet, block, lineItem, thickness);

                if (select)
                {
                    BlockEditor.MarkSelected(line);
                }
            }

            foreach (var rectangleItem in blockItem.Rectangles)
            {
                var rectangle = DeserializeRectangleItem(sheet, block, rectangleItem, thickness);

                if (select)
                {
                    BlockEditor.MarkSelected(rectangle);
                }
            }

            foreach (var ellipseItem in blockItem.Ellipses)
            {
                var ellipse = DeserializeEllipseItem(sheet, block, ellipseItem, thickness);

                if (select)
                {
                    BlockEditor.MarkSelected(ellipse);
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
    }

    #endregion

    #region Block Editor

    public static class BlockEditor
    {
        #region Insert

        public static void InsertLines(ISheet sheet, IEnumerable<LineItem> lineItems, Block parent, Block selected, bool select, double thickness)
        {
            if (select)
            {
                selected.Lines = new List<Line>();
            }

            foreach (var lineItem in lineItems)
            {
                var line = BlockSerializer.DeserializeLineItem(sheet, parent, lineItem, thickness);

                if (select)
                {
                    MarkSelected(line);
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
                var rectangle = BlockSerializer.DeserializeRectangleItem(sheet, parent, rectangleItem, thickness);

                if (select)
                {
                    MarkSelected(rectangle);
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
                var ellipse = BlockSerializer.DeserializeEllipseItem(sheet, parent, ellipseItem, thickness);

                if (select)
                {
                    MarkSelected(ellipse);
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
                var text = BlockSerializer.DeserializeTextItem(sheet, parent, textItem);

                if (select)
                {
                    MarkSelected(text);
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
                var block = BlockSerializer.DeserializeBlockItem(sheet, parent, blockItem, select, thickness);

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

        #region Mark Selection

        private static int NormalZIndex = 0;
        private static int SelectedZIndex = 1;

        public static void MarkNormal(Line line)
        {
            line.Stroke = BlockFactory.NormalBrush;
            Panel.SetZIndex(line, NormalZIndex);
        }

        public static void MarkNormal(Rectangle rectangle)
        {
            rectangle.Stroke = BlockFactory.NormalBrush;
            rectangle.Fill = rectangle.Fill == BlockFactory.TransparentBrush ? BlockFactory.TransparentBrush : BlockFactory.NormalBrush;
            Panel.SetZIndex(rectangle, NormalZIndex);
        }

        public static void MarkNormal(Ellipse ellipse)
        {
            ellipse.Stroke = BlockFactory.NormalBrush;
            ellipse.Fill = ellipse.Fill == BlockFactory.TransparentBrush ? BlockFactory.TransparentBrush : BlockFactory.NormalBrush;
            Panel.SetZIndex(ellipse, NormalZIndex);
        }

        public static void MarkNormal(Grid text)
        {
            BlockFactory.GetTextBlock(text).Foreground = BlockFactory.NormalBrush;
            Panel.SetZIndex(text, NormalZIndex);
        }

        public static void MarkSelected(Line line)
        {
            line.Stroke = BlockFactory.SelectedBrush;
            Panel.SetZIndex(line, SelectedZIndex);
        }

        public static void MarkSelected(Rectangle rectangle)
        {
            rectangle.Stroke = BlockFactory.SelectedBrush;
            rectangle.Fill = rectangle.Fill == BlockFactory.TransparentBrush ? BlockFactory.TransparentBrush : BlockFactory.SelectedBrush;
            Panel.SetZIndex(rectangle, SelectedZIndex);
        }

        public static void MarkSelected(Ellipse ellipse)
        {
            ellipse.Stroke = BlockFactory.SelectedBrush;
            ellipse.Fill = ellipse.Fill == BlockFactory.TransparentBrush ? BlockFactory.TransparentBrush : BlockFactory.SelectedBrush;
            Panel.SetZIndex(ellipse, SelectedZIndex);
        }

        public static void MarkSelected(Grid text)
        {
            BlockFactory.GetTextBlock(text).Foreground = BlockFactory.SelectedBrush;
            Panel.SetZIndex(text, SelectedZIndex);
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

        public static bool HaveOneBlockSelected(Block selected)
        {
            return (selected.Lines == null
                && selected.Rectangles == null
                && selected.Ellipses == null
                && selected.Blocks != null
                && selected.Texts == null
                && selected.Blocks.Count == 1);
        }

        public static void Select(Block parent)
        {
            foreach (var line in parent.Lines)
            {
                MarkSelected(line);
            }

            foreach (var rectangle in parent.Rectangles)
            {
                MarkSelected(rectangle);
            }

            foreach (var ellipse in parent.Ellipses)
            {
                MarkSelected(ellipse);
            }

            foreach (var text in parent.Texts)
            {
                MarkSelected(text);
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
                    MarkNormal(line);
                }
            }

            if (parent.Rectangles != null)
            {
                foreach (var rectangle in parent.Rectangles)
                {
                    MarkNormal(rectangle);
                }
            }

            if (parent.Ellipses != null)
            {
                foreach (var ellipse in parent.Ellipses)
                {
                    MarkNormal(ellipse);
                }
            }

            if (parent.Texts != null)
            {
                foreach (var text in parent.Texts)
                {
                    MarkNormal(text);
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
                MarkSelected(line);
                selected.Lines.Add(line);
            }

            foreach (var rectangle in logic.Rectangles)
            {
                MarkSelected(rectangle);
                selected.Rectangles.Add(rectangle);
            }

            foreach (var ellipse in logic.Ellipses)
            {
                MarkSelected(ellipse);
                selected.Ellipses.Add(ellipse);
            }

            foreach (var text in logic.Texts)
            {
                MarkSelected(text);
                selected.Texts.Add(text);
            }

            foreach (var parent in logic.Blocks)
            {
                foreach (var line in parent.Lines)
                {
                    MarkSelected(line);
                }

                foreach (var rectangle in parent.Rectangles)
                {
                    MarkSelected(rectangle);
                }

                foreach (var ellipse in parent.Ellipses)
                {
                    MarkSelected(ellipse);
                }

                foreach (var text in parent.Texts)
                {
                    MarkSelected(text);
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
                    MarkNormal(line);
                }

                selected.Lines = null;
            }

            if (selected.Rectangles != null)
            {
                foreach (var rectangle in selected.Rectangles)
                {
                    MarkNormal(rectangle);
                }

                selected.Rectangles = null;
            }

            if (selected.Ellipses != null)
            {
                foreach (var ellipse in selected.Ellipses)
                {
                    MarkNormal(ellipse);
                }

                selected.Ellipses = null;
            }

            if (selected.Texts != null)
            {
                foreach (var text in selected.Texts)
                {
                    MarkNormal(text);
                }

                selected.Texts = null;
            }

            if (selected.Blocks != null)
            {
                foreach (var parent in selected.Blocks)
                {
                    foreach (var line in parent.Lines)
                    {
                        MarkNormal(line);
                    }

                    foreach (var rectangle in parent.Rectangles)
                    {
                        MarkNormal(rectangle);
                    }

                    foreach (var ellipse in parent.Ellipses)
                    {
                        MarkNormal(ellipse);
                    }

                    foreach (var text in parent.Texts)
                    {
                        MarkNormal(text);
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
                        if (line.Stroke != BlockFactory.SelectedBrush)
                        {
                            MarkSelected(line);
                            selected.Lines.Add(line);
                        }
                        else
                        {
                            MarkNormal(line);
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
                        if (rectangle.Stroke != BlockFactory.SelectedBrush)
                        {
                            MarkSelected(rectangle);
                            selected.Rectangles.Add(rectangle);
                        }
                        else
                        {
                            MarkNormal(rectangle);
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
                        if (ellipse.Stroke != BlockFactory.SelectedBrush)
                        {
                            MarkSelected(ellipse);
                            selected.Ellipses.Add(ellipse);
                        }
                        else
                        {
                            MarkNormal(ellipse);
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
                        var tb = BlockFactory.GetTextBlock(text);
                        if (tb.Foreground != BlockFactory.SelectedBrush)
                        {
                            MarkSelected(text);
                            selected.Texts.Add(text);
                        }
                        else
                        {
                            MarkNormal(text);
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

        public static bool HitTestClick(ISheet sheet, Block parent, Block selected, Point p, double size, bool selectInsideBlock, bool resetSelected)
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

        public static bool HitTestForBlocks(ISheet sheet, Block parent, Block selected, Point p, double size)
        {
            selected.Init();

            var rect = new Rect(p.X - size, p.Y - size, 2 * size, 2 * size);

            if (parent.Blocks != null)
            {
                bool result = HitTestBlocks(parent.Blocks, selected, rect, true, true, false, sheet.GetParent());
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

        #region Toggle Fill

        public static void ToggleFill(Rectangle rectangle)
        {
            rectangle.Fill = rectangle.Fill == BlockFactory.TransparentBrush ? BlockFactory.NormalBrush : BlockFactory.TransparentBrush;
        }

        public static void ToggleFill(Ellipse ellipse)
        {
            ellipse.Fill = ellipse.Fill == BlockFactory.TransparentBrush ? BlockFactory.NormalBrush : BlockFactory.TransparentBrush;
        }

        #endregion
    }

    #endregion

    #region Block Factory

    public static class BlockFactory
    {
        #region Brushes

        public static SolidColorBrush NormalBrush = Brushes.Black;
        public static SolidColorBrush SelectedBrush = Brushes.Red;
        public static SolidColorBrush TransparentBrush = Brushes.Transparent;
        public static SolidColorBrush GridBrush = Brushes.LightGray;
        public static SolidColorBrush FrameBrush = Brushes.DarkGray;

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
            grid.Background = TransparentBrush;
            grid.Width = width;
            grid.Height = height;
            Canvas.SetLeft(grid, x);
            Canvas.SetTop(grid, y);

            var tb = new TextBlock();
            tb.HorizontalAlignment = halign;
            tb.VerticalAlignment = valign;
            tb.Background = TransparentBrush;
            tb.Foreground = NormalBrush;
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
                Stroke = NormalBrush,
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
                Fill = isFilled ? NormalBrush : TransparentBrush,
                Stroke = NormalBrush,
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
                Fill = isFilled ? NormalBrush : TransparentBrush,
                Stroke = NormalBrush,
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
    }

    #endregion

    public partial class SheetControl : UserControl
    {
        #region Fields

        private SheetOptions options = null;
        private ISheet back = null;
        private ISheet sheet = null;
        private ISheet overlay = null;
        private Stack<ChangeMessage> undos = new Stack<ChangeMessage>();
        private Stack<ChangeMessage> redos = new Stack<ChangeMessage>();
        private Mode mode = Mode.Selection;
        private Mode tempMode = Mode.None;
        private Point panStartPoint;
        private Line tempLine = null;
        private Ellipse tempStartEllipse = null;
        private Ellipse tempEndEllipse = null;
        private Rectangle tempRectangle = null;
        private Ellipse tempEllipse = null;
        private Point selectionStartPoint;
        private Rectangle selectionRect = null;
        private bool isFirstMove = true;
        private List<Line> gridLines = new List<Line>();
        private List<Line> frameLines = new List<Line>();

        #endregion

        #region Properties

        public ILibrary Library { get; set; }

        public IDatabase Database { get; set; }

        public ITextEditor TextEditor { get; set; }

        private int zoomIndex = -1;
        public int ZoomIndex
        {
            get { return zoomIndex; }
            set
            {
                if (value >= 0 && value <= options.MaxZoomIndex)
                {
                    zoomIndex = value;
                    Zoom = options.ZoomFactors[zoomIndex];
                }
            }
        }

        public double Zoom
        {
            get { return zoom.ScaleX; }
            set
            {
                if (IsLoaded)
                {
                    AdjustThickness(value);
                }
                zoom.ScaleX = value;
                zoom.ScaleY = value;
            }
        }

        public double PanX
        {
            get { return pan.X; }
            set { pan.X = value; }
        }

        public double PanY
        {
            get { return pan.Y; }
            set { pan.Y = value; }
        }

        private Block logic = null;
        public Block Logic
        {
            get { return logic; }
            set { logic = value; }
        }

        private Block selected = null;
        public Block Selected
        {
            get { return selected; }
            set { selected = value; }
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

        #region Default Options

        private SheetOptions DefaultOptions()
        {
            return new SheetOptions()
            {
                PageOriginX = 0.0,
                PageOriginY = 0.0,
                PageWidth = 1260.0,
                PageHeight = 891.0,
                SnapSize = 15,
                GridSize = 30,
                FrameThickness = 1.0,
                GridThickness = 1.0,
                SelectionThickness = 1.0,
                LineThickness = 2.0,
                HitTestSize = 3.5,
                DefaultZoomIndex = 9,
                MaxZoomIndex = 21,
                ZoomFactors = new double[] { 0.01, 0.0625, 0.0833, 0.125, 0.25, 0.3333, 0.5, 0.6667, 0.75, 1, 1.25, 1.5, 2, 3, 4, 6, 8, 12, 16, 24, 32, 64 }
            };
        }

        #endregion

        #region Init

        private void Init()
        {
            options = DefaultOptions();
            zoomIndex = options.DefaultZoomIndex;

            back = new CanvasSheet(Root.Back);
            sheet = new CanvasSheet(Root.Sheet);
            overlay = new CanvasSheet(Root.Overlay);

            Logic = new Block(0, -1, "LOGIC");
            Logic.Init();

            Selected = new Block(0, -1, "SELECTED");
        }

        private void InitLoaded()
        {
            CreateGrid(back, gridLines, 330.0, 30.0, 600.0, 750.0, options.GridSize, options.GridThickness, BlockFactory.GridBrush);
            CreateFrame(back, frameLines, options.GridSize, options.GridThickness, BlockFactory.FrameBrush);
            AdjustThickness(gridLines, options.GridThickness / options.ZoomFactors[zoomIndex]);
            AdjustThickness(frameLines, options.FrameThickness / options.ZoomFactors[zoomIndex]);
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
            double x = ItemEditor.Snap(p.X, options.SnapSize);
            double y = ItemEditor.Snap(p.Y, options.SnapSize);
            RegisterChange("Create Text");
            var text = BlockFactory.CreateText("Text", x, y, 30.0, 15.0, HorizontalAlignment.Center, VerticalAlignment.Center, 11.0);
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
            double padding = 6.0;
            double width = 1260.0;
            double height = 891.0;
            double startX = padding;
            double startY = padding;
            double rowsStart = 60;
            double rowsEnd = 780.0;

            // frame left rows
            for (double y = rowsStart; y < rowsEnd; y += size)
            {
                CreateLine(sheet, lines, thickness, startX, y, 330.0, y, stroke);
            }

            // frame right rows
            for (double y = rowsStart; y < rowsEnd; y += size)
            {
                CreateLine(sheet, lines, thickness, 930.0, y, 1260.0 - padding, y, stroke);
            }

            // frame columns
            double[] columnWidth = {  30.0, 210.0,   90.0,  600.0, 210.0,  90.0 };
            double[] columnX     = {  30.0,  30.0, startY, startY,  30.0,  30.0 };
            double[] columnY     = {  rowsEnd,  rowsEnd,   rowsEnd,   rowsEnd,  rowsEnd,  rowsEnd };

            double start = 0.0;
            for(int i = 0; i < columnWidth.Length; i++)
            {
                start += columnWidth[i];
                CreateLine(sheet, lines, thickness, start, columnX[i], start, columnY[i], stroke);
            }

            // frame header
            CreateLine(sheet, lines, thickness, startX, 30.0, width - padding, 30.0, stroke);
            
            // frame footer
            CreateLine(sheet, lines, thickness, startX, rowsEnd, width - padding, rowsEnd, stroke);

            // frame border
            CreateLine(sheet, lines, thickness, startX, startY, width - padding, startY, stroke);
            CreateLine(sheet, lines, thickness, startX, height - padding, width - padding, height - padding, stroke);
            CreateLine(sheet, lines, thickness, startX, startY, startX, height - padding, stroke);
            CreateLine(sheet, lines, thickness, width - padding, startY, width - padding, height - padding, stroke);
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
            BlockEditor.InsertBlockContents(sheet, block, Logic, Selected, select, options.LineThickness / Zoom);
        }

        private BlockItem SerializeLogicBlock()
        {
            return BlockSerializer.SerializerBlockContents(Logic, 0, Logic.DataId, "LOGIC");
        }

        private static string SerializeBlockContents(int id, int dataId, string name, Block parent)
        {
            var block = BlockSerializer.SerializerBlockContents(parent, id, dataId, name);
            var sb = new StringBuilder();
            ItemSerializer.Serialize(sb, block, "", ItemSerializeOptions.Default);
            return sb.ToString();
        }

        private BlockItem CreateBlockItem(string name)
        {
            var text = SerializeBlockContents(0, -1, name, Selected);
            Delete();
            var block = ItemSerializer.DeserializeContents(text);
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
                    RegisterChange("Create Block");
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
                var text = ItemSerializer.SerializeContents(BlockSerializer.SerializerBlockContents(Selected, 0, -1, "SELECTED"));
                var block = ItemSerializer.DeserializeContents(text);
                RegisterChange("Break Block");
                Delete();
                BlockEditor.InsertBrokenBlock(sheet, block, Logic, Selected, true, options.LineThickness / Zoom);
            }
        }

        #endregion

        #region Undo/Redo Changes

        private ChangeMessage CreateChangeMessage(string message)
        {
            var change = new ChangeMessage()
            {
                Message = message,
                Model = ItemSerializer.SerializeContents(SerializeLogicBlock())
            };
            return change;
        }

        public void RegisterChange(string message)
        {
            undos.Push(CreateChangeMessage(message));
            redos.Clear();
        }

        public void Undo()
        {
            if (undos.Count > 0)
            {
                redos.Push(CreateChangeMessage("Redo"));
                Reset();
                var undo = undos.Pop();
                InsertBlock(ItemSerializer.DeserializeContents(undo.Model), false);
            }
        }

        public void Redo()
        {
            if (redos.Count > 0)
            {
                undos.Push(CreateChangeMessage("Undo"));
                Reset();
                var redo = redos.Pop();
                InsertBlock(ItemSerializer.DeserializeContents(redo.Model), false);
            }
        }

        #endregion

        #region Clipboard

        public void Cut()
        {
            Copy();
            RegisterChange("Cut");

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
            var block = BlockEditor.HaveSelected(Selected) ?
                BlockSerializer.SerializerBlockContents(Selected, 0, -1, "SELECTED") : SerializeLogicBlock();
            var text = ItemSerializer.SerializeContents(block);
            Clipboard.SetData(DataFormats.UnicodeText, text);
            //string json = JsonConvert.SerializeObject(block, Formatting.Indented);
            //Clipboard.SetData(DataFormats.UnicodeText, json);
        }

        public void Paste()
        {
            var text = (string)Clipboard.GetData(DataFormats.UnicodeText);
            var block = ItemSerializer.DeserializeContents(text);
            InsertBlock(block, true);
            //var block = JsonConvert.DeserializeObject<BlockItem>(text);
            //InsertBlock(block, true);
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

        private Block Insert(BlockItem blockItem, Point p, bool select)
        {
            BlockEditor.DeselectAll(Selected);
            double thickness = options.LineThickness / Zoom;

            if (select)
            {
                Selected.Blocks = new List<Block>();
            }

            RegisterChange("Insert Block");

            var block = BlockSerializer.DeserializeBlockItem(sheet, Logic, blockItem, select, thickness);

            if (select)
            {
                Selected.Blocks.Add(block);
            }

            BlockEditor.Move(ItemEditor.Snap(p.X, options.SnapSize), ItemEditor.Snap(p.Y, options.SnapSize), block);

            return block;
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
                var blocks = ItemSerializer.DeserializeContents(text).Blocks;
                Library.SetSource(blocks);
            }
        }

        private void AddToLibrary(BlockItem blockItem)
        {
            if (Library != null && blockItem != null)
            {
                var source = Library.GetSource() as IEnumerable<BlockItem>;
                var items = new List<BlockItem>(source);
                ItemEditor.ResetPosition(blockItem, options.PageOriginX, options.PageOriginY, options.PageWidth, options.PageHeight);
                items.Add(blockItem);
                Library.SetSource(items);
            }
        }

        #endregion

        #region Reset

        public void Delete()
        {
            if (BlockEditor.HaveSelected(Selected))
            {
                RegisterChange("Delete");
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
            RegisterChange("Move");
            BlockEditor.Move(x, y, BlockEditor.HaveSelected(Selected) ? Selected : Logic);
        }

        public void MoveUp()
        {
            Move(0.0, -options.SnapSize);
        }

        public void MoveDown()
        {
            Move(0.0, options.SnapSize);
        }

        public void MoveLeft()
        {
            Move(-options.SnapSize, 0.0);
        }

        public void MoveRight()
        {
            Move(options.SnapSize, 0.0);
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
                BlockEditor.ToggleFill(tempRectangle);
            }

            if (tempEllipse != null)
            {
                BlockEditor.ToggleFill(tempEllipse);
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
            var temp = new Block(0, -1, "TEMP");
            BlockEditor.HitTestClick(sheet, Selected, temp, p, options.HitTestSize, false, true);

            if (BlockEditor.HaveSelected(temp))
            {
                return true;
            }

            return false;
        }

        private void InitMove(Point p)
        {
            isFirstMove = true;
            StoreTempMode();
            ModeMove();
            p.X = ItemEditor.Snap(p.X, options.SnapSize);
            p.Y = ItemEditor.Snap(p.Y, options.SnapSize);
            panStartPoint = p;
            ResetTempOverlayElements();
            overlay.Capture();
        }

        private void Move(Point p)
        {
            if (isFirstMove)
            {
                RegisterChange("Move");
                isFirstMove = false;
                Cursor = Cursors.SizeAll;
            }

            p.X = ItemEditor.Snap(p.X, options.SnapSize);
            p.Y = ItemEditor.Snap(p.Y, options.SnapSize);

            double dx = p.X - panStartPoint.X;
            double dy = p.Y - panStartPoint.Y;

            if (dx != 0.0 || dy != 0.0)
            {
                BlockEditor.Move(dx, dy, Selected);
                panStartPoint = p;
            }  
        }

        private void FinishMove()
        {
            RestoreTempMode();
            Cursor = Cursors.Arrow;
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
            double gridThicknessZoomed = options.GridThickness / zoom;
            double frameThicknessZoomed = options.FrameThickness / zoom;
            double lineThicknessZoomed = options.LineThickness / zoom;
            double selectionThicknessZoomed = options.SelectionThickness / zoom;

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
            double oldZoom = options.ZoomFactors[oldZoomIndex];
            double newZoom = options.ZoomFactors[zoomIndex];
            Zoom = newZoom;
            PanX = (x * oldZoom + PanX) - x * newZoom;
            PanY = (y * oldZoom + PanY) - y * newZoom;
        }

        private void ZoomTo(int delta, Point p)
        {
            if (delta > 0)
            {
                if (zoomIndex < options.MaxZoomIndex)
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
            Cursor = Cursors.ScrollAll;
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
            Cursor = Cursors.Arrow;
            overlay.ReleaseCapture();
        }

        private void ResetPanAndZoom()
        {
            zoomIndex = options.DefaultZoomIndex;
            Zoom = options.ZoomFactors[zoomIndex];
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
            selectionRect = BlockFactory.CreateSelectionRectangle(options.SelectionThickness / Zoom, x, y, 0.0, 0.0);
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
            double x = ItemEditor.Snap(p.X, options.SnapSize);
            double y = ItemEditor.Snap(p.Y, options.SnapSize);
            tempLine = BlockFactory.CreateLine(options.LineThickness / Zoom, x, y, x, y);
            tempStartEllipse = BlockFactory.CreateEllipse(options.LineThickness / Zoom, x - 4.0, y - 4.0, 8.0, 8.0, true);
            tempEndEllipse = BlockFactory.CreateEllipse(options.LineThickness / Zoom, x - 4.0, y - 4.0, 8.0, 8.0, true);
            overlay.Add(tempLine);
            overlay.Add(tempStartEllipse);
            overlay.Add(tempEndEllipse);
            overlay.Capture();
        }

        private void MoveTempLine(Point p)
        {
            double x = ItemEditor.Snap(p.X, options.SnapSize);
            double y = ItemEditor.Snap(p.Y, options.SnapSize);
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
                RegisterChange("Create Line");
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
            double x = ItemEditor.Snap(p.X, options.SnapSize);
            double y = ItemEditor.Snap(p.Y, options.SnapSize);
            selectionStartPoint = new Point(x, y);
            tempRectangle = BlockFactory.CreateRectangle(options.SelectionThickness / Zoom, x, y, 0.0, 0.0, true);
            overlay.Add(tempRectangle);
            overlay.Capture();
        }

        private void MoveTempRect(Point p)
        {
            double sx = selectionStartPoint.X;
            double sy = selectionStartPoint.Y;
            double x = ItemEditor.Snap(p.X, options.SnapSize);
            double y = ItemEditor.Snap(p.Y, options.SnapSize);
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
                RegisterChange("Create Rectangle");
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
            double x = ItemEditor.Snap(p.X, options.SnapSize);
            double y = ItemEditor.Snap(p.Y, options.SnapSize);
            selectionStartPoint = new Point(x, y);
            tempEllipse = BlockFactory.CreateEllipse(options.SelectionThickness / Zoom, x, y, 0.0, 0.0, true);
            overlay.Add(tempEllipse);
            overlay.Capture();
        }

        private void MoveTempEllipse(Point p)
        {
            double sx = selectionStartPoint.X;
            double sy = selectionStartPoint.Y;
            double x = ItemEditor.Snap(p.X, options.SnapSize);
            double y = ItemEditor.Snap(p.Y, options.SnapSize);
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
                RegisterChange("Create Ellipse");
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
            var temp = new Block(0, -1, "TEMP");
            BlockEditor.HitTestClick(sheet, Logic, temp, p, options.HitTestSize, true, true);

            if (BlockEditor.HaveOneTextSelected(temp))
            {
                var tb = BlockFactory.GetTextBlock(temp.Texts[0]);

                StoreTempMode();
                ModeNone();

                Action<string> ok = (text) =>
                {
                    RegisterChange("Edit Text");
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

        #region Preview

        private CanvasControl CreatePreview(Block parent)
        {
            var root = new CanvasControl();
            var sheet = new CanvasSheet(root.Sheet);

            //CreateGrid(sheet, null, 330.0, 30.0, 600.0, 720.0, options.GridSize, 0.013 * 96.0 / 2.54 /*options.GridThickness*/, BlockEditor.GridBrush);
            CreateFrame(sheet, null, options.GridSize, /* 0.013 * 96.0 / 2.54 */ options.GridThickness, BlockFactory.NormalBrush);

            var block = BlockSerializer.SerializerBlockContents(parent, 0, -1, "LOGIC");
            BlockEditor.InsertBlockContents(sheet, block, null, null, false, /* 0.035 * 96.0 / 2.54 */ options.LineThickness);

            //var vb = new Viewbox { Child = root };
            //vb.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            //vb.Arrange(new Rect(vb.DesiredSize));

            return root;
        }

        public void Preview()
        {
            var window = new Window()
            {
                Title = "Preview",
                SizeToContent = SizeToContent.WidthAndHeight,
                WindowStartupLocation = WindowStartupLocation.CenterScreen
            };

            window.Content = CreatePreview(Logic);
            window.Show();
        }

        #endregion

        #region Export Pdf

        private void ExportToPdf(string fileName)
        {
            var writer = new BlockPdfWriter();

            var page = new BlockItem();
            page.Init(0, -1, "");

            //var grid = CreateGridBlock();
            var frame = CreateFrameBlock();
            var logic = BlockSerializer.SerializerBlockContents(Logic, 0, Logic.DataId, "LOGIC");

            //page.Blocks.Add(grid);
            page.Blocks.Add(frame);
            page.Blocks.Add(logic);

            writer.Create(fileName, 1260.0, 891.0, page);
        }

        private BlockItem CreateGridBlock()
        {
            var grid = new BlockItem();
            grid.Init(0, -1, "");

            foreach (var line in gridLines)
            {
                var lineItem = BlockSerializer.SerializeLine(line);
                lineItem.StrokeThickness = 0.013 * 72.0 / 2.54;// 0.13mm
                //lineItem.Stroke = new ItemColor() { Alpha = 255, Red = 0, Green = 0, Blue = 0 };
                grid.Lines.Add(lineItem);
            }
            return grid;
        }

        private BlockItem CreateFrameBlock()
        {
            var frame = new BlockItem();
            frame.Init(0, -1, "");

            foreach (var line in frameLines)
            {
                var lineItem = BlockSerializer.SerializeLine(line);
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
                Filter = "TXT Files (*.txt)|*.txt|JSON Files (*.json)|*.json|All Files (*.*)|*.*"
            };

            if (dlg.ShowDialog() == true)
            {
                var text = ItemEditor.OpenText(dlg.FileName);
                if (text != null)
                {
                    switch (dlg.FilterIndex)
                    {
                        case 1:
                            {
                                RegisterChange("Open");
                                Reset();
                                var block = ItemSerializer.DeserializeContents(text);
                                InsertBlock(block, false);
                            }
                            break;
                        case 2:
                        case 3:
                            {
                                RegisterChange("Open");
                                Reset();
                                var block = JsonConvert.DeserializeObject<BlockItem>(text);
                                InsertBlock(block, false);
                            }
                            break;
                    }
                }
            }
        }

        public void Save()
        {
            var dlg = new Microsoft.Win32.SaveFileDialog()
            {
                Filter = "TXT Files (*.txt)|*.txt|JSON Files (*.json)|*.json|All Files (*.*)|*.*",
                FileName = "sheet"
            };

            if (dlg.ShowDialog() == true)
            {
                switch (dlg.FilterIndex)
                {
                    case 1:
                        {
                            var block = SerializeLogicBlock();
                            var text = ItemSerializer.SerializeContents(block);
                            ItemEditor.SaveText(dlg.FileName, text);
                        }
                        break;
                    case 2:
                    case 3:
                        {
                            var block = SerializeLogicBlock();
                            string text = JsonConvert.SerializeObject(block, Formatting.Indented);
                            ItemEditor.SaveText(dlg.FileName, text);
                        }
                        break;
                }
            }
        }

        public void Export()
        {
            var dlg = new Microsoft.Win32.SaveFileDialog()
            {
                Filter = "PDF Documents (*.pdf)|*.pdf|All Files (*.*)|*.*",
                FileName = "sheet"
            };

            if (dlg.ShowDialog() == true)
            {
                switch(dlg.FilterIndex)
                {
                    case 1:
                    default:
                        {
                            ExportToPdf(dlg.FileName);
                        }
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
                    InitMove(e.GetPosition(overlay.GetParent()));
                    return;
                }

                BlockEditor.DeselectAll(Selected);
            }

            bool resetSelected = ctrl && BlockEditor.HaveSelected(Selected) ? false : true;

            if (GetMode() == Mode.Selection)
            {
                bool result = BlockEditor.HitTestClick(sheet, Logic, Selected, e.GetPosition(overlay.GetParent()), options.HitTestSize, false, resetSelected);
                if ((ctrl || !BlockEditor.HaveSelected(Selected)) && !result)
                {
                    InitSelectionRect(e.GetPosition(overlay.GetParent()));
                }
                else
                {
                    InitMove(e.GetPosition(overlay.GetParent()));
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
            bool shift = (Keyboard.Modifiers == ModifierKeys.Shift);

            // mouse over selection when holding Shift key
            if (shift && selectionRect == null && !overlay.IsCaptured)
            {
                if (BlockEditor.HaveSelected(Selected))
                {
                    BlockEditor.DeselectAll(Selected);
                }

                BlockEditor.HitTestClick(sheet, Logic, Selected, e.GetPosition(overlay.GetParent()), options.HitTestSize, false, false);
            }

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
                Move(e.GetPosition(overlay.GetParent()));
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

        #region Data Binding

        private bool BindDataToBlock(Point p, DataItem dataItem)
        {
            var temp = new Block(0, -1, "TEMP");
            BlockEditor.HitTestForBlocks(sheet, Logic, temp, p, options.HitTestSize);

            if (BlockEditor.HaveOneBlockSelected(temp))
            {
                RegisterChange("Bind Data");
                var block = temp.Blocks[0];
                BindDataToBlock(block, dataItem);
                BlockEditor.Deselect(temp);
                return true;
            }

            BlockEditor.Deselect(temp);
            return false;
        }

        private bool BindDataToBlock(Block block, DataItem dataItem)
        {
            if (block != null && block.Texts != null 
                && dataItem != null && dataItem.Columns != null  && dataItem.Data != null
                && block.Texts.Count == dataItem.Columns.Length - 1)
            {
                // assign block data id
                block.DataId = int.Parse(dataItem.Data[0]);

                // skip index column
                int i = 1;

                foreach (var text in block.Texts)
                {
                    var tb = BlockFactory.GetTextBlock(text);
                    tb.Text = dataItem.Data[i];
                    i++;
                }

                return true;
            }

            return false;
        }

        private void TryToBindData(Point p, DataItem dataItem)
        {
            // first try binding to existing block
            bool firstTryResult = BindDataToBlock(p, dataItem);

            // if failed insert selected block from library and try again to bind
            if (!firstTryResult)
            {
                var blockItem = Library.GetSelected();
                if (blockItem != null)
                {
                    var block = Insert(blockItem, p, false);
                    bool secondTryResult = BindDataToBlock(block, dataItem);
                    if (!secondTryResult)
                    {
                        // remove block if failed to bind
                        var temp = new Block(0, -1, "TEMP");
                        temp.Init();
                        temp.Blocks.Add(block);
                        BlockEditor.Remove(sheet, Logic, temp);
                    }
                }
            }
        }

        #endregion

        #region Drop

        private void UserControl_DragEnter(object sender, DragEventArgs e)
        {
            if (!e.Data.GetDataPresent("Block") || !e.Data.GetDataPresent("Data") || sender == e.Source)
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
            else if (e.Data.GetDataPresent("Data"))
            {
                var dataItem = e.Data.GetData("Data") as DataItem;
                if (dataItem != null)
                {
                    TryToBindData(e.GetPosition(overlay.GetParent()), dataItem);
                    e.Handled = true;
                }
            }
        }

        #endregion
    }
}
