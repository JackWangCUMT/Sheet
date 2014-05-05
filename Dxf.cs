using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Dxf.Enums
{
    #region DxfAcadVer

    // DXF support status: 
    // AC1006 - supported
    // AC1009 - supported
    // AC1012 - not supported
    // AC1014 - not supported
    // AC1015 - supported

    // AutoCAD drawing database version number: 
    // AC1006 = R10
    // AC1009 = R11 and R12, 
    // AC1012 = R13
    // AC1014 = R14
    // AC1015 = AutoCAD 2000

    public enum DxfAcadVer : int
    {
        AC1006 = 0, // R10
        AC1009 = 1, // R11 and R12
        AC1012 = 2, // R13
        AC1014 = 3, // R14
        AC1015 = 4, // AutoCAD 2000
        AC1018 = 5, // AutoCAD 2004
        AC1021 = 6, // AutoCAD 2007
        AC1024 = 7, // AutoCAD 2010
        AC1027 = 8, // AutoCAD 2013
    }

    #endregion

    #region DxfAppidStandardFlags

    // Group code: 70
    public enum DxfAppidStandardFlags : int
    {
        Default = 0,
        IgnoreXdata = 1,
        Xref = 16,
        XrefSuccess = 32,
        References = 64
    }

    #endregion

    #region DxfAttributeFlags

    // Group code: 70
    public enum DxfAttributeFlags : int
    {
        Default = 0,
        Invisible = 1,
        Constant = 2,
        Verification = 4,
        Preset = 8
    }

    #endregion

    #region DxfBlockTypeFlags

    // Group code: 70
    public enum DxfBlockTypeFlags : int
    {
        Default = 0,
        Anonymous = 1,
        NonConstantAttributes = 2,
        Xref = 4,
        XrefOverlay = 8,
        Dependant = 16,
        Reference = 32,
        ReferencesSuccess = 64
    }

    #endregion

    #region DxfDefaultColors

    public enum DxfDefaultColors : int
    {
        ByBlock = 0,
        Red = 1,
        Yellow = 2,
        Green = 3,
        Cyan = 4,
        Blue = 5,
        Magenta = 6,
        Default = 7,
        DarkGrey = 8,
        LightGrey = 9,
        ByLayer = 256
    }

    #endregion

    #region DxfDimstyleStandardFlags

    // Group code: 70
    public enum DxfDimstyleStandardFlags : int
    {
        Default = 0,
        Xref = 16,
        XrefSuccess = 32,
        References = 64
    }

    #endregion

    #region DxfDuplicateRecordCloningFlags

    public enum DxfDuplicateRecordCloningFlags : int
    {
        NotApplicable = 0,
        KeepExisting = 1,
        UseClone = 2,
        XrefPrefixName = 3,
        PrefixName = 4,
        UnmangleName = 5
    }

    #endregion

    #region DxfHorizontalTextJustification

    // Group code: 72
    public enum DxfHorizontalTextJustification : int
    {
        Default = 0,
        Left = 0,
        Center = 1,
        Right = 2,
        Aligned = 3,
        Middle = 4,
        Fit = 5
    }

    #endregion

    #region DxfLayerStandardFlags

    // Group code: 70
    public enum DxfLayerStandardFlags : int
    {
        Default = 0,
        Frozen = 1,
        FrozenByDefault = 2,
        Locked = 4,
        Xref = 16,
        XrefSuccess = 32,
        References = 64
    }

    #endregion

    #region DxfLineWeight

    public enum DxfLineWeight : short
    {
        LnWt000 = 0,
        LnWt005 = 5,
        LnWt009 = 9,
        LnWt013 = 13,
        LnWt015 = 15,
        LnWt018 = 18,
        LnWt020 = 20,
        LnWt025 = 25,
        LnWt030 = 30,
        LnWt035 = 35,
        LnWt040 = 40,
        LnWt050 = 50,
        LnWt053 = 53,
        LnWt060 = 60,
        LnWt070 = 70,
        LnWt080 = 80,
        LnWt090 = 90,
        LnWt100 = 100,
        LnWt106 = 106,
        LnWt120 = 120,
        LnWt140 = 140,
        LnWt158 = 158,
        LnWt200 = 200,
        LnWt211 = 211,
        LnWtByLayer = -1,
        LnWtByBlock = -2,
        LnWtByLwDefault = -3
    };

    #endregion

    #region DxfLtypeStandardFlags

    // Group code: 70
    public enum DxfLtypeStandardFlags : int
    {
        Default = 0,
        Xref = 16,
        XrefSuccess = 32,
        Referenced = 64
    }

    #endregion

    #region DxfLwpolylineFlags

    // Group code: 70
    public enum DxfLwpolylineFlags : int
    {
        Default = 0,
        Closed = 1,
        Plinegen = 128
    }

    #endregion

    #region DxfOrthographicType

    public enum DxfOrthographicType : int
    {
        Top = 1,
        Bottom = 2,
        Front = 3,
        Back = 4,
        Left = 5,
        Right = 6
    }

    #endregion

    #region DxfOrthographicViewType

    public enum DxfOrthographicViewType : int
    {
        NotOrthographic = 0,
        Top = 1,
        Bottom = 2,
        Front = 3,
        Back = 4,
        Left = 5,
        Right = 6
    }

    #endregion

    #region DxfProxyCapabilitiesFlags

    public enum DxfProxyCapabilitiesFlags : int
    {
        NoOperationsAllowed = 0,
        EraseAllowed = 1,
        TransformAllowed = 2,
        ColorChangeAllowed = 4,
        LayerChangeAllowed = 8,
        LinetypeChangeAllowed = 16,
        LinetypeScaleChangeAllowed = 32,
        VisibilityChangeAllowed = 64,
        AllOperationsExceptCloningAllowed = 127,
        CloningAllowed = 128,
        AllOperationsAllowed = 255,
        R13FormatProxy = 32768
    }

    #endregion

    #region DxfStyleFlags

    // Group code: 70
    public enum DxfStyleFlags : int
    {
        Default = 0,
        Shape = 1,
        VerticalText = 4,
        Xref = 16,
        XrefSuccess = 32,
        Referenced = 64
    }

    #endregion

    #region DxfTableStandardFlags

    // Group code: 70
    public enum DxfTableStandardFlags : int
    {
        Default = 0,
        Xref = 16,
        XrefSuccess = 32,
        Referenced = 64
    }

    #endregion

    #region DxfTextGenerationFlags

    // Group code: 71, default = 0
    public enum DxfTextGenerationFlags : int
    {
        Default = 0,
        MirroredInX = 2,
        MirroredInY = 4
    }

    #endregion

    #region DxfVerticalTextJustification

    // Group code: 73
    public enum DxfVerticalTextJustification : int
    {
        Default = 0,
        Baseline = 0,
        Bottom = 1,
        Middle = 2,
        Top = 3
    }

    #endregion

    #region DxfViewStandardFlags

    // Group code: 70
    public enum DxfViewStandardFlags : int
    {
        Default = 0,
        PaperSpace = 1,
        Xref = 16,
        XrefSuccess = 32,
        References = 64
    }

    #endregion

    #region DxfVportStandardFlags

    // Group code: 70
    public enum DxfVportStandardFlags : int
    {
        Default = 0,
        Xref = 16,
        XrefSuccess = 32,
        References = 64
    }

    #endregion
}

namespace Dxf.Core
{
    using Dxf.Enums;

    #region CodeName

    public static class CodeName
    {
        public const string Section = "SECTION";
        public const string EndSec = "ENDSEC";

        public const string SeqEnd = "SEQEND";

        public const string Entities = "ENTITIES";

        public const string Attdef = "ATTDEF";
        public const string Attrib = "ATTRIB";
        public const string Circle = "CIRCLE";
        public const string Ellipse = "ELLIPSE";
        public const string Insert = "INSERT";
        public const string Line = "LINE";
        public const string Text = "TEXT";

        public const string Ltype = "LTYPE";
        public const string BlockRecord = "BLOCK_RECORD";
        public const string Block = "BLOCK";
        public const string Endblk = "ENDBLK";

        public const string Vport = "VPORT";
        public const string Dimstyle = "DIMSTYLE";
        public const string Layer = "LAYER";
        public const string Ucs = "UCS";
        public const string Dictionary = "DICTIONARY";
        public const string Class = "CLASS";
    }

    #endregion

    #region SubclassMarker

    public static class SubclassMarker
    {
        public const string Line = "AcDbLine";
        public const string Text = "AcDbText";
        public const string Circle = "AcDbCircle";
        public const string Ellipse = "AcDbEllipse";

        public const string Dictionary = "AcDbDictionary";
        public const string Entity = "AcDbEntity";

        public const string AttributeDefinition = "AcDbAttributeDefinition";
        public const string Attribute = "AcDbAttribute";

        public const string DimStyleTable = "AcDbDimStyleTable";
        public const string SymbolTable = "AcDbSymbolTable";

        public const string BlockTableRecord = "AcDbBlockTableRecord";
        public const string LayerTableRecord = "AcDbLayerTableRecord";
        public const string ViewportTableRecord = "AcDbViewportTableRecord";
        public const string DimStyleTableRecord = "AcDbDimStyleTableRecord";
        public const string UCSTableRecord = "AcDbUCSTableRecord";
        public const string SymbolTableRecord = "AcDbSymbolTableRecord";
        public const string TextStyleTableRecord = "AcDbTextStyleTableRecord";
        public const string LinetypeTableRecord = "AcDbLinetypeTableRecord";
        public const string RegAppTableRecord = "AcDbRegAppTableRecord";

        public const string BlockBegin = "AcDbBlockBegin";
        public const string BlockEnd = "AcDbBlockEnd";
        public const string BlockReference = "AcDbBlockReference";
    }

    #endregion

    #region DxfObject

    public abstract class DxfObject<T> where T : DxfObject<T>
    {
        public virtual DxfAcadVer Version { get; private set; }
        public virtual int Id { get; private set; }

        public DxfObject(DxfAcadVer version, int id)
        {
            this.Version = version;
            this.Id = id;
        }

        protected StringBuilder sb = new StringBuilder();

