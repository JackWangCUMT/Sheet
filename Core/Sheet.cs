using Sheet.Block;
using Sheet.Block.Core;
using Sheet.Block.Model;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Sheet
{
    #region FileDialogSettings

    public static class FileDialogSettings
    {
        #region Extensions

        public static string SolutionExtension = ".solution";
        public static string DocumentExtension = ".document";
        public static string PageExtension = ".page";
        public static string LibraryExtension = ".library";

        public static string DatabaseExtension = ".csv";

        public static string JsonSolutionExtension = ".jsolution";
        public static string JsonDocumentExtension = ".jdocument";
        public static string JsonPageExtension = ".jpage";
        public static string JsonLibraryExtension = ".jlibrary";

        #endregion

        #region Filters

        public static string SolutionFilter = "Solution Files (*.solution)|*.solution|Json Solution Files (*.jsolution)|*.jsolution|All Files (*.*)|*.*";
        public static string DocumentFilter = "Document Files (*.document)|*.document|Json Document Files (*.jdocument)|*.jdocument|All Files (*.*)|*.*";
        public static string PageFilter = "Page Files (*.page)|*.page|Json Page Files (*.jpage)|*.jpage|All Files (*.*)|*.*";
        public static string LibraryFilter = "Library Files (*.library)|*.library|Json Library Files (*.jlibrary)|*.jlibrary|All Files (*.*)|*.*";

        public static string DatabaseFilter = "Csv Files (*.csv)|*.csv|All Files (*.*)|*.*";
        public static string ImageFilter = "Supported Files (*.png;*.jpg;*.jpeg)|*.png;*.jpg;*.jpeg|Png Files (*.png)|*.png|Jpg Files (*.jpg)|*.jpg|Jpeg Files (*.jpeg)|*.jpeg|All Files (*.*)|*.*";
        public static string ExportFilter = "Pdf Documents (*.pdf)|*.pdf|Dxf Documents (*.dxf)|*.dxf|All Files (*.*)|*.*";

        #endregion
    }

    #endregion

    #region SheetHistoryController

    public class SheetHistoryController : IHistoryController
    {
        #region IoC

        private readonly ISheetController _sheetController;
        private readonly IItemSerializer _itemSerializer;

        public SheetHistoryController(ISheetController sheetController, IItemSerializer itemSerializer)
        {
            this._sheetController = sheetController;
            this._itemSerializer = itemSerializer;
        }

        #endregion

        #region Fields

        private Stack<ChangeItem> undos = new Stack<ChangeItem>();
        private Stack<ChangeItem> redos = new Stack<ChangeItem>();

        #endregion

        #region Factory

        private async Task<ChangeItem> CreateChange(string message)
        {
            var block = _sheetController.SerializePage();
            var text = await Task.Run(() => _itemSerializer.SerializeContents(block));
            var change = new ChangeItem()
            {
                Message = message,
                Model = text
            };
            return change;
        }

        #endregion

        #region IHistoryController

        public async void Register(string message)
        {
            var change = await CreateChange(message);
            undos.Push(change);
            redos.Clear();
        }

        public void Reset()
        {
            undos.Clear();
            redos.Clear();
        }

        public async void Undo()
        {
            if (undos.Count > 0)
            {
                try
                {
                    var change = await CreateChange("Redo");
                    redos.Push(change);
                    var undo = undos.Pop();
                    var block = await Task.Run(() => _itemSerializer.DeserializeContents(undo.Model));
                    _sheetController.ResetPage();
                    _sheetController.DeserializePage(block);
                }
                catch (Exception ex)
                {
                    Debug.Print(ex.Message);
                    Debug.Print(ex.StackTrace);
                }
            }
        }

        public async void Redo()
        {
            if (redos.Count > 0)
            {
                try
                {
                    var change = await CreateChange("Undo");
                    undos.Push(change);
                    var redo = redos.Pop();
                    var block = await Task.Run(() => _itemSerializer.DeserializeContents(redo.Model));
                    _sheetController.ResetPage();
                    _sheetController.DeserializePage(block);
                }
                catch (Exception ex)
                {
                    Debug.Print(ex.Message);
                    Debug.Print(ex.StackTrace);
                }
            }
        }

        #endregion
    }

    #endregion

    #region SheetPageFactory

    public class SheetPageFactory : IPageFactory
    {
        #region IoC

        private readonly IServiceLocator _serviceLocator;
        private readonly IBlockFactory _blockFactory;

        public SheetPageFactory(IServiceLocator serviceLocator)
        {
            this._serviceLocator = serviceLocator;
            this._blockFactory = serviceLocator.GetInstance<IBlockFactory>();
        }

        #endregion

        #region Create

        public void CreateLine(ISheet sheet, IList<ILine> lines, double thickness, double x1, double y1, double x2, double y2, ItemColor stroke)
        {
            var line = _blockFactory.CreateLine(thickness, x1, y1, x2, y2, stroke);

            if (lines != null)
            {
                lines.Add(line);
            }

            if (sheet != null)
            {
                sheet.Add(line);
            }
        }

        public void CreateText(ISheet sheet, IList<IText> texts, string content, double x, double y, double width, double height, int halign, int valign, double size, ItemColor foreground)
        {
            var text = _blockFactory.CreateText(content, x, y, width, height, halign, valign, size, ItemColors.Transparent, foreground);

            if (texts != null)
            {
                texts.Add(text);
            }

            if (sheet != null)
            {
                sheet.Add(text);
            }
        }

        public void CreateFrame(ISheet sheet, IBlock block, double size, double thickness, ItemColor stroke)
        {
            double padding = 6.0;
            double width = 1260.0;
            double height = 891.0;

            double startX = padding;
            double startY = padding;

            double rowsStart = 60;
            double rowsEnd = 780.0;

            double tableStartX = startX;
            double tableStartY = rowsEnd + 25.0;

            bool frameShowBorder = true;
            bool frameShowRows = true;
            bool frameShowTable = true;

            double row0 = 0.0;
            double row1 = 20.0;
            double row2 = 40.0;
            double row3 = 60.0;
            double row4 = 80.0;

            bool tableShowRevisions = true;
            bool tableShowLogos = true;
            bool tableShowInfo = true;

            if (frameShowRows)
            {
                // frame left rows
                int leftRowNumber = 1;
                for (double y = rowsStart; y < rowsEnd; y += size)
                {
                    CreateLine(sheet, block.Lines, thickness, startX, y, 330.0, y, stroke);
                    CreateText(sheet, block.Texts, leftRowNumber.ToString("00"), startX, y, 30.0 - padding, size, (int)XHorizontalAlignment.Center, (int)XVerticalAlignment.Center, 14.0, stroke);
                    leftRowNumber++;
                }

                // frame right rows
                int rightRowNumber = 1;
                for (double y = rowsStart; y < rowsEnd; y += size)
                {
                    CreateLine(sheet, block.Lines, thickness, 930.0, y, width - padding, y, stroke);
                    CreateText(sheet, block.Texts, rightRowNumber.ToString("00"), width - 30.0, y, 30.0 - padding, size, (int)XHorizontalAlignment.Center, (int)XVerticalAlignment.Center, 14.0, stroke);
                    rightRowNumber++;
                }

                // frame columns
                double[] columnWidth = { 30.0, 210.0, 90.0, 600.0, 210.0, 90.0 };
                double[] columnX = { 30.0, 30.0, startY, startY, 30.0, 30.0 };
                double[] columnY = { rowsEnd, rowsEnd, rowsEnd, rowsEnd, rowsEnd, rowsEnd };

                double start = 0.0;
                for (int i = 0; i < columnWidth.Length; i++)
                {
                    start += columnWidth[i];
                    CreateLine(sheet, block.Lines, thickness, start, columnX[i], start, columnY[i], stroke);
                }

                // frame header
                CreateLine(sheet, block.Lines, thickness, startX, 30.0, width - padding, 30.0, stroke);

                // frame footer
                CreateLine(sheet, block.Lines, thickness, startX, rowsEnd, width - padding, rowsEnd, stroke);
            }

            if (frameShowTable)
            {
                // table header
                CreateLine(sheet, block.Lines, thickness, tableStartX + 0, tableStartY + row0, tableStartX + 1248, tableStartY + row0, stroke);

                // table revisions
                if (tableShowRevisions)
                {
                    CreateLine(sheet, block.Lines, thickness, tableStartX + 24, tableStartY + row0, tableStartX + 24, tableStartY + row4, stroke);
                    CreateLine(sheet, block.Lines, thickness, tableStartX + 75, tableStartY + row0, tableStartX + 75, tableStartY + row4, stroke);
                    CreateLine(sheet, block.Lines, thickness, tableStartX + 0, tableStartY + row1, tableStartX + 175, tableStartY + row1, stroke);
                    CreateLine(sheet, block.Lines, thickness, tableStartX + 0, tableStartY + row2, tableStartX + 175, tableStartY + row2, stroke);
                    CreateLine(sheet, block.Lines, thickness, tableStartX + 0, tableStartY + row3, tableStartX + 175, tableStartY + row3, stroke);
                }

                // table logos
                if (tableShowLogos)
                {
                    CreateLine(sheet, block.Lines, thickness, tableStartX + 175, tableStartY + row0, tableStartX + 175, tableStartY + row4, stroke);
                    CreateLine(sheet, block.Lines, thickness, tableStartX + 290, tableStartY + row0, tableStartX + 290, tableStartY + row4, stroke);
                    CreateLine(sheet, block.Lines, thickness, tableStartX + 405, tableStartY + row0, tableStartX + 405, tableStartY + row4, stroke);
                }

                // table info
                if (tableShowInfo)
                {
                    CreateLine(sheet, block.Lines, thickness, tableStartX + 405, tableStartY + row1, tableStartX + 1248, tableStartY + row1, stroke);
                    CreateLine(sheet, block.Lines, thickness, tableStartX + 405, tableStartY + row2, tableStartX + 695, tableStartY + row2, stroke);
                    CreateLine(sheet, block.Lines, thickness, tableStartX + 965, tableStartY + row2, tableStartX + 1248, tableStartY + row2, stroke);
                    CreateLine(sheet, block.Lines, thickness, tableStartX + 405, tableStartY + row3, tableStartX + 695, tableStartY + row3, stroke);
                    CreateLine(sheet, block.Lines, thickness, tableStartX + 965, tableStartY + row3, tableStartX + 1248, tableStartY + row3, stroke);

                    CreateLine(sheet, block.Lines, thickness, tableStartX + 465, tableStartY + row0, tableStartX + 465, tableStartY + row4, stroke);
                    CreateLine(sheet, block.Lines, thickness, tableStartX + 595, tableStartY + row0, tableStartX + 595, tableStartY + row4, stroke);
                    CreateLine(sheet, block.Lines, thickness, tableStartX + 640, tableStartY + row0, tableStartX + 640, tableStartY + row4, stroke);
                    CreateLine(sheet, block.Lines, thickness, tableStartX + 695, tableStartY + row0, tableStartX + 695, tableStartY + row4, stroke);
                    CreateLine(sheet, block.Lines, thickness, tableStartX + 965, tableStartY + row0, tableStartX + 965, tableStartY + row4, stroke);

                    CreateLine(sheet, block.Lines, thickness, tableStartX + 1005, tableStartY + row0, tableStartX + 1005, tableStartY + row4, stroke);
                    CreateLine(sheet, block.Lines, thickness, tableStartX + 1045, tableStartY + row0, tableStartX + 1045, tableStartY + row4, stroke);
                    CreateLine(sheet, block.Lines, thickness, tableStartX + 1100, tableStartY + row0, tableStartX + 1100, tableStartY + row4, stroke);
                }
            }

            if (frameShowBorder)
            {
                // frame border
                CreateLine(sheet, block.Lines, thickness, startX, startY, width - padding, startY, stroke);
                CreateLine(sheet, block.Lines, thickness, startX, height - padding, width - padding, height - padding, stroke);
                CreateLine(sheet, block.Lines, thickness, startX, startY, startX, height - padding, stroke);
                CreateLine(sheet, block.Lines, thickness, width - padding, startY, width - padding, height - padding, stroke);
            }
        }

        public void CreateGrid(ISheet sheet, IBlock block, double startX, double startY, double width, double height, double size, double thickness, ItemColor stroke)
        {
            for (double y = startY + size; y < height + startY; y += size)
            {
                CreateLine(sheet, block.Lines, thickness, startX, y, width + startX, y, stroke);
            }

            for (double x = startX + size; x < startX + width; x += size)
            {
                CreateLine(sheet, block.Lines, thickness, x, startY, x, height + startY, stroke);
            }
        }

        public IRectangle CreateSelectionRectangle(double thickness, double x, double y, double width, double height)
        {
            var stroke = new ItemColor() { Alpha = 0x7A, Red = 0x00, Green = 0x00, Blue = 0xFF };
            var fill = new ItemColor() { Alpha = 0x3A, Red = 0x00, Green = 0x00, Blue = 0xFF };
            var xrect = _blockFactory.CreateRectangle(thickness, x, y, width, height, true, stroke, fill);
            return xrect;
        }

        #endregion
    }

    #endregion

    #region SheetMode

    public enum SheetMode
    {
        None,
        Selection,
        Insert,
        Pan,
        Move,
        Edit,
        Point,
        Line,
        Rectangle,
        Ellipse,
        Text,
        Image,
        TextEditor
    }

    #endregion

    #region SheetOptions

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
        public double Zoom { get; set; }
        public double PanX { get; set; }
        public double PanY { get; set; }
    } 

    #endregion

    #region InputButton

    public enum InputButton
    {
        None,
        Left,
        Right,
        Middle
    }

    #endregion

    #region InputArgs

    public class InputArgs
    {
        // Mouse Generic
        public bool OnlyControl { get; set; }
        public bool OnlyShift { get; set; }
        public ItemType SourceType { get; set; }
        public XImmutablePoint SheetPosition { get; set; }
        public XImmutablePoint RootPosition { get; set; }
        public Action<bool> Handled { get; set; }
        // Mouse Wheel
        public int Delta { get; set; }
        // Mouse Down
        public InputButton Button { get; set; }
        public int Clicks { get; set; }
    }

    #endregion

    #region SheetController

    public class SheetController : ISheetController
    {
        #region IoC

        private readonly IServiceLocator _serviceLocator;
        private readonly IBlockController _blockController;
        private readonly IBlockFactory _blockFactory;
        private readonly IBlockSerializer _blockSerializer;
        private readonly IBlockHelper _blockHelper;
        private readonly IItemController _itemController;
        private readonly IItemSerializer _itemSerializer;
        private readonly IJsonSerializer _jsonSerializer;
        private readonly IClipboard _clipboard;
        private readonly IBase64 _base64;
        private readonly IPointController _pointController;
        private readonly IPageFactory _pageFactory;

        public SheetController(IServiceLocator serviceLocator)
        {
            this._serviceLocator = serviceLocator;
            this._blockController = serviceLocator.GetInstance<IBlockController>();
            this._blockFactory = serviceLocator.GetInstance<IBlockFactory>();
            this._blockSerializer = serviceLocator.GetInstance<IBlockSerializer>();
            this._blockHelper = serviceLocator.GetInstance<IBlockHelper>();
            this._itemController = serviceLocator.GetInstance<IItemController>();
            this._itemSerializer = serviceLocator.GetInstance<IItemSerializer>();
            this._jsonSerializer = serviceLocator.GetInstance<IJsonSerializer>();
            this._clipboard = serviceLocator.GetInstance<IClipboard>();
            this._base64 = serviceLocator.GetInstance<IBase64>();
            this._pointController = serviceLocator.GetInstance<IPointController>();
            this._pageFactory = serviceLocator.GetInstance<IPageFactory>();
        }

        #endregion

        #region Properties

        public IHistoryController HistoryController { get; set; }
        public ILibraryController LibraryController { get; set; }
        public IZoomController ZoomController { get; set; }
        public ICursorController CursorController { get; set; }
        public SheetOptions Options { get; set; }
        public ISheet EditorSheet { get; set; }
        public ISheet BackSheet { get; set; }
        public ISheet ContentSheet { get; set; }
        public ISheet OverlaySheet { get; set; }
        public ISheetView View { get; set; }
        public double LastFinalWidth { get; set; }
        public double LastFinalHeight { get; set; }

        #endregion

        #region Fields

        private SheetMode Mode = SheetMode.Selection;
        private SheetMode TempMode = SheetMode.None;
        private IBlock SelectedBlock;
        private IBlock ContentBlock;
        private IBlock FrameBlock;
        private IBlock GridBlock;
        private ILine TempLine;
        private IEllipse TempStartEllipse;
        private IEllipse TempEndEllipse;
        private IRectangle TempRectangle;
        private IEllipse TempEllipse;
        private IRectangle TempSelectionRect;
        private bool IsFirstMove = true;
        private XImmutablePoint PanStartPoint;
        private XImmutablePoint SelectionStartPoint;
        private ItemType SelectedType = ItemType.None;
        private ILine SelectedLine;
        private IThumb LineThumbStart;
        private IThumb LineThumbEnd;
        private IElement SelectedElement;
        private IThumb ThumbTopLeft;
        private IThumb ThumbTopRight;
        private IThumb ThumbBottomLeft;
        private IThumb ThumbBottomRight;
        private ISelectedBlockPlugin InvertLineStartPlugin;
        private ISelectedBlockPlugin InvertLineEndPlugin;

        #endregion

        #region ToSingle

        public static IEnumerable<T> ToSingle<T>(T item)
        {
            yield return item;
        }

        #endregion

        #region Init

        public void Init()
        {
            SetDefaults();

            CreateBlocks();
            CreatePlugins();
            CreatePage();

            LoadLibraryFromResource(string.Concat("Sheet.Libraries", '.', "Digital.library"));
        }

        private void SetDefaults()
        {
            Options = new SheetOptions()
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

            ZoomController.ZoomIndex = Options.DefaultZoomIndex;
        }

        private void CreateBlocks()
        {
            ContentBlock = _blockFactory.CreateBlock(-1, Options.PageOriginX, Options.PageOriginY, Options.PageWidth, Options.PageHeight, -1, "CONTENT", null);
            ContentBlock.Init();

            FrameBlock = _blockFactory.CreateBlock(-1, Options.PageOriginX, Options.PageOriginY, Options.PageWidth, Options.PageHeight, -1, "FRAME", null);
            FrameBlock.Init();

            GridBlock = _blockFactory.CreateBlock(-1, Options.PageOriginX, Options.PageOriginY, Options.PageWidth, Options.PageHeight, -1, "GRID", null);
            GridBlock.Init();

            SelectedBlock = _blockFactory.CreateBlock(-1, Options.PageOriginX, Options.PageOriginY, Options.PageWidth, Options.PageHeight, -1, "SELECTED", null);
        }

        #endregion

        #region Blocks
        
        public IBlock GetSelected()
        {
            return SelectedBlock;
        }
        
        public IBlock GetContent()
        {
            return ContentBlock;
        }
        
        #endregion
        
        #region Page

        public async void SetPage(string text)
        {
            try
            {
                if (text == null)
                {
                    HistoryController.Reset();
                    ResetPage();
                }
                else
                {
                    var block = await Task.Run(() => _itemSerializer.DeserializeContents(text));
                    HistoryController.Reset();
                    ResetPage();
                    DeserializePage(block);
                }
            }
            catch (Exception ex)
            {
                Debug.Print(ex.Message);
                Debug.Print(ex.StackTrace);
            }
        }

        public string GetPage()
        {
            var block = SerializePage();
            var text = _itemSerializer.SerializeContents(block);

            return text;
        }

        public void ExportPage(string text)
        {
            var block = _itemSerializer.DeserializeContents(text);
            Export(ToSingle(block));
        }

        public void ExportPages(IEnumerable<string> texts)
        {
            var blocks = texts.Select(text => _itemSerializer.DeserializeContents(text));
            Export(blocks);
        }

        public BlockItem SerializePage()
        {
            _blockController.DeselectContent(SelectedBlock);

            var grid = _blockSerializer.SerializerContents(GridBlock, -1, GridBlock.X, GridBlock.Y, GridBlock.Width, GridBlock.Height, -1, "GRID");
            var frame = _blockSerializer.SerializerContents(FrameBlock, -1, FrameBlock.X, FrameBlock.Y, FrameBlock.Width, FrameBlock.Height, -1, "FRAME");
            var content = _blockSerializer.SerializerContents(ContentBlock, -1, ContentBlock.X, ContentBlock.Y, ContentBlock.Width, ContentBlock.Height, -1, "CONTENT");

            var page = new BlockItem();
            page.Init(-1, Options.PageOriginX, Options.PageOriginY, Options.PageWidth, Options.PageHeight, -1, "PAGE");

            page.Blocks.Add(grid);
            page.Blocks.Add(frame);
            page.Blocks.Add(content);

            return page;
        }

        public void DeserializePage(BlockItem page)
        {
            BlockItem grid = page.Blocks.Where(block => block.Name == "GRID").FirstOrDefault();
            BlockItem frame = page.Blocks.Where(block => block.Name == "FRAME").FirstOrDefault();
            BlockItem content = page.Blocks.Where(block => block.Name == "CONTENT").FirstOrDefault();

            if (grid != null)
            {
                _blockController.AddContents(BackSheet, grid, GridBlock, null, false, Options.GridThickness / ZoomController.Zoom);
            }

            if (frame != null)
            {
                _blockController.AddContents(BackSheet, frame, FrameBlock, null, false, Options.FrameThickness / ZoomController.Zoom);
            }

            if (content != null)
            {
                _blockController.AddContents(ContentSheet, content, ContentBlock, null, false, Options.LineThickness / ZoomController.Zoom);
            }
        }

        public void ResetPage()
        {
            ResetOverlay();

            _blockController.Remove(BackSheet, GridBlock);
            _blockController.Remove(BackSheet, FrameBlock);
            _blockController.Remove(ContentSheet, ContentBlock);

            CreateBlocks();
        }

        public void ResetPageContent()
        {
            ResetOverlay();

            _blockController.Remove(ContentSheet, ContentBlock);
        }

        #endregion

        #region Mode

        public SheetMode GetMode()
        {
            return Mode;
        }

        public void SetMode(SheetMode mode)
        {
            Mode = mode;
        }

        private void StoreTempMode()
        {
            TempMode = GetMode();
        }

        private void RestoreTempMode()
        {
            SetMode(TempMode);
        }

        #endregion

        #region Clipboard

        public void CutText()
        {
            try
            {
                if (_blockController.HaveSelected(SelectedBlock))
                {
                    var copy = _blockController.ShallowCopy(SelectedBlock);
                    HistoryController.Register("Cut");
                    CopyText(copy);
                    Delete(copy);
                }
            }
            catch (Exception ex)
            {
                Debug.Print(ex.Message);
                Debug.Print(ex.StackTrace);
            }
        }

        private void CopyText(IBlock block)
        {
            try
            {
                var selected = _blockSerializer.SerializerContents(block, -1, 0.0, 0.0, 0.0, 0.0, -1, "SELECTED");
                var text = _itemSerializer.SerializeContents(selected);
                _clipboard.Set(text);
            }
            catch (Exception ex)
            {
                Debug.Print(ex.Message);
                Debug.Print(ex.StackTrace);
            }
        }

        public void CopyText()
        {
            if (_blockController.HaveSelected(SelectedBlock))
            {
                CopyText(SelectedBlock);
            }
        }

        public async void PasteText()
        {
            try
            {
                var text = _clipboard.Get();
                var block = await Task.Run(() => _itemSerializer.DeserializeContents(text));
                HistoryController.Register("Paste");
                InsertContent(block, true);
            }
            catch (Exception ex)
            {
                Debug.Print(ex.Message);
                Debug.Print(ex.StackTrace);
            }
        }

        public void CutJson()
        {
            try
            {
                if (_blockController.HaveSelected(SelectedBlock))
                {
                    var copy = _blockController.ShallowCopy(SelectedBlock);
                    HistoryController.Register("Cut");
                    CopyJson(copy);
                    Delete(copy);
                }
            }
            catch (Exception ex)
            {
                Debug.Print(ex.Message);
                Debug.Print(ex.StackTrace);
            }
        }

        private void CopyJson(IBlock block)
        {
            try
            {
                var selected = _blockSerializer.SerializerContents(block, -1, 0.0, 0.0, 0.0, 0.0, -1, "SELECTED");
                string json = _jsonSerializer.Serialize(selected);
                _clipboard.Set(json);
            }
            catch (Exception ex)
            {
                Debug.Print(ex.Message);
                Debug.Print(ex.StackTrace);
            }
        }

        public void CopyJson()
        {
            if (_blockController.HaveSelected(SelectedBlock))
            {
                CopyJson(SelectedBlock);
            }
        }

        public async void PasteJson()
        {
            try
            {
                var text = _clipboard.Get();
                var block = await Task.Run(() => _jsonSerializer.Deerialize<BlockItem>(text));
                HistoryController.Register("Paste");
                InsertContent(block, true);
            }
            catch (Exception ex)
            {
                Debug.Print(ex.Message);
                Debug.Print(ex.StackTrace);
            }
        }

        #endregion

        #region Overlay

        private void ResetOverlay()
        {
            if (TempLine != null)
            {
                OverlaySheet.Remove(TempLine);
                TempLine = null;
            }

            if (TempStartEllipse != null)
            {
                OverlaySheet.Remove(TempStartEllipse);
                TempLine = null;
            }

            if (TempEndEllipse != null)
            {
                OverlaySheet.Remove(TempEndEllipse);
                TempEndEllipse = null;
            }

            if (TempRectangle != null)
            {
                OverlaySheet.Remove(TempRectangle);
                TempRectangle = null;
            }

            if (TempEllipse != null)
            {
                OverlaySheet.Remove(TempEllipse);
                TempEllipse = null;
            }

            if (TempSelectionRect != null)
            {
                OverlaySheet.Remove(TempSelectionRect);
                TempSelectionRect = null;
            }

            if (LineThumbStart != null)
            {
                OverlaySheet.Remove(LineThumbStart);
            }

            if (LineThumbEnd != null)
            {
                OverlaySheet.Remove(LineThumbEnd);
            }
        }

        #endregion

        #region Delete

        public void Delete(IBlock block)
        {
            FinishEdit();
            _blockController.RemoveSelected(ContentSheet, ContentBlock, block);
        }

        public void Delete()
        {
            if (_blockController.HaveSelected(SelectedBlock))
            {
                var copy = _blockController.ShallowCopy(SelectedBlock);
                HistoryController.Register("Delete");
                Delete(copy);
            }
        }

        #endregion

        #region Select All

        public void SelecteAll()
        {
            _blockController.SelectContent(SelectedBlock, ContentBlock);
        }

        #endregion

        #region Toggle Fill

        public void ToggleFill()
        {
            if (TempRectangle != null)
            {
                _blockController.ToggleFill(TempRectangle);
            }

            if (TempEllipse != null)
            {
                _blockController.ToggleFill(TempEllipse);
            }
        }

        #endregion

        #region Insert Mode

        private void InsertContent(BlockItem block, bool select)
        {
            _blockController.DeselectContent(SelectedBlock);
            _blockController.AddContents(ContentSheet, block, ContentBlock, SelectedBlock, select, Options.LineThickness / ZoomController.Zoom);
        }

        private BlockItem CreateBlock(string name, IBlock block)
        {
            try
            {
                var blockItem = _blockSerializer.Serialize(block);
                blockItem.Name = name;
                return blockItem;
            }
            catch (Exception ex)
            {
                Debug.Print(ex.Message);
                Debug.Print(ex.StackTrace);
            }
            return null;
        }

        public void CreateBlock()
        {
            if (_blockController.HaveSelected(SelectedBlock))
            {
                StoreTempMode();
                SetMode(SheetMode.TextEditor);

                var tc = CreateTextEditor(new XImmutablePoint((EditorSheet.Width / 2) - (330 / 2), EditorSheet.Height / 2));

                Action<string> ok = (name) =>
                {
                    var block = CreateBlock(name, SelectedBlock);
                    if (block != null)
                    {
                        AddToLibrary(block);
                    }
                    EditorSheet.Remove(tc);
                    View.Focus();
                    RestoreTempMode();
                };

                Action cancel = () =>
                {
                    EditorSheet.Remove(tc);
                    View.Focus();
                    RestoreTempMode();
                };

                tc.Set(ok, cancel, "Create Block", "Name:", "BLOCK0");
                EditorSheet.Add(tc);
            }
        }

        public async void BreakBlock()
        {
            if (_blockController.HaveSelected(SelectedBlock))
            {
                var text = _itemSerializer.SerializeContents(_blockSerializer.SerializerContents(SelectedBlock, 0, 0.0, 0.0, 0.0, 0.0, -1, "SELECTED"));
                var block = await Task.Run(() => _itemSerializer.DeserializeContents(text));
                HistoryController.Register("Break Block");
                Delete();
                _blockController.AddBroken(ContentSheet, block, ContentBlock, SelectedBlock, true, Options.LineThickness / ZoomController.Zoom);
            }
        }

        #endregion

        #region Point Mode

        public IPoint InsertPoint(XImmutablePoint p, bool register, bool select)
        {
            double thickness = Options.LineThickness / ZoomController.Zoom;
            double x = _itemController.Snap(p.X, Options.SnapSize);
            double y = _itemController.Snap(p.Y, Options.SnapSize);

            var point = _blockFactory.CreatePoint(thickness, x, y, false);

            if (register)
            {
                _blockController.DeselectContent(SelectedBlock);
                HistoryController.Register("Insert Point");
            }

            ContentBlock.Points.Add(point);
            ContentSheet.Add(point);

            if (select)
            {
                SelectedBlock.Points = new List<IPoint>();
                SelectedBlock.Points.Add(point);

                _blockController.Select(point);
            }

            return point;
        }

        #endregion

        #region Move Mode

        private void Move(double x, double y)
        {
            if (_blockController.HaveSelected(SelectedBlock))
            {
                IBlock moveBlock = _blockController.ShallowCopy(SelectedBlock);
                FinishEdit();
                HistoryController.Register("Move");
                _blockController.Select(moveBlock);
                SelectedBlock = moveBlock;
                _blockController.MoveDelta(x, y, SelectedBlock);
            }
        }

        public void MoveUp()
        {
            Move(0.0, -Options.SnapSize);
        }

        public void MoveDown()
        {
            Move(0.0, Options.SnapSize);
        }

        public void MoveLeft()
        {
            Move(-Options.SnapSize, 0.0);
        }

        public void MoveRight()
        {
            Move(Options.SnapSize, 0.0);
        }

        private bool CanInitMove(XImmutablePoint p)
        {
            var temp = _blockFactory.CreateBlock(-1, Options.PageOriginX, Options.PageOriginY, Options.PageWidth, Options.PageHeight, -1, "TEMP", null);
            _blockController.HitTestClick(ContentSheet, SelectedBlock, temp, p, Options.HitTestSize, false, true);
            if (_blockController.HaveSelected(temp))
            {
                return true;
            }
            return false;
        }

        private void InitMove(XImmutablePoint p)
        {
            IsFirstMove = true;
            StoreTempMode();
            SetMode(SheetMode.Move);
            double x = _itemController.Snap(p.X, Options.SnapSize);
            double y = _itemController.Snap(p.Y, Options.SnapSize);
            PanStartPoint = new XImmutablePoint(x, y);
            ResetOverlay();
            OverlaySheet.Capture();
        }

        private void Move(XImmutablePoint p)
        {
            if (IsFirstMove)
            {
                IBlock moveBlock = _blockController.ShallowCopy(SelectedBlock);
                HistoryController.Register("Move");
                IsFirstMove = false;
                CursorController.Set(SheetCursor.Move);
                _blockController.Select(moveBlock);
                SelectedBlock = moveBlock;
            }

            double x = _itemController.Snap(p.X, Options.SnapSize);
            double y = _itemController.Snap(p.Y, Options.SnapSize);
            double dx = x - PanStartPoint.X;
            double dy = y - PanStartPoint.Y;

            if (dx != 0.0 || dy != 0.0)
            {
                _blockController.MoveDelta(dx, dy, SelectedBlock);
                PanStartPoint = new XImmutablePoint(x, y);
            }
        }

        private void FinishMove()
        {
            RestoreTempMode();
            CursorController.Set(SheetCursor.Normal);
            OverlaySheet.ReleaseCapture();
        }

        #endregion

        #region Pan & Zoom Mode

        public void SetAutoFitSize(double finalWidth, double finalHeight)
        {
            LastFinalWidth = finalWidth;
            LastFinalHeight = finalHeight;
        }

        private void ZoomTo(double x, double y, int oldZoomIndex)
        {
            double oldZoom = GetZoom(oldZoomIndex);
            double newZoom = GetZoom(ZoomController.ZoomIndex);
            ZoomController.Zoom = newZoom;

            ZoomController.PanX = (x * oldZoom + ZoomController.PanX) - x * newZoom;
            ZoomController.PanY = (y * oldZoom + ZoomController.PanY) - y * newZoom;
        }

        private void ZoomTo(int delta, XImmutablePoint p)
        {
            if (delta > 0)
            {

                if (ZoomController.ZoomIndex > -1 && ZoomController.ZoomIndex < Options.MaxZoomIndex)
                {
                    ZoomTo(p.X, p.Y, ZoomController.ZoomIndex++);
                }
            }
            else
            {
                if (ZoomController.ZoomIndex > 0)
                {
                    ZoomTo(p.X, p.Y, ZoomController.ZoomIndex--);
                }
            }
        }

        private double GetZoom(int index)
        {
            if (index >= 0 && index <= Options.MaxZoomIndex)
            {
                return Options.ZoomFactors[index];
            }
            return ZoomController.Zoom;
        }

        private void InitPan(XImmutablePoint p)
        {
            StoreTempMode();
            SetMode(SheetMode.Pan);
            PanStartPoint = new XImmutablePoint(p.X, p.Y);
            ResetOverlay();
            CursorController.Set(SheetCursor.Pan);
            OverlaySheet.Capture();
        }

        private void Pan(XImmutablePoint p)
        {
            ZoomController.PanX = ZoomController.PanX + p.X - PanStartPoint.X;
            ZoomController.PanY = ZoomController.PanY + p.Y - PanStartPoint.Y;
            PanStartPoint = new XImmutablePoint(p.X, p.Y);
        }

        private void FinishPan()
        {
            RestoreTempMode();
            CursorController.Set(SheetCursor.Normal);
            OverlaySheet.ReleaseCapture();
        }

        private void AdjustThickness(IEnumerable<ILine> lines, double thickness)
        {
            foreach (var line in lines)
            {
                _blockHelper.SetStrokeThickness(line, thickness);
            }
        }

        private void AdjustThickness(IEnumerable<IRectangle> rectangles, double thickness)
        {
            foreach (var rectangle in rectangles)
            {
                _blockHelper.SetStrokeThickness(rectangle, thickness);
            }
        }

        private void AdjustThickness(IEnumerable<IEllipse> ellipses, double thickness)
        {
            foreach (var ellipse in ellipses)
            {
                _blockHelper.SetStrokeThickness(ellipse, thickness);
            }
        }

        private void AdjustThickness(IBlock parent, double thickness)
        {
            if (parent != null)
            {
                AdjustThickness(parent.Lines, thickness);
                AdjustThickness(parent.Rectangles, thickness);
                AdjustThickness(parent.Ellipses, thickness);

                foreach (var block in parent.Blocks)
                {
                    AdjustThickness(block, thickness);
                }
            }
        }

        public void AdjustBackThickness(double zoom)
        {
            double gridThicknessZoomed = Options.GridThickness / zoom;
            double frameThicknessZoomed = Options.FrameThickness / zoom;

            AdjustThickness(GridBlock, gridThicknessZoomed);
            AdjustThickness(FrameBlock, frameThicknessZoomed);
        }

        public void AdjustPageThickness(double zoom)
        {
            double lineThicknessZoomed = Options.LineThickness / zoom;
            double selectionThicknessZoomed = Options.SelectionThickness / zoom;

            AdjustBackThickness(zoom);
            AdjustThickness(ContentBlock, lineThicknessZoomed);

            if (TempLine != null)
            {
                _blockHelper.SetStrokeThickness(TempLine, lineThicknessZoomed);
            }

            if (TempStartEllipse != null)
            {
                _blockHelper.SetStrokeThickness(TempStartEllipse, lineThicknessZoomed);
            }

            if (TempEndEllipse != null)
            {
                _blockHelper.SetStrokeThickness(TempEndEllipse, lineThicknessZoomed);
            }

            if (TempRectangle != null)
            {
                _blockHelper.SetStrokeThickness(TempRectangle, lineThicknessZoomed);
            }

            if (TempEllipse != null)
            {
                _blockHelper.SetStrokeThickness(TempEllipse, lineThicknessZoomed);
            }

            if (TempSelectionRect != null)
            {
                _blockHelper.SetStrokeThickness(TempSelectionRect, selectionThicknessZoomed);
            }
        }

        #endregion

        #region Selection Mode

        private void InitSelectionRect(XImmutablePoint p)
        {
            SelectionStartPoint = new XImmutablePoint(p.X, p.Y);
            TempSelectionRect = _pageFactory.CreateSelectionRectangle(Options.SelectionThickness / ZoomController.Zoom, p.X, p.Y, 0.0, 0.0);
            OverlaySheet.Add(TempSelectionRect);
            OverlaySheet.Capture();
        }

        private void MoveSelectionRect(XImmutablePoint p)
        {
            double sx = SelectionStartPoint.X;
            double sy = SelectionStartPoint.Y;
            double x = p.X;
            double y = p.Y;
            double width = Math.Abs(sx - x);
            double height = Math.Abs(sy - y);
            _blockHelper.SetLeft(TempSelectionRect, Math.Min(sx, x));
            _blockHelper.SetTop(TempSelectionRect, Math.Min(sy, y));
            _blockHelper.SetWidth(TempSelectionRect, width);
            _blockHelper.SetHeight(TempSelectionRect, height);
        }

        private void FinishSelectionRect()
        {
            double x = _blockHelper.GetLeft(TempSelectionRect);
            double y = _blockHelper.GetTop(TempSelectionRect);
            double width = _blockHelper.GetWidth(TempSelectionRect);
            double height = _blockHelper.GetHeight(TempSelectionRect);

            CancelSelectionRect();

            // get selected items
            bool onlyCtrl = Keyboard.Modifiers == ModifierKeys.Control;
            bool resetSelected = onlyCtrl && _blockController.HaveSelected(SelectedBlock) ? false : true;
            _blockController.HitTestSelectionRect(ContentSheet, ContentBlock, SelectedBlock, new XImmutableRect(x, y, width, height), resetSelected);

            // edit mode
            TryToEditSelected();
        }

        private void CancelSelectionRect()
        {
            OverlaySheet.ReleaseCapture();
            OverlaySheet.Remove(TempSelectionRect);
            TempSelectionRect = null;
        }

        #endregion

        #region Line Mode

        private IPoint TryToFindPoint(XImmutablePoint p)
        {
            var temp = _blockFactory.CreateBlock(-1, Options.PageOriginX, Options.PageOriginY, Options.PageWidth, Options.PageHeight, -1, "TEMP", null);
            _blockController.HitTestClick(ContentSheet, ContentBlock, temp, p, Options.HitTestSize, true, true);

            if (_blockController.HaveOnePointSelected(temp))
            {
                var xpoint = temp.Points[0];
                _blockController.Deselect(temp);
                return xpoint;
            }

            _blockController.Deselect(temp);
            return null;
        }

        private void InitTempLine(XImmutablePoint p, IPoint start)
        {
            double x = _itemController.Snap(p.X, Options.SnapSize);
            double y = _itemController.Snap(p.Y, Options.SnapSize);

            TempLine = _blockFactory.CreateLine(Options.LineThickness / ZoomController.Zoom, x, y, x, y, ItemColors.Black);

            if (start != null)
            {
                TempLine.Start = start;
            }

            TempStartEllipse = _blockFactory.CreateEllipse(Options.LineThickness / ZoomController.Zoom, x - 4.0, y - 4.0, 8.0, 8.0, true, ItemColors.Black, ItemColors.Black);
            TempEndEllipse = _blockFactory.CreateEllipse(Options.LineThickness / ZoomController.Zoom, x - 4.0, y - 4.0, 8.0, 8.0, true, ItemColors.Black, ItemColors.Black);

            OverlaySheet.Add(TempLine);
            OverlaySheet.Add(TempStartEllipse);
            OverlaySheet.Add(TempEndEllipse);
            OverlaySheet.Capture();
        }

        private void MoveTempLine(XImmutablePoint p)
        {
            double x = _itemController.Snap(p.X, Options.SnapSize);
            double y = _itemController.Snap(p.Y, Options.SnapSize);
            double x2 = _blockHelper.GetX2(TempLine);
            double y2 = _blockHelper.GetY2(TempLine);
            if (Math.Round(x, 1) != Math.Round(x2, 1)
                || Math.Round(y, 1) != Math.Round(y2, 1))
            {
                _blockHelper.SetX2(TempLine, x);
                _blockHelper.SetY2(TempLine, y);
                _blockHelper.SetLeft(TempEndEllipse, x - 4.0);
                _blockHelper.SetTop(TempEndEllipse, y - 4.0);
            }
        }

        private void FinishTempLine(IPoint end)
        {
            double x1 = _blockHelper.GetX1(TempLine);
            double y1 = _blockHelper.GetY1(TempLine);
            double x2 = _blockHelper.GetX2(TempLine);
            double y2 = _blockHelper.GetY2(TempLine);

            if (Math.Round(x1, 1) == Math.Round(x2, 1) && Math.Round(y1, 1) == Math.Round(y2, 1))
            {
                CancelTempLine();
            }
            else
            {
                if (end != null)
                {
                    TempLine.End = end;
                }

                OverlaySheet.ReleaseCapture();
                OverlaySheet.Remove(TempLine);
                OverlaySheet.Remove(TempStartEllipse);
                OverlaySheet.Remove(TempEndEllipse);

                HistoryController.Register("Create Line");

                if (TempLine.Start != null)
                {
                    _pointController.ConnectStart(TempLine.Start, TempLine);
                }

                if (TempLine.End != null)
                {
                    _pointController.ConnectEnd(TempLine.End, TempLine);
                }

                ContentBlock.Lines.Add(TempLine);
                ContentSheet.Add(TempLine);

                TempLine = null;
                TempStartEllipse = null;
                TempEndEllipse = null;
            }
        }

        private void CancelTempLine()
        {
            OverlaySheet.ReleaseCapture();
            OverlaySheet.Remove(TempLine);
            OverlaySheet.Remove(TempStartEllipse);
            OverlaySheet.Remove(TempEndEllipse);
            TempLine = null;
            TempStartEllipse = null;
            TempEndEllipse = null;
        }

        #endregion

        #region Rectangle Mode

        private void InitTempRect(XImmutablePoint p)
        {
            double x = _itemController.Snap(p.X, Options.SnapSize);
            double y = _itemController.Snap(p.Y, Options.SnapSize);
            SelectionStartPoint = new XImmutablePoint(x, y);
            TempRectangle = _blockFactory.CreateRectangle(Options.LineThickness / ZoomController.Zoom, x, y, 0.0, 0.0, false, ItemColors.Black, ItemColors.Transparent);
            OverlaySheet.Add(TempRectangle);
            OverlaySheet.Capture();
        }

        private void MoveTempRect(XImmutablePoint p)
        {
            double sx = SelectionStartPoint.X;
            double sy = SelectionStartPoint.Y;
            double x = _itemController.Snap(p.X, Options.SnapSize);
            double y = _itemController.Snap(p.Y, Options.SnapSize);
            _blockHelper.SetLeft(TempRectangle, Math.Min(sx, x));
            _blockHelper.SetTop(TempRectangle, Math.Min(sy, y));
            _blockHelper.SetWidth(TempRectangle, Math.Abs(sx - x));
            _blockHelper.SetHeight(TempRectangle, Math.Abs(sy - y));
        }

        private void FinishTempRect()
        {
            double x = _blockHelper.GetLeft(TempRectangle);
            double y = _blockHelper.GetTop(TempRectangle);
            double width = _blockHelper.GetWidth(TempRectangle);
            double height = _blockHelper.GetHeight(TempRectangle);
            if (width == 0.0 || height == 0.0)
            {
                CancelTempRect();
            }
            else
            {
                OverlaySheet.ReleaseCapture();
                OverlaySheet.Remove(TempRectangle);
                HistoryController.Register("Create Rectangle");
                ContentBlock.Rectangles.Add(TempRectangle);
                ContentSheet.Add(TempRectangle);
                TempRectangle = null;
            }
        }

        private void CancelTempRect()
        {
            OverlaySheet.ReleaseCapture();
            OverlaySheet.Remove(TempRectangle);
            TempRectangle = null;
        }

        #endregion

        #region Ellipse Mode

        private void InitTempEllipse(XImmutablePoint p)
        {
            double x = _itemController.Snap(p.X, Options.SnapSize);
            double y = _itemController.Snap(p.Y, Options.SnapSize);
            SelectionStartPoint = new XImmutablePoint(x, y);
            TempEllipse = _blockFactory.CreateEllipse(Options.LineThickness / ZoomController.Zoom, x, y, 0.0, 0.0, false, ItemColors.Black, ItemColors.Transparent);
            OverlaySheet.Add(TempEllipse);
            OverlaySheet.Capture();
        }

        private void MoveTempEllipse(XImmutablePoint p)
        {
            double sx = SelectionStartPoint.X;
            double sy = SelectionStartPoint.Y;
            double x = _itemController.Snap(p.X, Options.SnapSize);
            double y = _itemController.Snap(p.Y, Options.SnapSize);
            _blockHelper.SetLeft(TempEllipse, Math.Min(sx, x));
            _blockHelper.SetTop(TempEllipse, Math.Min(sy, y));
            _blockHelper.SetWidth(TempEllipse, Math.Abs(sx - x));
            _blockHelper.SetHeight(TempEllipse, Math.Abs(sy - y));
        }

        private void FinishTempEllipse()
        {
            double x = _blockHelper.GetLeft(TempEllipse);
            double y = _blockHelper.GetTop(TempEllipse);
            double width = _blockHelper.GetWidth(TempEllipse);
            double height = _blockHelper.GetHeight(TempEllipse);
            if (width == 0.0 || height == 0.0)
            {
                CancelTempEllipse();
            }
            else
            {
                OverlaySheet.ReleaseCapture();
                OverlaySheet.Remove(TempEllipse);
                HistoryController.Register("Create Ellipse");
                ContentBlock.Ellipses.Add(TempEllipse);
                ContentSheet.Add(TempEllipse);
                TempEllipse = null;
            }
        }

        private void CancelTempEllipse()
        {
            OverlaySheet.ReleaseCapture();
            OverlaySheet.Remove(TempEllipse);
            TempEllipse = null;
        }

        #endregion

        #region Text Mode

        private TextControl CreateTextEditor(XImmutablePoint p)
        {
            var tc = new TextControl() { Width = 330.0, Background = Brushes.WhiteSmoke };
            tc.RenderTransform = null;
            Canvas.SetLeft(tc, p.X);
            Canvas.SetTop(tc, p.Y);
            return tc;
        }

        private void CreateText(XImmutablePoint p)
        {
            double x = _itemController.Snap(p.X, Options.SnapSize);
            double y = _itemController.Snap(p.Y, Options.SnapSize);
            HistoryController.Register("Create Text");

            var text = _blockFactory.CreateText("Text", x, y, 30.0, 15.0, (int)XHorizontalAlignment.Center, (int)XVerticalAlignment.Center, 11.0, ItemColors.Transparent, ItemColors.Black);
            ContentBlock.Texts.Add(text);
            ContentSheet.Add(text);
        }

        private bool TryToEditText(XImmutablePoint p)
        {
            var temp = _blockFactory.CreateBlock(-1, Options.PageOriginX, Options.PageOriginY, Options.PageWidth, Options.PageHeight, -1, "TEMP", null);
            _blockController.HitTestClick(ContentSheet, ContentBlock, temp, p, Options.HitTestSize, true, true);

            if (_blockController.HaveOneTextSelected(temp))
            {
                var tb = WpfBlockHelper.GetTextBlock(temp.Texts[0]);

                StoreTempMode();
                SetMode(SheetMode.TextEditor);

                var tc = CreateTextEditor(new XImmutablePoint((EditorSheet.Width / 2) - (330 / 2), EditorSheet.Height / 2) /* p */);

                Action<string> ok = (text) =>
                {
                    HistoryController.Register("Edit Text");
                    tb.Text = text;
                    EditorSheet.Remove(tc);
                    View.Focus();
                    RestoreTempMode();
                };

                Action cancel = () =>
                {
                    EditorSheet.Remove(tc);
                    View.Focus();
                    RestoreTempMode();
                };

                tc.Set(ok, cancel, "Edit Text", "Text:", tb.Text);
                EditorSheet.Add(tc);

                _blockController.Deselect(temp);
                return true;
            }

            _blockController.Deselect(temp);
            return false;
        }

        #endregion

        #region Image Mode

        private void Image(XImmutablePoint p)
        {
            var dlg = _serviceLocator.GetInstance<IOpenFileDialog>();
            dlg.Filter = FileDialogSettings.ImageFilter;
            dlg.FilterIndex = 1;
            dlg.FileName = "";

            if (dlg.ShowDialog() == true)
            {
                try
                {
                    InsertImage(p, dlg.FileName);
                }
                catch (Exception ex)
                {
                    Debug.Print(ex.Message);
                    Debug.Print(ex.StackTrace);
                }
            }
        }

        private void InsertImage(XImmutablePoint p, string path)
        {
            byte[] data = _base64.ReadAllBytes(path);
            double x = _itemController.Snap(p.X, Options.SnapSize);
            double y = _itemController.Snap(p.Y, Options.SnapSize);
            var image = _blockFactory.CreateImage(x, y, 120.0, 90.0, data);
            ContentBlock.Images.Add(image);
            ContentSheet.Add(image);
        }

        #endregion

        #region Edit Mode

        private bool TryToEditSelected()
        {
            if (_blockController.HaveOneLineSelected(SelectedBlock))
            {
                InitLineEditor();
                return true;
            }
            else if (_blockController.HaveOneRectangleSelected(SelectedBlock))
            {
                InitRectangleEditor();
                return true;
            }
            else if (_blockController.HaveOneEllipseSelected(SelectedBlock))
            {
                InitEllipseEditor();
                return true;
            }
            else if (_blockController.HaveOneTextSelected(SelectedBlock))
            {
                InitTextEditor();
                return true;
            }
            else if (_blockController.HaveOneImageSelected(SelectedBlock))
            {
                InitImageEditor();
                return true;
            }
            return false;
        }

        private void FinishEdit()
        {
            switch (SelectedType)
            {
                case ItemType.Line:
                    FinishLineEditor();
                    break;
                case ItemType.Rectangle:
                case ItemType.Ellipse:
                case ItemType.Text:
                case ItemType.Image:
                    FinishFrameworkElementEditor();
                    break;
            }
        }

        #endregion

        #region Edit Line

        private void DragLineStart(ILine line, IThumb thumb, double dx, double dy)
        {
            if (line != null && thumb != null)
            {
                if (line.Start != null)
                {
                    double x = _itemController.Snap(line.Start.X + dx, Options.SnapSize);
                    double y = _itemController.Snap(line.Start.Y + dy, Options.SnapSize);
                    double sdx = x - line.Start.X;
                    double sdy = y - line.Start.Y;
                    _blockController.MoveDelta(sdx, sdy, line.Start);
                    _blockHelper.SetLeft(thumb, x);
                    _blockHelper.SetTop(thumb, y);
                }
                else
                {
                    double x = _itemController.Snap(_blockHelper.GetX1(line) + dx, Options.SnapSize);
                    double y = _itemController.Snap(_blockHelper.GetY1(line) + dy, Options.SnapSize);
                    _blockHelper.SetX1(line, x);
                    _blockHelper.SetY1(line, y);
                    _blockHelper.SetLeft(thumb, x);
                    _blockHelper.SetTop(thumb, y);
                }
            }
        }

        private void DragLineEnd(ILine line, IThumb thumb, double dx, double dy)
        {
            if (line != null && thumb != null)
            {
                if (line.End != null)
                {
                    double x = _itemController.Snap(line.End.X + dx, Options.SnapSize);
                    double y = _itemController.Snap(line.End.Y + dy, Options.SnapSize);
                    double sdx = x - line.End.X;
                    double sdy = y - line.End.Y;
                    _blockController.MoveDelta(sdx, sdy, line.End);
                    _blockHelper.SetLeft(thumb, x);
                    _blockHelper.SetTop(thumb, y);
                }
                else
                {
                    double x = _itemController.Snap(_blockHelper.GetX2(line) + dx, Options.SnapSize);
                    double y = _itemController.Snap(_blockHelper.GetY2(line) + dy, Options.SnapSize);
                    _blockHelper.SetX2(line, x);
                    _blockHelper.SetY2(line, y);
                    _blockHelper.SetLeft(thumb, x);
                    _blockHelper.SetTop(thumb, y);
                }
            }
        }

        private void InitLineEditor()
        {
            StoreTempMode();
            SetMode(SheetMode.Edit);

            try
            {
                SelectedType = ItemType.Line;
                SelectedLine = SelectedBlock.Lines.FirstOrDefault();

                LineThumbStart = _blockFactory.CreateThumb(0.0, 0.0, SelectedLine, DragLineStart);
                LineThumbEnd = _blockFactory.CreateThumb(0.0, 0.0, SelectedLine, DragLineEnd);

                _blockHelper.SetLeft(LineThumbStart, _blockHelper.GetX1(SelectedLine));
                _blockHelper.SetTop(LineThumbStart, _blockHelper.GetY1(SelectedLine));
                _blockHelper.SetLeft(LineThumbEnd, _blockHelper.GetX2(SelectedLine));
                _blockHelper.SetTop(LineThumbEnd, _blockHelper.GetY2(SelectedLine));

                OverlaySheet.Add(LineThumbStart);
                OverlaySheet.Add(LineThumbEnd);
            }
            catch (Exception ex)
            {
                Debug.Print(ex.Message);
                Debug.Print(ex.StackTrace);
            }
        }

        private void FinishLineEditor()
        {
            RestoreTempMode();

            SelectedType = ItemType.None;
            SelectedLine = null;

            if (LineThumbStart != null)
            {
                OverlaySheet.Remove(LineThumbStart);
                LineThumbStart = null;
            }

            if (LineThumbEnd != null)
            {
                OverlaySheet.Remove(LineThumbEnd);
                LineThumbEnd = null;
            }
        }

        #endregion

        #region Edit FrameworkElement

        private void DragThumbs(Rect rect)
        {
            var tl = rect.TopLeft;
            var tr = rect.TopRight;
            var bl = rect.BottomLeft;
            var br = rect.BottomRight;

            _blockHelper.SetLeft(ThumbTopLeft, tl.X);
            _blockHelper.SetTop(ThumbTopLeft, tl.Y);
            _blockHelper.SetLeft(ThumbTopRight, tr.X);
            _blockHelper.SetTop(ThumbTopRight, tr.Y);
            _blockHelper.SetLeft(ThumbBottomLeft, bl.X);
            _blockHelper.SetTop(ThumbBottomLeft, bl.Y);
            _blockHelper.SetLeft(ThumbBottomRight, br.X);
            _blockHelper.SetTop(ThumbBottomRight, br.Y);
        }

        private void DragTopLeft(IElement element, IThumb thumb, double dx, double dy)
        {
            if (element != null && thumb != null)
            {
                double left = _blockHelper.GetLeft(element);
                double top = _blockHelper.GetTop(element);
                double width = _blockHelper.GetWidth(element);
                double height = _blockHelper.GetHeight(element);

                var rect = new Rect(left, top, width, height);
                rect.X = _itemController.Snap(rect.X + dx, Options.SnapSize);
                rect.Y = _itemController.Snap(rect.Y + dy, Options.SnapSize);
                rect.Width = Math.Max(0.0, rect.Width - (rect.X - left));
                rect.Height = Math.Max(0.0, rect.Height - (rect.Y - top));

                _blockHelper.SetLeft(element, rect.X);
                _blockHelper.SetTop(element, rect.Y);
                _blockHelper.SetWidth(element, rect.Width);
                _blockHelper.SetHeight(element, rect.Height);

                DragThumbs(rect);
            }
        }

        private void DragTopRight(IElement element, IThumb thumb, double dx, double dy)
        {
            if (element != null && thumb != null)
            {
                double left = _blockHelper.GetLeft(element);
                double top = _blockHelper.GetTop(element);
                double width = _blockHelper.GetWidth(element);
                double height = _blockHelper.GetHeight(element);

                var rect = new Rect(left, top, width, height);
                rect.Width = Math.Max(0.0, _itemController.Snap(rect.Width + dx, Options.SnapSize));
                rect.Y = _itemController.Snap(rect.Y + dy, Options.SnapSize);
                rect.Height = Math.Max(0.0, rect.Height - (rect.Y - top));

                _blockHelper.SetLeft(element, rect.X);
                _blockHelper.SetTop(element, rect.Y);
                _blockHelper.SetWidth(element, rect.Width);
                _blockHelper.SetHeight(element, rect.Height);

                DragThumbs(rect);
            }
        }

        private void DragBottomLeft(IElement element, IThumb thumb, double dx, double dy)
        {
            if (element != null && thumb != null)
            {
                double left = _blockHelper.GetLeft(element);
                double top = _blockHelper.GetTop(element);
                double width = _blockHelper.GetWidth(element);
                double height = _blockHelper.GetHeight(element);

                var rect = new Rect(left, top, width, height);
                rect.X = _itemController.Snap(rect.X + dx, Options.SnapSize);
                rect.Height = Math.Max(0.0, _itemController.Snap(rect.Height + dy, Options.SnapSize));
                rect.Width = Math.Max(0.0, rect.Width - (rect.X - left));

                _blockHelper.SetLeft(element, rect.X);
                _blockHelper.SetTop(element, rect.Y);
                _blockHelper.SetWidth(element, rect.Width);
                _blockHelper.SetHeight(element, rect.Height);

                DragThumbs(rect);
            }
        }

        private void DragBottomRight(IElement element, IThumb thumb, double dx, double dy)
        {
            if (element != null && thumb != null)
            {
                double left = _blockHelper.GetLeft(element);
                double top = _blockHelper.GetTop(element);
                double width = _blockHelper.GetWidth(element);
                double height = _blockHelper.GetHeight(element);

                var rect = new Rect(left, top, width, height);
                rect.Width = Math.Max(0.0, _itemController.Snap(rect.Width + dx, Options.SnapSize));
                rect.Height = Math.Max(0.0, _itemController.Snap(rect.Height + dy, Options.SnapSize));

                _blockHelper.SetLeft(element, rect.X);
                _blockHelper.SetTop(element, rect.Y);
                _blockHelper.SetWidth(element, rect.Width);
                _blockHelper.SetHeight(element, rect.Height);

                DragThumbs(rect);
            }
        }

        private void InitFrameworkElementEditor()
        {
            double left = _blockHelper.GetLeft(SelectedElement);
            double top = _blockHelper.GetTop(SelectedElement);
            double width = _blockHelper.GetWidth(SelectedElement);
            double height = _blockHelper.GetHeight(SelectedElement);

            ThumbTopLeft = _blockFactory.CreateThumb(0.0, 0.0, SelectedElement, DragTopLeft);
            ThumbTopRight = _blockFactory.CreateThumb(0.0, 0.0, SelectedElement, DragTopRight);
            ThumbBottomLeft = _blockFactory.CreateThumb(0.0, 0.0, SelectedElement, DragBottomLeft);
            ThumbBottomRight = _blockFactory.CreateThumb(0.0, 0.0, SelectedElement, DragBottomRight);

            _blockHelper.SetLeft(ThumbTopLeft, left);
            _blockHelper.SetTop(ThumbTopLeft, top);
            _blockHelper.SetLeft(ThumbTopRight, left + width);
            _blockHelper.SetTop(ThumbTopRight, top);
            _blockHelper.SetLeft(ThumbBottomLeft, left);
            _blockHelper.SetTop(ThumbBottomLeft, top + height);
            _blockHelper.SetLeft(ThumbBottomRight, left + width);
            _blockHelper.SetTop(ThumbBottomRight, top + height);

            OverlaySheet.Add(ThumbTopLeft);
            OverlaySheet.Add(ThumbTopRight);
            OverlaySheet.Add(ThumbBottomLeft);
            OverlaySheet.Add(ThumbBottomRight);
        }

        private void FinishFrameworkElementEditor()
        {
            RestoreTempMode();

            SelectedType = ItemType.None;
            SelectedElement = null;

            if (ThumbTopLeft != null)
            {
                OverlaySheet.Remove(ThumbTopLeft);
                ThumbTopLeft = null;
            }

            if (ThumbTopRight != null)
            {
                OverlaySheet.Remove(ThumbTopRight);
                ThumbTopRight = null;
            }

            if (ThumbBottomLeft != null)
            {
                OverlaySheet.Remove(ThumbBottomLeft);
                ThumbBottomLeft = null;
            }

            if (ThumbBottomRight != null)
            {
                OverlaySheet.Remove(ThumbBottomRight);
                ThumbBottomRight = null;
            }
        }

        #endregion

        #region Edit Rectangle

        private void InitRectangleEditor()
        {
            StoreTempMode();
            SetMode(SheetMode.Edit);

            try
            {
                var rectangle = SelectedBlock.Rectangles.FirstOrDefault();
                SelectedType = ItemType.Rectangle;
                SelectedElement = rectangle;
                InitFrameworkElementEditor();
            }
            catch (Exception ex)
            {
                Debug.Print(ex.Message);
                Debug.Print(ex.StackTrace);
            }
        }

        #endregion

        #region Edit Ellipse

        private void InitEllipseEditor()
        {
            StoreTempMode();
            SetMode(SheetMode.Edit);

            try
            {
                var ellipse = SelectedBlock.Ellipses.FirstOrDefault();
                SelectedType = ItemType.Ellipse;
                SelectedElement = ellipse;
                InitFrameworkElementEditor();
            }
            catch (Exception ex)
            {
                Debug.Print(ex.Message);
                Debug.Print(ex.StackTrace);
            }
        }

        #endregion

        #region Edit Text

        private void InitTextEditor()
        {
            StoreTempMode();
            SetMode(SheetMode.Edit);

            try
            {
                var text = SelectedBlock.Texts.FirstOrDefault();
                SelectedType = ItemType.Text;
                SelectedElement = text;
                InitFrameworkElementEditor();
            }
            catch (Exception ex)
            {
                Debug.Print(ex.Message);
                Debug.Print(ex.StackTrace);
            }
        }

        #endregion

        #region Edit Image

        private void InitImageEditor()
        {
            StoreTempMode();
            SetMode(SheetMode.Edit);

            try
            {
                var image = SelectedBlock.Images.FirstOrDefault();
                SelectedType = ItemType.Image;
                SelectedElement = image;
                InitFrameworkElementEditor();
            }
            catch (Exception ex)
            {
                Debug.Print(ex.Message);
                Debug.Print(ex.StackTrace);
            }
        }

        #endregion

        #region Data Binding

        public bool BindDataToBlock(XImmutablePoint p, DataItem dataItem)
        {
            var temp = _blockFactory.CreateBlock(-1, Options.PageOriginX, Options.PageOriginY, Options.PageWidth, Options.PageHeight, -1, "TEMP", null);
            _blockController.HitTestForBlocks(ContentSheet, ContentBlock, temp, p, Options.HitTestSize);

            if (_blockController.HaveOneBlockSelected(temp))
            {
                HistoryController.Register("Bind Data");
                var block = temp.Blocks[0];
                var result = BindDataToBlock(block, dataItem);
                _blockController.Deselect(temp);

                if (result == true)
                {
                    _blockController.Select(block);
                    SelectedBlock.Blocks = new List<IBlock>();
                    SelectedBlock.Blocks.Add(block);
                }

                return true;
            }

            _blockController.Deselect(temp);
            return false;
        }

        public bool BindDataToBlock(IBlock block, DataItem dataItem)
        {
            if (block != null && block.Texts != null
                && dataItem != null && dataItem.Columns != null && dataItem.Data != null
                && block.Texts.Count == dataItem.Columns.Length - 1)
            {
                // assign block data id
                block.DataId = int.Parse(dataItem.Data[0]);

                // skip index column
                int i = 1;

                foreach (var text in block.Texts)
                {
                    var tb = WpfBlockHelper.GetTextBlock(text);
                    tb.Text = dataItem.Data[i];
                    i++;
                }

                return true;
            }

            return false;
        }

        public void TryToBindData(XImmutablePoint p, DataItem dataItem)
        {
            // first try binding to existing block
            bool firstTryResult = BindDataToBlock(p, dataItem);

            // if failed insert selected block from library and try again to bind
            if (!firstTryResult)
            {
                var blockItem = LibraryController.GetSelected();
                if (blockItem != null)
                {
                    var block = Insert(blockItem, p, false);
                    bool secondTryResult = BindDataToBlock(block, dataItem);
                    if (!secondTryResult)
                    {
                        // remove block if failed to bind
                        var temp = _blockFactory.CreateBlock(-1, Options.PageOriginX, Options.PageOriginY, Options.PageWidth, Options.PageHeight, -1, "TEMP", null);
                        temp.Init();
                        temp.Blocks.Add(block);
                        _blockController.RemoveSelected(ContentSheet, ContentBlock, temp);
                    }
                    else
                    {
                        _blockController.Select(block);
                        SelectedBlock.Blocks = new List<IBlock>();
                        SelectedBlock.Blocks.Add(block);
                    }
                }
            }
        }

        #endregion

        #region New Page

        public void NewPage()
        {
            HistoryController.Register("New");
            ResetPage();
            CreatePage();
            ZoomController.AutoFit();
        }

        #endregion

        #region Open Page

        public async Task OpenTextPage(string path)
        {
            var text = await _itemController.OpenText(path);
            if (text != null)
            {
                var page = await Task.Run(() => _itemSerializer.DeserializeContents(text));
                HistoryController.Register("Open Text");
                ResetPage();
                DeserializePage(page);
            }
        }

        public async Task OpenJsonPage(string path)
        {
            var text = await _itemController.OpenText(path);
            if (text != null)
            {
                var page = await Task.Run(() => _jsonSerializer.Deerialize<BlockItem>(text));
                HistoryController.Register("Open Json");
                ResetPage();
                DeserializePage(page);
            }
        }

        public async void OpenPage()
        {
            var dlg = _serviceLocator.GetInstance<IOpenFileDialog>();
            dlg.Filter = FileDialogSettings.PageFilter;
            dlg.FilterIndex = 1;
            dlg.FileName = "";

            if (dlg.ShowDialog() == true)
            {
                string path = dlg.FileName;
                switch (dlg.FilterIndex)
                {
                    case 1:
                        {
                            try
                            {
                                await OpenTextPage(path);
                            }
                            catch (Exception ex)
                            {
                                Debug.Print(ex.Message);
                                Debug.Print(ex.StackTrace);
                            }
                        }
                        break;
                    case 2:
                    case 3:
                        {
                            try
                            {
                                await OpenJsonPage(path);
                            }
                            catch (Exception ex)
                            {
                                Debug.Print(ex.Message);
                                Debug.Print(ex.StackTrace);
                            }
                        }
                        break;
                }
            }
        }

        #endregion

        #region Save Page

        public void SaveTextPage(string path)
        {
            var page = SerializePage();

            Task.Run(() =>
            {
                var text = _itemSerializer.SerializeContents(page);
                _itemController.SaveText(path, text);
            });
        }

        public void SaveJsonPage(string path)
        {
            var page = SerializePage();

            Task.Run(() =>
            {
                string text = _jsonSerializer.Serialize(page);
                _itemController.SaveText(path, text);
            });
        }

        public void SavePage()
        {
            var dlg = _serviceLocator.GetInstance<ISaveFileDialog>();
            dlg.Filter = FileDialogSettings.PageFilter;
            dlg.FilterIndex = 1;
            dlg.FileName = "sheet";
            
            if (dlg.ShowDialog() == true)
            {
                string path = dlg.FileName;
                switch (dlg.FilterIndex)
                {
                    case 1:
                        {
                            try
                            {
                                SaveTextPage(path);
                            }
                            catch (Exception ex)
                            {
                                Debug.Print(ex.Message);
                                Debug.Print(ex.StackTrace);
                            }
                        }
                        break;
                    case 2:
                    case 3:
                        {
                            try
                            {
                                SaveJsonPage(path);
                            }
                            catch (Exception ex)
                            {
                                Debug.Print(ex.Message);
                                Debug.Print(ex.StackTrace);
                            }
                        }
                        break;
                }
            }
        }

        #endregion

        #region Export Page

        private void ExportToPdf(IEnumerable<BlockItem> blocks, string fileName)
        {
            var pages = blocks.Select(content => CreatePage(content, true, false)).ToList();

            Task.Run(() =>
            {
                var writer = new BlockPdfWriter();
                writer.Create(fileName, Options.PageWidth, Options.PageHeight, pages);
                Process.Start(fileName);
            });
        }

        private void ExportToDxf(IEnumerable<BlockItem> blocks, string fileName)
        {
            var pages = blocks.Select(block => CreatePage(block, true, false)).ToList();

            Task.Run(() =>
            {
                var writer = new BlockDxfWriter();

                if (blocks.Count() > 1)
                {
                    string path = System.IO.Path.GetDirectoryName(fileName);
                    string name = System.IO.Path.GetFileNameWithoutExtension(fileName);
                    string extension = System.IO.Path.GetExtension(fileName);

                    int counter = 0;
                    foreach (var page in pages)
                    {
                        string fileNameWithCounter = System.IO.Path.Combine(path, string.Concat(name, '-', counter.ToString("000"), extension));
                        writer.Create(fileNameWithCounter, Options.PageWidth, Options.PageHeight, page);
                        counter++;
                    }
                }
                else
                {
                    var page = pages.FirstOrDefault();
                    if (page != null)
                    {
                        writer.Create(fileName, Options.PageWidth, Options.PageHeight, page);
                    }
                }
            });
        }

        public void Export(IEnumerable<BlockItem> blocks)
        {
            var dlg = _serviceLocator.GetInstance<ISaveFileDialog>();
            dlg.Filter = FileDialogSettings.ExportFilter;
            dlg.FilterIndex = 1;
            dlg.FileName = "sheet";

            if (dlg.ShowDialog() == true)
            {
                string path = dlg.FileName;
                switch (dlg.FilterIndex)
                {
                    case 1:
                    case 3:
                    default:
                        {
                            try
                            {
                                ExportToPdf(blocks, path);
                            }
                            catch (Exception ex)
                            {
                                Debug.Print(ex.Message);
                                Debug.Print(ex.StackTrace);
                            }
                        }
                        break;
                    case 2:
                        {
                            try
                            {
                                ExportToDxf(blocks, path);
                            }
                            catch (Exception ex)
                            {
                                Debug.Print(ex.Message);
                                Debug.Print(ex.StackTrace);
                            }
                        }
                        break;
                }
            }
        }

        public void Export(SolutionEntry solution)
        {
            var texts = solution.Documents.SelectMany(document => document.Pages).Select(page => page.Content);
            ExportPages(texts);
        }

        public void ExportPage()
        {
            var block = _blockSerializer.SerializerContents(ContentBlock, -1, ContentBlock.X, ContentBlock.Y, ContentBlock.Width, ContentBlock.Height, ContentBlock.DataId, "CONTENT");
            var blocks = ToSingle(block);
            Export(blocks);
        }

        #endregion

        #region Library

        public void Insert(XImmutablePoint p)
        {
            if (LibraryController != null)
            {
                var blockItem = LibraryController.GetSelected() as BlockItem;
                Insert(blockItem, p, true);
            }
        }

        public IBlock Insert(BlockItem blockItem, XImmutablePoint p, bool select)
        {
            _blockController.DeselectContent(SelectedBlock);
            double thickness = Options.LineThickness / ZoomController.Zoom;

            HistoryController.Register("Insert Block");

            var block = _blockSerializer.Deserialize(ContentSheet, ContentBlock, blockItem, thickness);

            if (select)
            {
                _blockController.Select(block);
                SelectedBlock.Blocks = new List<IBlock>();
                SelectedBlock.Blocks.Add(block);
            }

            _blockController.MoveDelta(_itemController.Snap(p.X, Options.SnapSize), _itemController.Snap(p.Y, Options.SnapSize), block);

            return block;
        }

        private async void LoadLibraryFromResource(string name)
        {
            var assembly = System.Reflection.Assembly.GetExecutingAssembly();
            if (assembly == null)
            {
                return;
            }

            using (var stream = assembly.GetManifestResourceStream(name))
            {
                using (var reader = new System.IO.StreamReader(stream))
                {
                    string text = await reader.ReadToEndAsync();
                    if (text != null)
                    {
                        InitLibrary(text);
                    }
                }
            }
        }

        public async Task LoadLibrary(string path)
        {
            var text = await _itemController.OpenText(path);
            if (text != null)
            {
                InitLibrary(text);
            }
        }

        private async void InitLibrary(string text)
        {
            if (LibraryController != null && text != null)
            {
                var block = await Task.Run(() => _itemSerializer.DeserializeContents(text));
                LibraryController.SetSource(block.Blocks);
            }
        }

        private void AddToLibrary(BlockItem blockItem)
        {
            if (LibraryController != null && blockItem != null)
            {
                var source = LibraryController.GetSource();
                var items = new List<BlockItem>(source);
                _itemController.ResetPosition(blockItem, Options.PageOriginX, Options.PageOriginY, Options.PageWidth, Options.PageHeight);
                items.Add(blockItem);
                LibraryController.SetSource(items);
            }
        }

        public async void LoadLibrary()
        {
            var dlg = _serviceLocator.GetInstance<IOpenFileDialog>();
            dlg.Filter = FileDialogSettings.LibraryFilter;
            dlg.FilterIndex = 1;
            dlg.FileName = "";
            
            if (dlg.ShowDialog() == true)
            {
                try
                {
                    await LoadLibrary(dlg.FileName);
                }
                catch (Exception ex)
                {
                    Debug.Print(ex.Message);
                    Debug.Print(ex.StackTrace);
                }
            }
        }

        #endregion

        #region Input

        public void LeftDown(InputArgs args)
        {
            // edit mode
            if (SelectedType != ItemType.None)
            {
                if (args.SourceType != ItemType.Thumb)
                {
                    _blockController.DeselectContent(SelectedBlock);
                    FinishEdit();
                }
                else
                {
                    return;
                }
            }

            // text editor
            if (GetMode() == SheetMode.None || GetMode() == SheetMode.TextEditor)
            {
                return;
            }

            // move mode
            if (!args.OnlyControl)
            {
                if (_blockController.HaveSelected(SelectedBlock) && CanInitMove(args.SheetPosition))
                {
                    InitMove(args.SheetPosition);
                    return;
                }

                _blockController.DeselectContent(SelectedBlock);
            }

            bool resetSelected = args.OnlyControl && _blockController.HaveSelected(SelectedBlock) ? false : true;

            if (GetMode() == SheetMode.Selection)
            {
                bool result = _blockController.HitTestClick(ContentSheet, ContentBlock, SelectedBlock, new XImmutablePoint(args.SheetPosition.X, args.SheetPosition.Y), Options.HitTestSize, false, resetSelected);
                if ((args.OnlyControl || !_blockController.HaveSelected(SelectedBlock)) && !result)
                {
                    InitSelectionRect(args.SheetPosition);
                }
                else
                {
                    // TODO: If control key is pressed then switch to move mode instead to edit mode
                    bool editModeEnabled = args.OnlyControl == true ? false : TryToEditSelected();
                    if (!editModeEnabled)
                    {
                        InitMove(args.SheetPosition);
                    }
                }
            }
            else if (GetMode() == SheetMode.Insert && !OverlaySheet.IsCaptured)
            {
                Insert(args.SheetPosition);
            }
            else if (GetMode() == SheetMode.Point && !OverlaySheet.IsCaptured)
            {
                InsertPoint(args.SheetPosition, true, true);
            }
            else if (GetMode() == SheetMode.Line && !OverlaySheet.IsCaptured)
            {
                // try to find point to connect line start
                IPoint start = TryToFindPoint(args.SheetPosition);

                // create start if Control key is pressed and start point has not been found
                if (args.OnlyControl && start == null)
                {
                    start = InsertPoint(args.SheetPosition, true, false);
                }

                InitTempLine(args.SheetPosition, start);
            }
            else if (GetMode() == SheetMode.Line && OverlaySheet.IsCaptured)
            {
                // try to find point to connect line end
                IPoint end = TryToFindPoint(args.SheetPosition);

                // create end point if Control key is pressed and end point has not been found
                if (args.OnlyControl && end == null)
                {
                    end = InsertPoint(args.SheetPosition, true, false);
                }

                FinishTempLine(end);
            }
            else if (GetMode() == SheetMode.Rectangle && !OverlaySheet.IsCaptured)
            {
                InitTempRect(args.SheetPosition);
            }
            else if (GetMode() == SheetMode.Rectangle && OverlaySheet.IsCaptured)
            {
                FinishTempRect();
            }
            else if (GetMode() == SheetMode.Ellipse && !OverlaySheet.IsCaptured)
            {
                InitTempEllipse(args.SheetPosition);
            }
            else if (GetMode() == SheetMode.Ellipse && OverlaySheet.IsCaptured)
            {
                FinishTempEllipse();
            }
            else if (GetMode() == SheetMode.Pan && OverlaySheet.IsCaptured)
            {
                FinishPan();
            }
            else if (GetMode() == SheetMode.Text && !OverlaySheet.IsCaptured)
            {
                CreateText(args.SheetPosition);
            }
            else if (GetMode() == SheetMode.Image && !OverlaySheet.IsCaptured)
            {
                Image(args.SheetPosition);
            }
        }

        public void LeftUp(InputArgs args)
        {
            if (GetMode() == SheetMode.Selection && OverlaySheet.IsCaptured)
            {
                FinishSelectionRect();
            }
            else if (GetMode() == SheetMode.Move && OverlaySheet.IsCaptured)
            {
                FinishMove();
            }
        }

        public void Move(InputArgs args)
        {
            if (GetMode() == SheetMode.Edit)
            {
                return;
            }

            // mouse over selection when holding Shift key
            if (args.OnlyShift && TempSelectionRect == null && !OverlaySheet.IsCaptured)
            {
                if (_blockController.HaveSelected(SelectedBlock))
                {
                    _blockController.DeselectContent(SelectedBlock);
                }

                _blockController.HitTestClick(ContentSheet, ContentBlock, SelectedBlock, args.SheetPosition, Options.HitTestSize, false, false);
            }

            if (GetMode() == SheetMode.Selection && OverlaySheet.IsCaptured)
            {
                MoveSelectionRect(args.SheetPosition);
            }
            else if (GetMode() == SheetMode.Line && OverlaySheet.IsCaptured)
            {
                MoveTempLine(args.SheetPosition);
            }
            else if (GetMode() == SheetMode.Rectangle && OverlaySheet.IsCaptured)
            {
                MoveTempRect(args.SheetPosition);
            }
            else if (GetMode() == SheetMode.Ellipse && OverlaySheet.IsCaptured)
            {
                MoveTempEllipse(args.SheetPosition);
            }
            else if (GetMode() == SheetMode.Pan && OverlaySheet.IsCaptured)
            {
                Pan(args.RootPosition);
            }
            else if (GetMode() == SheetMode.Move && OverlaySheet.IsCaptured)
            {
                Move(args.SheetPosition);
            }
        }

        public void RightDown(InputArgs args)
        {
            if (GetMode() == SheetMode.None || GetMode() == SheetMode.TextEditor)
            {
                return;
            }

            // edit mode
            if (SelectedType != ItemType.None)
            {
                _blockController.DeselectContent(SelectedBlock);
                FinishEdit();
                return;
            }

            // text editor
            if (GetMode() == SheetMode.Text && TryToEditText(args.SheetPosition))
            {
                args.Handled(true);
                return;
            }
            else
            {
                _blockController.DeselectContent(SelectedBlock);

                if (GetMode() == SheetMode.Selection && OverlaySheet.IsCaptured)
                {
                    CancelSelectionRect();
                }
                else if (GetMode() == SheetMode.Line && OverlaySheet.IsCaptured)
                {
                    CancelTempLine();
                }
                else if (GetMode() == SheetMode.Rectangle && OverlaySheet.IsCaptured)
                {
                    CancelTempRect();
                }
                else if (GetMode() == SheetMode.Ellipse && OverlaySheet.IsCaptured)
                {
                    CancelTempEllipse();
                }
                else if (!OverlaySheet.IsCaptured)
                {
                    InitPan(args.RootPosition);
                }
            }
        }

        public void RightUp(InputArgs args)
        {
            if (GetMode() == SheetMode.Pan && OverlaySheet.IsCaptured)
            {
                FinishPan();
            }
        }

        public void Wheel(int delta, XImmutablePoint position)
        {
            ZoomTo(delta, position);
        }

        public void Down(InputArgs args)
        {
            if (args.Button == InputButton.Middle && args.Clicks == 2)
            {
                // Mouse Middle Double-Click + Control key pressed to reset Pan and Zoom
                // Mouse Middle Double-Click to Auto Fit page to window size
                if (args.OnlyControl)
                {
                    ZoomController.ActualSize();
                }
                else
                {
                    ZoomController.AutoFit();
                }
            }
        }

        #endregion

        #region Page Frame & Grid

        private void CreatePage()
        {
            _pageFactory.CreateGrid(BackSheet, GridBlock, 330.0, 30.0, 600.0, 750.0, Options.GridSize, Options.GridThickness, ItemColors.LightGray);
            _pageFactory.CreateFrame(BackSheet, FrameBlock, Options.GridSize, Options.GridThickness, ItemColors.DarkGray);

            AdjustThickness(GridBlock, Options.GridThickness / GetZoom(ZoomController.ZoomIndex));
            AdjustThickness(FrameBlock, Options.FrameThickness / GetZoom(ZoomController.ZoomIndex));
        }

        private BlockItem CreateGridBlock(IBlock gridBlock, bool adjustThickness, bool adjustColor)
        {
            var grid = _blockSerializer.SerializerContents(gridBlock, -1, 0.0, 0.0, 0.0, 0.0, -1, "GRID");

            // lines
            foreach (var lineItem in grid.Lines)
            {
                if (adjustThickness)
                {
                    lineItem.StrokeThickness = 0.013 * 72.0 / 2.54; // 0.13mm 
                }

                if (adjustColor)
                {
                    lineItem.Stroke = ItemColors.Black;
                }
            }

            return grid;
        }

        private BlockItem CreateFrameBlock(IBlock frameBlock, bool adjustThickness, bool adjustColor)
        {
            var frame = _blockSerializer.SerializerContents(frameBlock, -1, 0.0, 0.0, 0.0, 0.0, -1, "FRAME");

            // texts
            foreach (var textItem in frame.Texts)
            {
                if (adjustColor)
                {
                    textItem.Foreground = ItemColors.Black;
                }
            }

            // lines
            foreach (var lineItem in frame.Lines)
            {
                if (adjustThickness)
                {
                    lineItem.StrokeThickness = 0.018 * 72.0 / 2.54; // 0.18mm 
                }

                if (adjustColor)
                {
                    lineItem.Stroke = ItemColors.Black;
                }
            }

            return frame;
        }

        private BlockItem CreatePage(BlockItem content, bool enableFrame, bool enableGrid)
        {
            var page = new BlockItem();
            page.Init(-1, 0.0, 0.0, 0.0, 0.0, -1, "PAGE");

            if (enableGrid)
            {
                var grid = CreateGridBlock(GridBlock, true, false);
                page.Blocks.Add(grid);
            }

            if (enableFrame)
            {
                var frame = CreateFrameBlock(FrameBlock, true, true);
                page.Blocks.Add(frame);
            }

            page.Blocks.Add(content);

            return page;
        }

        #endregion

        #region Plugins

        private void CreatePlugins()
        {
            InvertLineStartPlugin = new InvertLineStartPlugin(_serviceLocator);
            InvertLineEndPlugin = new InvertLineEndPlugin(_serviceLocator);
        }

        private void ProcessPlugin(ISelectedBlockPlugin plugin)
        {
            if (plugin.CanProcess(ContentSheet, ContentBlock, SelectedBlock, Options))
            {
                var selectedBlock = _blockController.ShallowCopy(SelectedBlock);

                FinishEdit();
                HistoryController.Register(plugin.Name);

                plugin.Process(ContentSheet, ContentBlock, selectedBlock, Options);

                SelectedBlock = selectedBlock;
            }
        }

        public void InvertSelectedLineStart()
        {
            ProcessPlugin(InvertLineStartPlugin);
        }

        public void InvertSelectedLineEnd()
        {
            ProcessPlugin(InvertLineEndPlugin);
        }

        #endregion
    }

    #endregion
}
