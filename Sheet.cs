using Newtonsoft.Json;
using Splat;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.IO.Packaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
    #region Entry Model

    public abstract class Entry
    {
        public string Name { get; set; }
        public bool IsNew { get; set; }
        public bool IsModified { get; set; }
    }

    public class SolutionEntry : Entry
    {
        public ObservableCollection<DocumentEntry> Documents { get; set; }
    }

    public class DocumentEntry : Entry
    {
        public SolutionEntry Solution { get; set; }
        public ObservableCollection<PageEntry> Pages { get; set; }
    }

    public class PageEntry : Entry
    {
        public DocumentEntry Document { get; set; }
        public string Content { get; set; }
    }

    public interface IEntryController
    {
        void Set(string text);
        string Get();
        void Export(string text);
        void Export(IEnumerable<string> texts);
    }

    #endregion

    #region Entry Editor

    public static class EntryController
    {
        #region Fields

        private static char[] entryNameSeparator = { '/' };

        #endregion

        #region Archive

        public static void NewSolutionArchive(string path)
        {
            using (FileStream fs = new FileStream(path, FileMode.Create))
            {
                using (ZipArchive zip = new ZipArchive(fs, ZipArchiveMode.Update))
                {
                    EntryController.AddPageEntry(zip, "Document0", "Page", "");
                }
            }
        }

        public static SolutionEntry OpenSolutionArchive(string path)
        {
            string solutionName = System.IO.Path.GetFileNameWithoutExtension(path);

            var dict = new Dictionary<string, List<Tuple<string, string>>>();
            var solution = new SolutionEntry() { Name = solutionName, Documents = new ObservableCollection<DocumentEntry>() };

            using (var zip = ZipFile.Open(path, ZipArchiveMode.Read))
            {
                foreach (var entry in zip.Entries)
                {
                    var e = entry.FullName.Split(entryNameSeparator);
                    if (e.Length == 1)
                    {
                        string key = e[0];

                        if (!dict.ContainsKey(key))
                        {
                            dict.Add(key, new List<Tuple<string, string>>());
                        }
                    }
                    else if (e.Length == 2)
                    {
                        string key = e[0];
                        string data = e[1];
                        string content = null;

                        using (var reader = new StreamReader(entry.Open()))
                        {
                            content = reader.ReadToEnd();
                        }

                        if (!dict.ContainsKey(key))
                        {
                            dict.Add(key, new List<Tuple<string, string>>());
                        }

                        dict[key].Add(new Tuple<string, string>(data, content));
                    }
                }
            }

            foreach (var item in dict)
            {
                var document = EntryController.CreateDocument(solution, item.Key);
                solution.Documents.Add(document);
                foreach (var tuple in item.Value)
                {
                    var page = EntryController.CreatePage(document, tuple.Item2, tuple.Item1);
                    document.Pages.Add(page);
                }
            }

            return solution;
        }

        public static void CreateSolutionArchive(SolutionEntry solution, string path)
        {
            using (FileStream fs = new FileStream(path, FileMode.Create))
            {
                using (ZipArchive zip = new ZipArchive(fs, ZipArchiveMode.Update))
                {
                    foreach (var document in solution.Documents)
                    {
                        if (document.Pages.Count <= 0)
                        {
                            AddDocumentEntry(zip, document.Name);
                        }

                        foreach (var page in document.Pages)
                        {
                            AddPageEntry(zip, document.Name, page.Name, page.Content);
                        }
                    }
                }
            }
        }

        public static void CreateSolutionArchive(SolutionEntry solution)
        {
            CreateSolutionArchive(solution, string.Concat(solution.Name, ".zip"));
        }

        public static void AddDocumentEntry(ZipArchive zip, string document)
        {
            var name = string.Concat(document, '/');
            var entry = zip.CreateEntry(name);
        }

        public static void AddPageEntry(ZipArchive zip, string document, string page, string content)
        {
            var name = string.Concat(document, '/', page);
            var entry = zip.CreateEntry(name);
            using (var writer = new StreamWriter(entry.Open()))
            {
                writer.Write(content);
            }
        }

        #endregion

        #region Factory

        public static PageEntry CreatePage(DocumentEntry document, string content, string name = null)
        {
            var page = new PageEntry()
            {
                Name = name == null ? "Page" : name,
                Content = content,
                Document = document
            };
            return page;
        }

        public static DocumentEntry CreateDocument(SolutionEntry solution, string name = null)
        {
            var document = new DocumentEntry()
            {
                Name = name == null ? string.Concat("Document", solution.Documents.Count) : name,
                Pages = new ObservableCollection<PageEntry>(),
                Solution = solution
            };
            return document;
        }

        #endregion

        #region Page

        public static PageEntry AddPage(DocumentEntry document, string content)
        {
            var page = CreatePage(document, content);
            document.Pages.Add(page);
            return page;
        }

        public static PageEntry AddPageBefore(DocumentEntry document, PageEntry beofore, string content)
        {
            var page = CreatePage(document, content);
            int index = document.Pages.IndexOf(beofore);
            document.Pages.Insert(index, page);
            return page;
        }

        public static PageEntry AddPageAfter(DocumentEntry document, PageEntry after, string content)
        {
            var page = CreatePage(document, content);
            int index = document.Pages.IndexOf(after);
            document.Pages.Insert(index + 1, page);
            return page;
        }

        public static void AddPageAfter(object item)
        {
            if (item != null && item is PageEntry)
            {
                var page = item as PageEntry;
                var document = page.Document;
                if (document != null)
                {
                    AddPageAfter(document, page, "");
                }
            }
        }

        public static void AddPageBefore(object item)
        {
            if (item != null && item is PageEntry)
            {
                var page = item as PageEntry;
                var document = page.Document;
                if (document != null)
                {
                    AddPageBefore(document, page, "");
                }
            }
        }

        public static void DuplicatePage(object item)
        {
            if (item != null && item is PageEntry)
            {
                var page = item as PageEntry;
                var document = page.Document;
                if (document != null)
                {
                    AddPage(document, page.Content);
                }
            }
        }

        public static void RemovePage(object item)
        {
            if (item != null && item is PageEntry)
            {
                var page = item as PageEntry;
                var document = page.Document;
                if (document != null)
                {
                    document.Pages.Remove(page);
                }
            }
        }

        #endregion

        #region Document

        public static DocumentEntry AddDocumentBefore(SolutionEntry solution, DocumentEntry after)
        {
            var document = CreateDocument(solution);
            int index = solution.Documents.IndexOf(after);
            solution.Documents.Insert(index, document);
            return document;
        }

        public static DocumentEntry AddDocumentAfter(SolutionEntry solution, DocumentEntry after)
        {
            var document = CreateDocument(solution);
            int index = solution.Documents.IndexOf(after);
            solution.Documents.Insert(index + 1, document);
            return document;
        }

        public static DocumentEntry AddDocument(SolutionEntry solution)
        {
            var document = CreateDocument(solution);
            solution.Documents.Add(document);
            return document;
        }

        public static void DocumentAddPage(object item)
        {
            if (item != null && item is DocumentEntry)
            {
                var document = item as DocumentEntry;
                AddPage(document, "");
            }
        }

        public static void AddDocumentAfter(object item)
        {
            if (item != null && item is DocumentEntry)
            {
                var document = item as DocumentEntry;
                var solution = document.Solution;
                if (solution != null)
                {
                    AddDocumentAfter(solution, document);
                }
            }
        }

        public static void AddDocumentBefore(object item)
        {
            if (item != null && item is DocumentEntry)
            {
                var document = item as DocumentEntry;
                var solution = document.Solution;
                if (solution != null)
                {
                    AddDocumentBefore(solution, document);
                }
            }
        }

        public static void DulicateDocument(object item)
        {
            if (item != null && item is DocumentEntry)
            {
                var document = item as DocumentEntry;
                var solution = document.Solution;
                if (solution != null)
                {
                    var duplicate = AddDocument(solution);
                    foreach (var page in document.Pages)
                    {
                        AddPage(duplicate, page.Content);
                    }
                }
            }
        }

        public static void RemoveDocument(object item)
        {
            if (item != null && item is DocumentEntry)
            {
                var document = item as DocumentEntry;
                var solution = document.Solution;
                if (solution != null)
                {
                    solution.Documents.Remove(document);
                }
            }
        }

        #endregion
    }

    #endregion

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

    public class ImageItem : Item
    {
        public double X { get; set; }
        public double Y { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }
        public byte[] Data { get; set; }
    }

    public class BlockItem : Item
    {
        public double X { get; set; }
        public double Y { get; set; }
        public string Name { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }
        public ItemColor Backgroud { get; set; }
        public int DataId { get; set; }
        public List<LineItem> Lines { get; set; }
        public List<RectangleItem> Rectangles { get; set; }
        public List<EllipseItem> Ellipses { get; set; }
        public List<TextItem> Texts { get; set; }
        public List<ImageItem> Images { get; set; }
        public List<BlockItem> Blocks { get; set; }
        public void Init(int id, double x, double y, int dataId, string name)
        {
            X = x;
            Y = y;
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
            Images = new List<ImageItem>();
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

    public interface ITextController
    {
        void Set(Action<string> ok, Action cancel, string title, string label, string text);
    }

    public interface IBlockController
    {
        BlockItem Serialize();
        void Insert(BlockItem block);
        void Reset();
    }

    public interface IHistoryController
    {
        void Register(string message);
        void Reset();
        void Undo();
        void Redo();
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

        public static void Serialize(StringBuilder sb, ImageItem image, string indent, ItemSerializeOptions options)
        {
            sb.Append(indent);
            sb.Append("IMAGE");
            sb.Append(options.ModelSeparator);
            sb.Append(image.Id);
            sb.Append(options.ModelSeparator);
            sb.Append(image.X);
            sb.Append(options.ModelSeparator);
            sb.Append(image.Y);
            sb.Append(options.ModelSeparator);
            sb.Append(image.Width);
            sb.Append(options.ModelSeparator);
            sb.Append(image.Height);
            sb.Append(options.ModelSeparator);
            sb.Append(Base64.ToBase64(image.Data));
            sb.Append(options.LineSeparator);
        }

        public static void Serialize(StringBuilder sb, BlockItem block, string indent, ItemSerializeOptions options)
        {
            sb.Append(indent);
            sb.Append("BLOCK");
            sb.Append(options.ModelSeparator);
            sb.Append(block.Id);
            sb.Append(options.ModelSeparator);
            sb.Append(block.X);
            sb.Append(options.ModelSeparator);
            sb.Append(block.Y);
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
            Serialize(sb, block.Images, indent + options.IndentWhiteSpace, options);
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

        public static void Serialize(StringBuilder sb, IEnumerable<ImageItem> images, string indent, ItemSerializeOptions options)
        {
            foreach (var image in images)
            {
                Serialize(sb, image, indent, options);
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
            Serialize(sb, block.Images, "", options);
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
            lineItem.Stroke = new ItemColor()
            {
                Alpha = int.Parse(m[6]),
                Red = int.Parse(m[7]),
                Green = int.Parse(m[8]),
                Blue = int.Parse(m[9])
            };
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
            return textItem;
        }

        private static ImageItem DeserializeImage(string[] m)
        {
            var imageItem = new ImageItem();
            imageItem.Id = int.Parse(m[1]);
            imageItem.X = double.Parse(m[2]);
            imageItem.Y = double.Parse(m[3]);
            imageItem.Width = double.Parse(m[4]);
            imageItem.Height = double.Parse(m[5]);
            imageItem.Data = Base64.ToBytes(m[6]);
            return imageItem;
        }

        private static BlockItem DeserializeBlockRecursive(string[] lines,
            int length,
            ref int end,
            string[] m,
            ItemSerializeOptions options)
        {
            var blockItem = DeserializeRootBlock(lines,
                length,
                ref end,
                m[4],
                int.Parse(m[1]),
                double.Parse(m[2]),
                double.Parse(m[3]),
                int.Parse(m[11]),
                options);

            blockItem.Width = double.Parse(m[5]);
            blockItem.Width = double.Parse(m[6]);
            blockItem.Backgroud = new ItemColor()
            {
                Alpha = int.Parse(m[7]),
                Red = int.Parse(m[8]),
                Green = int.Parse(m[9]),
                Blue = int.Parse(m[10])
            };
            blockItem.DataId = int.Parse(m[11]);
            return blockItem;
        }

        private static BlockItem DeserializeRootBlock(string[] lines,
            int length,
            ref int end,
            string name,
            int id,
            double x,
            double y,
            int dataId,
            ItemSerializeOptions options)
        {
            var root = new BlockItem();
            root.Init(id, x, y, dataId, name);

            for (; end < length; end++)
            {
                string line = lines[end].TrimStart(options.WhiteSpace);
                var m = line.Split(options.ModelSeparators);
                if (m.Length == 10 && string.Compare(m[0], "LINE", true) == 0)
                {
                    if (m.Length == 10)
                    {
                        var lineItem = DeserializeLine(m);
                        root.Lines.Add(lineItem);
                    }
                    else
                    {
                        throw new Exception(string.Format("Invalid LINE item at line {0}", end + 1));
                    }
                }
                else if (string.Compare(m[0], "RECTANGLE", true) == 0)
                {
                    if (m.Length == 15)
                    {
                        var rectangleItem = DeserializeRectangle(m);
                        root.Rectangles.Add(rectangleItem);
                    }
                    else
                    {
                        throw new Exception(string.Format("Invalid RECTANGLE item at line {0}", end + 1));
                    }
                }
                else if (string.Compare(m[0], "ELLIPSE", true) == 0)
                {
                    if (m.Length == 15)
                    {
                        var ellipseItem = DeserializeEllipse(m);
                        root.Ellipses.Add(ellipseItem);
                    }
                    else
                    {
                        throw new Exception(string.Format("Invalid ELLIPSE item at line {0}", end + 1));
                    }
                }
                else if (string.Compare(m[0], "TEXT", true) == 0)
                {
                    if (m.Length == 18)
                    {
                        var textItem = DeserializeText(m);
                        root.Texts.Add(textItem);
                    }
                    else
                    {
                        throw new Exception(string.Format("Invalid TEXT item at line {0}", end + 1));
                    }
                }
                else if (string.Compare(m[0], "IMAGE", true) == 0)
                {
                    if (m.Length == 7)
                    {
                        var imageItem = DeserializeImage(m);
                        root.Images.Add(imageItem);
                    }
                    else
                    {
                        throw new Exception(string.Format("Invalid IMAGE item at line {0}", end + 1));
                    }
                }
                else if (string.Compare(m[0], "BLOCK", true) == 0)
                {
                    if (m.Length == 12)
                    {
                        end++;
                        var blockItem = DeserializeBlockRecursive(lines, length, ref end, m, options);
                        root.Blocks.Add(blockItem);
                        continue;
                    }
                    else
                    {
                        throw new Exception(string.Format("Invalid BLOCK item at line {0}", end + 1));
                    }
                }
                else if (string.Compare(m[0], "END", true) == 0)
                {
                    if (m.Length == 1)
                    {
                        return root;
                    }
                    else
                    {
                        throw new Exception(string.Format("Invalid END item at line {0}", end + 1));
                    }
                }
                else if (m[0].StartsWith("//"))
                {
                    continue;
                }
                else
                {
                    throw new Exception(string.Format("Invalid item at line {0}", end + 1));
                }
            }
            return root;
        }

        public static BlockItem DeserializeContents(string model, ItemSerializeOptions options)
        {
            try
            {
                string[] lines = model.Split(options.LineSeparators, StringSplitOptions.RemoveEmptyEntries);
                int length = lines.Length;
                int end = 0;
                return DeserializeRootBlock(lines, length, ref end, "", 0, 0.0, 0.0, -1, options);
            }
            catch (Exception ex)
            {
                Debug.Print(ex.Message);
                Debug.Print(ex.StackTrace);
            }
            return null;
        }

        public static BlockItem DeserializeContents(string model)
        {
            return DeserializeContents(model, ItemSerializeOptions.Default);
        }

        #endregion
    }

    #endregion

    #region Item Controller

    public static class ItemController
    {
        #region Text

        public async static Task<string> OpenText(string fileName)
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

        public async static void SaveText(string fileName, string text)
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
            MinMax(block.Images, ref minX, ref minY, ref maxX, ref maxY);
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

        public static void MinMax(IEnumerable<ImageItem> images, ref double minX, ref double minY, ref double maxX, ref double maxY)
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
            Move(block.Images, x, y);
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

        public static void Move(IEnumerable<ImageItem> images, double x, double y)
        {
            foreach (var image in images)
            {
                Move(image, x, y);
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

        public static void Move(ImageItem image, double x, double y)
        {
            image.X += x;
            image.Y += y;
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
        public double X { get; set; }
        public double Y { get; set; }
        public string Name { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }
        public Color Backgroud { get; set; }
        public int DataId { get; set; }
        public List<Line> Lines { get; set; }
        public List<Rectangle> Rectangles { get; set; }
        public List<Ellipse> Ellipses { get; set; }
        public List<Grid> Texts { get; set; }
        public List<Image> Images { get; set; }
        public List<Block> Blocks { get; set; }
        public Block(int id, double x, double y, int dataId, string name)
        {
            Id = id;
            X = x;
            Y = y;
            DataId = dataId;
            Name = name;
        }
        public void Init()
        {
            Lines = new List<Line>();
            Rectangles = new List<Rectangle>();
            Ellipses = new List<Ellipse>();
            Texts = new List<Grid>();
            Images = new List<Image>();
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

            if (Images == null)
            {
                Images = new List<Image>();
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
        Edit,
        Line,
        Rectangle,
        Ellipse,
        Text,
        Image,
        TextEditor
    }

    public enum ItemType
    {
        None,
        Line,
        Rectangle,
        Ellipse,
        Text,
        Image
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

        public static ImageItem SerializeImage(Image image)
        {
            var imageItem = new ImageItem();

            imageItem.Id = 0;
            imageItem.X = Canvas.GetLeft(image);
            imageItem.Y = Canvas.GetTop(image);
            imageItem.Width = image.Width;
            imageItem.Height = image.Height;
            imageItem.Data = image.Tag as byte[];

            return imageItem;
        }

        public static BlockItem SerializeBlock(Block parent)
        {
            var blockItem = new BlockItem();
            blockItem.Init(0, parent.X, parent.Y, parent.DataId, parent.Name);
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

            foreach (var image in parent.Images)
            {
                blockItem.Images.Add(SerializeImage(image));
            }

            foreach (var block in parent.Blocks)
            {
                blockItem.Blocks.Add(SerializeBlock(block));
            }

            return blockItem;
        }

        public static BlockItem SerializerBlockContents(Block parent, int id, double x, double y, int dataId, string name)
        {
            var lines = parent.Lines;
            var rectangles = parent.Rectangles;
            var ellipses = parent.Ellipses;
            var texts = parent.Texts;
            var images = parent.Images;
            var blocks = parent.Blocks;

            var sheet = new BlockItem() { Backgroud = new ItemColor() };
            sheet.Init(id, x, y, dataId, name);

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

            if (images != null)
            {
                foreach (var image in images)
                {
                    sheet.Images.Add(SerializeImage(image));
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
            var line = BlockFactory.CreateLine(thickness, lineItem.X1, lineItem.Y1, lineItem.X2, lineItem.Y2, BlockFactory.NormalBrush);

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
                textItem.Size,
                BlockFactory.TransparentBrush,
                BlockFactory.NormalBrush);

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

        public static Image DeserializeImageItem(ISheet sheet, Block parent, ImageItem imageItem)
        {
            var image = BlockFactory.CreateImage(imageItem.X, imageItem.Y, imageItem.Width, imageItem.Height, imageItem.Data);

            if (parent != null)
            {
                parent.Images.Add(image);
            }

            if (sheet != null)
            {
                sheet.Add(image);
            }

            return image;
        }

        public static Block DeserializeBlockItem(ISheet sheet, Block parent, BlockItem blockItem, bool select, double thickness)
        {
            var block = new Block(blockItem.Id, blockItem.X, blockItem.Y, blockItem.DataId, blockItem.Name);
            block.Init();

            foreach (var textItem in blockItem.Texts)
            {
                var text = DeserializeTextItem(sheet, block, textItem);

                if (select)
                {
                    BlockController.SelectText(text);
                }
            }

            foreach (var imageItem in blockItem.Images)
            {
                var image = DeserializeImageItem(sheet, block, imageItem);

                if (select)
                {
                    BlockController.SelectImage(image);
                }
            }

            foreach (var lineItem in blockItem.Lines)
            {
                var line = DeserializeLineItem(sheet, block, lineItem, thickness);

                if (select)
                {
                    BlockController.SelectLine(line);
                }
            }

            foreach (var rectangleItem in blockItem.Rectangles)
            {
                var rectangle = DeserializeRectangleItem(sheet, block, rectangleItem, thickness);

                if (select)
                {
                    BlockController.SelectRectangle(rectangle);
                }
            }

            foreach (var ellipseItem in blockItem.Ellipses)
            {
                var ellipse = DeserializeEllipseItem(sheet, block, ellipseItem, thickness);

                if (select)
                {
                    BlockController.SelectEllipse(ellipse);
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

    #region Block Controller

    public static class BlockController
    {
        #region Add

        public static void AddLines(ISheet sheet, IEnumerable<LineItem> lineItems, Block parent, Block selected, bool select, double thickness)
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
                    SelectLine(line);
                    selected.Lines.Add(line);
                }
            }
        }

        public static void AddRectangles(ISheet sheet, IEnumerable<RectangleItem> rectangleItems, Block parent, Block selected, bool select, double thickness)
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
                    SelectRectangle(rectangle);
                    selected.Rectangles.Add(rectangle);
                }
            }
        }

        public static void AddEllipses(ISheet sheet, IEnumerable<EllipseItem> ellipseItems, Block parent, Block selected, bool select, double thickness)
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
                    SelectEllipse(ellipse);
                    selected.Ellipses.Add(ellipse);
                }
            }
        }

        public static void AddTexts(ISheet sheet, IEnumerable<TextItem> textItems, Block parent, Block selected, bool select, double thickness)
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
                    SelectText(text);
                    selected.Texts.Add(text);
                }
            }
        }

        public static void AddImages(ISheet sheet, IEnumerable<ImageItem> imageItems, Block parent, Block selected, bool select, double thickness)
        {
            if (select)
            {
                selected.Images = new List<Image>();
            }

            foreach (var imageItem in imageItems)
            {
                var image = BlockSerializer.DeserializeImageItem(sheet, parent, imageItem);

                if (select)
                {
                    SelectImage(image);
                    selected.Images.Add(image);
                }
            }
        }

        public static void AddBlocks(ISheet sheet, IEnumerable<BlockItem> blockItems, Block parent, Block selected, bool select, double thickness)
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

        public static void AddBlockContents(ISheet sheet, BlockItem blockItem, Block logic, Block selected, bool select, double thickness)
        {
            if (blockItem != null)
            {
                AddTexts(sheet, blockItem.Texts, logic, selected, select, thickness);
                AddImages(sheet, blockItem.Images, logic, selected, select, thickness);
                AddLines(sheet, blockItem.Lines, logic, selected, select, thickness);
                AddRectangles(sheet, blockItem.Rectangles, logic, selected, select, thickness);
                AddEllipses(sheet, blockItem.Ellipses, logic, selected, select, thickness);
                AddBlocks(sheet, blockItem.Blocks, logic, selected, select, thickness);
            }
        }

        public static void AddBrokenBlock(ISheet sheet, BlockItem blockItem, Block logic, Block selected, bool select, double thickness)
        {
            AddTexts(sheet, blockItem.Texts, logic, selected, select, thickness);
            AddImages(sheet, blockItem.Images, logic, selected, select, thickness);
            AddLines(sheet, blockItem.Lines, logic, selected, select, thickness);
            AddRectangles(sheet, blockItem.Rectangles, logic, selected, select, thickness);
            AddEllipses(sheet, blockItem.Ellipses, logic, selected, select, thickness);

            foreach (var block in blockItem.Blocks)
            {
                AddTexts(sheet, block.Texts, logic, selected, select, thickness);
                AddImages(sheet, block.Images, logic, selected, select, thickness);
                AddLines(sheet, block.Lines, logic, selected, select, thickness);
                AddRectangles(sheet, block.Rectangles, logic, selected, select, thickness);
                AddEllipses(sheet, block.Ellipses, logic, selected, select, thickness);
                AddBlocks(sheet, block.Blocks, logic, selected, select, thickness);
            }
        }

        #endregion

        #region Remove

        public static void RemoveLines(ISheet sheet, IEnumerable<Line> lines)
        {
            if (lines != null)
            {
                foreach (var line in lines)
                {
                    sheet.Remove(line);
                }
            }
        }

        public static void RemoveRectangles(ISheet sheet, IEnumerable<Rectangle> rectangles)
        {
            if (rectangles != null)
            {
                foreach (var rectangle in rectangles)
                {
                    sheet.Remove(rectangle);
                }
            }
        }

        public static void RemoveEllipses(ISheet sheet, IEnumerable<Ellipse> ellipses)
        {
            if (ellipses != null)
            {
                foreach (var ellipse in ellipses)
                {
                    sheet.Remove(ellipse);
                }
            }
        }

        public static void RemoveTexts(ISheet sheet, IEnumerable<Grid> texts)
        {
            if (texts != null)
            {
                foreach (var text in texts)
                {
                    sheet.Remove(text);
                }
            }
        }

        public static void RemoveImages(ISheet sheet, IEnumerable<Image> images)
        {
            if (images != null)
            {
                foreach (var image in images)
                {
                    sheet.Remove(image);
                }
            }
        }

        public static void RemoveBlocks(ISheet sheet, IEnumerable<Block> blocks)
        {
            if (blocks != null)
            {
                foreach (var block in blocks)
                {
                    RemoveBlock(sheet, block);
                }
            }
        }

        public static void RemoveBlock(ISheet sheet, Block block)
        {
            RemoveLines(sheet, block.Lines);
            RemoveRectangles(sheet, block.Rectangles);
            RemoveEllipses(sheet, block.Ellipses);
            RemoveTexts(sheet, block.Texts);
            RemoveImages(sheet, block.Images);
            RemoveBlocks(sheet, block.Blocks);
        }

        public static void RemoveSelectedFromBlock(ISheet sheet, Block parent, Block selected)
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

            if (selected.Images != null)
            {
                RemoveImages(sheet, selected.Images);

                foreach (var image in selected.Images)
                {
                    parent.Images.Remove(image);
                }

                selected.Images = null;
            }

            if (selected.Blocks != null)
            {
                foreach (var block in selected.Blocks)
                {
                    RemoveLines(sheet, block.Lines);
                    RemoveRectangles(sheet, block.Rectangles);
                    RemoveEllipses(sheet, block.Ellipses);
                    RemoveTexts(sheet, block.Texts);
                    RemoveImages(sheet, block.Images);
                    RemoveBlocks(sheet, block.Blocks);

                    parent.Blocks.Remove(block);
                }

                selected.Blocks = null;
            }
        }

        #endregion

        #region Move

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

        public static void MoveImages(double x, double y, IEnumerable<Image> images)
        {
            foreach (var image in images)
            {
                Canvas.SetLeft(image, Canvas.GetLeft(image) + x);
                Canvas.SetTop(image, Canvas.GetTop(image) + y);
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
                MoveImages(x, y, block.Images);
                MoveBlocks(x, y, block.Blocks);
            }
        }

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

            if (block.Images != null)
            {
                MoveImages(x, y, block.Images);
            }

            if (block.Blocks != null)
            {
                MoveBlocks(x, y, block.Blocks);
            }
        }

        #endregion

        #region Select

        private static int DeselectedZIndex = 0;
        private static int SelectedZIndex = 1;

        public static void DeselectLine(Line line)
        {
            line.Stroke = BlockFactory.NormalBrush;
            Panel.SetZIndex(line, DeselectedZIndex);
        }

        public static void DeselectRectangle(Rectangle rectangle)
        {
            rectangle.Stroke = BlockFactory.NormalBrush;
            rectangle.Fill = rectangle.Fill == BlockFactory.TransparentBrush ? BlockFactory.TransparentBrush : BlockFactory.NormalBrush;
            Panel.SetZIndex(rectangle, DeselectedZIndex);
        }

        public static void DeselectEllipse(Ellipse ellipse)
        {
            ellipse.Stroke = BlockFactory.NormalBrush;
            ellipse.Fill = ellipse.Fill == BlockFactory.TransparentBrush ? BlockFactory.TransparentBrush : BlockFactory.NormalBrush;
            Panel.SetZIndex(ellipse, DeselectedZIndex);
        }

        public static void DeselectText(Grid text)
        {
            BlockFactory.GetTextBlock(text).Foreground = BlockFactory.NormalBrush;
            Panel.SetZIndex(text, DeselectedZIndex);
        }

        public static void DeselectImage(Image image)
        {
            image.OpacityMask = BlockFactory.NormalBrush;
            Panel.SetZIndex(image, DeselectedZIndex);
        }

        public static void DeselectBlock(Block parent)
        {
            if (parent.Lines != null)
            {
                foreach (var line in parent.Lines)
                {
                    DeselectLine(line);
                }
            }

            if (parent.Rectangles != null)
            {
                foreach (var rectangle in parent.Rectangles)
                {
                    DeselectRectangle(rectangle);
                }
            }

            if (parent.Ellipses != null)
            {
                foreach (var ellipse in parent.Ellipses)
                {
                    DeselectEllipse(ellipse);
                }
            }

            if (parent.Texts != null)
            {
                foreach (var text in parent.Texts)
                {
                    DeselectText(text);
                }
            }

            if (parent.Images != null)
            {
                foreach (var image in parent.Images)
                {
                    DeselectImage(image);
                }
            }

            if (parent.Blocks != null)
            {
                foreach (var block in parent.Blocks)
                {
                    DeselectBlock(block);
                }
            }
        }

        public static void SelectLine(Line line)
        {
            line.Stroke = BlockFactory.SelectedBrush;
            Panel.SetZIndex(line, SelectedZIndex);
        }

        public static void SelectRectangle(Rectangle rectangle)
        {
            rectangle.Stroke = BlockFactory.SelectedBrush;
            rectangle.Fill = rectangle.Fill == BlockFactory.TransparentBrush ? BlockFactory.TransparentBrush : BlockFactory.SelectedBrush;
            Panel.SetZIndex(rectangle, SelectedZIndex);
        }

        public static void SelectEllipse(Ellipse ellipse)
        {
            ellipse.Stroke = BlockFactory.SelectedBrush;
            ellipse.Fill = ellipse.Fill == BlockFactory.TransparentBrush ? BlockFactory.TransparentBrush : BlockFactory.SelectedBrush;
            Panel.SetZIndex(ellipse, SelectedZIndex);
        }

        public static void SelectText(Grid text)
        {
            BlockFactory.GetTextBlock(text).Foreground = BlockFactory.SelectedBrush;
            Panel.SetZIndex(text, SelectedZIndex);
        }

        public static void SelectImage(Image image)
        {
            image.OpacityMask = BlockFactory.SelectedBrush;
            Panel.SetZIndex(image, SelectedZIndex);
        }

        public static void SelectBlock(Block parent)
        {
            foreach (var line in parent.Lines)
            {
                SelectLine(line);
            }

            foreach (var rectangle in parent.Rectangles)
            {
                SelectRectangle(rectangle);
            }

            foreach (var ellipse in parent.Ellipses)
            {
                SelectEllipse(ellipse);
            }

            foreach (var text in parent.Texts)
            {
                SelectText(text);
            }

            foreach (var image in parent.Images)
            {
                SelectImage(image);
            }

            foreach (var block in parent.Blocks)
            {
                SelectBlock(block);
            }
        }

        public static void SelectAll(Block selected, Block logic)
        {
            selected.Init();

            foreach (var line in logic.Lines)
            {
                SelectLine(line);
                selected.Lines.Add(line);
            }

            foreach (var rectangle in logic.Rectangles)
            {
                SelectRectangle(rectangle);
                selected.Rectangles.Add(rectangle);
            }

            foreach (var ellipse in logic.Ellipses)
            {
                SelectEllipse(ellipse);
                selected.Ellipses.Add(ellipse);
            }

            foreach (var text in logic.Texts)
            {
                SelectText(text);
                selected.Texts.Add(text);
            }

            foreach (var image in logic.Images)
            {
                SelectImage(image);
                selected.Images.Add(image);
            }

            foreach (var parent in logic.Blocks)
            {
                foreach (var line in parent.Lines)
                {
                    SelectLine(line);
                }

                foreach (var rectangle in parent.Rectangles)
                {
                    SelectRectangle(rectangle);
                }

                foreach (var ellipse in parent.Ellipses)
                {
                    SelectEllipse(ellipse);
                }

                foreach (var text in parent.Texts)
                {
                    SelectText(text);
                }

                foreach (var image in parent.Images)
                {
                    SelectImage(image);
                }

                foreach (var block in parent.Blocks)
                {
                    SelectBlock(block);
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
                    DeselectLine(line);
                }

                selected.Lines = null;
            }

            if (selected.Rectangles != null)
            {
                foreach (var rectangle in selected.Rectangles)
                {
                    DeselectRectangle(rectangle);
                }

                selected.Rectangles = null;
            }

            if (selected.Ellipses != null)
            {
                foreach (var ellipse in selected.Ellipses)
                {
                    DeselectEllipse(ellipse);
                }

                selected.Ellipses = null;
            }

            if (selected.Texts != null)
            {
                foreach (var text in selected.Texts)
                {
                    DeselectText(text);
                }

                selected.Texts = null;
            }

            if (selected.Images != null)
            {
                foreach (var image in selected.Images)
                {
                    DeselectImage(image);
                }

                selected.Images = null;
            }

            if (selected.Blocks != null)
            {
                foreach (var parent in selected.Blocks)
                {
                    foreach (var line in parent.Lines)
                    {
                        DeselectLine(line);
                    }

                    foreach (var rectangle in parent.Rectangles)
                    {
                        DeselectRectangle(rectangle);
                    }

                    foreach (var ellipse in parent.Ellipses)
                    {
                        DeselectEllipse(ellipse);
                    }

                    foreach (var text in parent.Texts)
                    {
                        DeselectText(text);
                    }

                    foreach (var image in parent.Images)
                    {
                        DeselectImage(image);
                    }

                    foreach (var block in parent.Blocks)
                    {
                        DeselectBlock(block);
                    }
                }

                selected.Blocks = null;
            }
        }

        public static bool HaveSelected(Block selected)
        {
            return (selected.Lines != null
                || selected.Rectangles != null
                || selected.Ellipses != null
                || selected.Texts != null
                || selected.Images != null
                || selected.Blocks != null);
        }

        public static bool HaveOneLineSelected(Block selected)
        {
            return (selected.Lines != null
                && selected.Lines.Count == 1
                && selected.Rectangles == null
                && selected.Ellipses == null
                && selected.Texts == null
                && selected.Images == null
                && selected.Blocks == null);
        }

        public static bool HaveOneRectangleSelected(Block selected)
        {
            return (selected.Lines == null
                && selected.Rectangles != null
                && selected.Rectangles.Count == 1
                && selected.Ellipses == null
                && selected.Texts == null
                && selected.Images == null
                && selected.Blocks == null);
        }

        public static bool HaveOneEllipseSelected(Block selected)
        {
            return (selected.Lines == null
                && selected.Rectangles == null
                && selected.Ellipses != null
                && selected.Ellipses.Count == 1
                && selected.Texts == null
                && selected.Images == null
                && selected.Blocks == null);
        }

        public static bool HaveOneTextSelected(Block selected)
        {
            return (selected.Lines == null
                && selected.Rectangles == null
                && selected.Ellipses == null
                && selected.Texts != null
                && selected.Texts.Count == 1
                && selected.Images == null
                && selected.Blocks == null);
        }

        public static bool HaveOneImageSelected(Block selected)
        {
            return (selected.Lines == null
                && selected.Rectangles == null
                && selected.Ellipses == null
                && selected.Texts == null
                && selected.Images != null
                && selected.Images.Count == 1
                && selected.Blocks == null);
        }

        public static bool HaveOneBlockSelected(Block selected)
        {
            return (selected.Lines == null
                && selected.Rectangles == null
                && selected.Ellipses == null
                && selected.Texts == null
                && selected.Images == null
                && selected.Blocks != null
                && selected.Blocks.Count == 1);
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
                            SelectLine(line);
                            selected.Lines.Add(line);
                        }
                        else
                        {
                            DeselectLine(line);
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
                            SelectRectangle(rectangle);
                            selected.Rectangles.Add(rectangle);
                        }
                        else
                        {
                            DeselectRectangle(rectangle);
                            selected.Rectangles.Remove(rectangle);
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
                            SelectEllipse(ellipse);
                            selected.Ellipses.Add(ellipse);
                        }
                        else
                        {
                            DeselectEllipse(ellipse);
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
                            SelectText(text);
                            selected.Texts.Add(text);
                        }
                        else
                        {
                            DeselectText(text);
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

        public static bool HitTestImages(IEnumerable<Image> images, Block selected, Rect rect, bool onlyFirst, bool select, UIElement relative)
        {
            foreach (var image in images)
            {
                var bounds = VisualTreeHelper.GetContentBounds(image);
                var offset = image.TranslatePoint(new Point(0, 0), relative);
                bounds.Offset(offset.X, offset.Y);
                if (rect.IntersectsWith(bounds))
                {
                    if (select)
                    {
                        if (image.OpacityMask != BlockFactory.SelectedBrush)
                        {
                            SelectImage(image);
                            selected.Images.Add(image);
                        }
                        else
                        {
                            DeselectImage(image);
                            selected.Images.Remove(image);
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
                            SelectBlock(block);
                        }
                        else
                        {
                            selected.Blocks.Remove(block);
                            DeselectBlock(block);
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

            result = HitTestImages(parent.Images, selected, rect, onlyFirst, selectInsideBlock, relative);
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

            if (parent.Images != null)
            {
                bool result = HitTestImages(parent.Images, selected, rect, true, true, sheet.GetParent());
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

            if (parent.Images != null)
            {
                HitTestImages(parent.Images, selected, rect, false, true, sheet.GetParent());
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

            if (selected.Images != null && selected.Images.Count == 0)
            {
                selected.Images = null;
            }

            if (selected.Blocks != null && selected.Blocks.Count == 0)
            {
                selected.Blocks = null;
            }
        }

        #endregion

        #region Fill

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
            double fontSize,
            Brush backgroud, Brush foreground)
        {
            var grid = new Grid();
            grid.Background = backgroud;
            grid.Width = width;
            grid.Height = height;
            Canvas.SetLeft(grid, x);
            Canvas.SetTop(grid, y);

            var tb = new TextBlock();
            tb.HorizontalAlignment = halign;
            tb.VerticalAlignment = valign;
            tb.Background = backgroud;
            tb.Foreground = foreground;
            tb.FontSize = fontSize;
            tb.FontFamily = new FontFamily("Calibri");
            tb.Text = text;

            grid.Children.Add(tb);

            return grid;
        }

        public static Image CreateImage(double x, double y, double width, double height, byte[] data)
        {
            Image image = new Image();

            // enable high quality image scaling
            RenderOptions.SetBitmapScalingMode(image, BitmapScalingMode.HighQuality);

            // store original image data is Tag property
            image.Tag = data;

            // opacity mask is used for determining selection state
            image.OpacityMask = NormalBrush;

            //using(var ms = new MemoryStream(data))
            //{
            //    image = Image.FromStream(ms);
            //}
            using (var ms = new MemoryStream(data))
            {
                IBitmap profileImage = BitmapLoader.Current.Load(ms, null, null).Result;
                image.Source = profileImage.ToNative();
            }
            
            image.Width = width;
            image.Height = height;

            Canvas.SetLeft(image, x);
            Canvas.SetTop(image, y);

            return image;
        }

        public static Line CreateLine(double thickness, double x1, double y1, double x2, double y2, Brush stroke)
        {
            var line = new Line()
            {
                Stroke = stroke,
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

    #region Base64

    public static class Base64
    {
        public static string ToBase64(byte[] bytes)
        {
            if (bytes != null)
            {
                return Convert.ToBase64String(bytes);
            }
            return null;
        }

        public static MemoryStream ToStream(byte[] bytes)
        {
            if (bytes != null)
            {
                return new MemoryStream(bytes, 0, bytes.Length); 
            }
            return null;
        }

        public static byte[] ToBytes(string base64)
        {
            if (!string.IsNullOrEmpty(base64))
            {
                return Convert.FromBase64String(base64);
            }
            return null;
        }

        public static MemoryStream ToStream(string base64)
        {
            if (!string.IsNullOrEmpty(base64))
            {
                byte[] bytes = ToBytes(base64);
                if (bytes != null)
                {
                    return new MemoryStream(bytes, 0, bytes.Length);
                }
                return null;
            }
            return null;
        }

        public static byte[] ReadAllBytes(string path)
        {
            if (!string.IsNullOrEmpty(path))
            {
                return System.IO.File.ReadAllBytes(path);
            }
            return null;
        }

        public static string FromFileToBase64(string path)
        {
            if (!string.IsNullOrEmpty(path))
            {
                byte[] bytes = ReadAllBytes(path);
                if (bytes != null)
                {
                    return ToBase64(bytes); 
                }
                return null;
            }
            return null;
        }

        public static MemoryStream FromFileToStream(string path)
        {
            if (!string.IsNullOrEmpty(path))
            {
                byte[] bytes = ReadAllBytes(path);
                if (bytes != null)
                {
                    return new MemoryStream(bytes, 0, bytes.Length); 
                }
                return null;
            }
            return null;
        }
    }

    #endregion
}