        public override string ToString()
        {
            return this.Build();
        }

        public virtual void Reset()
        {
            this.sb.Length = 0;
        }

        public virtual string Build()
        {
            return this.sb.ToString();
        }

        public virtual T Add(string code, string data)
        {
            this.sb.AppendLine(code);
            this.sb.AppendLine(data);
            return this as T;
        }

        public virtual T Add(string code, bool data)
        {
            this.sb.AppendLine(code);
            this.sb.AppendLine(data == true ? 1.ToString() : 0.ToString());
            return this as T;
        }

        public virtual T Add(string code, int data)
        {
            this.sb.AppendLine(code);
            this.sb.AppendLine(data.ToString());
            return this as T;
        }

        public virtual T Add(string code, double data)
        {
            this.sb.AppendLine(code);
            this.sb.AppendLine(data.ToString(System.Globalization.CultureInfo.GetCultureInfo("en-GB")));
            return this as T;
        }

        public virtual T Add(int code, string data)
        {
            this.sb.AppendLine(code.ToString());
            this.sb.AppendLine(data);
            return this as T;
        }

        public virtual T Add(int code, bool data)
        {
            this.sb.AppendLine(code.ToString());
            this.sb.AppendLine(data == true ? 1.ToString() : 0.ToString());
            return this as T;
        }

        public virtual T Add(int code, int data)
        {
            this.sb.AppendLine(code.ToString());
            this.sb.AppendLine(data.ToString());
            return this as T;
        }

        public virtual T Add(int code, double data)
        {
            this.sb.AppendLine(code.ToString());
            this.sb.AppendLine(data.ToString(System.Globalization.CultureInfo.GetCultureInfo("en-GB")));
            return this as T;
        }

        protected virtual T Append(string str)
        {
            this.sb.Append(str);
            return this as T;
        }

        public virtual T Comment(string comment)
        {
            Add(999, comment);
            return this as T;
        }

        public virtual T Handle(string handle)
        {
            Add(5, handle);
            return this as T;
        }

        public virtual T Handle(int handle)
        {
            Add(5, handle.ToDxfHandle());
            return this as T;
        }

        public virtual T Subclass(string subclass)
        {
            Add(100, subclass);
            return this as T;
        }

        public virtual T Entity()
        {
            if (Version > DxfAcadVer.AC1009)
            {
                Add(5, Id.ToDxfHandle());
                Add(100, SubclassMarker.Entity);
            }

            // TODO: unify common Entity codes for all Entities
            //Add(8, layer);
            //Add(62, color);
            //Add(6, lineType);
            //Add(370, lineweight);
            //Add(78, lineTypeScale);
            //Add(60, isVisible);

            return this as T;
        }
    }

    #endregion

    #region DxfRawTag

    public class DxfRawTag
    {
        public string Code { get; set; }
        public string Data { get; set; }
    }

    #endregion

    #region Vector2

    public class Vector2
    {
        public double X { get; private set; }
        public double Y { get; private set; }

        public Vector2(double x, double y)
        {
            X = x;
            Y = y;
        }
    }

    #endregion

    #region Vector3

    public class Vector3
    {
        public double X { get; private set; }
        public double Y { get; private set; }
        public double Z { get; private set; }

        public Vector3(double x, double y, double z)
        {
            X = x;
            Y = y;
            Z = z;
        }
    }

    #endregion

    #region DxfUtil

    public static class DxfUtil
    {
        public static string ToDxfHandle(this int handle)
        {
            return handle.ToString("X");
        }

        public static string ColorToString(this DxfDefaultColors color)
        {
            return ((int)color).ToString();
        }
    }

    #endregion
}

namespace Dxf.Classes
{
    using Dxf.Core;
    using Dxf.Enums;

    #region DxfClass

    public class DxfClass : DxfObject<DxfClass>
    {
        public string DxfClassName { get; set; }
        public string CppClassName { get; set; }
        public DxfProxyCapabilitiesFlags ProxyCapabilitiesFlags { get; set; }
        public bool WasAProxyFlag { get; set; }
        public bool IsAnEntityFlag { get; set; }

        public DxfClass(DxfAcadVer version, int id)
            : base(version, id)
        {
        }

        public DxfClass Defaults()
        {
            DxfClassName = string.Empty;
            CppClassName = string.Empty;
            ProxyCapabilitiesFlags = DxfProxyCapabilitiesFlags.NoOperationsAllowed;
            WasAProxyFlag = false;
            IsAnEntityFlag = false;
            return this;
        }

        public DxfClass Create()
        {
            if (Version > DxfAcadVer.AC1009)
            {
                Add(0, CodeName.Class);
                Add(1, DxfClassName);
                Add(2, CppClassName);
                Add(90, (int)ProxyCapabilitiesFlags);
                Add(280, WasAProxyFlag);
                Add(281, IsAnEntityFlag);
            }

            return this;
        }
    }

    #endregion

    #region DxfClasses

    public class DxfClasses : DxfObject<DxfClasses>
    {
        public DxfClasses(DxfAcadVer version, int id)
            : base(version, id)
        {
        }

        public DxfClasses Begin()
        {
            Add(0, "SECTION");
            Add(2, "CLASSES");
            return this;
        }

        public DxfClasses Add(DxfClass cls)
        {
            Append(cls.ToString());
            return this;
        }

        public DxfClasses Add(IEnumerable<DxfClass> classes)
        {
            foreach (var cls in classes)
            {
                Add(cls);
            }

            return this;
        }

        public DxfClasses End()
        {
            Add(0, "ENDSEC");
            return this;
        }
    }

    #endregion
}

namespace Dxf.Tables
{
    using Dxf.Core;
    using Dxf.Enums;

    #region DxfAppid

    public class DxfAppid : DxfObject<DxfAppid>
    {
        public DxfAppid(DxfAcadVer version, int id)
            : base(version, id)
        {
            Add(0, "APPID");

            if (version > DxfAcadVer.AC1009)
            {
                Handle(id);
                Subclass(SubclassMarker.SymbolTableRecord);
                Subclass(SubclassMarker.RegAppTableRecord);
            }
        }

        public DxfAppid Application(string name)
        {
            Add(2, name);
            return this;
        }

        public DxfAppid StandardFlags(DxfAppidStandardFlags flags)
        {
            Add(70, (int)flags);
            return this;
        }
    }

    #endregion

    #region DxfBlockRecord

    public class DxfBlockRecord : DxfObject<DxfBlockRecord>
    {
        public string Name { get; set; }

        public DxfBlockRecord(DxfAcadVer version, int id)
            : base(version, id)
        {
        }

        public DxfBlockRecord Defaults()
        {
            Name = string.Empty;
            return this;
        }

        public DxfBlockRecord Create()
        {
            Add(0, CodeName.BlockRecord);

            if (Version > DxfAcadVer.AC1009)
            {
                Handle(Id);
                Subclass(SubclassMarker.SymbolTableRecord);
                Subclass(SubclassMarker.BlockTableRecord);
            }

            Add(2, Name);

            return this;
        }
    }


    #endregion

    #region DxfDimstyle

    public class DxfDimstyle : DxfObject<DxfDimstyle>
    {
        public string Name { get; set; }
        public DxfDimstyleStandardFlags DimstyleStandardFlags { get; set; }

        public DxfDimstyle(DxfAcadVer version, int id)
            : base(version, id)
        {
        }

        public DxfDimstyle Defaults()
        {
            Name = string.Empty;
            DimstyleStandardFlags = DxfDimstyleStandardFlags.Default;
            return this;
        }

        public DxfDimstyle Create()
        {
            Add(0, CodeName.Dimstyle);

            if (Version > DxfAcadVer.AC1009)
            {
                Add(105, Id.ToDxfHandle()); // Dimstyle handle code is 105 instead of 5
                Subclass(SubclassMarker.SymbolTableRecord);
                Subclass(SubclassMarker.DimStyleTableRecord);
            }

            Add(2, Name);
            Add(70, (int)DimstyleStandardFlags);

            return this;
        }
    }

    #endregion

    #region DxfLayer

    public class DxfLayer : DxfObject<DxfLayer>
    {
        public string Name { get; set; }
        public DxfLayerStandardFlags LayerStandardFlags { get; set; }
        public string Color { get; set; }
        public string LineType { get; set; }
        public bool PlottingFlag { get; set; }
        public DxfLineWeight LineWeight { get; set; }
        public string PlotStyleNameHandle { get; set; }

        public DxfLayer(DxfAcadVer version, int id)
            : base(version, id)
        {
        }

        public DxfLayer Defaults()
        {
            Name = string.Empty;
            LayerStandardFlags = DxfLayerStandardFlags.Default;
            Color = DxfDefaultColors.Default.ColorToString();
            LineType = string.Empty;
            PlottingFlag = true;
            LineWeight = DxfLineWeight.LnWtByLwDefault;
            PlotStyleNameHandle = "0";

            return this;
        }

        public DxfLayer Create()
        {
            Add(0, CodeName.Layer);

            if (Version > DxfAcadVer.AC1009)
            {
                Handle(Id);
                Subclass(SubclassMarker.SymbolTableRecord);
                Subclass(SubclassMarker.LayerTableRecord);
            }

            Add(2, Name);
            Add(70, (int)LayerStandardFlags);
            Add(62, Color);
            Add(6, LineType);

            if (Version > DxfAcadVer.AC1009)
            {
                Add(290, PlottingFlag);
                Add(370, (int)LineWeight);
                Add(390, PlotStyleNameHandle);
            }

            return this;
        }
    }

    #endregion

    #region DxfLtype

    public class DxfLtype : DxfObject<DxfLtype>
    {
        public string Name { get; set; }
        public DxfLtypeStandardFlags LtypeStandardFlags { get; set; }
        public string Description { get; set; }
        public int DashLengthItems { get; set; }
        public double TotalPatternLenght { get; set; }
        public double[] DashLenghts { get; set; }

        public DxfLtype(DxfAcadVer version, int id)
            : base(version, id)
        {
        }

        public DxfLtype Defaults()
        {
            Name = string.Empty;
            LtypeStandardFlags = DxfLtypeStandardFlags.Default;
            Description = string.Empty;
            DashLengthItems = 0;
            TotalPatternLenght = 0.0;
            DashLenghts = null;

            return this;
        }

