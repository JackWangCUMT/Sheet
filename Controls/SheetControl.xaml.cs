using Newtonsoft.Json;
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

    #region PointController
    
    public static class PointController
    {
         public static void ConnectStart(XPoint point, XLine line)
        {
            var dependecy = new XDependency(line, (element, p) => { (element.Element as Line).X1 = p.X; (element.Element as Line).Y1 = p.Y; });
            point.Connected.Add(dependecy);
        }

        public static void ConnectEnd(XPoint point, XLine line)
        {
            var dependecy = new XDependency(line, (element, p) => { (element.Element as Line).X2 = p.X; (element.Element as Line).Y2 = p.Y; });
            point.Connected.Add(dependecy);
        }
    }
    
    #endregion

    public partial class SheetControl : UserControl, IPageController, IZoomController
    {
        #region Fields

        private SheetOptions options = null;

        private ISheet backSheet = null;
        private ISheet contentSheet = null;
        private ISheet overlaySheet = null;

        private SheetMode mode = SheetMode.Selection;
        private SheetMode tempMode = SheetMode.None;

        private bool isFirstMove = true;

        private Point panStartPoint;
        private Point selectionStartPoint;

        private XLine tempLine = null;
        private XEllipse tempStartEllipse = null;
        private XEllipse tempEndEllipse = null;
        private XRectangle tempRectangle = null;
        private XEllipse tempEllipse = null;
        private XRectangle tempSelectionRect = null;

        private XBlock contentBlock = null;
        private XBlock frameBlock = null;
        private XBlock gridBlock = null;

        private XBlock selectedBlock = null;

        private Size lastFinalSize = new Size();

        #endregion

        #region Properties

        public IHistoryController History { get; private set; }
        public ILibraryController Library { get; set; }

        #endregion
        
        #region IZoomController
        
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
                    AdjustPageThickness(value);
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

        #endregion

        #region Constructor

        public SheetControl()
        {
            InitializeComponent();

            History = new PageHistory(this);

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

        #region ToSingle

        private IEnumerable<T> ToSingle<T>(T item)
        {
            yield return item;
        } 

        #endregion

        #region Init

        private void Init()
        {
            InitOptions();
            InitSheets();
            InitBlocks();
        }

        private void InitOptions()
        {
            options = DefaultOptions();
            zoomIndex = options.DefaultZoomIndex;
        }

        private void InitSheets()
        {
            backSheet = new WpfCanvasSheet(Root.Back);
            contentSheet = new WpfCanvasSheet(Root.Sheet);
            overlaySheet = new WpfCanvasSheet(Root.Overlay);
        }

        private void InitBlocks()
        {
            contentBlock = new XBlock(-1, options.PageOriginX, options.PageOriginY, options.PageWidth, options.PageHeight, -1, "CONTENT");
            contentBlock.Init();

            frameBlock = new XBlock(-1, options.PageOriginX, options.PageOriginY, options.PageWidth, options.PageHeight, -1, "FRAME");
            frameBlock.Init();

            gridBlock = new XBlock(-1, options.PageOriginX, options.PageOriginY, options.PageWidth, options.PageHeight, -1, "GRID");
            gridBlock.Init();

            selectedBlock = new XBlock(-1, options.PageOriginX, options.PageOriginY, options.PageWidth, options.PageHeight, -1, "SELECTED");
        }

        private void InitLoaded()
        {
            LoadStandardPage();

            LoadLibraryFromResource(string.Concat("Sheet.Libraries", '.', "Digital.library"));

            //LoadPointDemo();

            Focus();
        }

        #endregion

        #region Point Demo

        private void LoadPointDemo()
        {
            var p0 = BlockFactory.CreatePoint(options.LineThickness / Zoom, 450, 300, false);
            var p1 = BlockFactory.CreatePoint(options.LineThickness / Zoom, 600, 300, true);
            var p2 = BlockFactory.CreatePoint(options.LineThickness / Zoom, 600, 450, true);
            var p3 = BlockFactory.CreatePoint(options.LineThickness / Zoom, 600, 150, false);

            var l0 = BlockFactory.CreateLine(options.LineThickness / Zoom, p0, p1, ItemColors.Black);
            var l1 = BlockFactory.CreateLine(options.LineThickness / Zoom, p1, p2, ItemColors.Black);
            var l2 = BlockFactory.CreateLine(options.LineThickness / Zoom, p1, p3, ItemColors.Black);

            PointController.ConnectStart(p0, l0);
            PointController.ConnectEnd(p1, l0);

            PointController.ConnectStart(p1, l1);
            PointController.ConnectEnd(p2, l1);

            PointController.ConnectStart(p1, l2);
            PointController.ConnectEnd(p3, l2);

            History.Register("Point Demo");

            contentBlock.Lines.Add(l0);
            contentBlock.Lines.Add(l1);
            contentBlock.Lines.Add(l2);

            contentBlock.Points.Add(p0);
            contentBlock.Points.Add(p1);
            contentBlock.Points.Add(p2);
            contentBlock.Points.Add(p3);

            contentSheet.Add(l0);
            contentSheet.Add(l1);
            contentSheet.Add(l2);

            contentSheet.Add(p0);
            contentSheet.Add(p1);
            contentSheet.Add(p2);
            contentSheet.Add(p3);
        }

        #endregion

        #region IPageController

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
                    var block = await Task.Run(() => ItemSerializer.DeserializeContents(text));
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
            var text = ItemSerializer.SerializeContents(block);

            return text;
        }

        public void ExportPage(string text)
        {
            var block = ItemSerializer.DeserializeContents(text);
            Export(ToSingle(block));
        }

        public void ExportPages(IEnumerable<string> texts)
        {
            var blocks = texts.Select(text => ItemSerializer.DeserializeContents(text));

            Export(blocks);
        }

        public BlockItem SerializePage()
        {
            BlockController.DeselectContent(selectedBlock);

            var grid = BlockSerializer.SerializerContents(gridBlock, -1, gridBlock.X, gridBlock.Y, gridBlock.Width, gridBlock.Height, -1, "GRID");
            var frame = BlockSerializer.SerializerContents(frameBlock, -1, frameBlock.X, frameBlock.Y, frameBlock.Width, frameBlock.Height, -1, "FRAME");
            var content = BlockSerializer.SerializerContents(contentBlock, -1, contentBlock.X, contentBlock.Y, contentBlock.Width, contentBlock.Height, -1, "CONTENT");

            var page = new BlockItem();
            page.Init(-1, options.PageOriginX, options.PageOriginY, options.PageWidth, options.PageHeight, -1, "PAGE");

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
                BlockController.AddContents(backSheet, grid, gridBlock, null, false, options.GridThickness / Zoom);
            }

            if (frame != null)
            {
                BlockController.AddContents(backSheet, frame, frameBlock, null, false, options.FrameThickness / Zoom);
            }

            if (content != null)
            {
                BlockController.AddContents(contentSheet, content, contentBlock, null, false, options.LineThickness / Zoom);
            }
        }

        public void ResetPage()
        {
            ResetOverlay();

            BlockController.Remove(backSheet, gridBlock);
            BlockController.Remove(backSheet, frameBlock);
            BlockController.Remove(contentSheet, contentBlock);

            InitBlocks();
        }

        public void ResetPageContent()
        {
            ResetOverlay();

            BlockController.Remove(contentSheet, contentBlock);
        }

        #endregion

        #region Mode

        public SheetMode GetMode()
        {
            return mode;
        }

        public void SetMode(SheetMode m)
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

        #region Clipboard

        public void Cut()
        {
            try
            {
                if (BlockController.HaveSelected(selectedBlock))
                {
                    var copy = BlockController.ShallowCopy(selectedBlock);
                    History.Register("Cut");
                    Copy(copy);
                    Delete(copy);
                }
            }
            catch (Exception ex)
            {
                Debug.Print(ex.Message);
                Debug.Print(ex.StackTrace);
            }
        }

        private void Copy(XBlock block)
        {
            try
            {
                var selected = BlockSerializer.SerializerContents(block, 0, 0.0, 0.0, 0.0, 0.0, -1, "SELECTED");
                var text = ItemSerializer.SerializeContents(selected);
                Clipboard.SetData(DataFormats.UnicodeText, text);
                //string json = JsonConvert.SerializeObject(selected, Formatting.Indented);
                //Clipboard.SetData(DataFormats.UnicodeText, json);
            }
            catch (Exception ex)
            {
                Debug.Print(ex.Message);
                Debug.Print(ex.StackTrace);
            }
        }

        public void Copy()
        {
            if (BlockController.HaveSelected(selectedBlock))
            {
                Copy(selectedBlock);
            }
        }

        public async void Paste()
        {
            try
            {
                var text = (string)Clipboard.GetData(DataFormats.UnicodeText);
                var block = await Task.Run(() => ItemSerializer.DeserializeContents(text));
                History.Register("Paste");
                InsertContent(block, true);
                //var block = JsonConvert.DeserializeObject<BlockItem>(text);
                //InsertBlock(block, true);
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
            if (tempLine != null)
            {
                overlaySheet.Remove(tempLine);
                tempLine = null;
            }

            if (tempStartEllipse != null)
            {
                overlaySheet.Remove(tempStartEllipse);
                tempLine = null;
            }

            if (tempEndEllipse != null)
            {
                overlaySheet.Remove(tempEndEllipse);
                tempEndEllipse = null;
            }

            if (tempRectangle != null)
            {
                overlaySheet.Remove(tempRectangle);
                tempRectangle = null;
            }

            if (tempEllipse != null)
            {
                overlaySheet.Remove(tempEllipse);
                tempEllipse = null;
            }

            if (tempSelectionRect != null)
            {
                overlaySheet.Remove(tempSelectionRect);
                tempSelectionRect = null;
            }

            if (lineThumbStart != null)
            {
                overlaySheet.Remove(lineThumbStart);
            }

            if (lineThumbEnd != null)
            {
                overlaySheet.Remove(lineThumbEnd);
            }
        }

        #endregion

        #region Delete

        public void Delete(XBlock block)
        {
            FinishEdit();
            BlockController.RemoveSelected(contentSheet, contentBlock, block);
        }

        public void Delete()
        {
            if (BlockController.HaveSelected(selectedBlock))
            {
                var copy = BlockController.ShallowCopy(selectedBlock);
                History.Register("Delete");
                Delete(copy);
            }
        }

        #endregion

        #region Select All

        public void SelecteAll()
        {
            BlockController.SelectContent(selectedBlock, contentBlock);
        }

        #endregion

        #region Toggle Fill

        public void ToggleFill()
        {
            if (tempRectangle != null)
            {
                BlockController.ToggleFill(tempRectangle);
            }

            if (tempEllipse != null)
            {
                BlockController.ToggleFill(tempEllipse);
            }
        }

        #endregion

        #region Insert Mode

        private void InsertContent(BlockItem block, bool select)
        {
            BlockController.DeselectContent(selectedBlock);
            BlockController.AddContents(contentSheet, block, contentBlock, selectedBlock, select, options.LineThickness / Zoom);
        }

        public static BlockItem CreateBlock(string name, XBlock block)
        {
            try
            {
                var blockItem = BlockSerializer.Serialize(block);
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
            if (BlockController.HaveSelected(selectedBlock))
            {
                StoreTempMode();
                ModeTextEditor();

                var tc = CreateTextEditor(new Point((EditorCanvas.Width / 2) - (330 / 2), EditorCanvas.Height / 2));

                Action<string> ok = (name) =>
                {
                    var block = CreateBlock(name, selectedBlock);
                    if (block != null)
                    {
                        AddToLibrary(block);
                    }
                    EditorCanvas.Children.Remove(tc);
                    Focus();
                    RestoreTempMode();
                };

                Action cancel = () =>
                {
                    EditorCanvas.Children.Remove(tc);
                    Focus();
                    RestoreTempMode();
                };

                tc.Set(ok, cancel, "Create Block", "Name:", "BLOCK0");
                EditorCanvas.Children.Add(tc);
            }
        }

        public async void BreakBlock()
        {
            if (BlockController.HaveSelected(selectedBlock))
            {
                var text = ItemSerializer.SerializeContents(BlockSerializer.SerializerContents(selectedBlock, 0, 0.0, 0.0, 0.0, 0.0, -1, "SELECTED"));
                var block = await Task.Run(() => ItemSerializer.DeserializeContents(text));
                History.Register("Break Block");
                Delete();
                BlockController.AddBroken(contentSheet, block, contentBlock, selectedBlock, true, options.LineThickness / Zoom);
            }
        }

        #endregion

        #region Point Mode
        
        public XPoint InsertPoint(Point p, bool register, bool select)
        {
            double thickness = options.LineThickness / Zoom;
            double x = ItemController.Snap(p.X, options.SnapSize);
            double y = ItemController.Snap(p.Y, options.SnapSize);
            
            var point = BlockFactory.CreatePoint(thickness, x, y, false);
            
            if (register)
            {
                BlockController.DeselectContent(selectedBlock);
                History.Register("Insert Point");
            }
            
            contentBlock.Points.Add(point);
            contentSheet.Add(point);

            if (select)
            {
                selectedBlock.Points = new List<XPoint>();
                selectedBlock.Points.Add(point);

                BlockController.Select(point);
            }
            
            return point;
        }
        
        #endregion
        
        #region Move Mode

        private void Move(double x, double y)
        {
            if (BlockController.HaveSelected(selectedBlock))
            {
                XBlock moveBlock = BlockController.ShallowCopy(selectedBlock);
                FinishEdit();
                History.Register("Move");
                BlockController.Select(moveBlock);
                selectedBlock = moveBlock;
                BlockController.Move(x, y, selectedBlock);
            }
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

        private bool CanInitMove(Point p)
        {
            var temp = new XBlock(-1, options.PageOriginX, options.PageOriginY, options.PageWidth, options.PageHeight, -1, "TEMP");
            BlockController.HitTestClick(contentSheet, selectedBlock, temp, p, options.HitTestSize, false, true);

            if (BlockController.HaveSelected(temp))
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
            p.X = ItemController.Snap(p.X, options.SnapSize);
            p.Y = ItemController.Snap(p.Y, options.SnapSize);
            panStartPoint = p;
            ResetOverlay();
            overlaySheet.Capture();
        }

        private void Move(Point p)
        {
            if (isFirstMove)
            {
                XBlock moveBlock = BlockController.ShallowCopy(selectedBlock);
                History.Register("Move");
                isFirstMove = false;
                Cursor = Cursors.SizeAll;
                BlockController.Select(moveBlock);
                selectedBlock = moveBlock;
            }

            p.X = ItemController.Snap(p.X, options.SnapSize);
            p.Y = ItemController.Snap(p.Y, options.SnapSize);

            double dx = p.X - panStartPoint.X;
            double dy = p.Y - panStartPoint.Y;

            if (dx != 0.0 || dy != 0.0)
            {
                BlockController.Move(dx, dy, selectedBlock);
                panStartPoint = p;
            }
        }

        private void FinishMove()
        {
            RestoreTempMode();
            Cursor = Cursors.Arrow;
            overlaySheet.ReleaseCapture();
        }

        #endregion

        #region Pan & Zoom Mode

        private int FindZoomIndex(double factor)
        {
            int index = -1;
            for (int i = 0; i < options.ZoomFactors.Length; i++)
            {
                if (options.ZoomFactors[i] > factor)
                {
                    index = i;
                    break;
                }
            }
            index = Math.Max(0, index);
            index = Math.Min(index, options.MaxZoomIndex);
            return index;
        }

        public void SetAutoFitSize(Size finalSize)
        {
            lastFinalSize = finalSize;
        }

        public void AutoFit(Size size)
        {
            // calculate factor
            double fwidth = size.Width / options.PageWidth;
            double fheight = size.Height / options.PageHeight;
            double factor = Math.Min(fwidth, fheight);
            double panX = (size.Width - (options.PageWidth * factor)) / 2.0;
            double panY = (size.Height - (options.PageHeight * factor)) / 2.0;
            double dx = Math.Max(0, (size.Width - DesiredSize.Width) / 2.0);
            double dy = Math.Max(0, (size.Height - DesiredSize.Height) / 2.0);

            // adjust zoom
            zoomIndex = FindZoomIndex(factor);
            Zoom = factor;

            // adjust pan
            PanX = panX - dx;
            PanY = panY - dy;
        }

        public void AutoFit()
        {
            AutoFit(lastFinalSize);
        }

        public void ActualSize()
        {
            zoomIndex = options.DefaultZoomIndex;
            Zoom = options.ZoomFactors[zoomIndex];
            PanX = 0.0;
            PanY = 0.0;
        }

        private void ZoomTo(double x, double y, int oldZoomIndex)
        {
            double oldZoom = GetZoom(oldZoomIndex);
            double newZoom = GetZoom(zoomIndex);
            Zoom = newZoom;

            PanX = (x * oldZoom + PanX) - x * newZoom;
            PanY = (y * oldZoom + PanY) - y * newZoom;
        }

        private void ZoomTo(int delta, Point p)
        {
            if (delta > 0)
            {
                if (zoomIndex > -1 && zoomIndex < options.MaxZoomIndex)
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
            if (index >= 0 && index <= options.MaxZoomIndex)
            {
                return options.ZoomFactors[index];
            }
            return zoom.ScaleX;
        }

        private void InitPan(Point p)
        {
            StoreTempMode();
            ModePan();
            panStartPoint = p;
            ResetOverlay();
            Cursor = Cursors.ScrollAll;
            overlaySheet.Capture();
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
            overlaySheet.ReleaseCapture();
        }

        private static void AdjustThickness(IEnumerable<XLine> lines, double thickness)
        {
            foreach (var line in lines)
            {
                (line.Element as Line).StrokeThickness = thickness;
            }
        }

        private static void AdjustThickness(IEnumerable<XRectangle> rectangles, double thickness)
        {
            foreach (var rectangle in rectangles)
            {
                (rectangle.Element as Rectangle).StrokeThickness = thickness;
            }
        }

        private static void AdjustThickness(IEnumerable<XEllipse> ellipses, double thickness)
        {
            foreach (var ellipse in ellipses)
            {
                (ellipse.Element as Ellipse).StrokeThickness = thickness;
            }
        }

        private static void AdjustThickness(XBlock parent, double thickness)
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
            double gridThicknessZoomed = options.GridThickness / zoom;
            double frameThicknessZoomed = options.FrameThickness / zoom;

            AdjustThickness(gridBlock, gridThicknessZoomed);
            AdjustThickness(frameBlock, frameThicknessZoomed);
        }

        private void AdjustPageThickness(double zoom)
        {
            double lineThicknessZoomed = options.LineThickness / zoom;
            double selectionThicknessZoomed = options.SelectionThickness / zoom;

            AdjustBackThickness(zoom);

            AdjustThickness(contentBlock, lineThicknessZoomed);

            if (tempLine != null)
            {
                (tempLine.Element as Line).StrokeThickness = lineThicknessZoomed;
            }

            if (tempStartEllipse != null)
            {
                (tempStartEllipse.Element as Ellipse).StrokeThickness = lineThicknessZoomed;
            }

            if (tempEndEllipse != null)
            {
                (tempEndEllipse.Element as Ellipse).StrokeThickness = lineThicknessZoomed;
            }

            if (tempRectangle != null)
            {
                (tempRectangle.Element as Rectangle).StrokeThickness = lineThicknessZoomed;
            }

            if (tempEllipse != null)
            {
                (tempEllipse.Element as Ellipse).StrokeThickness = lineThicknessZoomed;
            }

            if (tempSelectionRect != null)
            {
                (tempSelectionRect.Element as Rectangle).StrokeThickness = selectionThicknessZoomed;
            }
        }

        #endregion

        #region Selection Mode

        private void InitSelectionRect(Point p)
        {
            selectionStartPoint = p;
            double x = p.X;
            double y = p.Y;
            tempSelectionRect = PageFactory.CreateSelectionRectangle(options.SelectionThickness / Zoom, x, y, 0.0, 0.0);
            overlaySheet.Add(tempSelectionRect);
            overlaySheet.Capture();
        }

        private void MoveSelectionRect(Point p)
        {
            double sx = selectionStartPoint.X;
            double sy = selectionStartPoint.Y;
            double x = p.X;
            double y = p.Y;
            double width = Math.Abs(sx - x);
            double height = Math.Abs(sy - y);

            var rectangle = tempSelectionRect.Element as Rectangle;
            Canvas.SetLeft(rectangle, Math.Min(sx, x));
            Canvas.SetTop(rectangle, Math.Min(sy, y));
            rectangle.Width = width;
            rectangle.Height = height;
        }

        private void FinishSelectionRect()
        {
            var rectangle = tempSelectionRect.Element as Rectangle;
            double x = Canvas.GetLeft(rectangle);
            double y = Canvas.GetTop(rectangle);
            double width = rectangle.Width;
            double height = rectangle.Height;

            CancelSelectionRect();

            // get selected items
            bool ctrl = (Keyboard.Modifiers & ModifierKeys.Control) > 0;
            bool resetSelected = ctrl && BlockController.HaveSelected(selectedBlock) ? false : true;
            BlockController.HitTestSelectionRect(contentSheet, contentBlock, selectedBlock, new Rect(x, y, width, height), resetSelected);

            // edit mode
            TryToEditSelected();
        }

        private void CancelSelectionRect()
        {
            overlaySheet.ReleaseCapture();
            overlaySheet.Remove(tempSelectionRect);
            tempSelectionRect = null;
        }

        #endregion

        #region Line Mode

        private XPoint TryToFindPoint(Point p)
        {
            var temp = new XBlock(-1, options.PageOriginX, options.PageOriginY, options.PageWidth, options.PageHeight, -1, "TEMP");
            BlockController.HitTestClick(contentSheet, contentBlock, temp, p, options.HitTestSize, true, true);

            if (BlockController.HaveOnePointSelected(temp))
            {
                var xpoint = temp.Points[0];
                BlockController.Deselect(temp);
                return xpoint;
            }

            BlockController.Deselect(temp);
            return null;
        }
        
        private void InitTempLine(Point p, XPoint start)
        {
            double x = ItemController.Snap(p.X, options.SnapSize);
            double y = ItemController.Snap(p.Y, options.SnapSize);
            
            tempLine = BlockFactory.CreateLine(options.LineThickness / Zoom, x, y, x, y, ItemColors.Black);
            
            if (start != null)
            {
                tempLine.Start = start;
            }

            tempStartEllipse = BlockFactory.CreateEllipse(options.LineThickness / Zoom, x - 4.0, y - 4.0, 8.0, 8.0, true);
            tempEndEllipse = BlockFactory.CreateEllipse(options.LineThickness / Zoom, x - 4.0, y - 4.0, 8.0, 8.0, true);
            
            overlaySheet.Add(tempLine);
            overlaySheet.Add(tempStartEllipse);
            overlaySheet.Add(tempEndEllipse);
            overlaySheet.Capture();
        }

        private void MoveTempLine(Point p)
        {
            double x = ItemController.Snap(p.X, options.SnapSize);
            double y = ItemController.Snap(p.Y, options.SnapSize);
            var ellipse = tempEndEllipse.Element as Ellipse;
            var line = tempLine.Element as Line;
            if (Math.Round(x, 1) != Math.Round(line.X2, 1)
                || Math.Round(y, 1) != Math.Round(line.Y2, 1))
            {
                line.X2 = x;
                line.Y2 = y;
                Canvas.SetLeft(ellipse, x - 4.0);
                Canvas.SetTop(ellipse, y - 4.0);
            }
        }

        private void FinishTempLine(XPoint end)
        {
            var line = tempLine.Element as Line;
            if (Math.Round(line.X1, 1) == Math.Round(line.X2, 1) &&
                Math.Round(line.Y1, 1) == Math.Round(line.Y2, 1))
            {
                CancelTempLine();
            }
            else
            {
                if (end != null)
                {
                    tempLine.End = end;
                }
            
                overlaySheet.ReleaseCapture();
                overlaySheet.Remove(tempLine);
                overlaySheet.Remove(tempStartEllipse);
                overlaySheet.Remove(tempEndEllipse);
                
                History.Register("Create Line");
                
                if (tempLine.Start != null)
                {
                    PointController.ConnectStart(tempLine.Start, tempLine);
                }
                
                if (tempLine.End != null)
                {
                    PointController.ConnectEnd(tempLine.End, tempLine);
                }
                
                contentBlock.Lines.Add(tempLine);
                contentSheet.Add(tempLine);
                
                tempLine = null;
                tempStartEllipse = null;
                tempEndEllipse = null;
            }
        }

        private void CancelTempLine()
        {
            overlaySheet.ReleaseCapture();
            overlaySheet.Remove(tempLine);
            overlaySheet.Remove(tempStartEllipse);
            overlaySheet.Remove(tempEndEllipse);
            tempLine = null;
            tempStartEllipse = null;
            tempEndEllipse = null;
        }

        #endregion

        #region Rectangle Mode

        private void InitTempRect(Point p)
        {
            double x = ItemController.Snap(p.X, options.SnapSize);
            double y = ItemController.Snap(p.Y, options.SnapSize);
            selectionStartPoint = new Point(x, y);
            tempRectangle = BlockFactory.CreateRectangle(options.LineThickness / Zoom, x, y, 0.0, 0.0, true);
            overlaySheet.Add(tempRectangle);
            overlaySheet.Capture();
        }

        private void MoveTempRect(Point p)
        {
            double sx = selectionStartPoint.X;
            double sy = selectionStartPoint.Y;
            double x = ItemController.Snap(p.X, options.SnapSize);
            double y = ItemController.Snap(p.Y, options.SnapSize);
            double width = Math.Abs(sx - x);
            double height = Math.Abs(sy - y);
            var rectangle = tempRectangle.Element as Rectangle;
            Canvas.SetLeft(rectangle, Math.Min(sx, x));
            Canvas.SetTop(rectangle, Math.Min(sy, y));
            rectangle.Width = width;
            rectangle.Height = height;
        }

        private void FinishTempRect()
        {
            var rectangle = tempRectangle.Element as Rectangle;
            double x = Canvas.GetLeft(rectangle);
            double y = Canvas.GetTop(rectangle);
            double width = rectangle.Width;
            double height = rectangle.Height;

            if (width == 0.0 || height == 0.0)
            {
                CancelTempRect();
            }
            else
            {
                overlaySheet.ReleaseCapture();
                overlaySheet.Remove(tempRectangle);
                History.Register("Create Rectangle");
                contentBlock.Rectangles.Add(tempRectangle);
                contentSheet.Add(tempRectangle);
                tempRectangle = null;
            }
        }

        private void CancelTempRect()
        {
            overlaySheet.ReleaseCapture();
            overlaySheet.Remove(tempRectangle);
            tempRectangle = null;
        }

        #endregion

        #region Ellipse Mode

        private void InitTempEllipse(Point p)
        {
            double x = ItemController.Snap(p.X, options.SnapSize);
            double y = ItemController.Snap(p.Y, options.SnapSize);
            selectionStartPoint = new Point(x, y);
            tempEllipse = BlockFactory.CreateEllipse(options.LineThickness / Zoom, x, y, 0.0, 0.0, true);
            overlaySheet.Add(tempEllipse);
            overlaySheet.Capture();
        }

        private void MoveTempEllipse(Point p)
        {
            double sx = selectionStartPoint.X;
            double sy = selectionStartPoint.Y;
            double x = ItemController.Snap(p.X, options.SnapSize);
            double y = ItemController.Snap(p.Y, options.SnapSize);
            double width = Math.Abs(sx - x);
            double height = Math.Abs(sy - y);

            var ellipse = tempEllipse.Element as Ellipse;
            Canvas.SetLeft(ellipse, Math.Min(sx, x));
            Canvas.SetTop(ellipse, Math.Min(sy, y));
            ellipse.Width = width;
            ellipse.Height = height;
        }

        private void FinishTempEllipse()
        {
            var ellipse = tempEllipse.Element as Ellipse;
            double x = Canvas.GetLeft(ellipse);
            double y = Canvas.GetTop(ellipse);
            double width = ellipse.Width;
            double height = ellipse.Height;

            if (width == 0.0 || height == 0.0)
            {
                CancelTempEllipse();
            }
            else
            {
                overlaySheet.ReleaseCapture();
                overlaySheet.Remove(tempEllipse);
                History.Register("Create Ellipse");
                contentBlock.Ellipses.Add(tempEllipse);
                contentSheet.Add(tempEllipse);
                tempEllipse = null;
            }
        }

        private void CancelTempEllipse()
        {
            overlaySheet.ReleaseCapture();
            overlaySheet.Remove(tempEllipse);
            tempEllipse = null;
        }

        #endregion

        #region Text Mode

        private TextControl CreateTextEditor(Point p)
        {
            var tc = new TextControl() { Width = 330.0, Background = Brushes.WhiteSmoke };
            tc.RenderTransform = null;
            Canvas.SetLeft(tc, p.X);
            Canvas.SetTop(tc, p.Y);
            return tc;
        }

        private void CreateText(Point p)
        {
            double x = ItemController.Snap(p.X, options.SnapSize);
            double y = ItemController.Snap(p.Y, options.SnapSize);
            History.Register("Create Text");
            var text = BlockFactory.CreateText("Text", x, y, 30.0, 15.0, HorizontalAlignment.Center, VerticalAlignment.Center, 11.0, ItemColors.Transparent, ItemColors.Black);
            contentBlock.Texts.Add(text);
            contentSheet.Add(text);
        }

        private bool TryToEditText(Point p)
        {
            var temp = new XBlock(-1, options.PageOriginX, options.PageOriginY, options.PageWidth, options.PageHeight, -1, "TEMP");
            BlockController.HitTestClick(contentSheet, contentBlock, temp, p, options.HitTestSize, true, true);

            if (BlockController.HaveOneTextSelected(temp))
            {
                var tb = BlockFactory.GetTextBlock(temp.Texts[0]);

                StoreTempMode();
                ModeTextEditor();

                var tc = CreateTextEditor(new Point((EditorCanvas.Width / 2) - (330 / 2), EditorCanvas.Height / 2) /* p */);

                Action<string> ok = (text) =>
                {
                    History.Register("Edit Text");
                    tb.Text = text;
                    EditorCanvas.Children.Remove(tc);
                    Focus();
                    RestoreTempMode();
                };

                Action cancel = () =>
                {
                    EditorCanvas.Children.Remove(tc);
                    Focus();
                    RestoreTempMode();
                };

                tc.Set(ok, cancel, "Edit Text", "Text:", tb.Text);
                EditorCanvas.Children.Add(tc);

                BlockController.Deselect(temp);
                return true;
            }

            BlockController.Deselect(temp);
            return false;
        }

        #endregion

        #region Image Mode

        private void Image(Point p)
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

        private void InsertImage(Point p, string path)
        {
            byte[] data = Base64.ReadAllBytes(path);
            double x = ItemController.Snap(p.X, options.SnapSize);
            double y = ItemController.Snap(p.Y, options.SnapSize);
            var image = BlockFactory.CreateImage(x, y, 120.0, 90.0, data);
            contentBlock.Images.Add(image);
            contentSheet.Add(image);
        }

        #endregion

        #region Edit Mode

        private ItemType selectedType = ItemType.None;
        private string editThumbTemplate = "<Thumb Cursor=\"SizeAll\" xmlns=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation\"><Thumb.Template><ControlTemplate><Rectangle Fill=\"Transparent\" Stroke=\"Red\" StrokeThickness=\"2\" Width=\"8\" Height=\"8\" Margin=\"-4,-4,0,0\"/></ControlTemplate></Thumb.Template></Thumb>";

        private XLine selectedLine = null;
        private XThumb lineThumbStart = null;
        private XThumb lineThumbEnd = null;

        private XElement selectedElement = null;
        private XThumb thumbTopLeft = null;
        private XThumb thumbTopRight = null;
        private XThumb thumbBottomLeft = null;
        private XThumb thumbBottomRight = null;

        private XThumb CreateEditThumb()
        {
            var stringReader = new System.IO.StringReader(editThumbTemplate);
            var xmlReader = System.Xml.XmlReader.Create(stringReader);
            var thumb = (Thumb)XamlReader.Load(xmlReader);
            return new XThumb(thumb);
        }

        private bool TryToEditSelected()
        {
            if (BlockController.HaveOneLineSelected(selectedBlock))
            {
                InitLineEditor();
                return true;
            }
            else if (BlockController.HaveOneRectangleSelected(selectedBlock))
            {
                InitRectangleEditor();
                return true;
            }
            else if (BlockController.HaveOneEllipseSelected(selectedBlock))
            {
                InitEllipseEditor();
                return true;
            }
            else if (BlockController.HaveOneTextSelected(selectedBlock))
            {
                InitTextEditor();
                return true;
            }
            else if (BlockController.HaveOneImageSelected(selectedBlock))
            {
                InitImageEditor();
                return true;
            }
            return false;
        }

        private void FinishEdit()
        {
            switch (selectedType)
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

        private void DragLineStart(XLine line, XThumb thumb, double dx, double dy)
        {
            if (line != null && thumb != null)
            {
                if (line.Start != null)
                {
                    double x = ItemController.Snap(line.Start.X + dx, options.SnapSize);
                    double y = ItemController.Snap(line.Start.Y + dy, options.SnapSize);
                    double sdx = x - line.Start.X;
                    double sdy = y - line.Start.Y;
                    BlockController.Move(sdx, sdy, line.Start);
                    Canvas.SetLeft(thumb.Element as Thumb, x);
                    Canvas.SetTop(thumb.Element as Thumb, y);
                }
                else
                {
                    double x = ItemController.Snap((line.Element as Line).X1 + dx, options.SnapSize);
                    double y = ItemController.Snap((line.Element as Line).Y1 + dy, options.SnapSize);
                    (line.Element as Line).X1 = x;
                    (line.Element as Line).Y1 = y;
                    Canvas.SetLeft(thumb.Element as Thumb, x);
                    Canvas.SetTop(thumb.Element as Thumb, y);
                }
            }
        }

        private void DragLineEnd(XLine line, XThumb thumb, double dx, double dy)
        {
            if (line != null && thumb != null)
            {
                if (line.End != null)
                {
                    double x = ItemController.Snap(line.End.X + dx, options.SnapSize);
                    double y = ItemController.Snap(line.End.Y + dy, options.SnapSize);
                    double sdx = x - line.End.X;
                    double sdy = y - line.End.Y;
                    BlockController.Move(sdx, sdy, line.End);
                    Canvas.SetLeft(thumb.Element as Thumb, x);
                    Canvas.SetTop(thumb.Element as Thumb, y);
                }
                else
                {
                    double x = ItemController.Snap((line.Element as Line).X2 + dx, options.SnapSize);
                    double y = ItemController.Snap((line.Element as Line).Y2 + dy, options.SnapSize);
                    (line.Element as Line).X2 = x;
                    (line.Element as Line).Y2 = y;
                    Canvas.SetLeft(thumb.Element as Thumb, x);
                    Canvas.SetTop(thumb.Element as Thumb, y);
                }
            }
        }

        private void InitLineEditor()
        {
            StoreTempMode();
            ModeEdit();

            try
            {
                var line = selectedBlock.Lines.FirstOrDefault();
                selectedType = ItemType.Line;
                selectedLine = line;

                if (lineThumbStart == null)
                {
                    lineThumbStart = CreateEditThumb();
                    (lineThumbStart.Element as Thumb).DragDelta += (sender, e) => DragLineStart(selectedLine, lineThumbStart, e.HorizontalChange, e.VerticalChange);
                }

                if (lineThumbEnd == null)
                {
                    lineThumbEnd = CreateEditThumb();
                    (lineThumbEnd.Element as Thumb).DragDelta += (sender, e) => DragLineEnd(selectedLine, lineThumbEnd, e.HorizontalChange, e.VerticalChange);
                }

                Canvas.SetLeft(lineThumbStart.Element as Thumb, (line.Element as Line).X1);
                Canvas.SetTop(lineThumbStart.Element as Thumb, (line.Element as Line).Y1);
                Canvas.SetLeft(lineThumbEnd.Element as Thumb, (line.Element as Line).X2);
                Canvas.SetTop(lineThumbEnd.Element as Thumb, (line.Element as Line).Y2);

                overlaySheet.Add(lineThumbStart);
                overlaySheet.Add(lineThumbEnd);
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

            selectedType = ItemType.None;
            selectedLine = null;

            if (lineThumbStart != null)
            {
                overlaySheet.Remove(lineThumbStart);
            }

            if (lineThumbEnd != null)
            {
                overlaySheet.Remove(lineThumbEnd);
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

            Canvas.SetLeft(thumbTopLeft.Element as Thumb, tl.X);
            Canvas.SetTop(thumbTopLeft.Element as Thumb, tl.Y);

            Canvas.SetLeft(thumbTopRight.Element as Thumb, tr.X);
            Canvas.SetTop(thumbTopRight.Element as Thumb, tr.Y);

            Canvas.SetLeft(thumbBottomLeft.Element as Thumb, bl.X);
            Canvas.SetTop(thumbBottomLeft.Element as Thumb, bl.Y);

            Canvas.SetLeft(thumbBottomRight.Element as Thumb, br.X);
            Canvas.SetTop(thumbBottomRight.Element as Thumb, br.Y);
        }

        private void DragTopLeft(XElement element, XThumb thumb, double dx, double dy)
        {
            if (element != null && thumb != null)
            {
                double left = Canvas.GetLeft(element.Element as FrameworkElement);
                double top = Canvas.GetTop(element.Element as FrameworkElement);
                double width = (element.Element as FrameworkElement).Width;
                double height = (element.Element as FrameworkElement).Height;

                var rect = new Rect(left, top, width, height);

                rect.X = ItemController.Snap(rect.X + dx, options.SnapSize);
                rect.Y = ItemController.Snap(rect.Y + dy, options.SnapSize);

                rect.Width = Math.Max(0.0, rect.Width - (rect.X - left));
                rect.Height = Math.Max(0.0, rect.Height - (rect.Y - top));

                Canvas.SetLeft(element.Element as FrameworkElement, rect.X);
                Canvas.SetTop(element.Element as FrameworkElement, rect.Y);
                (element.Element as FrameworkElement).Width = rect.Width;
                (element.Element as FrameworkElement).Height = rect.Height;

                DragThumbs(rect);
            }
        }

        private void DragTopRight(XElement element, XThumb thumb, double dx, double dy)
        {
            if (element != null && thumb != null)
            {
                double left = Canvas.GetLeft(element.Element as FrameworkElement);
                double top = Canvas.GetTop(element.Element as FrameworkElement);
                double width = (element.Element as FrameworkElement).Width;
                double height = (element.Element as FrameworkElement).Height;

                var rect = new Rect(left, top, width, height);

                rect.Width = Math.Max(0.0, ItemController.Snap(rect.Width + dx, options.SnapSize));
                rect.Y = ItemController.Snap(rect.Y + dy, options.SnapSize);

                rect.Height = Math.Max(0.0, rect.Height - (rect.Y - top));

                Canvas.SetLeft(element.Element as FrameworkElement, rect.X);
                Canvas.SetTop(element.Element as FrameworkElement, rect.Y);
                (element.Element as FrameworkElement).Width = rect.Width;
                (element.Element as FrameworkElement).Height = rect.Height;

                DragThumbs(rect);
            }
        }

        private void DragBottomLeft(XElement element, XThumb thumb, double dx, double dy)
        {
            if (element != null && thumb != null)
            {
                double left = Canvas.GetLeft(element.Element as FrameworkElement);
                double top = Canvas.GetTop(element.Element as FrameworkElement);
                double width = (element.Element as FrameworkElement).Width;
                double height = (element.Element as FrameworkElement).Height;

                var rect = new Rect(left, top, width, height);

                rect.X = ItemController.Snap(rect.X + dx, options.SnapSize);
                rect.Height = Math.Max(0.0, ItemController.Snap(rect.Height + dy, options.SnapSize));

                rect.Width = Math.Max(0.0, rect.Width - (rect.X - left));

                Canvas.SetLeft(element.Element as FrameworkElement, rect.X);
                Canvas.SetTop(element.Element as FrameworkElement, rect.Y);
                (element.Element as FrameworkElement).Width = rect.Width;
                (element.Element as FrameworkElement).Height = rect.Height;

                DragThumbs(rect);
            }
        }

        private void DragBottomRight(XElement element, XThumb thumb, double dx, double dy)
        {
            if (element != null && thumb != null)
            {
                double left = Canvas.GetLeft(element.Element as FrameworkElement);
                double top = Canvas.GetTop(element.Element as FrameworkElement);
                double width = (element.Element as FrameworkElement).Width;
                double height = (element.Element as FrameworkElement).Height;

                var rect = new Rect(left, top, width, height);

                rect.Width = Math.Max(0.0, ItemController.Snap(rect.Width + dx, options.SnapSize));
                rect.Height = Math.Max(0.0, ItemController.Snap(rect.Height + dy, options.SnapSize));

                Canvas.SetLeft(element.Element as FrameworkElement, rect.X);
                Canvas.SetTop(element.Element as FrameworkElement, rect.Y);

                (element.Element as FrameworkElement).Width = rect.Width;
                (element.Element as FrameworkElement).Height = rect.Height;

                DragThumbs(rect);
            }
        }

        private void InitFrameworkElementEditor()
        {
            if (thumbTopLeft == null)
            {
                thumbTopLeft = CreateEditThumb();
                (thumbTopLeft.Element as Thumb).DragDelta += (sender, e) => DragTopLeft(selectedElement, thumbTopLeft, e.HorizontalChange, e.VerticalChange);
            }

            if (thumbTopRight == null)
            {
                thumbTopRight = CreateEditThumb();
                (thumbTopRight.Element as Thumb).DragDelta += (sender, e) => DragTopRight(selectedElement, thumbTopRight, e.HorizontalChange, e.VerticalChange);
            }

            if (thumbBottomLeft == null)
            {
                thumbBottomLeft = CreateEditThumb();
                (thumbBottomLeft.Element as Thumb).DragDelta += (sender, e) => DragBottomLeft(selectedElement, thumbBottomLeft, e.HorizontalChange, e.VerticalChange);
            }

            if (thumbBottomRight == null)
            {
                thumbBottomRight = CreateEditThumb();
                (thumbBottomRight.Element as Thumb).DragDelta += (sender, e) => DragBottomRight(selectedElement, thumbBottomRight, e.HorizontalChange, e.VerticalChange);
            }

            double left = Canvas.GetLeft(selectedElement.Element as FrameworkElement);
            double top = Canvas.GetTop(selectedElement.Element as FrameworkElement);
            double width = (selectedElement.Element as FrameworkElement).Width;
            double height = (selectedElement.Element as FrameworkElement).Height;

            Canvas.SetLeft(thumbTopLeft.Element as Thumb, left);
            Canvas.SetTop(thumbTopLeft.Element as Thumb, top);
            Canvas.SetLeft(thumbTopRight.Element as Thumb, left + width);
            Canvas.SetTop(thumbTopRight.Element as Thumb, top);
            Canvas.SetLeft(thumbBottomLeft.Element as Thumb, left);
            Canvas.SetTop(thumbBottomLeft.Element as Thumb, top + height);
            Canvas.SetLeft(thumbBottomRight.Element as Thumb, left + width);
            Canvas.SetTop(thumbBottomRight.Element as Thumb, top + height);

            overlaySheet.Add(thumbTopLeft);
            overlaySheet.Add(thumbTopRight);
            overlaySheet.Add(thumbBottomLeft);
            overlaySheet.Add(thumbBottomRight);
        }

        private void FinishFrameworkElementEditor()
        {
            RestoreTempMode();

            selectedType = ItemType.None;
            selectedElement = null;

            if (thumbTopLeft != null)
            {
                overlaySheet.Remove(thumbTopLeft);
            }

            if (thumbTopRight != null)
            {
                overlaySheet.Remove(thumbTopRight);
            }

            if (thumbBottomLeft != null)
            {
                overlaySheet.Remove(thumbBottomLeft);
            }

            if (thumbBottomRight != null)
            {
                overlaySheet.Remove(thumbBottomRight);
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
                var rectangle = selectedBlock.Rectangles.FirstOrDefault();
                selectedType = ItemType.Rectangle;
                selectedElement = rectangle;
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
                var ellipse = selectedBlock.Ellipses.FirstOrDefault();
                selectedType = ItemType.Ellipse;
                selectedElement = ellipse;
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
                var text = selectedBlock.Texts.FirstOrDefault();
                selectedType = ItemType.Text;
                selectedElement = text;
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
                var image = selectedBlock.Images.FirstOrDefault();
                selectedType = ItemType.Image;
                selectedElement = image;
                InitFrameworkElementEditor();
            }
            catch (Exception ex)
            {
                Debug.Print(ex.Message);
                Debug.Print(ex.StackTrace);
            }
        }

        #endregion

        #region Events

        private void UserControl_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Focus();

            bool ctrl = (Keyboard.Modifiers & ModifierKeys.Control) > 0;

            // edit mode
            if (selectedType != ItemType.None)
            {
                var source = (e.OriginalSource as FrameworkElement).TemplatedParent;
                if (!(source is Thumb))
                {
                    BlockController.DeselectContent(selectedBlock);
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
            if (!ctrl)
            {
                if (BlockController.HaveSelected(selectedBlock) && CanInitMove(e.GetPosition(overlaySheet.GetParent() as FrameworkElement)))
                {
                    InitMove(e.GetPosition(overlaySheet.GetParent() as FrameworkElement));
                    return;
                }

                BlockController.DeselectContent(selectedBlock);
            }

            bool resetSelected = ctrl && BlockController.HaveSelected(selectedBlock) ? false : true;

            if (GetMode() == SheetMode.Selection)
            {
                bool result = BlockController.HitTestClick(contentSheet, contentBlock, selectedBlock, e.GetPosition(overlaySheet.GetParent() as FrameworkElement), options.HitTestSize, false, resetSelected);
                if ((ctrl || !BlockController.HaveSelected(selectedBlock)) && !result)
                {
                    InitSelectionRect(e.GetPosition(overlaySheet.GetParent() as FrameworkElement));
                }
                else
                {
                    // TODO: If control key is pressed then switch to move mode instead to edit mode
                    bool editModeEnabled = ctrl == true ? false : TryToEditSelected();
                    if (!editModeEnabled)
                    {
                        InitMove(e.GetPosition(overlaySheet.GetParent() as FrameworkElement));
                    }
                }
            }
            else if (GetMode() == SheetMode.Insert && !overlaySheet.IsCaptured)
            {
                Insert(e.GetPosition(overlaySheet.GetParent() as FrameworkElement));
            }
            else if (GetMode() == SheetMode.Point && !overlaySheet.IsCaptured)
            {
                InsertPoint(e.GetPosition(overlaySheet.GetParent() as FrameworkElement), true, true);
            }
            else if (GetMode() == SheetMode.Line && !overlaySheet.IsCaptured)
            {
                // try to find point to connect line start
                var p = e.GetPosition(overlaySheet.GetParent() as FrameworkElement);
                XPoint start = TryToFindPoint(p);
                
                // create start if Control key is pressed and start point has not been found
                if (ctrl && start == null)
                {
                    start = InsertPoint(p, true, false);
                }
                
                InitTempLine(e.GetPosition(overlaySheet.GetParent() as FrameworkElement), start);
            }
            else if (GetMode() == SheetMode.Line && overlaySheet.IsCaptured)
            {
                // try to find point to connect line end
                var p = e.GetPosition(overlaySheet.GetParent() as FrameworkElement);
                XPoint end = TryToFindPoint(p);
                
                // create end point if Control key is pressed and end point has not been found
                if (ctrl && end == null)
                {
                    end = InsertPoint(p, true, false);
                }
                
                FinishTempLine(end);
            }
            else if (GetMode() == SheetMode.Rectangle && !overlaySheet.IsCaptured)
            {
                InitTempRect(e.GetPosition(overlaySheet.GetParent() as FrameworkElement));
            }
            else if (GetMode() == SheetMode.Rectangle && overlaySheet.IsCaptured)
            {
                FinishTempRect();
            }
            else if (GetMode() == SheetMode.Ellipse && !overlaySheet.IsCaptured)
            {
                InitTempEllipse(e.GetPosition(overlaySheet.GetParent() as FrameworkElement));
            }
            else if (GetMode() == SheetMode.Ellipse && overlaySheet.IsCaptured)
            {
                FinishTempEllipse();
            }
            else if (GetMode() == SheetMode.Pan && overlaySheet.IsCaptured)
            {
                FinishPan();
            }
            else if (GetMode() == SheetMode.Text && !overlaySheet.IsCaptured)
            {
                CreateText(e.GetPosition(overlaySheet.GetParent() as FrameworkElement));
            }
            else if (GetMode() == SheetMode.Image && !overlaySheet.IsCaptured)
            {
                Image(e.GetPosition(overlaySheet.GetParent() as FrameworkElement));
            }
        }

        private void UserControl_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (GetMode() == SheetMode.Selection && overlaySheet.IsCaptured)
            {
                FinishSelectionRect();
            }
            else if (GetMode() == SheetMode.Move && overlaySheet.IsCaptured)
            {
                FinishMove();
            }
        }

        private void UserControl_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (GetMode() == SheetMode.Edit)
            {
                return;
            }

            bool shift = (Keyboard.Modifiers == ModifierKeys.Shift);

            // mouse over selection when holding Shift key
            if (shift && tempSelectionRect == null && !overlaySheet.IsCaptured)
            {
                if (BlockController.HaveSelected(selectedBlock))
                {
                    BlockController.DeselectContent(selectedBlock);
                }

                BlockController.HitTestClick(contentSheet, contentBlock, selectedBlock, e.GetPosition(overlaySheet.GetParent() as FrameworkElement), options.HitTestSize, false, false);
            }

            if (GetMode() == SheetMode.Selection && overlaySheet.IsCaptured)
            {
                MoveSelectionRect(e.GetPosition(overlaySheet.GetParent() as FrameworkElement));
            }
            else if (GetMode() == SheetMode.Line && overlaySheet.IsCaptured)
            {
                MoveTempLine(e.GetPosition(overlaySheet.GetParent() as FrameworkElement));
            }
            else if (GetMode() == SheetMode.Rectangle && overlaySheet.IsCaptured)
            {
                MoveTempRect(e.GetPosition(overlaySheet.GetParent() as FrameworkElement));
            }
            else if (GetMode() == SheetMode.Ellipse && overlaySheet.IsCaptured)
            {
                MoveTempEllipse(e.GetPosition(overlaySheet.GetParent() as FrameworkElement));
            }
            else if (GetMode() == SheetMode.Pan && overlaySheet.IsCaptured)
            {
                Pan(e.GetPosition(this));
            }
            else if (GetMode() == SheetMode.Move && overlaySheet.IsCaptured)
            {
                Move(e.GetPosition(overlaySheet.GetParent() as FrameworkElement));
            }
        }

        private void UserControl_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            Focus();

            if (GetMode() == SheetMode.None || GetMode() == SheetMode.TextEditor)
            {
                return;
            }

            // edit mode
            if (selectedType != ItemType.None)
            {
                BlockController.DeselectContent(selectedBlock);
                FinishEdit();
                return;
            }

            // text editor
            if (TryToEditText(e.GetPosition(overlaySheet.GetParent() as FrameworkElement)))
            {
                e.Handled = true;
                return;
            }

            BlockController.DeselectContent(selectedBlock);

            if (GetMode() == SheetMode.Selection && overlaySheet.IsCaptured)
            {
                CancelSelectionRect();
            }
            else if (GetMode() == SheetMode.Line && overlaySheet.IsCaptured)
            {
                CancelTempLine();
            }
            else if (GetMode() == SheetMode.Rectangle && overlaySheet.IsCaptured)
            {
                CancelTempRect();
            }
            else if (GetMode() == SheetMode.Ellipse && overlaySheet.IsCaptured)
            {
                CancelTempEllipse();
            }
            else if (!overlaySheet.IsCaptured)
            {
                InitPan(e.GetPosition(this));
            }
        }

        private void UserControl_PreviewMouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (GetMode() == SheetMode.Pan && overlaySheet.IsCaptured)
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
                bool ctrl = (Keyboard.Modifiers & ModifierKeys.Control) > 0;

                // Mouse Middle Double-Click + Control key pressed to reset Pan and Zoom
                // Mouse Middle Double-Click to Auto Fit page to window size
                if (ctrl)
                {
                    ActualSize();
                }
                else
                {
                    AutoFit();
                }
            }
        }

        #endregion

        #region Data Binding

        private bool BindDataToBlock(Point p, DataItem dataItem)
        {
            var temp = new XBlock(-1, options.PageOriginX, options.PageOriginY, options.PageWidth, options.PageHeight, -1, "TEMP");
            BlockController.HitTestForBlocks(contentSheet, contentBlock, temp, p, options.HitTestSize);

            if (BlockController.HaveOneBlockSelected(temp))
            {
                History.Register("Bind Data");
                var block = temp.Blocks[0];
                BindDataToBlock(block, dataItem);
                BlockController.Deselect(temp);
                return true;
            }

            BlockController.Deselect(temp);
            return false;
        }

        private bool BindDataToBlock(XBlock block, DataItem dataItem)
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
                        var temp = new XBlock(-1, options.PageOriginX, options.PageOriginY, options.PageWidth, options.PageHeight, -1, "TEMP");
                        temp.Init();
                        temp.Blocks.Add(block);
                        BlockController.RemoveSelected(contentSheet, contentBlock, temp);
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
                    Insert(blockItem, e.GetPosition(overlaySheet.GetParent() as FrameworkElement), true);
                    e.Handled = true;
                }
            }
            else if (e.Data.GetDataPresent("Data"))
            {
                var dataItem = e.Data.GetData("Data") as DataItem;
                if (dataItem != null)
                {
                    TryToBindData(e.GetPosition(overlaySheet.GetParent() as FrameworkElement), dataItem);
                    e.Handled = true;
                }
            }
        }

        #endregion

        #region New

        public void New()
        {
            History.Register("New");
            ResetPage();
            LoadStandardPage();
            AutoFit();
        }

        #endregion

        #region Open

        public async Task OpenTextFile(string path)
        {
            var text = await ItemController.OpenText(path);
            if (text != null)
            {
                var page = await Task.Run(() => ItemSerializer.DeserializeContents(text));
                History.Register("Open Text");
                ResetPage();
                DeserializePage(page);
            }
        }

        public async Task OpenJsonFile(string path)
        {
            var text = await ItemController.OpenText(path);
            if (text != null)
            {
                var page = await Task.Run(() => JsonConvert.DeserializeObject<BlockItem>(text));
                History.Register("Open Json");
                ResetPage();
                DeserializePage(page);
            }
        }

        public async void Open()
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
                                await OpenTextFile(path);
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
                                await OpenJsonFile(path);
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

        #region Save

        public void Save()
        {
            var dlg = new Microsoft.Win32.SaveFileDialog()
            {
                Filter = FileDialogSettings.PageFilter,
                FileName = "sheet"
            };

            if (dlg.ShowDialog() == true)
            {
                var page = SerializePage();

                switch (dlg.FilterIndex)
                {
                    case 1:
                        {
                            try
                            {
                                Task.Run(() =>
                                {
                                    var text = ItemSerializer.SerializeContents(page);
                                    ItemController.SaveText(dlg.FileName, text);
                                });
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
                                Task.Run(() =>
                                {
                                    string text = JsonConvert.SerializeObject(page, Formatting.Indented);
                                    ItemController.SaveText(dlg.FileName, text);
                                });
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
                writer.Create(fileName, options.PageWidth, options.PageHeight, pages);
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
                        writer.Create(fileNameWithCounter, options.PageWidth, options.PageHeight, page);
                        counter++;
                    }
                }
                else
                {
                    var page = pages.FirstOrDefault();
                    if (page != null)
                    {
                        writer.Create(fileName, options.PageWidth, options.PageHeight, page);
                    }
                }
            });
        }

        public void Export(SolutionEntry solution)
        {
            var texts = solution.Documents.SelectMany(document => document.Pages).Select(page => page.Content);
            ExportPages(texts);
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
                switch (dlg.FilterIndex)
                {
                    case 1:
                    case 3:
                    default:
                        {
                            try
                            {
                                ExportToPdf(blocks, dlg.FileName);
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
                                ExportToDxf(blocks, dlg.FileName);
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

        public void Export()
        {
            var block = BlockSerializer.SerializerContents(contentBlock, 0, 
                contentBlock.X, contentBlock.Y, 
                contentBlock.Width, contentBlock.Height,
                contentBlock.DataId, "CONTENT");

            var blocks = ToSingle(block);

            Export(blocks);
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

        private XBlock Insert(BlockItem blockItem, Point p, bool select)
        {
            BlockController.DeselectContent(selectedBlock);
            double thickness = options.LineThickness / Zoom;

            History.Register("Insert Block");

            var block = BlockSerializer.Deserialize(contentSheet, contentBlock, blockItem, select, thickness);

            if (select)
            {
                selectedBlock.Blocks = new List<XBlock>();
                selectedBlock.Blocks.Add(block);
            }

            BlockController.Move(ItemController.Snap(p.X, options.SnapSize), ItemController.Snap(p.Y, options.SnapSize), block);

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
            var text = await ItemController.OpenText(fileName);
            if (text != null)
            {
                InitLibrary(text);
            }
        }

        private async void InitLibrary(string text)
        {
            if (Library != null && text != null)
            {
                var block = await Task.Run(() => ItemSerializer.DeserializeContents(text));
                Library.SetSource(block.Blocks);
            }
        }

        private void AddToLibrary(BlockItem blockItem)
        {
            if (Library != null && blockItem != null)
            {
                var source = Library.GetSource() as IEnumerable<BlockItem>;
                var items = new List<BlockItem>(source);
                ItemController.ResetPosition(blockItem, options.PageOriginX, options.PageOriginY, options.PageWidth, options.PageHeight);
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
            PageFactory.CreateGrid(backSheet, gridBlock, 330.0, 30.0, 600.0, 750.0, options.GridSize, options.GridThickness, ItemColors.LightGray);
            PageFactory.CreateFrame(backSheet, frameBlock, options.GridSize, options.GridThickness, ItemColors.DarkGray);

            AdjustThickness(gridBlock, options.GridThickness / GetZoom(zoomIndex));
            AdjustThickness(frameBlock, options.FrameThickness / GetZoom(zoomIndex));
        }

        private static BlockItem CreateGridBlock(XBlock gridBlock, bool adjustThickness, bool adjustColor)
        {
            var grid = BlockSerializer.SerializerContents(gridBlock, -1, 0.0, 0.0, 0.0, 0.0, -1, "GRID");

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

        private static BlockItem CreateFrameBlock(XBlock frameBlock, bool adjustThickness, bool adjustColor)
        {
            var frame = BlockSerializer.SerializerContents(frameBlock, -1, 0.0, 0.0, 0.0, 0.0, -1, "FRAME");

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
                var grid = CreateGridBlock(gridBlock, true, false);
                page.Blocks.Add(grid);
            }

            if (enableFrame)
            {
                var frame = CreateFrameBlock(frameBlock, true, true);
                page.Blocks.Add(frame);
            }

            page.Blocks.Add(content);

            return page;
        }

        #endregion

        #region Logic: Inverted Line

        private double invertedEllipseWidth = 10.0;
        private double invertedEllipseHeight = 10.0;

        private void AddInvertedLineEllipse(double x, double y, double width, double height)
        {
            // create ellipse
            var ellipse = BlockFactory.CreateEllipse(options.LineThickness / Zoom, x, y, width, height, false);
            contentBlock.Ellipses.Add(ellipse);
            contentSheet.Add(ellipse);

            // select ellipse
            BlockController.Select(ellipse);
            if (selectedBlock.Ellipses == null)
            {
                selectedBlock.Ellipses = new List<XEllipse>();
            }
            selectedBlock.Ellipses.Add(ellipse);
        }

        public void InvertSelectedLineStart()
        {
            // add for horizontal or vertical line start ellipse and shorten line
            if (BlockController.HaveSelected(selectedBlock) && selectedBlock.Lines != null && selectedBlock.Lines.Count > 0)
            {
                var block = BlockController.ShallowCopy(selectedBlock);
                FinishEdit();
                History.Register("Invert Line Start");

                foreach (var line in block.Lines)
                {
                    bool sameX = Math.Round((line.Element as Line).X1, 1) == Math.Round((line.Element as Line).X2, 1);
                    bool sameY = Math.Round((line.Element as Line).Y1, 1) == Math.Round((line.Element as Line).Y2, 1);

                    // vertical line
                    if (sameX && !sameY)
                    {
                        // X1, Y1 is start position
                        if ((line.Element as Line).Y1 < (line.Element as Line).Y2)
                        {
                            AddInvertedLineEllipse((line.Element as Line).X1 - invertedEllipseWidth / 2.0, (line.Element as Line).Y1, invertedEllipseWidth, invertedEllipseHeight);
                            (line.Element as Line).Y1 += invertedEllipseHeight;
                        }
                        // X2, Y2 is start position
                        else
                        {
                            AddInvertedLineEllipse((line.Element as Line).X2 - invertedEllipseWidth / 2.0, (line.Element as Line).Y2, invertedEllipseWidth, invertedEllipseHeight);
                            (line.Element as Line).Y2 += invertedEllipseHeight;
                        }
                    }
                    // horizontal line
                    else if (!sameX && sameY)
                    {
                        // X1, Y1 is start position
                        if ((line.Element as Line).X1 < (line.Element as Line).X2)
                        {
                            AddInvertedLineEllipse((line.Element as Line).X1, (line.Element as Line).Y1 - invertedEllipseHeight / 2.0, invertedEllipseWidth, invertedEllipseHeight);
                            (line.Element as Line).X1 += invertedEllipseWidth;
                        }
                        // X2, Y2 is start position
                        else
                        {
                            AddInvertedLineEllipse((line.Element as Line).X2, (line.Element as Line).Y2 - invertedEllipseHeight / 2.0, invertedEllipseWidth, invertedEllipseHeight);
                            (line.Element as Line).X2 += invertedEllipseWidth;
                        }
                    }
                    
                    // select line
                    BlockController.Select(line);
                    if (selectedBlock.Lines == null)
                    {
                        selectedBlock.Lines = new List<XLine>();
                    }
                    selectedBlock.Lines.Add(line);
                }
            }
        }

        public void InvertSelectedLineEnd()
        {
            // add for horizontal or vertical line end ellipse and shorten line
            if (BlockController.HaveSelected(selectedBlock) && selectedBlock.Lines != null && selectedBlock.Lines.Count > 0)
            {
                var block = BlockController.ShallowCopy(selectedBlock);
                FinishEdit();
                History.Register("Invert Line End");
                
                foreach (var line in block.Lines)
                {
                    bool sameX = Math.Round((line.Element as Line).X1, 1) == Math.Round((line.Element as Line).X2, 1);
                    bool sameY = Math.Round((line.Element as Line).Y1, 1) == Math.Round((line.Element as Line).Y2, 1);

                    // vertical line
                    if (sameX && !sameY)
                    {
                        // X2, Y2 is end position
                        if ((line.Element as Line).Y2 > (line.Element as Line).Y1)
                        {
                            AddInvertedLineEllipse((line.Element as Line).X2 - invertedEllipseWidth / 2.0, (line.Element as Line).Y2 - invertedEllipseHeight, invertedEllipseWidth, invertedEllipseHeight);
                            (line.Element as Line).Y2 -= invertedEllipseHeight;
                        }
                        // X1, Y1 is end position
                        else
                        {
                            AddInvertedLineEllipse((line.Element as Line).X1 - invertedEllipseWidth / 2.0, (line.Element as Line).Y1 - invertedEllipseHeight, invertedEllipseWidth, invertedEllipseHeight);
                            (line.Element as Line).Y1 -= invertedEllipseHeight;
                        }
                    }
                    // horizontal line
                    else if (!sameX && sameY)
                    {
                        // X2, Y2 is end position
                        if ((line.Element as Line).X2 > (line.Element as Line).X1)
                        {
                            AddInvertedLineEllipse((line.Element as Line).X2 - invertedEllipseWidth, (line.Element as Line).Y2 - invertedEllipseHeight / 2.0, invertedEllipseWidth, invertedEllipseHeight);
                            (line.Element as Line).X2 -= invertedEllipseWidth;
                        }
                        // X1, Y1 is end position
                        else
                        {
                            AddInvertedLineEllipse((line.Element as Line).X1 - invertedEllipseWidth, (line.Element as Line).Y1 - invertedEllipseHeight / 2.0, invertedEllipseWidth, invertedEllipseHeight);
                            (line.Element as Line).X1 -= invertedEllipseWidth;
                        }
                    }
                    
                    // select line
                    BlockController.Select(line);
                    if (selectedBlock.Lines == null)
                    {
                        selectedBlock.Lines = new List<XLine>();
                    }
                    selectedBlock.Lines.Add(line);
                }
            }
        }

        #endregion
    }
}
