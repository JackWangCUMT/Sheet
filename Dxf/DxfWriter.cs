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

namespace Dxf
{
    public class DxfWriter
    {
        private DxfAcadVer Version = DxfAcadVer.AC1015;
        private string Layer = "0";
        private int Handle = 0;
        private string DefaultStyle = "Standard";

        private string StylePrimatyFont = "arial.ttf"; // arialuni.ttf
        private string StylePrimatyFontDescription = "Arial"; // Arial Unicode MS
        private string StyleBigFont = "";
        private double PageWidth = 1260.0;
        private double PageHeight = 891.0;

        public DxfWriter() { }

        private int NextHandle() { return Handle += 1; }

        private double X(double x) { return x; }
        private double Y(double y) { return PageHeight - y; }

        private DxfBlockRecord CreateBlockRecordForBlock(string name)
        {
            var blockRecord = new DxfBlockRecord(Version, NextHandle())
            {
                Name = name
            };

            return blockRecord.Create();
        }

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

            // DXF - DxfFile
            var cade = new DxfAppid(Version, NextHandle())
                .Application("DXF")
                .StandardFlags(DxfAppidStandardFlags.Default);

            appids.Add(cade);

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

        public string Create(DxfAcadVer version)
        {
            Version = version;
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

            Entities.Add(CreateLine(0.0, 0.0, 0.0, 100.0));
            Entities.Add(CreateLine(0.0, 0.0, 100.0, 0.0));
            Entities.Add(CreateCircle(50.0, 50.0, 50.0));
            Entities.Add(CreateEllipse(0.0, 0.0, 100.0, 100.0));
            Entities.Add(CreateText("≥1", 50.0, 50.0, 14.0, DxfHorizontalTextJustification.Center, DxfVerticalTextJustification.Middle, DefaultStyle));


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

            // return dxf file contents
            return file.ToString();
        }
    }
}