        public DxfLtype Create()
        {
            Add(0, CodeName.Ltype);

            if (Version > DxfAcadVer.AC1009)
            {
                Handle(Id);
                Subclass(SubclassMarker.SymbolTableRecord);
                Subclass(SubclassMarker.LinetypeTableRecord);
            }

            Add(2, Name);
            Add(70, (int)LtypeStandardFlags);
            Add(3, Description);
            Add(72, 65); // alignment code; value is always 65, the ASCII code for A

            Add(73, DashLengthItems);
            Add(40, TotalPatternLenght);

            if (DashLenghts != null)
            {
                // dash length 0,1,2...n-1 = DashLengthItems
                foreach (var lenght in DashLenghts)
                {
                    Add(49, lenght);
                }
            }

            if (Version > DxfAcadVer.AC1009)
            {
                // TODO: multiple complex linetype elements
                // 74
                // 75
                // 340
                // 46
                // 50
                // 44
                // 45
                // 9
            }

            return this;
        }
    }

    #endregion

    #region DxfStyle

    public class DxfStyle : DxfObject<DxfStyle>
    {
        public DxfStyle(DxfAcadVer version, int id)
            : base(version, id)
        {
            Add(0, "STYLE");

            if (version > DxfAcadVer.AC1009)
            {
                Handle(id);
                Subclass(SubclassMarker.SymbolTableRecord);
                Subclass(SubclassMarker.TextStyleTableRecord);
            }
        }

        public DxfStyle Name(string name)
        {
            Add(2, name);
            return this;
        }

        public DxfStyle StandardFlags(DxfStyleFlags flags)
        {
            Add(70, (int)flags);
            return this;
        }

        public DxfStyle FixedTextHeight(double height)
        {
            Add(40, height);
            return this;
        }

        public DxfStyle WidthFactor(double factor)
        {
            Add(41, factor);
            return this;
        }

        public DxfStyle ObliqueAngle(double angle)
        {
            Add(50, angle);
            return this;
        }

        public DxfStyle TextGenerationFlags(DxfTextGenerationFlags flags)
        {
            Add(71, (int)flags);
            return this;
        }

        public DxfStyle LastHeightUsed(double height)
        {
            Add(42, height);
            return this;
        }

        public DxfStyle PrimaryFontFile(string name)
        {
            Add(3, name);
            return this;
        }

        public DxfStyle BifFontFile(string name)
        {
            Add(4, name);
            return this;
        }
    }

    #endregion

    #region DxfTables

    public class DxfTables : DxfObject<DxfTables>
    {
        public DxfTables(DxfAcadVer version, int id)
            : base(version, id)
        {
        }

        public DxfTables Begin()
        {
            Add(0, "SECTION");
            Add(2, "TABLES");
            return this;
        }

        public DxfTables Add<T>(T table)
        {
            Append(table.ToString());
            return this;
        }

        public DxfTables Add<T>(IEnumerable<T> tables)
        {
            foreach (var table in tables)
            {
                Add(table);
            }

            return this;
        }

        public DxfTables AddDimstyleTable(IEnumerable<DxfDimstyle> dimstyles, int id)
        {
            BeginDimstyles(dimstyles.Count(), id);
            Add(dimstyles);
            EndDimstyles();
            return this;
        }

        public DxfTables AddAppidTable(IEnumerable<DxfAppid> appids, int id)
        {
            BeginAppids(appids.Count(), id);
            Add(appids);
            EndAppids();
            return this;
        }

        public DxfTables AddBlockRecordTable(IEnumerable<DxfBlockRecord> records, int id)
        {
            BeginBlockRecords(records.Count(), id);
            Add(records);
            EndBlockRecords();
            return this;
        }

        public DxfTables AddLtypeTable(IEnumerable<DxfLtype> ltypes, int id)
        {
            BeginLtypes(ltypes.Count(), id);
            Add(ltypes);
            EndLtypes();
            return this;
        }

        public DxfTables AddLayerTable(IEnumerable<DxfLayer> layers, int id)
        {
            BeginLayers(layers.Count(), id);
            Add(layers);
            EndLayers();
            return this;
        }

        public DxfTables AddStyleTable(IEnumerable<DxfStyle> styles, int id)
        {
            BeginStyles(styles.Count(), id);
            Add(styles);
            EndStyles();
            return this;
        }

        public DxfTables AddUcsTable(IEnumerable<DxfUcs> ucss, int id)
        {
            BeginUcss(ucss.Count(), id);
            Add(ucss);
            EndUcss();
            return this;
        }

        public DxfTables AddViewTable(IEnumerable<DxfView> views, int id)
        {
            BeginViews(views.Count(), id);
            Add(views);
            EndViews();
            return this;
        }

        public DxfTables AddVportTable(IEnumerable<DxfVport> vports, int id)
        {
            BeginVports(vports.Count(), id);
            Add(vports);
            EndVports();
            return this;
        }

        public DxfTables End()
        {
            Add(0, "ENDSEC");
            return this;
        }

        private void BeginDimstyles(int count, int id)
        {
            BeginTable("DIMSTYLE", count, id);

            if (Version > DxfAcadVer.AC1009)
            {
                Subclass(SubclassMarker.DimStyleTable);
                Add(71, count);
            }
        }

        private void EndDimstyles()
        {
            EndTable();
        }

        private void BeginAppids(int count, int id)
        {
            BeginTable("APPID", count, id);
        }

        private void EndAppids()
        {
            EndTable();
        }

        private void BeginBlockRecords(int count, int id)
        {
            BeginTable("BLOCK_RECORD", count, id);
        }

        private void EndBlockRecords()
        {
            EndTable();
        }

        private void BeginLtypes(int count, int id)
        {
            BeginTable("LTYPE", count, id);
        }

        private void EndLtypes()
        {
            EndTable();
        }

        private void BeginLayers(int count, int id)
        {
            BeginTable("LAYER", count, id);
        }

        private void EndLayers()
        {
            EndTable();
        }

        private void BeginStyles(int count, int id)
        {
            BeginTable("STYLE", count, id);
        }

        private void EndStyles()
        {
            EndTable();
        }

        private void BeginUcss(int count, int id)
        {
            BeginTable("UCS", count, id);
        }

        private void EndUcss()
        {
            EndTable();
        }

        private void BeginViews(int count, int id)
        {
            BeginTable("VIEW", count, id);
        }

        private void EndViews()
        {
            EndTable();
        }

        private void BeginVports(int count, int id)
        {
            BeginTable("VPORT", count, id);
        }

        private void EndVports()
        {
            EndTable();
        }

        private void BeginTable(string name, int count, int id)
        {
            Add(0, "TABLE");
            Add(2, name);

            if (Version > DxfAcadVer.AC1009)
            {
                Handle(id);
                Subclass(SubclassMarker.SymbolTable);
            }

            Add(70, count);
        }

        private void EndTable()
        {
            Add(0, "ENDTAB");
        }
    }

    #endregion

    #region DxfUcs

    public class DxfUcs : DxfObject<DxfUcs>
    {
        public string Name { get; set; }
        public DxfTableStandardFlags TableStandardFlags { get; set; }
        public Vector3 Origin { get; set; }
        public Vector3 XAxisDirection { get; set; }
        public Vector3 YAxisDirection { get; set; }
        public DxfOrthographicViewType OrthographicViewType { get; set; }
        public double Elevation { get; set; }
        public string BaseUcsHandle { get; set; }
        public DxfOrthographicType[] OrthographicType { get; set; }
        public Vector3[] OrthographicOrigin { get; set; }

        public DxfUcs(DxfAcadVer version, int id)
            : base(version, id)
        {
        }

        public DxfUcs Defaults()
        {
            Name = string.Empty;
            TableStandardFlags = DxfTableStandardFlags.Default;
            Origin = new Vector3(0.0, 0.0, 0.0);
            XAxisDirection = new Vector3(0.0, 0.0, 0.0);
            YAxisDirection = new Vector3(0.0, 0.0, 0.0);
            OrthographicViewType = DxfOrthographicViewType.NotOrthographic;
            Elevation = 0;
            BaseUcsHandle = null;
            OrthographicType = null;
            OrthographicOrigin = null;

            return this;
        }

        public DxfUcs Create()
        {
            Add(0, CodeName.Ucs);

            if (Version > DxfAcadVer.AC1009)
            {
                Handle(Id);
                Subclass(SubclassMarker.SymbolTableRecord);
                Subclass(SubclassMarker.UCSTableRecord);
            }

            Add(2, Name);
            Add(70, (int)TableStandardFlags);

            Add(10, Origin.X);
            Add(20, Origin.Y);
            Add(30, Origin.Z);

            Add(11, XAxisDirection.X);
            Add(21, XAxisDirection.Y);
            Add(31, XAxisDirection.Z);

            Add(12, YAxisDirection.X);
            Add(22, YAxisDirection.Y);
            Add(32, YAxisDirection.Z);

            if (Version > DxfAcadVer.AC1009)
            {
                Add(79, (int)OrthographicViewType);
                Add(146, Elevation);

                if (OrthographicViewType != DxfOrthographicViewType.NotOrthographic
                    && BaseUcsHandle != null)
                {
                    Add(346, BaseUcsHandle);
                }

                if (OrthographicType != null &&
                    OrthographicOrigin != null &&
                    OrthographicType.Length == OrthographicOrigin.Length)
                {
                    int lenght = OrthographicType.Length;

                    for (int i = 0; i < lenght; i++)
                    {
                        Add(71, (int)OrthographicType[i]);
                        Add(13, OrthographicOrigin[i].X);
                        Add(23, OrthographicOrigin[i].Y);
                        Add(33, OrthographicOrigin[i].Z);
                    }
                }
            }

            return this;
        }
    }

    #endregion

    #region DxfView

    public class DxfView : DxfObject<DxfView>
    {
        public DxfView(DxfAcadVer version, int id)
            : base(version, id)
        {
            Add(0, "VIEW");

            if (version > DxfAcadVer.AC1009)
            {
                Handle(id);
                Subclass("AcDbSymbolTableRecord");
                Subclass("AcDbViewTableRecord");
            }
        }

