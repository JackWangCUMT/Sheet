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
    public partial class SheetControl : UserControl, IPageController
    {
        #region Fields

        private SheetOptions options = null;

        private ISheet<FrameworkElement> backSheet = null;
        private ISheet<FrameworkElement> contentSheet = null;
        private ISheet<FrameworkElement> overlaySheet = null;

        private Mode mode = Mode.Selection;
        private Mode tempMode = Mode.None;

        private bool isFirstMove = true;

        private Point panStartPoint;
        private Point selectionStartPoint;

        private Line tempLine = null;
        private Ellipse tempStartEllipse = null;
        private Ellipse tempEndEllipse = null;
        private Rectangle tempRectangle = null;
        private Ellipse tempEllipse = null;
        private Rectangle tempSelectionRect = null;

        private Block contentBlock = null;
        private Block frameBlock = null;
        private Block gridBlock = null;

        private Block selectedBlock = null;

        private Size lastFinalSize = new Size();

        #endregion

        #region Properties

        public IHistoryController History { get; private set; }
        public ILibrary Library { get; set; }
        public IDatabase Database { get; set; }

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

            History = new HistoryController(this);

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

        public IEnumerable<T> ToSingle<T>(T item)
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
            backSheet = new CanvasSheet(Root.Back);
            contentSheet = new CanvasSheet(Root.Sheet);
            overlaySheet = new CanvasSheet(Root.Overlay);
        }

        private void InitBlocks()
        {
            contentBlock = new Block(-1, options.PageOriginX, options.PageOriginY, options.PageWidth, options.PageHeight, -1, "CONTENT");
            contentBlock.Init();

            frameBlock = new Block(-1, options.PageOriginX, options.PageOriginY, options.PageWidth, options.PageHeight, -1, "FRAME");
            frameBlock.Init();

            gridBlock = new Block(-1, options.PageOriginX, options.PageOriginY, options.PageWidth, options.PageHeight, -1, "GRID");
            gridBlock.Init();

            selectedBlock = new Block(-1, options.PageOriginX, options.PageOriginY, options.PageWidth, options.PageHeight, -1, "SELECTED");
        }

        private void InitLoaded()
        {
            LoadStandardPage();

            LoadStandardLibrary();

            Focus();
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
            var grid = BlockSerializer.SerializerBlockContents(gridBlock, -1, gridBlock.X, gridBlock.Y, gridBlock.Width, gridBlock.Height, -1, "GRID");
            var frame = BlockSerializer.SerializerBlockContents(frameBlock, -1, frameBlock.X, frameBlock.Y, frameBlock.Width, frameBlock.Height, -1, "FRAME");
            var content = BlockSerializer.SerializerBlockContents(contentBlock, -1, contentBlock.X, contentBlock.Y, contentBlock.Width, contentBlock.Height, -1, "CONTENT");

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
                BlockController.AddBlockContents(backSheet, grid, gridBlock, null, false, options.GridThickness / Zoom);
            }

            if (frame != null)
            {
                BlockController.AddBlockContents(backSheet, frame, frameBlock, null, false, options.FrameThickness / Zoom);
            }

            if (content != null)
            {
                BlockController.AddBlockContents(contentSheet, content, contentBlock, null, false, options.LineThickness / Zoom);
            }
        }

        public void ResetPage()
        {
            ResetOverlay();

            BlockController.RemoveBlock(backSheet, gridBlock);
            BlockController.RemoveBlock(backSheet, frameBlock);
            BlockController.RemoveBlock(contentSheet, contentBlock);

            InitBlocks();
        }

        public void ResetPageContent()
        {
            ResetOverlay();

            BlockController.RemoveBlock(contentSheet, contentBlock);
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

        public void ModeEdit()
        {
            SetMode(Mode.Edit);
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

        public void ModeImage()
        {
            SetMode(Mode.Image);
        }

        public void ModeTextEditor()
        {
            SetMode(Mode.TextEditor);
        }

        #endregion

        #region Clipboard

        public void Cut()
        {
            try
            {
                if (BlockController.HaveSelected(selectedBlock))
                {
                    History.Register("Cut");
                    Copy();
                    Delete();
                }
            }
            catch (Exception ex)
            {
                Debug.Print(ex.Message);
                Debug.Print(ex.StackTrace);
            }
        }

        public void Copy()
        {
            try
            {
                if (BlockController.HaveSelected(selectedBlock))
                {
                    var selected = BlockSerializer.SerializerBlockContents(selectedBlock, 0, 0.0, 0.0, 0.0, 0.0, -1, "SELECTED");
                    var text = ItemSerializer.SerializeContents(selected);
                    Clipboard.SetData(DataFormats.UnicodeText, text);
                    //string json = JsonConvert.SerializeObject(selected, Formatting.Indented);
                    //Clipboard.SetData(DataFormats.UnicodeText, json);
                }
            }
            catch (Exception ex)
            {
                Debug.Print(ex.Message);
                Debug.Print(ex.StackTrace);
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

        public void Delete()
        {
            if (BlockController.HaveSelected(selectedBlock))
            {
                FinishEdit();
                History.Register("Delete");
                BlockController.RemoveSelectedFromBlock(contentSheet, contentBlock, selectedBlock);
            }
        }

        #endregion

        #region Select All

        public void SelecteAll()
        {
            BlockController.SelectAll(selectedBlock, contentBlock);
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

        public void InsertContent(BlockItem block, bool select)
        {
            BlockController.DeselectAll(selectedBlock);
            BlockController.AddBlockContents(contentSheet, block, contentBlock, selectedBlock, select, options.LineThickness / Zoom);
        }

        private static string SerializeBlockContents(int id, double x, double y, double width, double height, int dataId, string name, Block parent)
        {
            var block = BlockSerializer.SerializerBlockContents(parent, id, x, y, width, height, dataId, name);
            var sb = new StringBuilder();
            ItemSerializer.Serialize(sb, block, "", ItemSerializeOptions.Default);
            return sb.ToString();
        }

        private async Task<BlockItem> CreateBlockItem(string name)
        {
            var text = SerializeBlockContents(0, 0.0, 0.0, 0.0, 0.0, -1, name, selectedBlock);
            var block = await Task.Run(() => ItemSerializer.DeserializeContents(text));
            Delete();
            InsertContent(block, true);
            return block.Blocks.FirstOrDefault();
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
                    History.Register("Create Block");
                    CreateBlockItem(name).ContinueWith((block) =>
                    {
                        AddToLibrary(block.Result);
                        EditorCanvas.Children.Remove(tc);
                        Focus();
                        RestoreTempMode();
                    }, TaskScheduler.FromCurrentSynchronizationContext());
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
                var text = ItemSerializer.SerializeContents(BlockSerializer.SerializerBlockContents(selectedBlock, 0, 0.0, 0.0, 0.0, 0.0, -1, "SELECTED"));
                var block = await Task.Run(() => ItemSerializer.DeserializeContents(text));
                History.Register("Break Block");
                Delete();
                BlockController.AddBrokenBlock(contentSheet, block, contentBlock, selectedBlock, true, options.LineThickness / Zoom);
            }
        }

        #endregion

        #region Move Mode

        private void Move(double x, double y)
        {
            if (BlockController.HaveSelected(selectedBlock))
            {
                FinishEdit();
                History.Register("Move");
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
            var temp = new Block(-1, options.PageOriginX, options.PageOriginY, options.PageWidth, options.PageHeight, -1, "TEMP");
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
                History.Register("Move");
                isFirstMove = false;
                Cursor = Cursors.SizeAll;
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

        public double GetZoom(int index)
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

            if (tempSelectionRect != null)
            {
                tempSelectionRect.StrokeThickness = selectionThicknessZoomed;
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
            Canvas.SetLeft(tempSelectionRect, Math.Min(sx, x));
            Canvas.SetTop(tempSelectionRect, Math.Min(sy, y));
            tempSelectionRect.Width = width;
            tempSelectionRect.Height = height;
        }

        private void FinishSelectionRect()
        {
            double x = Canvas.GetLeft(tempSelectionRect);
            double y = Canvas.GetTop(tempSelectionRect);
            double width = tempSelectionRect.Width;
            double height = tempSelectionRect.Height;

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

        private void InitTempLine(Point p)
        {
            double x = ItemController.Snap(p.X, options.SnapSize);
            double y = ItemController.Snap(p.Y, options.SnapSize);
            tempLine = BlockFactory.CreateLine(options.LineThickness / Zoom, x, y, x, y, ItemColors.Black);
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
                overlaySheet.ReleaseCapture();
                overlaySheet.Remove(tempLine);
                overlaySheet.Remove(tempStartEllipse);
                overlaySheet.Remove(tempEndEllipse);
                History.Register("Create Line");
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
            var temp = new Block(-1, options.PageOriginX, options.PageOriginY, options.PageWidth, options.PageHeight, -1, "TEMP");
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

                BlockController.DeselectBlock(temp);
                return true;
            }

            BlockController.DeselectBlock(temp);
            return false;
        }

        #endregion

        #region Image Mode

        public void Image(Point p)
        {
            var dlg = new Microsoft.Win32.OpenFileDialog()
            {
                Filter = "Supported Files (*.png;*.jpg;*.jpeg)|*.png;*.jpg;*.jpeg|PNG Files (*.png)|*.png|JPG Files (*.jpg)|*.jpg|JPEG Files (*.jpeg)|*.jpeg|All Files (*.*)|*.*"
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

        private Line selectedLine = null;
        private Thumb lineThumbStart = null;
        private Thumb lineThumbEnd = null;

        private FrameworkElement selectedElement = null;
        private Thumb thumbTopLeft = null;
        private Thumb thumbTopRight = null;
        private Thumb thumbBottomLeft = null;
        private Thumb thumbBottomRight = null;

        private Thumb CreateEditThumb()
        {
            var stringReader = new System.IO.StringReader(editThumbTemplate);
            var xmlReader = System.Xml.XmlReader.Create(stringReader);
            return (Thumb)XamlReader.Load(xmlReader);
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

        private void DragLineStart(Line line, Thumb thumb, double dx, double dy)
        {
            if (line != null && thumb != null)
            {
                double x = ItemController.Snap(line.X1 + dx, options.SnapSize);
                double y = ItemController.Snap(line.Y1 + dy, options.SnapSize);
                line.X1 = x;
                line.Y1 = y;
                Canvas.SetLeft(thumb, x);
                Canvas.SetTop(thumb, y);
            }
        }

        private void DragLineEnd(Line line, Thumb thumb, double dx, double dy)
        {
            if (line != null && thumb != null)
            {
                double x = ItemController.Snap(line.X2 + dx, options.SnapSize);
                double y = ItemController.Snap(line.Y2 + dy, options.SnapSize);
                line.X2 = x;
                line.Y2 = y;
                Canvas.SetLeft(thumb, x);
                Canvas.SetTop(thumb, y);
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
                    lineThumbStart.DragDelta += (sender, e) => DragLineStart(selectedLine, lineThumbStart, e.HorizontalChange, e.VerticalChange);
                }

                if (lineThumbEnd == null)
                {
                    lineThumbEnd = CreateEditThumb();
                    lineThumbEnd.DragDelta += (sender, e) => DragLineEnd(selectedLine, lineThumbEnd, e.HorizontalChange, e.VerticalChange);
                }

                Canvas.SetLeft(lineThumbStart, line.X1);
                Canvas.SetTop(lineThumbStart, line.Y1);
                Canvas.SetLeft(lineThumbEnd, line.X2);
                Canvas.SetTop(lineThumbEnd, line.Y2);

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

            Canvas.SetLeft(thumbTopLeft, tl.X);
            Canvas.SetTop(thumbTopLeft, tl.Y);

            Canvas.SetLeft(thumbTopRight, tr.X);
            Canvas.SetTop(thumbTopRight, tr.Y);

            Canvas.SetLeft(thumbBottomLeft, bl.X);
            Canvas.SetTop(thumbBottomLeft, bl.Y);

            Canvas.SetLeft(thumbBottomRight, br.X);
            Canvas.SetTop(thumbBottomRight, br.Y);
        }

        private void DragTopLeft(FrameworkElement element, Thumb thumb, double dx, double dy)
        {
            if (element != null && thumb != null)
            {
                double left = Canvas.GetLeft(element);
                double top = Canvas.GetTop(element);
                double width = element.Width;
                double height = element.Height;

                var rect = new Rect(left, top, width, height);

                rect.X = ItemController.Snap(rect.X + dx, options.SnapSize);
                rect.Y = ItemController.Snap(rect.Y + dy, options.SnapSize);

                rect.Width = Math.Max(0.0, rect.Width - (rect.X - left));
                rect.Height = Math.Max(0.0, rect.Height - (rect.Y - top));

                Canvas.SetLeft(element, rect.X);
                Canvas.SetTop(element, rect.Y);
                element.Width = rect.Width;
                element.Height = rect.Height;

                DragThumbs(rect);
            }
        }

        private void DragTopRight(FrameworkElement element, Thumb thumb, double dx, double dy)
        {
            if (element != null && thumb != null)
            {
                double left = Canvas.GetLeft(element);
                double top = Canvas.GetTop(element);
                double width = element.Width;
                double height = element.Height;

                var rect = new Rect(left, top, width, height);

                rect.Width = Math.Max(0.0, ItemController.Snap(rect.Width + dx, options.SnapSize));
                rect.Y = ItemController.Snap(rect.Y + dy, options.SnapSize);

                rect.Height = Math.Max(0.0, rect.Height - (rect.Y - top));

                Canvas.SetLeft(element, rect.X);
                Canvas.SetTop(element, rect.Y);
                element.Width = rect.Width;
                element.Height = rect.Height;

                DragThumbs(rect);
            }
        }

        private void DragBottomLeft(FrameworkElement element, Thumb thumb, double dx, double dy)
        {
            if (element != null && thumb != null)
            {
                double left = Canvas.GetLeft(element);
                double top = Canvas.GetTop(element);
                double width = element.Width;
                double height = element.Height;

                var rect = new Rect(left, top, width, height);

                rect.X = ItemController.Snap(rect.X + dx, options.SnapSize);
                rect.Height = Math.Max(0.0, ItemController.Snap(rect.Height + dy, options.SnapSize));

                rect.Width = Math.Max(0.0, rect.Width - (rect.X - left));

                Canvas.SetLeft(element, rect.X);
                Canvas.SetTop(element, rect.Y);
                element.Width = rect.Width;
                element.Height = rect.Height;

                DragThumbs(rect);
            }
        }

        private void DragBottomRight(FrameworkElement element, Thumb thumb, double dx, double dy)
        {
            if (element != null && thumb != null)
            {
                double left = Canvas.GetLeft(element);
                double top = Canvas.GetTop(element);
                double width = element.Width;
                double height = element.Height;

                var rect = new Rect(left, top, width, height);

                rect.Width = Math.Max(0.0, ItemController.Snap(rect.Width + dx, options.SnapSize));
                rect.Height = Math.Max(0.0, ItemController.Snap(rect.Height + dy, options.SnapSize));

                Canvas.SetLeft(element, rect.X);
                Canvas.SetTop(element, rect.Y);

                element.Width = rect.Width;
                element.Height = rect.Height;

                DragThumbs(rect);
            }
        }

        private void InitFrameworkElementEditor()
        {
            if (thumbTopLeft == null)
            {
                thumbTopLeft = CreateEditThumb();
                thumbTopLeft.DragDelta += (sender, e) => DragTopLeft(selectedElement, thumbTopLeft, e.HorizontalChange, e.VerticalChange);
            }

            if (thumbTopRight == null)
            {
                thumbTopRight = CreateEditThumb();
                thumbTopRight.DragDelta += (sender, e) => DragTopRight(selectedElement, thumbTopRight, e.HorizontalChange, e.VerticalChange);
            }

            if (thumbBottomLeft == null)
            {
                thumbBottomLeft = CreateEditThumb();
                thumbBottomLeft.DragDelta += (sender, e) => DragBottomLeft(selectedElement, thumbBottomLeft, e.HorizontalChange, e.VerticalChange);
            }

            if (thumbBottomRight == null)
            {
                thumbBottomRight = CreateEditThumb();
                thumbBottomRight.DragDelta += (sender, e) => DragBottomRight(selectedElement, thumbBottomRight, e.HorizontalChange, e.VerticalChange);
            }

            double left = Canvas.GetLeft(selectedElement);
            double top = Canvas.GetTop(selectedElement);
            double width = selectedElement.Width;
            double height = selectedElement.Height;

            Canvas.SetLeft(thumbTopLeft, left);
            Canvas.SetTop(thumbTopLeft, top);
            Canvas.SetLeft(thumbTopRight, left + width);
            Canvas.SetTop(thumbTopRight, top);
            Canvas.SetLeft(thumbBottomLeft, left);
            Canvas.SetTop(thumbBottomLeft, top + height);
            Canvas.SetLeft(thumbBottomRight, left + width);
            Canvas.SetTop(thumbBottomRight, top + height);

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
                    BlockController.DeselectAll(selectedBlock);
                    FinishEdit();
                }
                else
                {
                    return;
                }
            }

            // text editor
            if (GetMode() == Mode.None || GetMode() == Mode.TextEditor)
            {
                return;
            }

            // move mode
            if (!ctrl)
            {
                if (BlockController.HaveSelected(selectedBlock) && CanInitMove(e.GetPosition(overlaySheet.GetParent())))
                {
                    InitMove(e.GetPosition(overlaySheet.GetParent()));
                    return;
                }

                BlockController.DeselectAll(selectedBlock);
            }

            bool resetSelected = ctrl && BlockController.HaveSelected(selectedBlock) ? false : true;

            if (GetMode() == Mode.Selection)
            {
                bool result = BlockController.HitTestClick(contentSheet, contentBlock, selectedBlock, e.GetPosition(overlaySheet.GetParent()), options.HitTestSize, false, resetSelected);
                if ((ctrl || !BlockController.HaveSelected(selectedBlock)) && !result)
                {
                    InitSelectionRect(e.GetPosition(overlaySheet.GetParent()));
                }
                else
                {
                    // TODO: If control key is pressed then switch to move mode instead to edit mode
                    bool editModeEnabled = ctrl == true ? false : TryToEditSelected();
                    if (!editModeEnabled)
                    {
                        InitMove(e.GetPosition(overlaySheet.GetParent()));
                    }
                }
            }
            else if (GetMode() == Mode.Insert && !overlaySheet.IsCaptured)
            {
                Insert(e.GetPosition(overlaySheet.GetParent()));
            }
            else if (GetMode() == Mode.Line && !overlaySheet.IsCaptured)
            {
                InitTempLine(e.GetPosition(overlaySheet.GetParent()));
            }
            else if (GetMode() == Mode.Line && overlaySheet.IsCaptured)
            {
                FinishTempLine();
            }
            else if (GetMode() == Mode.Rectangle && !overlaySheet.IsCaptured)
            {
                InitTempRect(e.GetPosition(overlaySheet.GetParent()));
            }
            else if (GetMode() == Mode.Rectangle && overlaySheet.IsCaptured)
            {
                FinishTempRect();
            }
            else if (GetMode() == Mode.Ellipse && !overlaySheet.IsCaptured)
            {
                InitTempEllipse(e.GetPosition(overlaySheet.GetParent()));
            }
            else if (GetMode() == Mode.Ellipse && overlaySheet.IsCaptured)
            {
                FinishTempEllipse();
            }
            else if (GetMode() == Mode.Pan && overlaySheet.IsCaptured)
            {
                FinishPan();
            }
            else if (GetMode() == Mode.Text && !overlaySheet.IsCaptured)
            {
                CreateText(e.GetPosition(overlaySheet.GetParent()));
            }
            else if (GetMode() == Mode.Image && !overlaySheet.IsCaptured)
            {
                Image(e.GetPosition(overlaySheet.GetParent()));
            }
        }

        private void UserControl_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (GetMode() == Mode.Selection && overlaySheet.IsCaptured)
            {
                FinishSelectionRect();
            }
            else if (GetMode() == Mode.Move && overlaySheet.IsCaptured)
            {
                FinishMove();
            }
        }

        private void UserControl_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (GetMode() == Mode.Edit)
            {
                return;
            }

            bool shift = (Keyboard.Modifiers == ModifierKeys.Shift);

            // mouse over selection when holding Shift key
            if (shift && tempSelectionRect == null && !overlaySheet.IsCaptured)
            {
                if (BlockController.HaveSelected(selectedBlock))
                {
                    BlockController.DeselectAll(selectedBlock);
                }

                BlockController.HitTestClick(contentSheet, contentBlock, selectedBlock, e.GetPosition(overlaySheet.GetParent()), options.HitTestSize, false, false);
            }

            if (GetMode() == Mode.Selection && overlaySheet.IsCaptured)
            {
                MoveSelectionRect(e.GetPosition(overlaySheet.GetParent()));
            }
            else if (GetMode() == Mode.Line && overlaySheet.IsCaptured)
            {
                MoveTempLine(e.GetPosition(overlaySheet.GetParent()));
            }
            else if (GetMode() == Mode.Rectangle && overlaySheet.IsCaptured)
            {
                MoveTempRect(e.GetPosition(overlaySheet.GetParent()));
            }
            else if (GetMode() == Mode.Ellipse && overlaySheet.IsCaptured)
            {
                MoveTempEllipse(e.GetPosition(overlaySheet.GetParent()));
            }
            else if (GetMode() == Mode.Pan && overlaySheet.IsCaptured)
            {
                Pan(e.GetPosition(this));
            }
            else if (GetMode() == Mode.Move && overlaySheet.IsCaptured)
            {
                Move(e.GetPosition(overlaySheet.GetParent()));
            }
        }

        private void UserControl_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            Focus();

            if (GetMode() == Mode.None || GetMode() == Mode.TextEditor)
            {
                return;
            }

            // edit mode
            if (selectedType != ItemType.None)
            {
                BlockController.DeselectAll(selectedBlock);
                FinishEdit();
                return;
            }

            // text editor
            if (TryToEditText(e.GetPosition(overlaySheet.GetParent())))
            {
                e.Handled = true;
                return;
            }

            BlockController.DeselectAll(selectedBlock);

            if (GetMode() == Mode.Selection && overlaySheet.IsCaptured)
            {
                CancelSelectionRect();
            }
            else if (GetMode() == Mode.Line && overlaySheet.IsCaptured)
            {
                CancelTempLine();
            }
            else if (GetMode() == Mode.Rectangle && overlaySheet.IsCaptured)
            {
                CancelTempRect();
            }
            else if (GetMode() == Mode.Ellipse && overlaySheet.IsCaptured)
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
            if (GetMode() == Mode.Pan && overlaySheet.IsCaptured)
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
            var temp = new Block(-1, options.PageOriginX, options.PageOriginY, options.PageWidth, options.PageHeight, -1, "TEMP");
            BlockController.HitTestForBlocks(contentSheet, contentBlock, temp, p, options.HitTestSize);

            if (BlockController.HaveOneBlockSelected(temp))
            {
                History.Register("Bind Data");
                var block = temp.Blocks[0];
                BindDataToBlock(block, dataItem);
                BlockController.DeselectBlock(temp);
                return true;
            }

            BlockController.DeselectBlock(temp);
            return false;
        }

        private bool BindDataToBlock(Block block, DataItem dataItem)
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
                        var temp = new Block(-1, options.PageOriginX, options.PageOriginY, options.PageWidth, options.PageHeight, -1, "TEMP");
                        temp.Init();
                        temp.Blocks.Add(block);
                        BlockController.RemoveSelectedFromBlock(contentSheet, contentBlock, temp);
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
                    Insert(blockItem, e.GetPosition(overlaySheet.GetParent()), true);
                    e.Handled = true;
                }
            }
            else if (e.Data.GetDataPresent("Data"))
            {
                var dataItem = e.Data.GetData("Data") as DataItem;
                if (dataItem != null)
                {
                    TryToBindData(e.GetPosition(overlaySheet.GetParent()), dataItem);
                    e.Handled = true;
                }
            }
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
                //InsertContent(page, false);
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
                InsertContent(page, false);
            }
        }

        public async void Open()
        {
            var dlg = new Microsoft.Win32.OpenFileDialog()
            {
                Filter = "TXT Files (*.txt)|*.txt|JSON Files (*.json)|*.json|All Files (*.*)|*.*"
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
                Filter = "TXT Files (*.txt)|*.txt|JSON Files (*.json)|*.json|All Files (*.*)|*.*",
                FileName = "sheet"
            };

            if (dlg.ShowDialog() == true)
            {
                BlockController.DeselectAll(selectedBlock);

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
                Filter = "PDF Documents (*.pdf)|*.pdf|DXF Documents (*.dxf)|*.dxf|All Files (*.*)|*.*",
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
            var block = BlockSerializer.SerializerBlockContents(contentBlock, 0, 
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

        private Block Insert(BlockItem blockItem, Point p, bool select)
        {
            BlockController.DeselectAll(selectedBlock);
            double thickness = options.LineThickness / Zoom;

            if (select)
            {
                selectedBlock.Blocks = new List<Block>();
            }

            History.Register("Insert Block");

            var block = BlockSerializer.DeserializeBlockItem(contentSheet, contentBlock, blockItem, select, thickness);

            if (select)
            {
                selectedBlock.Blocks.Add(block);
            }

            BlockController.Move(ItemController.Snap(p.X, options.SnapSize), ItemController.Snap(p.Y, options.SnapSize), block);

            return block;
        }

        private async void LoadStandardLibrary()
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
                    string text = await reader.ReadToEndAsync();
                    if (text != null)
                    {
                        InitLibrary(text);
                    }
                }
            }
        }

        private async void LoadLibrary(string fileName)
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

        public void LoadLibrary()
        {
            var dlg = new Microsoft.Win32.OpenFileDialog()
            {
                Filter = "TXT Files (*.txt)|*.txt|All Files (*.*)|*.*"
            };

            if (dlg.ShowDialog() == true)
            {
                try
                {
                    LoadLibrary(dlg.FileName);
                }
                catch (Exception ex)
                {
                    Debug.Print(ex.Message);
                    Debug.Print(ex.StackTrace);
                }
            }
        }

        #endregion

        #region Preview

        public void ShowPreview()
        {
            var window = new Window()
            {
                Title = "Preview",
                SizeToContent = SizeToContent.WidthAndHeight,
                WindowStartupLocation = WindowStartupLocation.CenterScreen
            };

            var canvas = new CanvasControl();
            var sheet = new CanvasSheet(canvas.Sheet);

            PageFactory.CreateGrid(sheet, null, 330.0, 30.0, 600.0, 750.0, options.GridSize, options.GridThickness, ItemColors.LightGray);
            PageFactory.CreateFrame(sheet, null, options.GridSize, options.GridThickness, ItemColors.DarkGray);

            var blockItem = BlockSerializer.SerializerBlockContents(contentBlock, 0, 0.0, 0.0, 0.0, 0.0, -1, "PREVIEW");
            BlockController.AddBlockContents(sheet, blockItem, null, null, false, options.LineThickness);

            window.Content = canvas;
            window.Show();
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

        private static BlockItem CreateGridBlock(Block gridBlock, bool adjustThickness, bool adjustColor)
        {
            var grid = BlockSerializer.SerializerBlockContents(gridBlock, -1, 0.0, 0.0, 0.0, 0.0, -1, "GRID");

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

        private static BlockItem CreateFrameBlock(Block frameBlock, bool adjustThickness, bool adjustColor)
        {
            var frame = BlockSerializer.SerializerBlockContents(frameBlock, -1, 0.0, 0.0, 0.0, 0.0, -1, "FRAME");

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
            var ellipse = BlockFactory.CreateEllipse(options.LineThickness / Zoom, x, y, width, height, false);
            contentBlock.Ellipses.Add(ellipse);
            contentSheet.Add(ellipse);

            BlockController.SelectEllipse(ellipse);
            if (selectedBlock.Ellipses == null)
            {
                selectedBlock.Ellipses = new List<Ellipse>();
            }
            selectedBlock.Ellipses.Add(ellipse);
        }

        public void InvertSelectedLineStart()
        {
            // add for horizontal or vertical line start ellipse and shorten line
            if (BlockController.HaveSelected(selectedBlock) && selectedBlock.Lines != null && selectedBlock.Lines.Count > 0)
            {
                History.Register("Invert Line Start");

                foreach (var line in selectedBlock.Lines)
                {
                    bool sameX = Math.Round(line.X1, 1) == Math.Round(line.X2, 1);
                    bool sameY = Math.Round(line.Y1, 1) == Math.Round(line.Y2, 1);

                    // vertical line
                    if (sameX && !sameY)
                    {
                        // X1, Y1 is start position
                        if (line.Y1 < line.Y2)
                        {
                            AddInvertedLineEllipse(line.X1 - invertedEllipseWidth / 2.0, line.Y1, invertedEllipseWidth, invertedEllipseHeight);
                            line.Y1 += invertedEllipseHeight;
                        }
                        // X2, Y2 is start position
                        else
                        {
                            AddInvertedLineEllipse(line.X2 - invertedEllipseWidth / 2.0, line.Y2, invertedEllipseWidth, invertedEllipseHeight);
                            line.Y2 += invertedEllipseHeight;
                        }
                    }
                    // horizontal line
                    else if (!sameX && sameY)
                    {
                        // X1, Y1 is start position
                        if (line.X1 < line.X2)
                        {
                            AddInvertedLineEllipse(line.X1, line.Y1 - invertedEllipseHeight / 2.0, invertedEllipseWidth, invertedEllipseHeight);
                            line.X1 += invertedEllipseWidth;
                        }
                        // X2, Y2 is start position
                        else
                        {
                            AddInvertedLineEllipse(line.X2, line.Y2 - invertedEllipseHeight / 2.0, invertedEllipseWidth, invertedEllipseHeight);
                            line.X2 += invertedEllipseWidth;
                        }
                    }
                }
            }
        }

        public void InvertSelectedLineEnd()
        {
            // add for horizontal or vertical line end ellipse and shorten line
            if (BlockController.HaveSelected(selectedBlock) && selectedBlock.Lines != null && selectedBlock.Lines.Count > 0)
            {
                History.Register("Invert Line End");

                foreach (var line in selectedBlock.Lines)
                {
                    bool sameX = Math.Round(line.X1, 1) == Math.Round(line.X2, 1);
                    bool sameY = Math.Round(line.Y1, 1) == Math.Round(line.Y2, 1);

                    // vertical line
                    if (sameX && !sameY)
                    {
                        // X2, Y2 is end position
                        if (line.Y2 > line.Y1)
                        {
                            AddInvertedLineEllipse(line.X2 - invertedEllipseWidth / 2.0, line.Y2 - invertedEllipseHeight, invertedEllipseWidth, invertedEllipseHeight);
                            line.Y2 -= invertedEllipseHeight;
                        }
                        // X1, Y1 is end position
                        else
                        {
                            AddInvertedLineEllipse(line.X1 - invertedEllipseWidth / 2.0, line.Y1 - invertedEllipseHeight, invertedEllipseWidth, invertedEllipseHeight);
                            line.Y1 -= invertedEllipseHeight;
                        }
                    }
                    // horizontal line
                    else if (!sameX && sameY)
                    {
                        // X2, Y2 is end position
                        if (line.X2 > line.X1)
                        {
                            AddInvertedLineEllipse(line.X2 - invertedEllipseWidth, line.Y2 - invertedEllipseHeight / 2.0, invertedEllipseWidth, invertedEllipseHeight);
                            line.X2 -= invertedEllipseWidth;
                        }
                        // X1, Y1 is end position
                        else
                        {
                            AddInvertedLineEllipse(line.X1 - invertedEllipseWidth, line.Y1 - invertedEllipseHeight / 2.0, invertedEllipseWidth, invertedEllipseHeight);
                            line.X1 -= invertedEllipseWidth;
                        }
                    }
                }
            }
        }

        #endregion
    }
}
