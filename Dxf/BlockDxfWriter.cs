using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Sheet;
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

        private DxfAcadVer Version = DxfAcadVer.AC1015;
        private string Layer = "0";
        private int Handle = 0;
        private string DefaultStyle = "Standard";

        private double PageWidth = 1260.0;
        private double PageHeight = 891.0;

        private string StylePrimatyFont = "calibri.ttf"; // "arial.ttf"; "arialuni.ttf";
        private string StylePrimatyFontDescription = "Calibri"; // "Arial"; "Arial Unicode MS"
        private string StyleBigFont = "";

        #endregion

        #region Constructor

        public BlockDxfWriter() { }

        #endregion

        #region Helpers

        private int NextHandle() { return Handle += 1; }

        private double X(double x) { return x; }
        private double Y(double y) { return PageHeight - y; }

        private string EncodeText(string text)
        {
            if (Version >= DxfAcadVer.AC1021)
                return text;
            if (string.IsNullOrEmpty(text))
                return text;
            var sb = new StringBuilder();
            foreach (char c in text)
            {
                if (c > 255)
                    sb.Append(string.Concat("\\U+", Convert.ToInt32(c).ToString("X4")));
                else
                    sb.Append(c);
            }
            return sb.ToString();
        }

        #endregion

        #region Tables

        private IEnumerable<DxfAppid> TableAppids()
        {
            var appids = new List<DxfAppid>();

            // ACAD - default must be present
            if (Version > DxfAcadVer.AC1009)
            {
                var acad = new DxfAppid(Version, NextHandle())
                    .Application("ACAD")
                    .StandardFlags(DxfAppidStandardFlags.Default);

                appids.Add(acad);
            }

            return appids;
        }

        private IEnumerable<DxfDimstyle> TableDimstyles()
        {
            var dimstyles = new List<DxfDimstyle>();

            if (Version > DxfAcadVer.AC1009)
            {
                dimstyles.Add(new DxfDimstyle(Version, NextHandle())
                {
                    Name = "Standard"
                }.Create());
            }

            return dimstyles;
        }

        private IEnumerable<DxfLayer> TableLayers()
        {
            var layers = new List<DxfLayer>();

            // default layer 0 - must be present
            if (Version > DxfAcadVer.AC1009)
            {
                layers.Add(new DxfLayer(Version, NextHandle())
                {
                    Name = "0",
                    LayerStandardFlags = DxfLayerStandardFlags.Default,
                    Color = DxfDefaultColors.Default.ColorToString(),
                    LineType = "Continuous",
                    PlottingFlag = true,
                    LineWeight = DxfLineWeight.LnWtByLwDefault,
                    PlotStyleNameHandle = "0"
                }.Create());
            }

            return layers;
        }

        private IEnumerable<DxfLtype> TableLtypes()
        {
            var ltypes = new List<DxfLtype>();

            // default ltypes ByLayer, ByBlock and Continuous - must be present

            // ByLayer
            ltypes.Add(new DxfLtype(Version, NextHandle())
            {
                Name = "ByLayer",
                LtypeStandardFlags = DxfLtypeStandardFlags.Default,
                Description = "ByLayer",
                DashLengthItems = 0,
                TotalPatternLenght = 0,
                DashLenghts = null,
            }.Create());

            // ByBlock
            ltypes.Add(new DxfLtype(Version, NextHandle())
            {
                Name = "ByBlock",
                LtypeStandardFlags = DxfLtypeStandardFlags.Default,
                Description = "ByBlock",
                DashLengthItems = 0,
                TotalPatternLenght = 0,
                DashLenghts = null,
            }.Create());

            // Continuous
            ltypes.Add(new DxfLtype(Version, NextHandle())
            {
                Name = "Continuous",
                LtypeStandardFlags = DxfLtypeStandardFlags.Default,
                Description = "Solid line",
                DashLengthItems = 0,
                TotalPatternLenght = 0,
                DashLenghts = null,
            }.Create());

            return ltypes;
        }

        private IEnumerable<DxfStyle> TableStyles()
        {
            var styles = new List<DxfStyle>();

            // style: Standard
            var standard = new DxfStyle(Version, NextHandle())
                .Name("Standard")
                .StandardFlags(DxfStyleFlags.Default)
                .FixedTextHeight(0)
                .WidthFactor(1)
                .ObliqueAngle(0)
                .TextGenerationFlags(DxfTextGenerationFlags.Default)
                .LastHeightUsed(1)
                .PrimaryFontFile(StylePrimatyFont)
                .BifFontFile(StyleBigFont);

            if (Version > DxfAcadVer.AC1009)
            {
                // extended STYLE data
                standard.Add(1001, "ACAD");
                standard.Add(1000, StylePrimatyFontDescription);
                standard.Add(1071, 0);
            }

            styles.Add(standard);

            return styles;
        }

        private IEnumerable<DxfUcs> TableUcss()
        {
            return Enumerable.Empty<DxfUcs>();
        }

        private IEnumerable<DxfView> TableViews()
        {
            var view = new DxfView(Version, NextHandle())
                .Name("View")
                .StandardFlags(DxfViewStandardFlags.Default)
                .Height(PageHeight)
                .Width(PageWidth)
                .Center(new Vector2(PageWidth / 2, PageHeight / 2))
                .ViewDirection(new Vector3(0, 0, 1))
                .TargetPoint(new Vector3(0, 0, 0))
                .FrontClippingPlane(0)
                .BackClippingPlane(0)
                .Twist(0);

            yield return view;
        }

        private IEnumerable<DxfVport> TableVports()
        {
            var vports = new List<DxfVport>();

            if (Version > DxfAcadVer.AC1009)
            {
                vports.Add(new DxfVport(Version, NextHandle())
                {
                    Name = "*Active"
                }.Create());
            }

            return vports;
        }

        #endregion

        #region Blocks

        public IEnumerable<DxfBlock> DefaultBlocks()
        {
            if (Version > DxfAcadVer.AC1009)
            {
                var blocks = new List<DxfBlock>();
                string layer = "0";

                blocks.Add(new DxfBlock(Version, NextHandle())
                {
                    Name = "*Model_Space",
                    Layer = layer,
                    BlockTypeFlags = DxfBlockTypeFlags.Default,
                    BasePoint = new Vector3(0, 0, 0),
                    XrefPathName = null,
                    Description = null,
                    EndId = NextHandle(),
                    EndLayer = layer,
                    Entities = null
                }.Create());

                blocks.Add(new DxfBlock(Version, NextHandle())
                {
                    Name = "*Paper_Space",
                    Layer = layer,
                    BlockTypeFlags = DxfBlockTypeFlags.Default,
                    BasePoint = new Vector3(0, 0, 0),
                    XrefPathName = null,
                    Description = null,
                    EndId = NextHandle(),
                    EndLayer = layer,
                    Entities = null
                }.Create());

                blocks.Add(new DxfBlock(Version, NextHandle())
                {
                    Name = "*Paper_Space0",
                    Layer = layer,
                    BlockTypeFlags = DxfBlockTypeFlags.Default,
                    BasePoint = new Vector3(0, 0, 0),
                    XrefPathName = null,
                    Description = null,
                    EndId = NextHandle(),
                    EndLayer = layer,
                    Entities = null
                }.Create());

                return blocks;
            }

            return Enumerable.Empty<DxfBlock>();
        }

        private DxfBlockRecord CreateBlockRecordForBlock(string name)
        {
            var blockRecord = new DxfBlockRecord(Version, NextHandle())
            {
                Name = name
            };

            return blockRecord.Create();
        }

        #endregion

        #region Factory

        private DxfLine CreateLine(double x1, double y1, double x2, double y2)
        {
            double _x1 = X(x1);
            double _y1 = Y(y1);
            double _x2 = X(x2);
            double _y2 = Y(y2);

            var line = new DxfLine(Version, NextHandle())
            {
                Layer = Layer,
                Color = DxfDefaultColors.ByLayer.ColorToString(),
                Thickness = 0.0,
                StartPoint = new Vector3(_x1, _y1, 0),
                EndPoint = new Vector3(_x2, _y2, 0),
                ExtrusionDirection = new Vector3(0, 0, 1)
            };

            return line.Create();
        }

        private DxfCircle CreateCircle(double x, double y, double radius)
        {
            double _x = X(x);
            double _y = Y(y);

            var circle = new DxfCircle(Version, NextHandle())
            {
                Layer = Layer,
                Color = DxfDefaultColors.ByLayer.ColorToString(),
                Thickness = 0.0,
                CenterPoint = new Vector3(_x, _y, 0),
                Radius = radius,
                ExtrusionDirection = new Vector3(0, 0, 1),
            };

            return circle.Create();
        }

        private DxfEllipse CreateEllipse(double x, double y, double width, double height)
        {
            double _cx = X(x + width / 2.0);
            double _cy = Y(y + height / 2.0);
            double _ex = width / 2.0; // relative to _cx
            double _ey = 0.0; // relative to _cy

            var ellipse = new DxfEllipse(Version, NextHandle())
            {
                Layer = Layer,
                Color = DxfDefaultColors.ByLayer.ColorToString(),
                CenterPoint = new Vector3(_cx, _cy, 0),
                EndPoint = new Vector3(_ex, _ey, 0),
                ExtrusionDirection = new Vector3(0, 0, 1),
                Ratio = height / width,
                StartParameter = 0.0,
                EndParameter = 2.0 * Math.PI
            };

            return ellipse.Create();
        }

        private DxfText CreateText(string text, double x, double y, double height, DxfHorizontalTextJustification horizontalJustification, DxfVerticalTextJustification verticalJustification, string style)
        {
            var txt = new DxfText(Version, NextHandle())
            {
                Thickness = 0,
                Layer = Layer,
                Color = DxfDefaultColors.ByLayer.ColorToString(),
                FirstAlignment = new Vector3(X(x), Y(y), 0),
                TextHeight = height,
                DefaultValue = EncodeText(text),
                TextRotation = 0,
                ScaleFactorX = 1,
                ObliqueAngle = 0,
                TextStyle = style,
                TextGenerationFlags = DxfTextGenerationFlags.Default,
                HorizontalTextJustification = horizontalJustification,
                SecondAlignment = new Vector3(X(x), Y(y), 0),
                ExtrusionDirection = new Vector3(0, 0, 1),
                VerticalTextJustification = verticalJustification
            };

            return txt.Create();
        }

        #endregion

        #region Create

        public void Create(string fileName, double sourceWidth, double sourceHeight, BlockItem blockItem)
        {
            


            PageWidth = sourceWidth;
            PageHeight = sourceHeight;



            Version = DxfAcadVer.AC1015;

            Layer = "0";
            Handle = 0;

            // dxf file sections
            DxfHeader header = null;
            DxfClasses classes = null;
            DxfTables tables = null;
            DxfBlocks blocks = null;
            DxfObjects objects = null;

            // create dxf file
            var file = new DxfFile(Version, NextHandle());

            // create header
            header = new DxfHeader(Version, NextHandle()).Begin().Default();

            // create classes
            if (Version > DxfAcadVer.AC1009)
            {
                classes = new DxfClasses(Version, NextHandle()).Begin();

                // classes.Add(new DxfClass(...));

                classes.End();
            }

            // create tables
            tables = new DxfTables(Version, NextHandle());
            tables.Begin();
            tables.AddAppidTable(TableAppids(), NextHandle());
            tables.AddDimstyleTable(TableDimstyles(), NextHandle());

            if (Version > DxfAcadVer.AC1009)
            {
                var records = new List<DxfBlockRecord>();

                // TODO: each BLOCK must have BLOCK_RECORD entry

                // required block records by dxf format
                records.Add(CreateBlockRecordForBlock("*Model_Space"));
                records.Add(CreateBlockRecordForBlock("*Paper_Space"));
                records.Add(CreateBlockRecordForBlock("*Paper_Space0"));


                // TODO: add user layers
                //records.Add(CreateBlockRecordForBlock("NEW_LAYER_NAME"));


                tables.AddBlockRecordTable(records, NextHandle());
            }

            tables.AddLtypeTable(TableLtypes(), NextHandle());
            tables.AddLayerTable(TableLayers(), NextHandle());
            tables.AddStyleTable(TableStyles(), NextHandle());
            tables.AddUcsTable(TableUcss(), NextHandle());
            tables.AddViewTable(TableViews(), NextHandle());
            tables.AddVportTable(TableVports(), NextHandle());

            tables.End();

            // create blocks
            blocks = new DxfBlocks(Version, NextHandle()).Begin();
            blocks.Add(DefaultBlocks());


            // TODO: add user blocks


            blocks.End();

            // create entities
            var Entities = new DxfEntities(Version, NextHandle()).Begin();



            // TODO: add user entities


            // Demo
            //Entities.Add(CreateLine(0.0, 0.0, 0.0, 100.0));
            //Entities.Add(CreateLine(0.0, 0.0, 100.0, 0.0));
            //Entities.Add(CreateCircle(50.0, 50.0, 50.0));
            //Entities.Add(CreateEllipse(0.0, 0.0, 100.0, 100.0));
            //Entities.Add(CreateText("≥1", 50.0, 50.0, 14.0, DxfHorizontalTextJustification.Center, DxfVerticalTextJustification.Middle, DefaultStyle));



            DrawBlock(Entities, blockItem);




            Entities.End();

            // create objects
            if (Version > DxfAcadVer.AC1009)
            {
                objects = new DxfObjects(Version, NextHandle()).Begin();

                // mamed dictionary
                var namedDict = new DxfDictionary(Version, NextHandle())
                {
                    OwnerDictionaryHandle = 0.ToDxfHandle(),
                    HardOwnerFlag = false,
                    DuplicateRecordCloningFlags = DxfDuplicateRecordCloningFlags.KeepExisting,
                    Entries = new Dictionary<string, string>()
                };

                // base dictionary
                var baseDict = new DxfDictionary(Version, NextHandle())
                {
                    OwnerDictionaryHandle = namedDict.Id.ToDxfHandle(),
                    HardOwnerFlag = false,
                    DuplicateRecordCloningFlags = DxfDuplicateRecordCloningFlags.KeepExisting,
                    Entries = new Dictionary<string, string>()
                };

                // add baseDict to namedDict
                namedDict.Entries.Add(baseDict.Id.ToDxfHandle(), "ACAD_GROUP");

                // finalize dictionaries
                objects.Add(namedDict.Create());
                objects.Add(baseDict.Create());

                // TODO: add Group dictionary
                // TODO: add MLine style dictionary
                // TODO: add image dictionary dictionary

                // finalize objects
                objects.End();
            }

            // finalize dxf file
            file.Header(header.End(NextHandle()));

            if (Version > DxfAcadVer.AC1009)
                file.Classes(classes);

            file.Tables(tables);
            file.Blocks(blocks);
            file.Entities(Entities);

            if (Version > DxfAcadVer.AC1009)
                file.Objects(objects);

            file.Eof();

            // create dxf file contents
            string dxf = file.ToString();

            DxfInspect.Save(fileName, dxf);
        }

        #endregion

        #region Draw

        private void DrawLine(DxfEntities entities, LineItem line)
        {
            //var pen = new XPen(XColor.FromArgb(line.Stroke.Alpha, line.Stroke.Red, line.Stroke.Green, line.Stroke.Blue),
            //    X(line.StrokeThickness == 0.0 ? XUnit.FromMillimeter(defaultStrokeThickness).Value : line.StrokeThickness)) { LineCap = XLineCap.Round };
            //gfx.DrawLine(pen, X(line.X1), Y(line.Y1), X(line.X2), Y(line.Y2));

            entities.Add(CreateLine(line.X1, line.Y1, line.X2, line.Y2));
        }

        private void DrawRectangle(DxfEntities entities, RectangleItem rectangle)
        {
            //if (rectangle.IsFilled)
            //{
            //    var pen = new XPen(XColor.FromArgb(rectangle.Stroke.Alpha, rectangle.Stroke.Red, rectangle.Stroke.Green, rectangle.Stroke.Blue),
            //        X(rectangle.StrokeThickness == 0.0 ? XUnit.FromMillimeter(defaultStrokeThickness).Value : rectangle.StrokeThickness)) { LineCap = XLineCap.Round };
            //    var brush = new XSolidBrush(XColor.FromArgb(rectangle.Fill.Alpha, rectangle.Fill.Red, rectangle.Fill.Green, rectangle.Fill.Blue));
            //    gfx.DrawRectangle(pen, brush, X(rectangle.X), Y(rectangle.Y), X(rectangle.Width), Y(rectangle.Height));
            //}
            //else
            //{
            //    var pen = new XPen(XColor.FromArgb(rectangle.Stroke.Alpha, rectangle.Stroke.Red, rectangle.Stroke.Green, rectangle.Stroke.Blue),
            //        X(rectangle.StrokeThickness == 0.0 ? XUnit.FromMillimeter(defaultStrokeThickness).Value : rectangle.StrokeThickness)) { LineCap = XLineCap.Round };
            //    gfx.DrawRectangle(pen, X(rectangle.X), Y(rectangle.Y), X(rectangle.Width), Y(rectangle.Height));
            //}

            entities.Add(CreateLine(rectangle.X, rectangle.Y, rectangle.X + rectangle.Width, rectangle.Y));
            entities.Add(CreateLine(rectangle.X, rectangle.Y + rectangle.Height, rectangle.X + rectangle.Width, rectangle.Y + rectangle.Height));
            entities.Add(CreateLine(rectangle.X, rectangle.Y, rectangle.X, rectangle.Y + rectangle.Height));
            entities.Add(CreateLine(rectangle.X + rectangle.Width, rectangle.Y, rectangle.X + rectangle.Width, rectangle.Y + rectangle.Height));
        }

        private void DrawEllipse(DxfEntities entities, EllipseItem ellipse)
        {
            //if (ellipse.IsFilled)
            //{
            //    var pen = new XPen(XColor.FromArgb(ellipse.Stroke.Alpha, ellipse.Stroke.Red, ellipse.Stroke.Green, ellipse.Stroke.Blue),
            //        X(ellipse.StrokeThickness == 0.0 ? XUnit.FromMillimeter(defaultStrokeThickness).Value : ellipse.StrokeThickness)) { LineCap = XLineCap.Round };
            //    var brush = new XSolidBrush(XColor.FromArgb(ellipse.Fill.Alpha, ellipse.Fill.Red, ellipse.Fill.Green, ellipse.Fill.Blue));
            //    gfx.DrawEllipse(pen, brush, X(ellipse.X), Y(ellipse.Y), X(ellipse.Width), Y(ellipse.Height));
            //}
            //else
            //{
            //    var pen = new XPen(XColor.FromArgb(ellipse.Stroke.Alpha, ellipse.Stroke.Red, ellipse.Stroke.Green, ellipse.Stroke.Blue),
            //        X(ellipse.StrokeThickness == 0.0 ? XUnit.FromMillimeter(defaultStrokeThickness).Value : ellipse.StrokeThickness)) { LineCap = XLineCap.Round };
            //    gfx.DrawEllipse(pen, X(ellipse.X), Y(ellipse.Y), X(ellipse.Width), Y(ellipse.Height));
            //}

            entities.Add(CreateEllipse(ellipse.X, ellipse.Y, ellipse.Width, ellipse.Height));
        }

        private void DrawText(DxfEntities entities, TextItem text)
        {
            //XPdfFontOptions options = new XPdfFontOptions(PdfFontEncoding.Unicode, PdfFontEmbedding.Always);
            //XFont font = new XFont("Calibri", Y(text.Size), XFontStyle.Regular, options);

            //XStringFormat format = new XStringFormat();
            //XRect rect = new XRect(X(text.X), Y(text.Y), X(text.Width), Y(text.Height));

            //switch (text.HAlign)
            //{
            //    case 0: format.Alignment = XStringAlignment.Near; break;
            //    case 1: format.Alignment = XStringAlignment.Center; break;
            //    case 2: format.Alignment = XStringAlignment.Far; break;
            //}

            //switch (text.VAlign)
            //{
            //    case 0: format.LineAlignment = XLineAlignment.Near; break;
            //    case 1: format.LineAlignment = XLineAlignment.Center; break;
            //    case 2: format.LineAlignment = XLineAlignment.Far; break;
            //}

            //var brushBackground = new XSolidBrush(XColor.FromArgb(text.Backgroud.Alpha, text.Backgroud.Red, text.Backgroud.Green, text.Backgroud.Blue));
            //gfx.DrawRectangle(brushBackground, rect);

            //var brushForeground = new XSolidBrush(XColor.FromArgb(text.Foreground.Alpha, text.Foreground.Red, text.Foreground.Green, text.Foreground.Blue));
            //gfx.DrawString(text.Text, font, brushForeground, rect, format);

            DxfHorizontalTextJustification halign;
            DxfVerticalTextJustification valign;
            double x, y;

            switch (text.HAlign)
            {
                default:
                case 0: 
                    halign = DxfHorizontalTextJustification.Left;
                    x = text.X;
                    break;
                case 1:
                    halign = DxfHorizontalTextJustification.Center;
                    x = text.X + text.Width / 2.0;
                    break;
                case 2:
                    halign = DxfHorizontalTextJustification.Right;
                    x = text.X + text.Width;
                    break;
            }

            switch (text.VAlign)
            {
                default:
                case 0:
                    valign = DxfVerticalTextJustification.Top;
                    y = text.Y;
                    break;
                case 1:
                    valign = DxfVerticalTextJustification.Middle;
                    y = text.Y + text.Height / 2.0;
                    break;
                case 2:
                    valign = DxfVerticalTextJustification.Bottom;
                    y = text.Y + text.Height;
                    break;
            }

            entities.Add(CreateText(text.Text, x, y, text.Size * (72.0 / 96.0), halign, valign, DefaultStyle));
        }

        private void DrawLines(DxfEntities entities, IEnumerable<LineItem> lines)
        {
            foreach (var line in lines)
            {
                DrawLine(entities, line);
            }
        }

        private void DrawRectangles(DxfEntities entities, IEnumerable<RectangleItem> rectangles)
        {
            foreach (var rectangle in rectangles)
            {
                DrawRectangle(entities, rectangle);
            }
        }

        private void DrawEllipses(DxfEntities entities, IEnumerable<EllipseItem> ellipses)
        {
            foreach (var ellipse in ellipses)
            {
                DrawEllipse(entities, ellipse);
            }
        }

        private void DrawTexts(DxfEntities entities, IEnumerable<TextItem> texts)
        {
            foreach (var text in texts)
            {
                DrawText(entities, text);
            }
        }

        private void DrawBlocks(DxfEntities entities, IEnumerable<BlockItem> blocks)
        {
            foreach (var block in blocks)
            {
                DrawBlock(entities, block);
            }
        }

        private void DrawBlock(DxfEntities entities, BlockItem block)
        {
            DrawLines(entities, block.Lines);
            DrawRectangles(entities, block.Rectangles);
            DrawEllipses(entities, block.Ellipses);
            DrawTexts(entities, block.Texts);
            DrawBlocks(entities, block.Blocks);
        }

        #endregion
    }
}