        public DxfView Name(string name)
        {
            Add(2, name);
            return this;
        }

        public DxfView StandardFlags(DxfViewStandardFlags flags)
        {
            Add(70, (int)flags);
            return this;
        }

        public DxfView Height(double height)
        {
            Add(40, height);
            return this;
        }

        public DxfView Width(double width)
        {
            Add(41, width);
            return this;
        }

        public DxfView Center(Vector2 point)
        {
            Add(10, point.X);
            Add(20, point.Y);
            return this;
        }

        public DxfView ViewDirection(Vector3 wcs)
        {
            Add(11, wcs.X);
            Add(21, wcs.Y);
            Add(31, wcs.Z);
            return this;
        }

        public DxfView TargetPoint(Vector3 wcs)
        {
            Add(12, wcs.X);
            Add(22, wcs.Y);
            Add(32, wcs.Z);
            return this;
        }

        public DxfView LensLenght(double lenght)
        {
            Add(42, lenght);
            return this;
        }

        public DxfView FrontClippingPlane(double offset)
        {
            Add(43, offset);
            return this;
        }

        public DxfView BackClippingPlane(double offset)
        {
            Add(44, offset);
            return this;
        }

        public DxfView Twist(double angle)
        {
            Add(50, angle);
            return this;
        }
    }

    #endregion

    #region DxfVport

    public class DxfVport : DxfObject<DxfVport>
    {
        public string Name { get; set; }
        public DxfVportStandardFlags VportStandardFlags { get; set; }

        public DxfVport(DxfAcadVer version, int id)
            : base(version, id)
        {
        }

        public DxfVport Defaults()
        {
            Name = string.Empty;
            VportStandardFlags = DxfVportStandardFlags.Default;
            return this;
        }

        public DxfVport Create()
        {
            Add(0, CodeName.Vport);

            if (Version > DxfAcadVer.AC1009)
            {
                Handle(Id);
                Subclass(SubclassMarker.SymbolTableRecord);
                Subclass(SubclassMarker.ViewportTableRecord);
            }

            Add(2, Name);
            Add(70, (int)VportStandardFlags);

            return this;
        }
    }

    #endregion
}

namespace Dxf.Blocks
{
    using Dxf.Core;
    using Dxf.Enums;

    #region DxfBlock

    public class DxfBlock : DxfObject<DxfBlock>
    {
        public string Name { get; set; }
        public string Layer { get; set; }
        public DxfBlockTypeFlags BlockTypeFlags { get; set; }
        public Vector3 BasePoint { get; set; }
        public string XrefPathName { get; set; }
        public string Description { get; set; }
        public int EndId { get; set; }
        public string EndLayer { get; set; }
        public List<object> Entities { get; set; }

        public DxfBlock(DxfAcadVer version, int id)
            : base(version, id)
        {
        }

        public DxfBlock Defaults()
        {
            Name = string.Empty;
            Layer = "0";
            BlockTypeFlags = DxfBlockTypeFlags.Default;
            BasePoint = new Vector3(0.0, 0.0, 0.0);
            XrefPathName = null;
            Description = null;
            EndId = 0;
            EndLayer = "0";
            Entities = null;

            return this;
        }

        public DxfBlock Create()
        {
            Add(0, CodeName.Block);

            if (Version > DxfAcadVer.AC1009)
            {
                Handle(Id);
                Subclass(SubclassMarker.Entity);
            }

            Add(8, Layer);

            if (Version > DxfAcadVer.AC1009)
            {
                Subclass(SubclassMarker.BlockBegin);
            }

            Add(2, Name);
            Add(70, (int)BlockTypeFlags);

            Add(10, BasePoint.X);
            Add(20, BasePoint.Y);
            Add(30, BasePoint.Z);

            Add(3, Name);

            if (XrefPathName != null)
            {
                Add(1, XrefPathName);
            }

            if (Version > DxfAcadVer.AC1014 && Description != null)
            {
                Add(4, Description);
            }

            if (Entities != null)
            {
                foreach (var entity in Entities)
                {
                    Append(entity.ToString());
                }
            }

            Add(0, CodeName.Endblk);

            if (Version > DxfAcadVer.AC1009)
            {
                Handle(EndId);
                Subclass(SubclassMarker.Entity);
                Add(8, EndLayer);
                Subclass(SubclassMarker.BlockEnd);
            }

            return this;
        }
    }

    #endregion

    #region DxfBlocks

    public class DxfBlocks : DxfObject<DxfBlocks>
    {
        public DxfBlocks(DxfAcadVer version, int id)
            : base(version, id)
        {
        }

        public DxfBlocks Begin()
        {
            Add(0, "SECTION");
            Add(2, "BLOCKS");
            return this;
        }

        public DxfBlocks Add(DxfBlock block)
        {
            Append(block.ToString());
            return this;
        }

        public DxfBlocks Add(IEnumerable<DxfBlock> blocks)
        {
            foreach (var block in blocks)
                Add(block);

            return this;
        }

        public DxfBlocks End()
        {
            Add(0, "ENDSEC");
            return this;
        }
    }

    #endregion
}

namespace Dxf.Entities
{
    using Dxf.Core;
    using Dxf.Enums;

    #region Dxf3Dface

    public class Dxf3Dface : DxfObject<Dxf3Dface>
    {
        public Dxf3Dface(DxfAcadVer version, int id)
            : base(version, id)
        {
        }
    }

    #endregion

    #region DxfArc

    public class DxfArc : DxfObject<DxfArc>
    {
        public DxfArc(DxfAcadVer version, int id)
            : base(version, id)
        {
        }
    }

    #endregion

    #region DxfAttdef

    public class DxfAttdef : DxfObject<DxfAttdef>
    {
        public double Thickness { get; set; }
        public string Layer { get; set; }
        public string Color { get; set; }
        public Vector3 FirstAlignment { get; set; }
        public double TextHeight { get; set; }
        public string DefaultValue { get; set; }
        public double TextRotation { get; set; }
        public double ScaleFactorX { get; set; }
        public double ObliqueAngle { get; set; }
        public string TextStyle { get; set; }
        public DxfTextGenerationFlags TextGenerationFlags { get; set; }
        public DxfHorizontalTextJustification HorizontalTextJustification { get; set; }
        public Vector3 SecondAlignment { get; set; }
        public Vector3 ExtrusionDirection { get; set; }
        public string Prompt { get; set; }
        public string Tag { get; set; }
        public DxfAttributeFlags AttributeFlags { get; set; }
        public int FieldLength { get; set; }
        public DxfVerticalTextJustification VerticalTextJustification { get; set; }

        public DxfAttdef(DxfAcadVer version, int id)
            : base(version, id)
        {
        }

        public DxfAttdef Defaults()
        {
            Thickness = 0.0;
            Layer = "0";
            Color = DxfDefaultColors.ByLayer.ColorToString();
            FirstAlignment = new Vector3(0.0, 0.0, 0.0);
            TextHeight = 1.0;
            DefaultValue = string.Empty;
            TextRotation = 0.0;
            ScaleFactorX = 1.0;
            ObliqueAngle = 0.0;
            TextStyle = "Standard";
            TextGenerationFlags = DxfTextGenerationFlags.Default;
            HorizontalTextJustification = DxfHorizontalTextJustification.Default;
            SecondAlignment = new Vector3(0.0, 0.0, 0.0); ;
            ExtrusionDirection = new Vector3(0.0, 0.0, 1.0);
            Prompt = string.Empty;
            Tag = string.Empty;
            AttributeFlags = DxfAttributeFlags.Default;
            FieldLength = 0;
            VerticalTextJustification = DxfVerticalTextJustification.Default;

            return this;
        }

        public DxfAttdef Create()
        {
            Add(0, CodeName.Attdef);

            Entity();

            if (Version > DxfAcadVer.AC1009)
                Subclass(SubclassMarker.Text);

            Add(8, Layer);
            Add(62, Color);
            Add(39, Thickness);

            Add(10, FirstAlignment.X);
            Add(20, FirstAlignment.Y);
            Add(30, FirstAlignment.Z);

            Add(40, TextHeight);
            Add(1, DefaultValue);
            Add(50, TextRotation);
            Add(41, ScaleFactorX);
            Add(51, ObliqueAngle);
            Add(7, TextStyle);
            Add(71, (int)TextGenerationFlags);
            Add(72, (int)HorizontalTextJustification);

            Add(11, SecondAlignment.X);
            Add(21, SecondAlignment.Y);
            Add(31, SecondAlignment.Z);

            Add(210, ExtrusionDirection.X);
            Add(220, ExtrusionDirection.Y);
            Add(230, ExtrusionDirection.Z);

            if (Version > DxfAcadVer.AC1009)
                Subclass(SubclassMarker.AttributeDefinition);

            Add(3, Prompt);
            Add(2, Tag);

            Add(70, (int)AttributeFlags);
            Add(73, FieldLength);
            Add(74, (int)VerticalTextJustification);

            return this;
        }
    }

    #endregion

    #region DxfAttrib

    public class DxfAttrib : DxfObject<DxfAttrib>
    {
        public double Thickness { get; set; }
        public string Layer { get; set; }
        public Vector3 StartPoint { get; set; }
        public double TextHeight { get; set; }
        public string DefaultValue { get; set; }
        public double TextRotation { get; set; }
        public double ScaleFactorX { get; set; }
        public double ObliqueAngle { get; set; }
        public string TextStyle { get; set; }
        public DxfTextGenerationFlags TextGenerationFlags { get; set; }
        public DxfHorizontalTextJustification HorizontalTextJustification { get; set; }
        public Vector3 AlignmentPoint { get; set; }
        public Vector3 ExtrusionDirection { get; set; }
        public string Tag { get; set; }
        public DxfAttributeFlags AttributeFlags { get; set; }
        public int FieldLength { get; set; }
        public DxfVerticalTextJustification VerticalTextJustification { get; set; }

        public DxfAttrib(DxfAcadVer version, int id)
            : base(version, id)
        {
        }

