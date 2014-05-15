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
    #region History

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

    public class CanvasHistoryController : IHistoryController
    {
        #region Properties

        public IBlockController BlockController { get; private set; }

        #endregion

        #region Fields

        private Stack<ChangeMessage> undos = new Stack<ChangeMessage>();
        private Stack<ChangeMessage> redos = new Stack<ChangeMessage>();

        #endregion

        #region Constructor

        public CanvasHistoryController(IBlockController blockController)
        {
            BlockController = blockController;
        }

        #endregion

        #region Undo/Redo Changes

        private async Task<ChangeMessage> CreateChangeMessage(string message)
        {
            var block = BlockController.Serialize();
            var text = await Task.Run(() => ItemSerializer.SerializeContents(block));
            var change = new ChangeMessage()
            {
                Message = message,
                Model = text
            };
            return change;
        }

        public async void Register(string message)
        {
            var change = await CreateChangeMessage(message);
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
                var change = await CreateChangeMessage("Redo");
                redos.Push(change);
                var undo = undos.Pop();
                var block = await Task.Run(() => ItemSerializer.DeserializeContents(undo.Model));
                BlockController.Reset();
                BlockController.Insert(block);
            }
        }

        public async void Redo()
        {
            if (redos.Count > 0)
            {
                var change = await CreateChangeMessage("Undo");
                undos.Push(change);
                var redo = redos.Pop();
                var block = await Task.Run(() => ItemSerializer.DeserializeContents(redo.Model));
                BlockController.Reset();
                BlockController.Insert(block);
            }
        }

        #endregion
    }

    #endregion

    public partial class SheetControl : UserControl, IEntryController, IBlockController
    {
        #region Fields

        private SheetOptions options = null;

        private ISheet backSheet = null;
        private ISheet logicSheet = null;
        private ISheet overlaySheet = null;

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

        private Block logicBlock = null;
        private Block selectedBlock = null;

        private Block frameBlock = null;
        private Block gridBlock = null;

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

        #endregion

        #region Constructor

        public SheetControl()
        {
            InitializeComponent();

            History = new CanvasHistoryController(this);

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
            InitOptions();
            InitCanvas();
            InitContentBlocks();
            InitPageBlocks();
        }

        private void InitOptions()
        {
            options = DefaultOptions();
            zoomIndex = options.DefaultZoomIndex;
        }

        private void InitCanvas()
        {
            backSheet = new CanvasSheet(Root.Back);
            logicSheet = new CanvasSheet(Root.Sheet);
            overlaySheet = new CanvasSheet(Root.Overlay);
        }

        private void InitContentBlocks()
        {
            logicBlock = new Block(0, 0.0, 0.0, -1, "LOGIC");
            logicBlock.Init();

            selectedBlock = new Block(0, 0.0, 0.0, -1, "SELECTED");
        }

        private void InitPageBlocks()
        {
            frameBlock = new Block(0, 0.0, 0.0, -1, "FRAME");
            frameBlock.Init();

            gridBlock = new Block(0, 0.0, 0.0, -1, "GRID");
            gridBlock.Init();
        }

        private void InitLoaded()
        {
            CreateGrid(backSheet, gridBlock, 330.0, 30.0, 600.0, 750.0, options.GridSize, options.GridThickness, BlockFactory.GridBrush);
            CreateFrame(backSheet, frameBlock, options.GridSize, options.GridThickness, BlockFactory.FrameBrush);
            AdjustThickness(gridBlock, options.GridThickness / GetZoom(zoomIndex));
            AdjustThickness(frameBlock, options.FrameThickness / GetZoom(zoomIndex));
            LoadStandardLibrary();
            Focus();
        }

        #endregion

        #region IEntryController

        public IEnumerable<BlockItem> ToSingle(BlockItem block)
        {
            yield return block;
        }

        public async void Set(string text)
        {
            try
            {
                if (text == null)
                {
                    History.Reset();
                    Reset();
                }
                else
                {
                    var block = await Task.Run(() => ItemSerializer.DeserializeContents(text));
                    History.Reset();
                    Reset();
                    InsertBlock(block, false);
                }
            }
            catch (Exception ex)
            {
                Debug.Print(ex.Message);
                Debug.Print(ex.StackTrace);
            }
        }

        public string Get()
        {
            var block = Serialize();
            var text = ItemSerializer.SerializeContents(block);
            return text;
        }

        public void Export(string text)
        {
            var block = ItemSerializer.DeserializeContents(text);
            Export(ToSingle(block));
        }

        public void Export(IEnumerable<string> texts)
        {
            var blocks = texts.Select(text => ItemSerializer.DeserializeContents(text));
            Export(blocks);
        }

        #endregion

        #region IBlockController

        public BlockItem Serialize()
        {
            return BlockSerializer.SerializerBlockContents(logicBlock, 0, logicBlock.X, logicBlock.Y, logicBlock.DataId, "LOGIC");
        }

        public void Insert(BlockItem block)
        {
            InsertBlock(block, false);
        }

        public void Reset()
        {
            ResetOverlay();

            BlockEditor.RemoveBlock(logicSheet, logicBlock);

            InitContentBlocks();
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

        public void ModeTextEditor()
        {
            SetMode(Mode.TextEditor);
        }

        #endregion

        #region Back

        private static void CreateLine(ISheet sheet, List<Line> lines, double thickness, double x1, double y1, double x2, double y2, Brush stroke)
        {
            var line = BlockFactory.CreateLine(thickness, x1, y1, x2, y2, stroke);

            if (lines != null)
            {
                lines.Add(line);
            }

            if (sheet != null)
            {
                sheet.Add(line); 
            }
        }

        private static void CreateText(ISheet sheet, List<Grid> texts, string content, double x, double y, double width, double height, HorizontalAlignment halign, VerticalAlignment valign, double size, Brush foreground)
        {
            var text = BlockFactory.CreateText(content, x, y, width, height, halign, valign, size, BlockFactory.TransparentBrush, foreground);

            if (texts != null)
            {
                texts.Add(text);
            }

            if (sheet != null)
            {
                sheet.Add(text);
            }
        }

        private static void CreateFrame(ISheet sheet, Block block, double size, double thickness, Brush stroke)
        {
            double padding = 6.0;
            double width = 1260.0;
            double height = 891.0;
            double startX = padding;
            double startY = padding;
            double rowsStart = 60;
            double rowsEnd = 780.0;

            // frame left rows
            int leftRowNumber = 1;
            for (double y = rowsStart; y < rowsEnd; y += size)
            {
                CreateLine(sheet, block.Lines, thickness, startX, y, 330.0, y, stroke);
                CreateText(sheet, block.Texts, leftRowNumber.ToString("00"), startX, y, 30.0 - padding, size, HorizontalAlignment.Center, VerticalAlignment.Center, 14.0, stroke);
                leftRowNumber++;
            }

            // frame right rows
            int rightRowNumber = 1;
            for (double y = rowsStart; y < rowsEnd; y += size)
            {
                CreateLine(sheet, block.Lines, thickness, 930.0, y, 1260.0 - padding, y, stroke);
                CreateText(sheet, block.Texts, rightRowNumber.ToString("00"), 1260.0 - 30.0, y, 30.0 - padding, size, HorizontalAlignment.Center, VerticalAlignment.Center, 14.0, stroke);
                rightRowNumber++;
            }

            // frame columns
            double[] columnWidth = {  30.0, 210.0,   90.0,  600.0, 210.0,  90.0 };
            double[] columnX     = {  30.0,  30.0, startY, startY,  30.0,  30.0 };
            double[] columnY     = {  rowsEnd,  rowsEnd,   rowsEnd,   rowsEnd,  rowsEnd,  rowsEnd };

            double start = 0.0;
            for(int i = 0; i < columnWidth.Length; i++)
            {
                start += columnWidth[i];
                CreateLine(sheet, block.Lines, thickness, start, columnX[i], start, columnY[i], stroke);
            }

            // frame header
            CreateLine(sheet, block.Lines, thickness, startX, 30.0, width - padding, 30.0, stroke);
            
            // frame footer
            CreateLine(sheet, block.Lines, thickness, startX, rowsEnd, width - padding, rowsEnd, stroke);

            // frame border
            CreateLine(sheet, block.Lines, thickness, startX, startY, width - padding, startY, stroke);
            CreateLine(sheet, block.Lines, thickness, startX, height - padding, width - padding, height - padding, stroke);
            CreateLine(sheet, block.Lines, thickness, startX, startY, startX, height - padding, stroke);
            CreateLine(sheet, block.Lines, thickness, width - padding, startY, width - padding, height - padding, stroke);
        }

        private static void CreateGrid(ISheet sheet, Block block, double startX, double startY, double width, double height, double size, double thickness, Brush stroke)
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

        #endregion

        #region Block

        public void InsertBlock(BlockItem block, bool select)
        {
            BlockEditor.DeselectAll(selectedBlock);
            BlockEditor.AddBlockContents(logicSheet, block, logicBlock, selectedBlock, select, options.LineThickness / Zoom);
        }

        private static string SerializeBlockContents(int id, double x, double y, int dataId, string name, Block parent)
        {
            var block = BlockSerializer.SerializerBlockContents(parent, id, x, y, dataId, name);
            var sb = new StringBuilder();
            ItemSerializer.Serialize(sb, block, "", ItemSerializeOptions.Default);
            return sb.ToString();
        }

        private async Task<BlockItem> CreateBlockItem(string name)
        {
            var text = SerializeBlockContents(0, 0.0, 0.0, -1, name, selectedBlock);
            var block = await Task.Run(() => ItemSerializer.DeserializeContents(text));
            Delete();
            InsertBlock(block, true);
            return block.Blocks.FirstOrDefault();
        }

        public void CreateBlock()
        {
            if (BlockEditor.HaveSelected(selectedBlock))
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
            if (BlockEditor.HaveSelected(selectedBlock))
            {
                var text = ItemSerializer.SerializeContents(BlockSerializer.SerializerBlockContents(selectedBlock, 0, 0.0, 0.0, -1, "SELECTED"));
                var block = await Task.Run(() => ItemSerializer.DeserializeContents(text));
                History.Register("Break Block");
                Delete();
                BlockEditor.AddBrokenBlock(logicSheet, block, logicBlock, selectedBlock, true, options.LineThickness / Zoom);
            }
        }

        #endregion

        #region Clipboard

        public void Cut()
        {
            try
            {
                if (BlockEditor.HaveSelected(selectedBlock))
                {
                    History.Register("Cut");
                    Copy();
                    Delete();
                }
            }
            catch(Exception ex)
            {
                Debug.Print(ex.Message);
                Debug.Print(ex.StackTrace);
            }
        }

        public void Copy()
        {
            try
            {
                var block = BlockEditor.HaveSelected(selectedBlock) ?
                    BlockSerializer.SerializerBlockContents(selectedBlock, 0, 0.0, 0.0, -1, "SELECTED") : Serialize();
                var text = ItemSerializer.SerializeContents(block);
                Clipboard.SetData(DataFormats.UnicodeText, text);
                //string json = JsonConvert.SerializeObject(block, Formatting.Indented);
                //Clipboard.SetData(DataFormats.UnicodeText, json);
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
                InsertBlock(block, true);
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
            BlockEditor.DeselectAll(selectedBlock);
            double thickness = options.LineThickness / Zoom;

            if (select)
            {
                selectedBlock.Blocks = new List<Block>();
            }

            History.Register("Insert Block");

            var block = BlockSerializer.DeserializeBlockItem(logicSheet, logicBlock, blockItem, select, thickness);

            if (select)
            {
                selectedBlock.Blocks.Add(block);
            }

            BlockEditor.Move(ItemEditor.Snap(p.X, options.SnapSize), ItemEditor.Snap(p.Y, options.SnapSize), block);

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
            var text = await ItemEditor.OpenText(fileName);
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
                ItemEditor.ResetPosition(blockItem, options.PageOriginX, options.PageOriginY, options.PageWidth, options.PageHeight);
                items.Add(blockItem);
                Library.SetSource(items);
            }
        }

        #endregion

        #region Reset Overlay

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
            if (BlockEditor.HaveSelected(selectedBlock))
            {
                FinishEdit();
                History.Register("Delete");
                BlockEditor.RemoveSelectedFromBlock(logicSheet, logicBlock, selectedBlock);
            }
        }

        #endregion

        #region Move

        private void Move(double x, double y)
        {
            if (BlockEditor.HaveSelected(selectedBlock))
            {
                FinishEdit();
                History.Register("Move");
                BlockEditor.Move(x, y, selectedBlock);
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

        #endregion

        #region Select

        public void SelecteAll()
        {
            BlockEditor.SelectAll(selectedBlock, logicBlock);
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

        #region Move Mode

        private bool CanInitMove(Point p)
        {
            var temp = new Block(0, 0.0, 0.0, -1, "TEMP");
            BlockEditor.HitTestClick(logicSheet, selectedBlock, temp, p, options.HitTestSize, false, true);

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

            p.X = ItemEditor.Snap(p.X, options.SnapSize);
            p.Y = ItemEditor.Snap(p.Y, options.SnapSize);

            double dx = p.X - panStartPoint.X;
            double dy = p.Y - panStartPoint.Y;

            if (dx != 0.0 || dy != 0.0)
            {
                BlockEditor.Move(dx, dy, selectedBlock);
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

            AdjustThickness(gridBlock, gridThicknessZoomed);
            AdjustThickness(frameBlock, frameThicknessZoomed);
            AdjustThickness(logicBlock, lineThicknessZoomed);

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
                else if(zoomIndex == -1)
                {
                    for (int i = 0; i < options.ZoomFactors.Length; i++)
                    {
                        if (options.ZoomFactors[i] > lastFactor)
                        {
                            zoomIndex = i;
                            break;
                        }
                    }
                    ZoomTo(p.X, p.Y, zoomIndex + 1);
                }
            }
            else
            {
                if (zoomIndex > 0)
                {
                    ZoomTo(p.X, p.Y, zoomIndex--);
                }
                else if (zoomIndex == -1)
                {
                    for (int i = 0; i < options.ZoomFactors.Length; i++)
                    {
                        if (options.ZoomFactors[i] > lastFactor)
                        {
                            zoomIndex = i - 1;
                            break;
                        }
                    }
                    ZoomTo(p.X, p.Y, zoomIndex + 1);
                }
            }
        }

        public double GetZoom(int index)
        {
            if (index >= 0)
            {
                return options.ZoomFactors[index];
            }
            else
            {
                return lastFactor;
            }
        }

        private double lastFactor = 1.0;
        private Size lastFinalSize = new Size();

        public void AutoFit(Size finalSize)
        {
            lastFinalSize = finalSize;

            // calculate factor
            double fwidth = finalSize.Width / options.PageWidth;
            double fheight = finalSize.Height / options.PageHeight;
            double factor = Math.Min(fwidth, fheight);
            double panX = (finalSize.Width - (options.PageWidth * factor)) / 2.0;
            double panY = (finalSize.Height - (options.PageHeight * factor)) / 2.0;

            double dx = Math.Max(0, (finalSize.Width - DesiredSize.Width) / 2.0);
            double dy = Math.Max(0, (finalSize.Height - DesiredSize.Height) / 2.0);

            // adjust zoom
            zoomIndex = -1;
            Zoom = factor;

            // adjust pan
            PanX = panX - dx;
            PanY = panY - dy;

            lastFactor = factor;
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
            tempSelectionRect = BlockFactory.CreateSelectionRectangle(options.SelectionThickness / Zoom, x, y, 0.0, 0.0);
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
            bool resetSelected = ctrl && BlockEditor.HaveSelected(selectedBlock) ? false : true;
            BlockEditor.HitTestSelectionRect(logicSheet, logicBlock, selectedBlock, new Rect(x, y, width, height), resetSelected);

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
            double x = ItemEditor.Snap(p.X, options.SnapSize);
            double y = ItemEditor.Snap(p.Y, options.SnapSize);
            tempLine = BlockFactory.CreateLine(options.LineThickness / Zoom, x, y, x, y, BlockFactory.NormalBrush);
            tempStartEllipse = BlockFactory.CreateEllipse(options.LineThickness / Zoom, x - 4.0, y - 4.0, 8.0, 8.0, true);
            tempEndEllipse = BlockFactory.CreateEllipse(options.LineThickness / Zoom, x - 4.0, y - 4.0, 8.0, 8.0, true);
            overlaySheet.Add(tempLine);
            overlaySheet.Add(tempStartEllipse);
            overlaySheet.Add(tempEndEllipse);
            overlaySheet.Capture();
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
                overlaySheet.ReleaseCapture();
                overlaySheet.Remove(tempLine);
                overlaySheet.Remove(tempStartEllipse);
                overlaySheet.Remove(tempEndEllipse);
                History.Register("Create Line");
                logicBlock.Lines.Add(tempLine);
                logicSheet.Add(tempLine);
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
            double x = ItemEditor.Snap(p.X, options.SnapSize);
            double y = ItemEditor.Snap(p.Y, options.SnapSize);
            selectionStartPoint = new Point(x, y);
            tempRectangle = BlockFactory.CreateRectangle(options.LineThickness / Zoom, x, y, 0.0, 0.0, true);
            overlaySheet.Add(tempRectangle);
            overlaySheet.Capture();
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
                overlaySheet.ReleaseCapture();
                overlaySheet.Remove(tempRectangle);
                History.Register("Create Rectangle");
                logicBlock.Rectangles.Add(tempRectangle);
                logicSheet.Add(tempRectangle);
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
            double x = ItemEditor.Snap(p.X, options.SnapSize);
            double y = ItemEditor.Snap(p.Y, options.SnapSize);
            selectionStartPoint = new Point(x, y);
            tempEllipse = BlockFactory.CreateEllipse(options.LineThickness / Zoom, x, y, 0.0, 0.0, true);
            overlaySheet.Add(tempEllipse);
            overlaySheet.Capture();
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
                overlaySheet.ReleaseCapture();
                overlaySheet.Remove(tempEllipse);
                History.Register("Create Ellipse");
                logicBlock.Ellipses.Add(tempEllipse);
                logicSheet.Add(tempEllipse);
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
            double x = ItemEditor.Snap(p.X, options.SnapSize);
            double y = ItemEditor.Snap(p.Y, options.SnapSize);
            History.Register("Create Text");
            var text = BlockFactory.CreateText("Text", x, y, 30.0, 15.0, HorizontalAlignment.Center, VerticalAlignment.Center, 11.0, BlockFactory.TransparentBrush, BlockFactory.NormalBrush);
            logicBlock.Texts.Add(text);
            logicSheet.Add(text);
        }

        private bool TryToEditText(Point p)
        {
            var temp = new Block(0, 0.0, 0.0, -1, "TEMP");
            BlockEditor.HitTestClick(logicSheet, logicBlock, temp, p, options.HitTestSize, true, true);

            if (BlockEditor.HaveOneTextSelected(temp))
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

                BlockEditor.DeselectBlock(temp);
                return true;
            }

            BlockEditor.DeselectBlock(temp);
            return false;
        }

        #endregion

        #region Preview

        public void Preview()
        {
            var window = new Window()
            {
                Title = "Preview",
                SizeToContent = SizeToContent.WidthAndHeight,
                WindowStartupLocation = WindowStartupLocation.CenterScreen
            };

            var canvas = new CanvasControl();
            var sheet = new CanvasSheet(canvas.Sheet);

            CreateGrid(sheet, null, 330.0, 30.0, 600.0, 750.0, options.GridSize, options.GridThickness, BlockFactory.GridBrush);
            CreateFrame(sheet, null, options.GridSize, options.GridThickness, BlockFactory.NormalBrush);

            var blockItem = BlockSerializer.SerializerBlockContents(logicBlock, 0, 0.0, 0.0, -1, "PREVIEW");
            BlockEditor.AddBlockContents(sheet, blockItem, null, null, false, options.LineThickness);

            window.Content = canvas;
            window.Show();
        }

        #endregion

        #region Export

        private IEnumerable<BlockItem> CreateFromCurrent()
        {
            var block = BlockSerializer.SerializerBlockContents(logicBlock, 0, logicBlock.X, logicBlock.Y, logicBlock.DataId, "LOGIC");
            return ToSingle(block);
        }

        private BlockItem CreateGridBlock()
        {
            var grid = new BlockItem();
            grid.Init(0, 0.0, 0.0, -1, "");

            foreach (var line in gridBlock.Lines)
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
            frame.Init(0, 0.0, 0.0, -1, "");

            foreach (var line in frameBlock.Lines)
            {
                var lineItem = BlockSerializer.SerializeLine(line);
                lineItem.StrokeThickness = 0.018 * 72.0 / 2.54; // 0.18mm
                lineItem.Stroke = new ItemColor() { Alpha = 255, Red = 0, Green = 0, Blue = 0 };
                frame.Lines.Add(lineItem);
            }
            return frame;
        }

        private BlockItem CreatePage(BlockItem block, bool enableFrame, bool enableGrid)
        {
            var page = new BlockItem();
            page.Init(0, 0.0, 0.0, -1, "");

            if (enableGrid)
            {
                var grid = CreateGridBlock();
                page.Blocks.Add(grid);
            }

            if (enableFrame)
            {
                var frame = CreateFrameBlock();
                page.Blocks.Add(frame);
            }

            page.Blocks.Add(block);

            return page;
        }

        #endregion

        #region Export To Pdf

        private void ExportToPdf(IEnumerable<BlockItem> blocks, string fileName)
        {
            var pages = blocks.Select(block => CreatePage(block, true, false)).ToList();

            Task.Run(() =>
            {
                var writer = new BlockPdfWriter();
                writer.Create(fileName, options.PageWidth, options.PageHeight, pages);
            });
        }

        #endregion

        #region Export To Dxf

        private void ExportToDxf(IEnumerable<BlockItem> blocks, string fileName)
        {
            var pages = blocks.Select(block => CreatePage(block, false, false)).ToList();

            Task.Run(() =>
            {
                var writer = new BlockDxfWriter();

                if (blocks.Count() > 1)
                {
                    string path = System.IO.Path.GetDirectoryName(fileName);
                    string name = System.IO.Path.GetFileNameWithoutExtension(fileName);
                    string extension = System.IO.Path.GetExtension(fileName);

                    int counter = 0;
                    foreach(var page in pages)
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

        #endregion

        #region File Dialogs

        public async void Open()
        {
            var dlg = new Microsoft.Win32.OpenFileDialog()
            {
                Filter = "TXT Files (*.txt)|*.txt|JSON Files (*.json)|*.json|All Files (*.*)|*.*"
            };

            if (dlg.ShowDialog() == true)
            {
                var text = await ItemEditor.OpenText(dlg.FileName);
                if (text != null)
                {
                    switch (dlg.FilterIndex)
                    {
                        case 1:
                            {
                                try
                                {
                                    var block = await Task.Run(() => ItemSerializer.DeserializeContents(text));
                                    History.Register("Open");
                                    Reset();
                                    InsertBlock(block, false);
                                }
                                catch(Exception ex)
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
                                     var block = await Task.Run(() => JsonConvert.DeserializeObject<BlockItem>(text));
                                     History.Register("Open");
                                     Reset();
                                     InsertBlock(block, false);
                                }
                                catch(Exception ex)
                                {
                                    Debug.Print(ex.Message);
                                    Debug.Print(ex.StackTrace);
                                }
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
                            try
                            {
                                var block = Serialize();

                                Task.Run(() =>
                                {
                                    var text = ItemSerializer.SerializeContents(block);
                                    ItemEditor.SaveText(dlg.FileName, text);
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
                                var block = Serialize();

                                Task.Run(() =>
                                {
                                    string text = JsonConvert.SerializeObject(block, Formatting.Indented);
                                    ItemEditor.SaveText(dlg.FileName, text);
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

        public void Export()
        {
            var blocks = CreateFromCurrent();
            Export(blocks);
        }

        public void Export(SolutionEntry solution)
        {
            var texts = solution.Documents.SelectMany(document => document.Pages).Select(page => page.Content);
            Export(texts);
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
                switch(dlg.FilterIndex)
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

        public void Load()
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
            if (BlockEditor.HaveOneLineSelected(selectedBlock))
            {
                InitLineEditor();
                return true;
            }
            else if (BlockEditor.HaveOneRectangleSelected(selectedBlock))
            {
                InitRectangleEditor();
                return true;
            }
            else if (BlockEditor.HaveOneEllipseSelected(selectedBlock))
            {
                InitEllipseEditor();
                return true;
            }
            else if (BlockEditor.HaveOneTextSelected(selectedBlock))
            {
                InitTextEditor();
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
                double x = ItemEditor.Snap(line.X1 + dx, options.SnapSize);
                double y = ItemEditor.Snap(line.Y1 + dy, options.SnapSize);
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
                double x = ItemEditor.Snap(line.X2 + dx, options.SnapSize);
                double y = ItemEditor.Snap(line.Y2 + dy, options.SnapSize);
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
            catch(Exception ex)
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

                rect.X = ItemEditor.Snap(rect.X + dx, options.SnapSize);
                rect.Y = ItemEditor.Snap(rect.Y + dy, options.SnapSize);

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

                rect.Width = Math.Max(0.0, ItemEditor.Snap(rect.Width + dx, options.SnapSize));
                rect.Y = ItemEditor.Snap(rect.Y + dy, options.SnapSize);

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

                rect.X = ItemEditor.Snap(rect.X + dx, options.SnapSize);
                rect.Height = Math.Max(0.0, ItemEditor.Snap(rect.Height + dy, options.SnapSize));

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

                rect.Width = Math.Max(0.0, ItemEditor.Snap(rect.Width + dx, options.SnapSize));
                rect.Height = Math.Max(0.0, ItemEditor.Snap(rect.Height + dy, options.SnapSize));

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

        #region Events

        private void UserControl_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Focus();

            // edit mode
            if (selectedType != ItemType.None)
            {
                if (!((e.OriginalSource as FrameworkElement).TemplatedParent is Thumb))
                {
                    BlockEditor.DeselectAll(selectedBlock);
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

            bool ctrl = (Keyboard.Modifiers & ModifierKeys.Control) > 0;

            // move mode
            if (!ctrl)
            {
                if (BlockEditor.HaveSelected(selectedBlock) && CanInitMove(e.GetPosition(overlaySheet.GetParent())))
                {
                    InitMove(e.GetPosition(overlaySheet.GetParent()));
                    return;
                }

                BlockEditor.DeselectAll(selectedBlock);
            }

            bool resetSelected = ctrl && BlockEditor.HaveSelected(selectedBlock) ? false : true;

            if (GetMode() == Mode.Selection)
            {
                bool result = BlockEditor.HitTestClick(logicSheet, logicBlock, selectedBlock, e.GetPosition(overlaySheet.GetParent()), options.HitTestSize, false, resetSelected);
                if ((ctrl || !BlockEditor.HaveSelected(selectedBlock)) && !result)
                {
                    InitSelectionRect(e.GetPosition(overlaySheet.GetParent()));
                }
                else
                {
                    bool editModeEnabled = TryToEditSelected();
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
                if (BlockEditor.HaveSelected(selectedBlock))
                {
                    BlockEditor.DeselectAll(selectedBlock);
                }

                BlockEditor.HitTestClick(logicSheet, logicBlock, selectedBlock, e.GetPosition(overlaySheet.GetParent()), options.HitTestSize, false, false);
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
                BlockEditor.DeselectAll(selectedBlock);
                FinishEdit();
                return;
            }

            // text editor
            if (TryToEditText(e.GetPosition(overlaySheet.GetParent())))
            {
                e.Handled = true;
                return;
            }

            BlockEditor.DeselectAll(selectedBlock);

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
                //ResetPanAndZoom();
                AutoFit(lastFinalSize);
            }
        }

        #endregion

        #region Data Binding

        private bool BindDataToBlock(Point p, DataItem dataItem)
        {
            var temp = new Block(0, 0.0, 0.0, -1, "TEMP");
            BlockEditor.HitTestForBlocks(logicSheet, logicBlock, temp, p, options.HitTestSize);

            if (BlockEditor.HaveOneBlockSelected(temp))
            {
                History.Register("Bind Data");
                var block = temp.Blocks[0];
                BindDataToBlock(block, dataItem);
                BlockEditor.DeselectBlock(temp);
                return true;
            }

            BlockEditor.DeselectBlock(temp);
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
                        var temp = new Block(0, 0.0, 0.0, -1, "TEMP");
                        temp.Init();
                        temp.Blocks.Add(block);
                        BlockEditor.RemoveSelectedFromBlock(logicSheet, logicBlock, temp);
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

        #region Logic: Invert Line Start & End

        private double invertedEllipseWidth = 10.0;
        private double invertedEllipseHeight = 10.0;

        private void AddInvertedLineEllipse(double x, double y, double width, double height)
        {
            var ellipse = BlockFactory.CreateEllipse(options.LineThickness / Zoom, x, y, width, height, false);
            logicBlock.Ellipses.Add(ellipse);
            logicSheet.Add(ellipse);

            BlockEditor.SelectEllipse(ellipse);
            if (selectedBlock.Ellipses == null)
            {
                selectedBlock.Ellipses = new List<Ellipse>();
            }
            selectedBlock.Ellipses.Add(ellipse);
        }

        public void InvertSelectedLineStart()
        {
            // add for horizontal or vertical line start ellipse and shorten line
            if (BlockEditor.HaveSelected(selectedBlock) && selectedBlock.Lines != null && selectedBlock.Lines.Count > 0)
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
            if (BlockEditor.HaveSelected(selectedBlock) && selectedBlock.Lines != null && selectedBlock.Lines.Count > 0)
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
