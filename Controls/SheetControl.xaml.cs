using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

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

    #region InputButton

    //public enum InputButton
    //{
    //    None,
    //    Left,
    //    Right,
    //    Middle
    //} 

    #endregion

    #region InputArgs

    //public class InputArgs
    //{
    //    // Mouse Generic
    //    public bool OnlyControl { get; set; }
    //    public bool OnlyShift { get; set; }
    //    public ItemType SourceType { get; set; }
    //    public XImmutablePoint Position { get; set; }
    //    // Mouse Wheel
    //    public int Delta { get; set; }
    //    // Mouse Down
    //    public InputButton Button { get; set; }
    //    public int Clicks { get; set; }
    //}

    #endregion
    
    #region ISheetController

    public interface ISheetController
    {

    }

    #endregion

    #region SheetController

    public class SheetController : ISheetController
    {

    } 

    #endregion

    public partial class SheetControl : UserControl, IPageController, IPanAndZoomController
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

        public SheetControl(IServiceLocator serviceLocator)
        {
            InitializeComponent();

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

            // history
            History = new PageHistoryController(this, this._itemSerializer);

            // pan & zoom
            PanAndZoom = this;

            // plugins
            CreatePlugins(serviceLocator);

            Init();

            Loaded += (s, e) => InitLoaded();
        }

        #endregion

        #region Properties

        public SheetOptions Options { get; set; }
        public SheetMode Mode { get; set; }
        public SheetMode TempMode { get; set; }
        public ISheet EditorSheet { get; set; }
        public ISheet BackSheet { get; set; }
        public ISheet ContentSheet { get; set; }
        public ISheet OverlaySheet { get; set; }
        public IBlock SelectedBlock { get; set; }
        public IBlock ContentBlock { get; set; }
        public IBlock FrameBlock { get; set; }
        public IBlock GridBlock { get; set; }
        public ILine TempLine { get; set; }
        public IEllipse TempStartEllipse { get; set; }
        public IEllipse TempEndEllipse { get; set; }
        public IRectangle TempRectangle { get; set; }
        public IEllipse TempEllipse { get; set; }
        public IRectangle TempSelectionRect { get; set; }
        public bool IsFirstMove { get; set; }
        public XImmutablePoint PanStartPoint { get; set; }
        public XImmutablePoint SelectionStartPoint { get; set; }
        public double LastFinalWidth { get; set; }
        public double LastFinalHeight { get; set; }

        #endregion

        #region Fields

        private ItemType SelectedType = ItemType.None;
        private ILine SelectedLine = null;
        private IThumb LineThumbStart = null;
        private IThumb LineThumbEnd = null;
        private IElement SelectedElement = null;
        private IThumb ThumbTopLeft = null;
        private IThumb ThumbTopRight = null;
        private IThumb ThumbBottomLeft = null;
        private IThumb ThumbBottomRight = null;

        #endregion

        #region Default Options

        private static SheetOptions DefaultOptions()
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

        #region ToSingle

        public static IEnumerable<T> ToSingle<T>(T item)
        {
            yield return item;
        } 

        #endregion

        #region Init

        private void Init()
        {
            InitDefaults();
            InitSheets();
            InitBlocks();
        }

        private void InitDefaults()
        {
            Mode = SheetMode.Selection;
            TempMode = SheetMode.None;
            IsFirstMove = true;
            Options = DefaultOptions();
            zoomIndex = Options.DefaultZoomIndex;
        }

        private void InitSheets()
        {
            EditorSheet = new WpfCanvasSheet(EditorCanvas);
            BackSheet = new WpfCanvasSheet(Root.Back);
            ContentSheet = new WpfCanvasSheet(Root.Sheet);
            OverlaySheet = new WpfCanvasSheet(Root.Overlay);
        }

        private void InitBlocks()
        {
            ContentBlock = _blockFactory.CreateBlock(-1, Options.PageOriginX, Options.PageOriginY, Options.PageWidth, Options.PageHeight, -1, "CONTENT", null);
            ContentBlock.Init();

            FrameBlock = _blockFactory.CreateBlock(-1, Options.PageOriginX, Options.PageOriginY, Options.PageWidth, Options.PageHeight, -1, "FRAME", null);
            FrameBlock.Init();

            GridBlock = _blockFactory.CreateBlock(-1, Options.PageOriginX, Options.PageOriginY, Options.PageWidth, Options.PageHeight, -1, "GRID", null);
            GridBlock.Init();

            SelectedBlock = _blockFactory.CreateBlock(-1, Options.PageOriginX, Options.PageOriginY, Options.PageWidth, Options.PageHeight, -1, "SELECTED", null);
        }

        private void InitLoaded()
        {
            LoadStandardPage();

            LoadLibraryFromResource(string.Concat("Sheet.Libraries", '.', "Digital.library"));

            Focus();
        }

        #endregion

        #region IPageController

        public IHistoryController History { get; set; }
        public ILibraryController Library { get; set; }
        public IPanAndZoomController PanAndZoom { get; set; }

        public async void SetPage(string text)
        {
            try
            {
                if (text == null)
                {
                    History.Reset();
                    ResetPage();
                }
                else
                {
                    var block = await Task.Run(() => _itemSerializer.DeserializeContents(text));
                    History.Reset();
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
                _blockController.AddContents(BackSheet, grid, GridBlock, null, false, Options.GridThickness / Zoom);
            }

            if (frame != null)
            {
                _blockController.AddContents(BackSheet, frame, FrameBlock, null, false, Options.FrameThickness / Zoom);
            }

            if (content != null)
            {
                _blockController.AddContents(ContentSheet, content, ContentBlock, null, false, Options.LineThickness / Zoom);
            }
        }

        public void ResetPage()
        {
            ResetOverlay();

            _blockController.Remove(BackSheet, GridBlock);
            _blockController.Remove(BackSheet, FrameBlock);
            _blockController.Remove(ContentSheet, ContentBlock);

            InitBlocks();
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

        public void SetMode(SheetMode m)
        {
            Mode = m;
        }

        private void StoreTempMode()
        {
            TempMode = GetMode();
        }

        private void RestoreTempMode()
        {
            SetMode(TempMode);
        }

        public void ModeNone()
        {
            SetMode(SheetMode.None);
        }

        public void ModeSelection()
        {
            SetMode(SheetMode.Selection);
        }

        public void ModeInsert()
        {
            SetMode(SheetMode.Insert);
        }

        public void ModePan()
        {
            SetMode(SheetMode.Pan);
        }

        public void ModeMove()
        {
            SetMode(SheetMode.Move);
        }

        public void ModeEdit()
        {
            SetMode(SheetMode.Edit);
        }

         public void ModePoint()
        {
            SetMode(SheetMode.Point);
        }
        
        public void ModeLine()
        {
            SetMode(SheetMode.Line);
        }

        public void ModeRectangle()
        {
            SetMode(SheetMode.Rectangle);
        }

        public void ModeEllipse()
        {
            SetMode(SheetMode.Ellipse);
        }

        public void ModeText()
        {
            SetMode(SheetMode.Text);
        }

        public void ModeImage()
        {
            SetMode(SheetMode.Image);
        }

        public void ModeTextEditor()
        {
            SetMode(SheetMode.TextEditor);
        }

        #endregion

        #region Clipboard Text

        public void CutAsText()
        {
            try
            {
                if (_blockController.HaveSelected(SelectedBlock))
                {
                    var copy = _blockController.ShallowCopy(SelectedBlock);
                    History.Register("Cut");
                    CopyAsText(copy);
                    Delete(copy);
                }
            }
            catch (Exception ex)
            {
                Debug.Print(ex.Message);
                Debug.Print(ex.StackTrace);
            }
        }

        private void CopyAsText(IBlock block)
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

        public void CopyAsText()
        {
            if (_blockController.HaveSelected(SelectedBlock))
            {
                CopyAsText(SelectedBlock);
            }
        }

        public async void PasteText()
        {
            try
            {
                var text = _clipboard.Get();
                var block = await Task.Run(() => _itemSerializer.DeserializeContents(text));
                History.Register("Paste");
                InsertContent(block, true);
            }
            catch (Exception ex)
            {
                Debug.Print(ex.Message);
                Debug.Print(ex.StackTrace);
            }
        }

        #endregion

        #region Clipboard Json

        public void CutAsJson()
        {
            try
            {
                if (_blockController.HaveSelected(SelectedBlock))
                {
                    var copy = _blockController.ShallowCopy(SelectedBlock);
                    History.Register("Cut");
                    CopyAsJson(copy);
                    Delete(copy);
                }
            }
            catch (Exception ex)
            {
                Debug.Print(ex.Message);
                Debug.Print(ex.StackTrace);
            }
        }

        private void CopyAsJson(IBlock block)
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

        public void CopyAsJson()
        {
            if (_blockController.HaveSelected(SelectedBlock))
            {
                CopyAsJson(SelectedBlock);
            }
        }

        public async void PasteJson()
        {
            try
            {
                var text = _clipboard.Get();
                var block = await Task.Run(() => _jsonSerializer.Deerialize<BlockItem>(text));
                History.Register("Paste");
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
                History.Register("Delete");
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
            _blockController.AddContents(ContentSheet, block, ContentBlock, SelectedBlock, select, Options.LineThickness / Zoom);
        }

        public BlockItem CreateBlock(string name, IBlock block)
        {
            try
            {
                var blockItem = _blockSerializer.Serialize(block);
                blockItem.Name = name;
                return blockItem;
            }
            catch(Exception ex)
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
                ModeTextEditor();

                var tc = CreateTextEditor(new XImmutablePoint((EditorSheet.Width / 2) - (330 / 2), EditorSheet.Height / 2));

                Action<string> ok = (name) =>
                {
                    var block = CreateBlock(name, SelectedBlock);
                    if (block != null)
                    {
                        AddToLibrary(block);
                    }
                    EditorSheet.Remove(tc);
                    Focus();
                    RestoreTempMode();
                };

                Action cancel = () =>
                {
                    EditorSheet.Remove(tc);
                    Focus();
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
                History.Register("Break Block");
                Delete();
                _blockController.AddBroken(ContentSheet, block, ContentBlock, SelectedBlock, true, Options.LineThickness / Zoom);
            }
        }

        #endregion

        #region Point Mode

        public IPoint InsertPoint(XImmutablePoint p, bool register, bool select)
        {
            double thickness = Options.LineThickness / Zoom;
            double x = _itemController.Snap(p.X, Options.SnapSize);
            double y = _itemController.Snap(p.Y, Options.SnapSize);

            var point = _blockFactory.CreatePoint(thickness, x, y, false);
            
            if (register)
            {
                _blockController.DeselectContent(SelectedBlock);
                History.Register("Insert Point");
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
                History.Register("Move");
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
            ModeMove();
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
                History.Register("Move");
                IsFirstMove = false;
                Cursor = Cursors.SizeAll;
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
            Cursor = Cursors.Arrow;
            OverlaySheet.ReleaseCapture();
        }

        #endregion

        #region IZoomController

        private int zoomIndex = -1;
        public int ZoomIndex
        {
            get { return zoomIndex; }
            set
            {
                if (value >= 0 && value <= Options.MaxZoomIndex)
                {
                    zoomIndex = value;
                    Zoom = Options.ZoomFactors[zoomIndex];
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
                    AdjustPageThickness(value);
                }
                zoom.ScaleX = value;
                zoom.ScaleY = value;
                Options.Zoom = value;
            }
        }

        public double PanX
        {
            get { return pan.X; }
            set 
            { 
                pan.X = value;
                Options.PanX = value;
            }
        }

        public double PanY
        {
            get { return pan.Y; }
            set 
            { 
                pan.Y = value;
                Options.PanY = value;
            }
        }

        public void AutoFit()
        {
            AutoFit(LastFinalWidth, LastFinalHeight);
        }

        public void ActualSize()
        {
            zoomIndex = Options.DefaultZoomIndex;
            Zoom = Options.ZoomFactors[zoomIndex];
            PanX = 0.0;
            PanY = 0.0;
        }

        #endregion

        #region Pan & Zoom Mode

        private int FindZoomIndex(double factor)
        {
            int index = -1;
            for (int i = 0; i < Options.ZoomFactors.Length; i++)
            {
                if (Options.ZoomFactors[i] > factor)
                {
                    index = i;
                    break;
                }
            }
            index = Math.Max(0, index);
            index = Math.Min(index, Options.MaxZoomIndex);
            return index;
        }

        public void SetAutoFitSize(double finalWidth, double finalHeight)
        {
            LastFinalWidth = finalWidth;
            LastFinalHeight = finalHeight;
        }

        public void AutoFit(double finalWidth, double finalHeight)
        {
            // calculate factor
            double fwidth = finalWidth / Options.PageWidth;
            double fheight = finalHeight / Options.PageHeight;
            double factor = Math.Min(fwidth, fheight);
            double panX = (finalWidth - (Options.PageWidth * factor)) / 2.0;
            double panY = (finalHeight - (Options.PageHeight * factor)) / 2.0;
            double dx = Math.Max(0, (finalWidth - DesiredSize.Width) / 2.0);
            double dy = Math.Max(0, (finalHeight - DesiredSize.Height) / 2.0);

            // adjust zoom
            zoomIndex = FindZoomIndex(factor);
            Zoom = factor;

            // adjust pan
            PanX = panX - dx;
            PanY = panY - dy;
        }

        private void ZoomTo(double x, double y, int oldZoomIndex)
        {
            double oldZoom = GetZoom(oldZoomIndex);
            double newZoom = GetZoom(zoomIndex);
            Zoom = newZoom;

            PanX = (x * oldZoom + PanX) - x * newZoom;
            PanY = (y * oldZoom + PanY) - y * newZoom;
        }

        private void ZoomTo(int delta, XImmutablePoint p)
        {
            if (delta > 0)
            {
                if (zoomIndex > -1 && zoomIndex < Options.MaxZoomIndex)
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

        private double GetZoom(int index)
        {
            if (index >= 0 && index <= Options.MaxZoomIndex)
            {
                return Options.ZoomFactors[index];
            }
            return Zoom;
        }

        private void InitPan(XImmutablePoint p)
        {
            StoreTempMode();
            ModePan();
            PanStartPoint = new XImmutablePoint(p.X, p.Y);
            ResetOverlay();
            Cursor = Cursors.ScrollAll;
            OverlaySheet.Capture();
        }

        private void Pan(XImmutablePoint p)
        {
            PanX = PanX + p.X - PanStartPoint.X;
            PanY = PanY + p.Y - PanStartPoint.Y;
            PanStartPoint = new XImmutablePoint(p.X, p.Y);
        }

        private void FinishPan()
        {
            RestoreTempMode();
            Cursor = Cursors.Arrow;
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
            AdjustThickness(parent.Lines, thickness);
            AdjustThickness(parent.Rectangles, thickness);
            AdjustThickness(parent.Ellipses, thickness);

            foreach (var block in parent.Blocks)
            {
                AdjustThickness(block, thickness);
            }
        }

        private void AdjustBackThickness(double zoom)
        {
            double gridThicknessZoomed = Options.GridThickness / zoom;
            double frameThicknessZoomed = Options.FrameThickness / zoom;

            AdjustThickness(GridBlock, gridThicknessZoomed);
            AdjustThickness(FrameBlock, frameThicknessZoomed);
        }

        private void AdjustPageThickness(double zoom)
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
            TempSelectionRect = _pageFactory.CreateSelectionRectangle(Options.SelectionThickness / Zoom, p.X, p.Y, 0.0, 0.0);
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
            
            TempLine = _blockFactory.CreateLine(Options.LineThickness / Zoom, x, y, x, y, ItemColors.Black);
            
            if (start != null)
            {
                TempLine.Start = start;
            }

            TempStartEllipse = _blockFactory.CreateEllipse(Options.LineThickness / Zoom, x - 4.0, y - 4.0, 8.0, 8.0, true, ItemColors.Black, ItemColors.Black);
            TempEndEllipse = _blockFactory.CreateEllipse(Options.LineThickness / Zoom, x - 4.0, y - 4.0, 8.0, 8.0, true, ItemColors.Black, ItemColors.Black);
            
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
                
                History.Register("Create Line");
                
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
            TempRectangle = _blockFactory.CreateRectangle(Options.LineThickness / Zoom, x, y, 0.0, 0.0, false, ItemColors.Black, ItemColors.Transparent);
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
                History.Register("Create Rectangle");
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
            TempEllipse = _blockFactory.CreateEllipse(Options.LineThickness / Zoom, x, y, 0.0, 0.0, false, ItemColors.Black, ItemColors.Transparent);
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
                History.Register("Create Ellipse");
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
            History.Register("Create Text");

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
                ModeTextEditor();

                var tc = CreateTextEditor(new XImmutablePoint((EditorSheet.Width / 2) - (330 / 2), EditorSheet.Height / 2) /* p */);

                Action<string> ok = (text) =>
                {
                    History.Register("Edit Text");
                    tb.Text = text;
                    EditorSheet.Remove(tc);
                    Focus();
                    RestoreTempMode();
                };

                Action cancel = () =>
                {
                    EditorSheet.Remove(tc);
                    Focus();
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
            var dlg = new Microsoft.Win32.OpenFileDialog()
            {
                Filter = FileDialogSettings.ImageFilter
            };

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
            ModeEdit();

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
            ModeEdit();

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
            ModeEdit();

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
            ModeEdit();

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
            ModeEdit();

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

        private bool BindDataToBlock(XImmutablePoint p, DataItem dataItem)
        {
            var temp = _blockFactory.CreateBlock(-1, Options.PageOriginX, Options.PageOriginY, Options.PageWidth, Options.PageHeight, -1, "TEMP", null);
            _blockController.HitTestForBlocks(ContentSheet, ContentBlock, temp, p, Options.HitTestSize);

            if (_blockController.HaveOneBlockSelected(temp))
            {
                History.Register("Bind Data");
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

        private bool BindDataToBlock(IBlock block, DataItem dataItem)
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

        private void TryToBindData(XImmutablePoint p, DataItem dataItem)
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
            History.Register("New");
            ResetPage();
            LoadStandardPage();
            AutoFit();
        }

        #endregion

        #region Open Page

        public async Task OpenTextPage(string path)
        {
            var text = await _itemController.OpenText(path);
            if (text != null)
            {
                var page = await Task.Run(() => _itemSerializer.DeserializeContents(text));
                History.Register("Open Text");
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
                History.Register("Open Json");
                ResetPage();
                DeserializePage(page);
            }
        }

        public async void OpenPage()
        {
            var dlg = new Microsoft.Win32.OpenFileDialog()
            {
                Filter = FileDialogSettings.PageFilter
            };

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
            var dlg = new Microsoft.Win32.SaveFileDialog()
            {
                Filter = FileDialogSettings.PageFilter,
                FileName = "sheet"
            };

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

        #region Export

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
            var dlg = new Microsoft.Win32.SaveFileDialog()
            {
                Filter = FileDialogSettings.ExportFilter,
                FileName = "sheet"
            };

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

        private void Insert(XImmutablePoint p)
        {
            if (Library != null)
            {
                var blockItem = Library.GetSelected() as BlockItem;
                Insert(blockItem, p, true);
            }
        }

        private IBlock Insert(BlockItem blockItem, XImmutablePoint p, bool select)
        {
            _blockController.DeselectContent(SelectedBlock);
            double thickness = Options.LineThickness / Zoom;

            History.Register("Insert Block");

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

        public async Task LoadLibrary(string fileName)
        {
            var text = await _itemController.OpenText(fileName);
            if (text != null)
            {
                InitLibrary(text);
            }
        }

        private async void InitLibrary(string text)
        {
            if (Library != null && text != null)
            {
                var block = await Task.Run(() => _itemSerializer.DeserializeContents(text));
                Library.SetSource(block.Blocks);
            }
        }

        private void AddToLibrary(BlockItem blockItem)
        {
            if (Library != null && blockItem != null)
            {
                var source = Library.GetSource();
                var items = new List<BlockItem>(source);
                _itemController.ResetPosition(blockItem, Options.PageOriginX, Options.PageOriginY, Options.PageWidth, Options.PageHeight);
                items.Add(blockItem);
                Library.SetSource(items);
            }
        }

        public async void LoadLibrary()
        {
            var dlg = new Microsoft.Win32.OpenFileDialog()
            {
                Filter = FileDialogSettings.LibraryFilter
            };

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

        #region Logic: Standard Page

        private void LoadStandardPage()
        {
            _pageFactory.CreateGrid(BackSheet, GridBlock, 330.0, 30.0, 600.0, 750.0, Options.GridSize, Options.GridThickness, ItemColors.LightGray);
            _pageFactory.CreateFrame(BackSheet, FrameBlock, Options.GridSize, Options.GridThickness, ItemColors.DarkGray);

            AdjustThickness(GridBlock, Options.GridThickness / GetZoom(zoomIndex));
            AdjustThickness(FrameBlock, Options.FrameThickness / GetZoom(zoomIndex));
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

        #region Wpf: Events

        private void LeftDown(MouseButtonEventArgs e)
        {
            bool onlyCtrl = Keyboard.Modifiers == ModifierKeys.Control;
            bool onlyShift = Keyboard.Modifiers == ModifierKeys.Shift;
            bool sourceIsThumb = ((e.OriginalSource as FrameworkElement).TemplatedParent) is Thumb;
            Point point = e.GetPosition(OverlaySheet.GetParent() as FrameworkElement);
            XImmutablePoint position = new XImmutablePoint(point.X, point.Y);

            //var args = new InputArgs()
            //{
            //    OnlyControl = onlyCtrl,
            //    OnlyShift = onlyShift,
            //    SourceType = sourceIsThumb ? ItemType.Thumb : ItemType.None,
            //    Position = position,
            //    Delta = 0,
            //    Button = InputButton.Left,
            //    Clicks = 1
            //};

            // edit mode
            if (SelectedType != ItemType.None)
            {
                if (!sourceIsThumb)
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
            if (!onlyCtrl)
            {
                if (_blockController.HaveSelected(SelectedBlock) && CanInitMove(position))
                {
                    InitMove(position);
                    return;
                }

                _blockController.DeselectContent(SelectedBlock);
            }

            bool resetSelected = onlyCtrl && _blockController.HaveSelected(SelectedBlock) ? false : true;

            if (GetMode() == SheetMode.Selection)
            {
                bool result = _blockController.HitTestClick(ContentSheet, ContentBlock, SelectedBlock, new XImmutablePoint(position.X, position.Y), Options.HitTestSize, false, resetSelected);
                if ((onlyCtrl || !_blockController.HaveSelected(SelectedBlock)) && !result)
                {
                    InitSelectionRect(position);
                }
                else
                {
                    // TODO: If control key is pressed then switch to move mode instead to edit mode
                    bool editModeEnabled = onlyCtrl == true ? false : TryToEditSelected();
                    if (!editModeEnabled)
                    {
                        InitMove(position);
                    }
                }
            }
            else if (GetMode() == SheetMode.Insert && !OverlaySheet.IsCaptured)
            {
                Insert(position);
            }
            else if (GetMode() == SheetMode.Point && !OverlaySheet.IsCaptured)
            {
                InsertPoint(position, true, true);
            }
            else if (GetMode() == SheetMode.Line && !OverlaySheet.IsCaptured)
            {
                // try to find point to connect line start
                var p = position;
                IPoint start = TryToFindPoint(p);

                // create start if Control key is pressed and start point has not been found
                if (onlyCtrl && start == null)
                {
                    start = InsertPoint(p, true, false);
                }

                InitTempLine(position, start);
            }
            else if (GetMode() == SheetMode.Line && OverlaySheet.IsCaptured)
            {
                // try to find point to connect line end
                var p = position;
                IPoint end = TryToFindPoint(p);

                // create end point if Control key is pressed and end point has not been found
                if (onlyCtrl && end == null)
                {
                    end = InsertPoint(p, true, false);
                }

                FinishTempLine(end);
            }
            else if (GetMode() == SheetMode.Rectangle && !OverlaySheet.IsCaptured)
            {
                InitTempRect(position);
            }
            else if (GetMode() == SheetMode.Rectangle && OverlaySheet.IsCaptured)
            {
                FinishTempRect();
            }
            else if (GetMode() == SheetMode.Ellipse && !OverlaySheet.IsCaptured)
            {
                InitTempEllipse(position);
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
                CreateText(position);
            }
            else if (GetMode() == SheetMode.Image && !OverlaySheet.IsCaptured)
            {
                Image(position);
            }
        }

        private void LeftUp(MouseButtonEventArgs e)
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

        private void Move(MouseEventArgs e)
        {
            bool onlyShift = Keyboard.Modifiers == ModifierKeys.Shift;
            Point point = e.GetPosition(OverlaySheet.GetParent() as FrameworkElement);
            XImmutablePoint position = new XImmutablePoint(point.X, point.Y);

            if (GetMode() == SheetMode.Edit)
            {
                return;
            }

            // mouse over selection when holding Shift key
            if (onlyShift && TempSelectionRect == null && !OverlaySheet.IsCaptured)
            {
                if (_blockController.HaveSelected(SelectedBlock))
                {
                    _blockController.DeselectContent(SelectedBlock);
                }

                _blockController.HitTestClick(ContentSheet, ContentBlock, SelectedBlock, position, Options.HitTestSize, false, false);
            }

            if (GetMode() == SheetMode.Selection && OverlaySheet.IsCaptured)
            {
                MoveSelectionRect(position);
            }
            else if (GetMode() == SheetMode.Line && OverlaySheet.IsCaptured)
            {
                MoveTempLine(position);
            }
            else if (GetMode() == SheetMode.Rectangle && OverlaySheet.IsCaptured)
            {
                MoveTempRect(position);
            }
            else if (GetMode() == SheetMode.Ellipse && OverlaySheet.IsCaptured)
            {
                MoveTempEllipse(position);
            }
            else if (GetMode() == SheetMode.Pan && OverlaySheet.IsCaptured)
            {
                var p = e.GetPosition(this);
                Pan(new XImmutablePoint(p.X, p.Y));
            }
            else if (GetMode() == SheetMode.Move && OverlaySheet.IsCaptured)
            {
                Move(position);
            }
        }

        private void RightDown(MouseButtonEventArgs e)
        {
            Point point = e.GetPosition(OverlaySheet.GetParent() as FrameworkElement);
            XImmutablePoint position = new XImmutablePoint(point.X, point.Y);

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
            if (GetMode() == SheetMode.Text && TryToEditText(position))
            {
                e.Handled = true;
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
                    var p = e.GetPosition(this);
                    InitPan(new XImmutablePoint(p.X, p.Y));
                }
            }
        }

        private void RightUp(MouseButtonEventArgs e)
        {
            if (GetMode() == SheetMode.Pan && OverlaySheet.IsCaptured)
            {
                FinishPan();
            }
        }

        private void Wheel(MouseWheelEventArgs e)
        {
            int d = e.Delta;
            var p = e.GetPosition(Layout);
            ZoomTo(d, new XImmutablePoint(p.X, p.Y));
        }

        private void Down(MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Middle && e.ClickCount == 2)
            {
                bool onlyCtrl = Keyboard.Modifiers == ModifierKeys.Control;
                // Mouse Middle Double-Click + Control key pressed to reset Pan and Zoom
                // Mouse Middle Double-Click to Auto Fit page to window size
                if (onlyCtrl)
                {
                    ActualSize();
                }
                else
                {
                    AutoFit();
                }
            }
        }

        private void UserControl_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Focus();
            LeftDown(e);
        }

        private void UserControl_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            LeftUp(e);
        }

        private void UserControl_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            Move(e);
        }

        private void UserControl_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            Focus();
            RightDown(e);
        }

        private void UserControl_PreviewMouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            RightUp(e);
        }

        private void UserControl_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            Wheel(e);
        }

        private void UserControl_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            Down(e);
        }

        #endregion

        #region Wpf: Drop

        private void UserControl_DragEnter(object sender, DragEventArgs e)
        {
            if (!e.Data.GetDataPresent("Block") || !e.Data.GetDataPresent("Data") || sender == e.Source)
            {
                e.Effects = DragDropEffects.None;
            }
        }

        private void UserControl_Drop(object sender, DragEventArgs e)
        {
            Point point = e.GetPosition(OverlaySheet.GetParent() as FrameworkElement);
            XImmutablePoint position = new XImmutablePoint(point.X, point.Y);

            if (e.Data.GetDataPresent("Block"))
            {
                var blockItem = e.Data.GetData("Block") as BlockItem;
                if (blockItem != null)
                {
                    Insert(blockItem, position, true);
                    e.Handled = true;
                }
            }
            else if (e.Data.GetDataPresent("Data"))
            {
                var dataItem = e.Data.GetData("Data") as DataItem;
                if (dataItem != null)
                {
                    TryToBindData(position, dataItem);
                    e.Handled = true;
                }
            }
        }

        #endregion

        #region Plugins

        private ISelectedBlockPlugin invertLineStartPlugin;
        private ISelectedBlockPlugin invertLineEndPlugin;

        private void CreatePlugins(IServiceLocator serviceLocator)
        {
            invertLineStartPlugin = new InvertLineStartPlugin(serviceLocator);
            invertLineEndPlugin = new InvertLineEndPlugin(serviceLocator);
        }

        private void ProcessPlugin(ISelectedBlockPlugin plugin)
        {
            if (plugin.CanProcess(ContentSheet, ContentBlock, SelectedBlock, Options))
            {
                var selectedBlock = _blockController.ShallowCopy(SelectedBlock);

                FinishEdit();
                History.Register(plugin.Name);

                plugin.Process(ContentSheet, ContentBlock, selectedBlock, Options);

                SelectedBlock = selectedBlock;
            }
        }

        public void InvertSelectedLineStart()
        {
            ProcessPlugin(invertLineStartPlugin);
        }

        public void InvertSelectedLineEnd()
        {
            ProcessPlugin(invertLineEndPlugin);
        }

        #endregion
    }
}