        public DxfAttrib Defaults()
        {
            Thickness = 0.0;
            Layer = "0";
            StartPoint = new Vector3(0.0, 0.0, 0.0);
            TextHeight = 1.0;
            DefaultValue = string.Empty;
            TextRotation = 0.0;
            ScaleFactorX = 1.0;
            ObliqueAngle = 0.0;
            TextStyle = "Standard";
            TextGenerationFlags = DxfTextGenerationFlags.Default;
            HorizontalTextJustification = DxfHorizontalTextJustification.Default;
            AlignmentPoint = new Vector3(0.0, 0.0, 0.0); ;
            ExtrusionDirection = new Vector3(0.0, 0.0, 1.0);
            Tag = string.Empty;
            AttributeFlags = DxfAttributeFlags.Default;
            FieldLength = 0;
            VerticalTextJustification = DxfVerticalTextJustification.Default;

            return this;
        }

        public DxfAttrib Create()
        {
            Add(0, CodeName.Attrib);

            Entity();

            if (Version > DxfAcadVer.AC1009)
                Subclass(SubclassMarker.Text);

            Add(8, Layer);
            Add(39, Thickness);

            Add(10, StartPoint.X);
            Add(20, StartPoint.Y);
            Add(30, StartPoint.Z);

            Add(40, TextHeight);
            Add(1, DefaultValue);
            Add(50, TextRotation);
            Add(41, ScaleFactorX);
            Add(51, ObliqueAngle);
            Add(7, TextStyle);
            Add(71, (int)TextGenerationFlags);
            Add(72, (int)HorizontalTextJustification);

            Add(11, AlignmentPoint.X);
            Add(21, AlignmentPoint.Y);
            Add(31, AlignmentPoint.Z);

            Add(210, ExtrusionDirection.X);
            Add(220, ExtrusionDirection.Y);
            Add(230, ExtrusionDirection.Z);

            if (Version > DxfAcadVer.AC1009)
                Subclass(SubclassMarker.Attribute);

            Add(2, Tag);

            Add(70, (int)AttributeFlags);
            Add(73, FieldLength);
            Add(74, (int)VerticalTextJustification);

            return this;
        }
    }

    #endregion

    #region DxfCircle

    public class DxfCircle : DxfObject<DxfCircle>
    {
        public string Layer { get; set; }
        public string Color { get; set; }
        public double Thickness { get; set; }
        public Vector3 CenterPoint { get; set; }
        public double Radius { get; set; }
        public Vector3 ExtrusionDirection { get; set; }

        public DxfCircle(DxfAcadVer version, int id)
            : base(version, id)
        {
        }

        public DxfCircle Defaults()
        {
            Layer = "0";
            Color = "0";
            Thickness = 0.0;
            CenterPoint = new Vector3(0.0, 0.0, 0.0);
            Radius = 0.0;
            ExtrusionDirection = new Vector3(0.0, 0.0, 1.0);

            return this;
        }

        public DxfCircle Create()
        {
            Add(0, CodeName.Circle);

            Entity();

            if (Version > DxfAcadVer.AC1009)
                Subclass(SubclassMarker.Circle);

            Add(8, Layer);
            Add(62, Color);

            Add(39, Thickness);

            Add(10, CenterPoint.X);
            Add(20, CenterPoint.Y);
            Add(30, CenterPoint.Z);

            Add(40, Radius);

            Add(210, ExtrusionDirection.X);
            Add(220, ExtrusionDirection.Y);
            Add(230, ExtrusionDirection.Z);

            return this;
        }
    }

    #endregion

    #region DxfDimension

    public class DxfDimension : DxfObject<DxfDimension>
    {
        public DxfDimension(DxfAcadVer version, int id)
            : base(version, id)
        {
        }
    }

    #endregion

    #region DxfEllipse

    public class DxfEllipse : DxfObject<DxfEllipse>
    {
        public string Layer { get; set; }
        public string Color { get; set; }
        public Vector3 CenterPoint { get; set; }
        public Vector3 EndPoint { get; set; }
        public Vector3 ExtrusionDirection { get; set; }
        public double Ratio { get; set; }
        public double StartParameter { get; set; }
        public double EndParameter { get; set; }

        public DxfEllipse(DxfAcadVer version, int id)
            : base(version, id)
        {
        }

        public DxfEllipse Defaults()
        {
            Layer = "0";
            Color = "0";
            CenterPoint = new Vector3(0.0, 0.0, 0.0);
            EndPoint = new Vector3(0.0, 0.0, 0.0);
            ExtrusionDirection = new Vector3(0.0, 0.0, 1.0);
            Ratio = 0.0;
            StartParameter = 0.0;
            EndParameter = 2 * Math.PI;

            return this;
        }

        public DxfEllipse Create()
        {
            Add(0, CodeName.Ellipse);

            Entity();

            if (Version > DxfAcadVer.AC1009)
                Subclass(SubclassMarker.Ellipse);

            Add(8, Layer);
            Add(62, Color);

            Add(10, CenterPoint.X);
            Add(20, CenterPoint.Y);
            Add(30, CenterPoint.Z);

            Add(11, EndPoint.X);
            Add(21, EndPoint.Y);
            Add(31, EndPoint.Z);

            Add(210, ExtrusionDirection.X);
            Add(220, ExtrusionDirection.Y);
            Add(230, ExtrusionDirection.Z);

            Add(40, Ratio);
            Add(41, StartParameter);
            Add(42, EndParameter);

            return this;
        }
    }

    #endregion

    #region DxfEntities

    public class DxfEntities : DxfObject<DxfEntities>
    {
        public DxfEntities(DxfAcadVer version, int id)
            : base(version, id)
        {
        }

        public DxfEntities Begin()
        {
            Add(0, CodeName.Section);
            Add(2, CodeName.Entities);
            return this;
        }

        public DxfEntities Add<T>(T entity)
        {
            Append(entity.ToString());
            return this;
        }

        public DxfEntities Add<T>(IEnumerable<T> entities)
        {
            foreach (var entity in entities)
            {
                Add(entity);
            }

            return this;
        }

        public DxfEntities End()
        {
            Add(0, CodeName.EndSec);
            return this;
        }
    }

    #endregion

    #region DxfInsert

    public class DxfInsert : DxfObject<DxfInsert>
    {
        public DxfInsert(DxfAcadVer version, int id)
            : base(version, id)
        {
            Add(0, CodeName.Insert);

            Entity();

            if (Version > DxfAcadVer.AC1009)
                Subclass(SubclassMarker.BlockReference);
        }

        public DxfInsert Block(string name)
        {
            Add(2, name);
            return this;
        }

        public DxfInsert Layer(string layer)
        {
            Add(8, layer);
            return this;
        }

        public DxfInsert Insertion(Vector3 point)
        {
            Add(10, point.X);
            Add(20, point.Y);
            Add(30, point.Z);
            return this;
        }

        public DxfInsert Scale(Vector3 factor)
        {
            Add(41, factor.X);
            Add(42, factor.Y);
            Add(43, factor.Z);
            return this;
        }

        public DxfInsert Rotation(double angle)
        {
            Add(50, angle);
            return this;
        }

        public DxfInsert Columns(int count)
        {
            Add(70, count);
            return this;
        }

        public DxfInsert Rows(int count)
        {
            Add(71, count);
            return this;
        }

        public DxfInsert ColumnSpacing(double value)
        {
            Add(44, value);
            return this;
        }

        public DxfInsert RowSpacing(double value)
        {
            Add(45, value);
            return this;
        }

        public DxfInsert AttributesBegin()
        {
            Add(66, "1"); // attributes follow: 0 - no, 1 - yes
            return this;
        }

        public DxfInsert AddAttribute(DxfAttrib attrib)
        {
            Append(attrib.ToString());
            return this;
        }

        public DxfInsert AttributesEnd(int id, string layer)
        {
            Add(0, CodeName.SeqEnd);

            if (Version > DxfAcadVer.AC1009)
            {
                Handle(id);
                Subclass(SubclassMarker.Entity);
                Add(8, layer);
            }

            return this;
        }
    }

    #endregion

    #region DxfLine

    public class DxfLine : DxfObject<DxfLine>
    {
        public string Layer { get; set; }
        public string Color { get; set; }
        public double Thickness { get; set; }
        public Vector3 StartPoint { get; set; }
        public Vector3 EndPoint { get; set; }
        public Vector3 ExtrusionDirection { get; set; }

        public DxfLine(DxfAcadVer version, int id)
            : base(version, id)
        {
        }

        public DxfLine Defaults()
        {
            Layer = "0";
            Color = "0";
            Thickness = 0.0;
            StartPoint = new Vector3(0.0, 0.0, 0.0);
            EndPoint = new Vector3(0.0, 0.0, 0.0);
            ExtrusionDirection = new Vector3(0.0, 0.0, 1.0);

            return this;
        }

        public DxfLine Create()
        {
            Add(0, CodeName.Line);

            Entity();

            if (Version > DxfAcadVer.AC1009)
                Subclass(SubclassMarker.Line);

            Add(8, Layer);
            Add(62, Color);
            Add(39, Thickness);

            Add(10, StartPoint.X);
            Add(20, StartPoint.Y);
            Add(30, StartPoint.Z);

            Add(11, EndPoint.X);
            Add(21, EndPoint.Y);
            Add(31, EndPoint.Z);

            Add(210, ExtrusionDirection.X);
            Add(220, ExtrusionDirection.Y);
            Add(230, ExtrusionDirection.Z);

            return this;
        }
    }

    #endregion

    #region DxfLwpolyline

    public class DxfLwpolyline : DxfObject<DxfLwpolyline>
    {
        public DxfLwpolyline(DxfAcadVer version, int id)
            : base(version, id)
        {
        }
    }

    #endregion

    /*
    public class DxfLwpolylineVertex
    {
        public Vector2 Coordinates { get; private set; }
        public double StartWidth { get; private set; }
        public double EndWidth { get; private set; }
        public double Bulge { get; private set; }

        internal DxfLwpolylineVertex(Vector2 coordinates,
            double startWidth,
            double endWidth,
            double bulge)
        {
            Coordinates = coordinates;
            StartWidth = startWidth;
            EndWidth = endWidth;
            Bulge = bulge;
        }
    }

    public static string DxfLwpolyline(int numberOfVertices,
        DxfLwpolylineFlags lwpolylineFlags,
        double constantWidth,
        double elevation,
        double thickness,
        DxfLwpolylineVertex [] vertices,
        Vector3 extrusionDirection,
        string layer,
        string color)
    {
        var b = new DxfBuilder();

        // lwpolyline
        b.Add(0, "LWPOLYLINE");

        // layer
        if (layer != null)
            b.Add(8, layer);

        // color
        if (color != null)
            b.Add(62, color);

        // number of vertices
        b.Add(90, numberOfVertices);

        // polyline flags
        if (lwpolylineFlags != DxfLwpolylineFlags.Default)
            b.Add(70, (int)lwpolylineFlags);

        // constant width
        if (constantWidth != 0.0)
            b.Add(43, constantWidth);

        // elevation 
        if (elevation != 0.0)
            b.Add(38, elevation);

        // thickness 
        if (thickness != 0.0)
            b.Add(39, thickness);
 
        if (vertices != null)
        {
            // vertices
            foreach(var vertex in vertices)
            {
                b.Add(10, vertex.Coordinates.X);
                b.Add(20, vertex.Coordinates.Y);

                if (constantWidth == 0.0)
                {
                    // starting width
                    if (vertex.StartWidth != 0.0)
                        b.Add(40, vertex.StartWidth);

                    // end width
                    if (vertex.EndWidth != 0.0)
                        b.Add(41, vertex.EndWidth);
                }

                // bulge
                if (vertex.Bulge != 0.0)
                    b.Add(42, vertex.Bulge);
            }
        }

        if (extrusionDirection != null)
        {
            b.Add(210, extrusionDirection.X);
            b.Add(220, extrusionDirection.Y);
            b.Add(230, extrusionDirection.Z);
        }

        return b.Build();
    }
    */

    #region DxfPoint

    public class DxfPoint : DxfObject<DxfPoint>
    {
        public DxfPoint(DxfAcadVer version, int id)
            : base(version, id)
        {
        }
    }

    #endregion

    #region DxfPolyline

    public class DxfPolyline : DxfObject<DxfPolyline>
    {
        public DxfPolyline(DxfAcadVer version, int id)
            : base(version, id)
        {
        }
    }

    #endregion

    #region DxfShape

    public class DxfShape : DxfObject<DxfShape>
    {
        public DxfShape(DxfAcadVer version, int id)
            : base(version, id)
        {
        }
    }

    #endregion

    #region DxfSolid

    public class DxfSolid : DxfObject<DxfSolid>
    {
        public DxfSolid(DxfAcadVer version, int id)
            : base(version, id)
        {
        }
    }

    #endregion

    #region DxfText

    public class DxfText : DxfObject<DxfText>
    {
        public double Thickness { get; set; }
        public string Layer { get; set; }
        public string Color { get; set; }
        public Vector3 FirstAlignment { get; set; }
        public double TextHeight { get; set; }
        public string DefaultValue { get; set; }
        public double TextRotation { get; set; }
        public double ScaleFactorX { get; set; }
        public double ObliqueAngle { get; set; }
        public string TextStyle { get; set; }
        public DxfTextGenerationFlags TextGenerationFlags { get; set; }
        public DxfHorizontalTextJustification HorizontalTextJustification { get; set; }
        public Vector3 SecondAlignment { get; set; }
        public Vector3 ExtrusionDirection { get; set; }
        public DxfVerticalTextJustification VerticalTextJustification { get; set; }

        public DxfText(DxfAcadVer version, int id)
            : base(version, id)
        {
        }

        public DxfText Defaults()
        {
            Thickness = 0.0;
            Layer = "0";
            Color = DxfDefaultColors.ByLayer.ColorToString();
            FirstAlignment = new Vector3(0.0, 0.0, 0.0);
            TextHeight = 1.0;
            DefaultValue = string.Empty;
            TextRotation = 0.0;
            ScaleFactorX = 1.0;
            ObliqueAngle = 0.0;
            TextStyle = "Standard";
            TextGenerationFlags = DxfTextGenerationFlags.Default;
            HorizontalTextJustification = DxfHorizontalTextJustification.Default;
            SecondAlignment = new Vector3(0.0, 0.0, 0.0); ;
            ExtrusionDirection = new Vector3(0.0, 0.0, 1.0);
            VerticalTextJustification = DxfVerticalTextJustification.Default;

            return this;
        }

        public DxfText Create()
        {
            Add(0, CodeName.Text);

            Entity();

            if (Version > DxfAcadVer.AC1009)
                Subclass(SubclassMarker.Text);

            Add(8, Layer);
            Add(62, Color);
            Add(39, Thickness);

            Add(10, FirstAlignment.X);
            Add(20, FirstAlignment.Y);
            Add(30, FirstAlignment.Z);

            Add(40, TextHeight);
            Add(1, DefaultValue);
            Add(50, TextRotation);
            Add(41, ScaleFactorX);
            Add(51, ObliqueAngle);
            Add(7, TextStyle);
            Add(71, (int)TextGenerationFlags);
            Add(72, (int)HorizontalTextJustification);

            Add(11, SecondAlignment.X);
            Add(21, SecondAlignment.Y);
            Add(31, SecondAlignment.Z);

            Add(210, ExtrusionDirection.X);
            Add(220, ExtrusionDirection.Y);
            Add(230, ExtrusionDirection.Z);

            if (Version > DxfAcadVer.AC1009)
                Subclass(SubclassMarker.Text);

            Add(73, (int)VerticalTextJustification);

            return this;
        }
    }

    #endregion

    #region DxfTrace

    public class DxfTrace : DxfObject<DxfTrace>
    {
        public DxfTrace(DxfAcadVer version, int id)
            : base(version, id)
        {
        }
    }

    #endregion

    #region DxfVertex

    public class DxfVertex : DxfObject<DxfVertex>
    {
        public DxfVertex(DxfAcadVer version, int id)
            : base(version, id)
        {
        }
    }

    #endregion

    #region DxfViewport

    public class DxfViewport : DxfObject<DxfViewport>
    {
        public DxfViewport(DxfAcadVer version, int id)
            : base(version, id)
        {
        }
    }

    #endregion
}

namespace Dxf.Objects
{
    using Dxf.Core;
    using Dxf.Enums;

    #region DxfDictionary

    public class DxfDictionary : DxfObject<DxfDictionary>
    {
        public string OwnerDictionaryHandle { get; set; }
        public bool HardOwnerFlag { get; set; }
        public DxfDuplicateRecordCloningFlags DuplicateRecordCloningFlags { get; set; }
        public Dictionary<string, string> Entries { get; set; }

        public DxfDictionary(DxfAcadVer version, int id)
            : base(version, id)
        {
        }

        public DxfDictionary Defaults()
        {
            OwnerDictionaryHandle = "0";
            HardOwnerFlag = false;
            DuplicateRecordCloningFlags = DxfDuplicateRecordCloningFlags.KeepExisting;
            Entries = null;

            return this;
        }

        public DxfDictionary Create()
        {
            if (Version > DxfAcadVer.AC1009)
            {
                Add(0, CodeName.Dictionary);

                Handle(Id);
                Add(330, OwnerDictionaryHandle);
                Subclass(SubclassMarker.Dictionary);
                Add(280, HardOwnerFlag);
                Add(281, (int)DuplicateRecordCloningFlags);

                if (Entries != null)
                {
                    foreach (var entry in Entries)
                    {
                        var entryName = entry.Value;
                        var entryObjectHandle = entry.Key;

                        Add(3, entryName);
                        Add(350, entryObjectHandle);
                    }
                }
            }

            return this;
        }
    }

    #endregion

    #region DxfObjects

    public class DxfObjects : DxfObject<DxfObjects>
    {
        public DxfObjects(DxfAcadVer version, int id)
            : base(version, id)
        {
        }

        public DxfObjects Begin()
        {
            Add(0, "SECTION");
            Add(2, "OBJECTS");
            return this;
        }

        public DxfObjects Add<T>(T obj)
        {
            Append(obj.ToString());
            return this;
        }

        public DxfObjects Add<T>(IEnumerable<T> objects)
        {
            foreach (var obj in objects)
            {
                Add(obj);
            }

            return this;
        }

        public DxfObjects End()
        {
            Add(0, "ENDSEC");
            return this;
        }
    }

    #endregion
}

namespace Dxf
{
    using Dxf.Core;
    using Dxf.Enums;
    using Dxf.Classes;
    using Dxf.Tables;
    using Dxf.Blocks;
    using Dxf.Entities;
    using Dxf.Objects;

    #region DxfFile

    public class DxfFile : DxfObject<DxfFile>
    {
        public DxfFile(DxfAcadVer version, int id)
            : base(version, id)
        {
        }

        public DxfFile Header(DxfHeader header)
        {
            Append(header.ToString());
            return this;
        }

        public DxfFile Classes(DxfClasses classes)
        {
            Append(classes.ToString());
            return this;
        }

        public DxfFile Tables(DxfTables tables)
        {
            Append(tables.ToString());
            return this;
        }

        public DxfFile Blocks(DxfBlocks blocks)
        {
            Append(blocks.ToString());
            return this;
        }

        public DxfFile Entities(DxfEntities entities)
        {
            Append(entities.ToString());
            return this;
        }

        public DxfFile Objects(DxfObjects objects)
        {
            Append(objects.ToString());
            return this;
        }

        public DxfFile Eof()
        {
            Add(0, "EOF");
            return this;
        }
    }

    #endregion

    #region DxfHeader

    public class DxfHeader : DxfObject<DxfHeader>
    {
        public DxfHeader(DxfAcadVer version, int id)
            : base(version, id)
        {
        }

        public DxfHeader Begin()
        {
            Add(0, "SECTION");
            Add(2, "HEADER");
            return this;
        }

        private void VarName(string name)
        {
            Add(9, name);
        }

        public DxfHeader AcadVer(DxfAcadVer version)
        {
            VarName("$ACADVER");
            Add(1, version.ToString());
            return this;
        }

        public DxfHeader Default()
        {
            AcadVer(Version);

            VarName("$DWGCODEPAGE");
            Add(3, "ANSI_" + Encoding.Default.WindowsCodePage);

            /*
            // Angle 0 direction
            VarName("$ANGBASE");
            Add(50, 0.0);

            // 1 = clockwise angles, 0 = counterclockwise
            VarName("$ANGDIR");
            Add(70, 1);

            // Attribute entry dialogs, 1 = on, 0 = off
            VarName("$ATTDIA");
            Add(70, 0);

            // Attribute visibility: 0 = none, 1 = normal, 2 = all
            VarName("$ATTMODE");
            Add(70, 0);

            // Attribute prompting during INSERT, 1 = on, 0 = off
            VarName("$ATTREQ");
            Add(70, 0);

            // Units format for angles
            VarName("$AUNITS");
            70, 

            // Units precision for angles 
            VarName("$AUPREC");
            70, 

            // Axis on if nonzero (not functional in Release 12)
            VarName("$AXISMODE");
            Add(70, 0);

            // Axis X and Y tick spacing (not functional in Release 12)
            VarName("$AXISUNIT");
            Add(10, 0.0);
            Add(20, 0.0);

            // Blip mode on if nonzero
            VarName("$BLIPMODE");
            Add(70, 0);

            // Entity color number; 0 = BYBLOCK, 256 = BYLAYER
            VarName("$CECOLOR");
            Add(62, 0);

            // Entity linetype name, or BYBLOCK or BYLAYER
            VarName("$CELTYPE");
            Add(6, "BYLAYER");

            // First chamfer distance
            VarName("$CHAMFERA");
            Add(40, 0.0);

            // Second chamfer distance
            VarName("$CHAMFERB");
            Add(40, 0.0);

            // Current layer name
            VarName("$CLAYER");
            Add(8, "0");

            // 0 = static coordinate display, 1 = continuous update, 2 = "d<a" format
            VarName("$COORDS");
            Add(70, 0);

            // Alternate unit dimensioning performed if nonzero
            VarName("$DIMALT");
            Add(70, 0);

            // Alternate unit decimal places
            VarName("$DIMALTD");
            Add(70, );

            // Alternate unit scale factor
            VarName("$DIMALTF");
            Add(40, 0.0);

            // Alternate dimensioning suffix
            VarName("$DIMAPOST");
            Add(1, "");

            // 1 = create associative dimensioning, 0 = draw individual entities
            VarName("$DIMASO");
            Add(70, );

            // Dimensioning arrow size
            VarName("$DIMASZ");
            Add(40, );

            // Arrow block name
            VarName("$DIMBLK");
            Add(2, "");

            // First arrow block name
            VarName("$DIMBLK1");
            Add(1, "");

            // Second arrow block name
            VarName("$DIMBLK2");
            Add(1, "");

            // Size of center mark/lines
            VarName("$DIMCEN");
            Add(40, 0.0);

            // 
            VarName("$DIMCLRD");
            Add(70, 0);

            // 
            VarName("$DIMCLRE");
            Add(70, 0);

            // 
            VarName("$DIMCLRT");
            Add(70, 0);

            // 
            VarName("$DIMDLE");
            Add(70, 0);

            // 
            VarName("$DIMDLI");
            Add(70, 0);

            // 
            VarName("$DIMEXE");
            Add(70, 0);

            // 
            VarName("$DIMEXO");
            Add(70, 0);

            // 
            VarName("$DIMGAP");
            Add(70, 0);

            // 
            VarName("$DIMLFAC");
            Add(70, 0);

            // 
            VarName("$DIMLIM");
            Add(70, 0);

            // 
            VarName("$DIMPOST");
            Add(70, 0);

            // 
            VarName("$DIMRND");
            Add(70, 0);

            // 
            VarName("$DIMSAH");
            Add(70, 0);

            // 
            VarName("$DIMSCALE");
            Add(70, 0);

            // 
            VarName("$DIMSE1");
            Add(70, 0);

            // 
            VarName("$DIMSE2");
            Add(70, 0);

            // 
            VarName("$DIMSHO");
            Add(70, 0);

            // 
            VarName("$DIMSOXD");
            Add(70, 0);

            // 
            VarName("$DIMSTYLE");
            Add(70, 0);

            // 
            VarName("$DIMTAD");
            Add(70, 0);

            // 
            VarName("$DIMTFAC");
            Add(70, 0);

            // 
            VarName("$DIMTIH");
            Add(70, 0);

            // 
            VarName("$DIMTIX");
            Add(70, 0);

            // 
            VarName("$DIMTM");
            Add(70, 0);

            // 
            VarName("$DIMTOFL");
            Add(70, 0);

            // 
            VarName("$DIMTOH");
            Add(70, 0);

            // 
            VarName("$DIMTOL");
            Add(70, 0);

            // 
            VarName("$DIMTP");
            Add(70, 0);

            // 
            VarName("$DIMTSZ");
            Add(70, 0);

            // 
            VarName("$DIMTVP");
            Add(70, 0);

            // 
            VarName("$DIMTXT");
            Add(70, 0);

            // 
            VarName("$DIMZIN");
            Add(70, 0);

            // 
            VarName("$DWGCODEPAGE");
            Add(70, 0);

            // 
            VarName("$DRAGMODE");
            Add(70, 0);

            // 
            VarName("$ELEVATION");
            Add(70, 0);
            */

            // X, Y, and Z drawing extents upper-right corner (in WCS)
            VarName("$EXTMAX");
            Add(10, 1260.0);
            Add(20, 891.0);

            // X, Y, and Z drawing extents lower-left corner (in WCS)
            VarName("$EXTMIN");
            Add(10, 0.0);
            Add(20, 0.0);

            /*
            // 
            VarName("$FILLETRAD");
            Add(70, 0);

            // 
            VarName("$FILLMODE");
            Add(70, 0);

            // 
            VarName("$HANDLING");
            Add(70, 0);

            // 
            VarName("$HANDSEED");
            Add(70, 0);

            // 
            VarName("$INSBASE");
            Add(70, 0);

            // 
            VarName("$LIMCHECK");
            Add(70, 0);

            // 
            VarName("$LIMMAX");
            Add(70, 0);

            // 
            VarName("$LIMMIN");
            Add(70, 0);

            // 
            VarName("$LTSCALE");
            Add(70, 0);

            // 
            VarName("$LUNITS");
            Add(70, 0);

            // 
            VarName("$LUPREC");
            Add(70, 0);

            // 
            VarName("$MAXACTVP");
            Add(70, 0);

            // 
            VarName("$MENU");
            Add(70, 0);

            // 
            VarName("$MIRRTEXT");
            Add(70, 0);

            // 
            VarName("$ORTHOMODE");
            Add(70, 0);

            // 
            VarName("$OSMODE");
            Add(70, 0);

            // 
            VarName("$PDMODE");
            Add(70, 0);

            // 
            VarName("$PDSIZE");
            Add(70, 0);

            // 
            VarName("$PELEVATION");
            Add(70, 0);

            // 
            VarName("$PEXTMAX");
            Add(70, 0);

            // 
            VarName("$PEXTMIN");
            Add(70, 0);

            // 
            VarName("$PLIMCHECK");
            Add(70, 0);

            // 
            VarName("$PLIMMAX");
            Add(70, 0);

            // 
            VarName("$PLIMMIN");
            Add(70, 0);

            // 
            VarName("$PLINEGEN");
            Add(70, 0);

            // 
            VarName("$PLINEWID");
            Add(70, 0);

            // 
            VarName("$PSLTSCALE");
            Add(70, 0);

            // 
            VarName("$PUCSNAME");
            Add(70, 0);

            // 
            VarName("$PUCSORG");
            Add(70, 0);

            // 
            VarName("$PUCSXDIR");
            Add(70, 0);

            // 
            VarName("$PUCSYDIR");
            Add(70, 0);

            // 
            VarName("$QTEXTMODE");
            Add(70, 0);

            // 
            VarName("$REGENMODE");
            Add(70, 0);

            // 
            VarName("$SHADEDGE");
            Add(70, 0);

            // 
            VarName("$SHADEDIF");
            Add(70, 0);

            // 
            VarName("$SKETCHINC");
            Add(70, 0);

            // 
            VarName("$SKPOLY");
            Add(70, 0);

            // 
            VarName("$SPLFRAME");
            Add(70, 0);

            // 
            VarName("$SPLINESEGS");
            Add(70, 0);

            // 
            VarName("$SPLINETYPE");
            Add(70, 0);

            // 
            VarName("$SURFTAB1");
            Add(70, 0);

            // 
            VarName("$SURFTAB2");
            Add(70, 0);

            // 
            VarName("$SURFTYPE");
            Add(70, 0);

            // 
            VarName("$SURFU");
            Add(70, 0);

            // 
            VarName("$SURFV");
            Add(70, 0);

            // 
            VarName("$TDCREATE");
            Add(70, 0);

            // 
            VarName("$TDINDWG");
            Add(70, 0);

            // 
            VarName("$TDUPDATE");
            Add(70, 0);

            // 
            VarName("$TDUSRTIMER");
            Add(70, 0);

            // 
            VarName("$TEXTSIZE");
            Add(70, 0);

            // 
            VarName("$TEXTSTYLE");
            Add(70, 0);

            // 
            VarName("$THICKNESS");
            Add(70, 0);

            // 
            VarName("$TILEMODE");
            Add(70, 0);

            // 
            VarName("$TRACEWID");
            Add(70, 0);

            // 
            VarName("$UCSNAME");
            Add(70, 0);

            // 
            VarName("$UCSORG");
            Add(70, 0);

            // 
            VarName("$UCSXDIR");
            Add(70, 0);

            // 
            VarName("$UCSYDIR");
            Add(70, 0);

            // 
            VarName("$UNITMODE");
            Add(70, 0);

            // 
            VarName("$USERI1");
            Add(70, 0);

            // 
            VarName("$USERI2");
            Add(70, 0);

            // 
            VarName("$USERI3");
            Add(70, 0);

            // 
            VarName("$USERI4");
            Add(70, 0);

            // 
            VarName("$USERI5");
            Add(70, 0);

            // 
            VarName("$USERR1");
            Add(40, 0.0);

            // 
            VarName("$USERR2");
            Add(40, 0.0);

            // 
            VarName("$USERR3");
            Add(40, 0.0);

            // 
            VarName("$USERR4");
            Add(40, 0.0);

            // 
            VarName("$USERR5");
            Add(40, 0.0);

            // 0 = timer off, 1 = timer on
            VarName("$USRTIMER");
            Add(70, 0);

            // 
            VarName("$VISRETAIN");
            Add(70, 0);

            // 
            VarName("$WORLDVIEW");
            Add(70, 0);

            */

            // insertion base 
            VarName("$INSBASE");
            Add(10, "0.0");
            Add(20, "0.0");
            Add(30, "0.0");

            // drawing limits upper-right corner 
            VarName("$LIMMAX");
            Add(10, "1260.0");
            Add(20, "891.0");

            // drawing limits lower-left corner 
            VarName("$LIMMIN");
            Add(10, "0.0");
            Add(20, "0.0");

            // default drawing units for AutoCAD DesignCenter blocks
            /* 
            0 = Unitless;
            1 = Inches; 
            2 = Feet; 
            3 = Miles; 
            4 = Millimeters; 
            5 = Centimeters; 
            6 = Meters; 
            7 = Kilometers; 
            8 = Microinches; 
            9 = Mils; 
            10 = Yards; 
            11 = Angstroms; 
            12 = Nanometers; 
            13 = Microns; 
            14 = Decimeters; 
            15 = Decameters; 
            16 = Hectometers; 
            17 = Gigameters; 
            18 = Astronomical units; 
            19 = Light years; 
            20 = Parsecs
            */

            // units format for coordinates and distances
            VarName("$INSUNITS");
            Add(70, (int)4);

            // units format for coordinates and distances
            VarName("$LUNITS");
            Add(70, (int)2);

            // units precision for coordinates and distances
            VarName("$LUPREC");
            Add(70, (int)4);

            // sets drawing units
            VarName("$MEASUREMENT");
            Add(70, (int)1); // 0 = English; 1 = Metric

            // VPORT header variables

            /*
            // Fast zoom enabled if nonzero
            VarName("$FASTZOOM");
            Add(70, 0);

            // Grid mode on if nonzero
            VarName("$GRIDMODE");
            Add(70, 0);

            //  Grid X and Y spacing
            VarName("$GRIDUNIT");
            Add(10, 30.0);
            Add(20, 30.0);

            // Snap grid rotation angle
            VarName("$SNAPANG");
            Add(50, 0.0);

            // Snap/grid base point (in UCS) 
            VarName("$SNAPBASE");
            Add(10, 0.0);
            Add(20, 0.0);

            // Isometric plane: 0 = left, 1 = top, 2 = right
            VarName("$SNAPISOPAIR");
            Add(70, 0);

            // Snap mode on if nonzero
            VarName("$SNAPMODE");
            Add(70, 0);

            // Snap style: 0 = standard, 1 = isometric
            VarName("$SNAPSTYLE");
            Add(70, 0);

            // $SNAPUNIT
            VarName("$SNAPUNIT");
            Add(10, 15.0);
            Add(20, 15.0);

            // XY center of current view on screen
            VarName("$VIEWCTR");
            Add(10, 0.0);
            Add(20, 0.0);

            // Viewing direction (direction from target, in WCS)
            VarName("$VIEWDIR");
            Add(10, 0.0);
            Add(20, 0.0);
            Add(30, 0.0);

            // Height of view
            VarName("$VIEWSIZE");
            Add(40, 0.0);
            */
            return this;
        }

        public DxfHeader End(int nextAvailableHandle)
        {
            if (Version > DxfAcadVer.AC1009)
            {
                VarName("$HANDSEED");
                Add(5, nextAvailableHandle.ToDxfHandle());
            }

            Add(0, "ENDSEC");

            return this;
        }
    }

    #endregion

    #region DxfInspect

    public class DxfInspect
    {
        #region Fields

        private const string DxfCodeForType = "0";
        private const string DxfCodeForName = "2";

        #endregion

        #region Inspect

        public string GetHtml(string fileName)
        {
            var sb = new StringBuilder();

            string data = GetDxfData(fileName);
            if (data == null)
                return null;

            ParseDxfData(sb, fileName, data);

            return sb.ToString();
        }

        private string GetDxfData(string fileName)
        {
            string data = null;

            try
            {
                using (var reader = new System.IO.StreamReader(fileName))
                {
                    data = reader.ReadToEnd();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.Print(ex.Message);
                System.Diagnostics.Debug.Print(ex.StackTrace);
            }

            return data;
        }

        private void ParseDxfData(StringBuilder sb, string fileName, string data)
        {
            WriteHtmlHeader(sb, fileName);
            WriteBodyHeader(sb, fileName);

            var lines = data.Split("\n".ToCharArray(),
                StringSplitOptions.RemoveEmptyEntries);

            var tags = new List<DxfRawTag>();
            DxfRawTag tag = null;

            bool previousName = false;
            bool haveSection = false;
            //bool haveDxfCodeForType = false;

            string[] entity = new string[2] { null, null };
            int lineNumber = 0;

            foreach (var line in lines)
            {
                var str = line.Trim();

                // DxfCodeForType data
                if (tag != null)
                {
                    tag.Data = str;
                    tags.Add(tag);

                    haveSection = WriteTag(sb, tag, haveSection, lineNumber, str);

                    //haveDxfCodeForType = true;

                    tag = null;
                }
                else
                {
                    if (str == DxfCodeForType && entity[0] == null)
                    {
                        tag = new DxfRawTag();
                        tag.Code = str;
                    }
                    else
                    {
                        if (entity[0] == null)
                        {
                            entity[0] = str;
                            entity[1] = null;
                        }
                        else
                        {
                            entity[1] = str;

                            WriteEntity(sb, entity, lineNumber);

                            // entity Name
                            previousName = entity[0] == DxfCodeForName;

                            entity[0] = null;
                            entity[1] = null;

                            //haveDxfCodeForType = false;
                        }
                    }
                }

                lineNumber++;
            }

            WriteBodyFooter(sb, haveSection);
            WriteHtmlFooter(sb);
        }

        private static bool WriteTag(StringBuilder sb,
            DxfRawTag tag,
            bool haveSection,
            int lineNumber,
            string str)
        {
            bool isHeaderSection = str == CodeName.Section;
            string entityClass = isHeaderSection == true ? "Section" : "Other";

            if (haveSection == true && isHeaderSection == true)
            {
                sb.AppendFormat("</div>");
                haveSection = false;
            }

            if (haveSection == false && isHeaderSection == true)
            {
                haveSection = true;
                sb.AppendFormat("<div class=\"Table\">");
                sb.AppendFormat("<div class=\"Header\"><div class=\"Cell\"><p>LINE</p></div><div class=\"Cell\"><p>CODE</p></div><div class=\"Cell\"><p>DATA</p></div></div>{0}", Environment.NewLine);
            }

            sb.AppendFormat("<div class=\"{3}\"><div class=\"Cell\"><p class=\"Line\">{4}</p></div><div class=\"Cell\"><p class=\"Code\">{0}:</p></div><div class=\"Cell\"><p class=\"Data\">{1}</p></div></div>{2}",
                tag.Code,
                tag.Data,
                Environment.NewLine,
                entityClass,
                lineNumber);

            return haveSection;
        }

        private static void WriteEntity(StringBuilder sb, string[] entity, int lineNumber)
        {

            sb.AppendFormat("<div class=\"Row\"><div class=\"Cell\"><p class=\"Line\">{3}</p></div><div class=\"Cell\"><p class=\"Code\">{0}:</p></div><div class=\"Cell\"><p class=\"Data\">{1}</p></div></div>{2}",
                entity[0],
                entity[1],
                Environment.NewLine, lineNumber);
        }

        private static void WriteBodyHeader(StringBuilder sb, string fileName)
        {
        }

        private static void WriteBodyFooter(StringBuilder sb, bool haveSection)
        {
            if (haveSection == true)
            {
                sb.AppendFormat("</div>");
            }
        }

        private void WriteHtmlHeader(StringBuilder sb, string fileName)
        {
            sb.AppendLine("<html><head>");
            sb.AppendFormat("<title>{0}</title>{1}", System.IO.Path.GetFileName(fileName), Environment.NewLine);
            sb.AppendFormat("<meta charset=\"utf-8\"/>");
            sb.AppendLine("<style type=\"text/css\">");
            sb.AppendLine("body     { background-color:rgb(221,221,221); }");
            sb.AppendLine(".Table   { display:table; font-family:\"Courier New\"; font-size:10pt; border-collapse:collapse; margin:10px; float:none; }");
            sb.AppendLine(".Header  { display:table-row; font-weight:bold; text-align:left; background-color:rgb(255,30,102); }");
            sb.AppendLine(".Section { display:table-row; font-weight:normal; text-align:left; background-color:rgb(255,242,102); }");
            sb.AppendLine(".Other   { display:table-row; font-weight:normal; text-align:left; background-color:rgb(191,191,191); }");
            sb.AppendLine(".Row     { display:table-row; background-color:rgb(221,221,221); }");
            sb.AppendLine(".Cell    { display:table-cell; padding-left:5px; padding-right:5px; border:none; }");
            sb.AppendLine(".Line    { overflow:hidden; white-space:nowrap; text-overflow:ellipsis; width:60px; color:rgb(84,84,84); }");
            sb.AppendLine(".Code    { overflow:hidden; white-space:nowrap; text-overflow:ellipsis; width:50px; color:rgb(116,116,116); }");
            sb.AppendLine(".Data    { overflow:hidden; white-space:nowrap; text-overflow:ellipsis; width:200px; color:rgb(0,0,0); }");
            sb.AppendLine("</style>");
            sb.AppendLine("</head><body>");
        }

        private void WriteHtmlFooter(StringBuilder sb)
        {
            sb.AppendLine("</body></html>");
        }

        #endregion
    }

    #endregion
}
