using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sheet
{
    #region IWindow

    public interface IWindow : IDisposable
    {
        void Show();
        bool? ShowDialog();
    }

    #endregion

    #region IView

    public interface IView : IDisposable
    {
        bool Focus();
        bool IsFocused { get; }
    }

    #endregion

    #region Windows

    public interface IMainWindow : IWindow
    {
    }

    #endregion

    #region Views

    public interface ISheetView : IView
    {
    }

    public interface ILibraryView : IView
    {
    }

    public interface ISolutionView : IView
    {
    }

    public interface IDatabaseView : IView
    {
    }

    public interface ITextView : IView
    {
    }

    #endregion

    #region IServiceLocator

    public interface IServiceLocator
    {
        T GetInstance<T>();
    }

    #endregion

    #region IScopeServiceLocator

    public interface IScopeServiceLocator
    {
        T GetInstance<T>();
        void CreateScope();
        void ReleaseScope();
    }
    
    #endregion
    
    #region IClipboard

    public interface IClipboard
    {
        void Set(string text);
        string Get();
    }

    #endregion

    #region IBase64

    public interface IBase64
    {
        string ToBase64(byte[] bytes);
        MemoryStream ToStream(byte[] bytes);
        byte[] ToBytes(string base64);
        MemoryStream ToStream(string base64);
        byte[] ReadAllBytes(string path);
        string FromFileToBase64(string path);
        MemoryStream FromFileToStream(string path);
    }

    #endregion

    #region IJsonSerializer

    public interface IJsonSerializer
    {
        string Serialize(object value);
        T Deerialize<T>(string value);
    }

    #endregion

    #region IItemSerializer

    public interface IItemSerializer
    {
        string SerializeContents(BlockItem block, ItemSerializeOptions options);
        string SerializeContents(BlockItem block);
        BlockItem DeserializeContents(string model, ItemSerializeOptions options);
        BlockItem DeserializeContents(string model);
    }

    #endregion

    #region IItemController

    public interface IItemController
    {
        Task<string> OpenText(string fileName);
        void SaveText(string fileName, string text);

        void ResetPosition(BlockItem block, double originX, double originY, double width, double height);

        double Snap(double val, double snap);
    }

    #endregion

    #region IEntrySerializer

    public interface IEntrySerializer
    {
        void CreateEmpty(string path);
        void Serialize(SolutionEntry solution, string path);
        SolutionEntry Deserialize(string path);
    }

    #endregion

    #region IEntryController

    public interface IEntryController
    {
        PageEntry AddPage(DocumentEntry document, string content);
        PageEntry AddPageBefore(DocumentEntry document, PageEntry beofore, string content);
        PageEntry AddPageAfter(DocumentEntry document, PageEntry after, string content);
        void AddPageAfter(object item);
        void AddPageBefore(object item);
        void DuplicatePage(object item);
        void RemovePage(object item);

        DocumentEntry AddDocumentBefore(SolutionEntry solution, DocumentEntry after);
        DocumentEntry AddDocumentAfter(SolutionEntry solution, DocumentEntry after);
        DocumentEntry AddDocument(SolutionEntry solution);
        void DocumentAddPage(object item);
        void AddDocumentAfter(object item);
        void AddDocumentBefore(object item);
        void DulicateDocument(object item);
        void RemoveDocument(object item);
    }

    #endregion

    #region IEntryFactory

    public interface IEntryFactory
    {
        PageEntry CreatePage(DocumentEntry document, string content, string name = null);
        DocumentEntry CreateDocument(SolutionEntry solution, string name = null);
    }

    #endregion

    #region IBlockSerializer

    public interface IBlockSerializer
    {
        PointItem Serialize(IPoint point);
        LineItem Serialize(ILine line);
        RectangleItem Serialize(IRectangle rectangle);
        EllipseItem Serialize(IEllipse ellipse);
        TextItem Serialize(IText text);
        ImageItem Serialize(IImage image);
        BlockItem Serialize(IBlock parent);
        BlockItem SerializerContents(IBlock parent, int id, double x, double y, double width, double height, int dataId, string name);
        IPoint Deserialize(ISheet sheet, IBlock parent, PointItem pointItem, double thickness);
        ILine Deserialize(ISheet sheet, IBlock parent, LineItem lineItem, double thickness);
        IRectangle Deserialize(ISheet sheet, IBlock parent, RectangleItem rectangleItem, double thickness);
        IEllipse Deserialize(ISheet sheet, IBlock parent, EllipseItem ellipseItem, double thickness);
        IText Deserialize(ISheet sheet, IBlock parent, TextItem textItem);
        IImage Deserialize(ISheet sheet, IBlock parent, ImageItem imageItem);
        IBlock Deserialize(ISheet sheet, IBlock parent, BlockItem blockItem, double thickness);
    }

    #endregion

    #region IBlockController

    public interface IBlockController
    {
        List<IPoint> Add(ISheet sheet, IEnumerable<PointItem> pointItems, IBlock parent, IBlock selected, bool select, double thickness);
        List<ILine> Add(ISheet sheet, IEnumerable<LineItem> lineItems, IBlock parent, IBlock selected, bool select, double thickness);
        List<IRectangle> Add(ISheet sheet, IEnumerable<RectangleItem> rectangleItems, IBlock parent, IBlock selected, bool select, double thickness);
        List<IEllipse> Add(ISheet sheet, IEnumerable<EllipseItem> ellipseItems, IBlock parent, IBlock selected, bool select, double thickness);
        List<IText> Add(ISheet sheet, IEnumerable<TextItem> textItems, IBlock parent, IBlock selected, bool select, double thickness);
        List<IImage> Add(ISheet sheet, IEnumerable<ImageItem> imageItems, IBlock parent, IBlock selected, bool select, double thickness);
        List<IBlock> Add(ISheet sheet, IEnumerable<BlockItem> blockItems, IBlock parent, IBlock selected, bool select, double thickness);
        void AddContents(ISheet sheet, BlockItem blockItem, IBlock content, IBlock selected, bool select, double thickness);
        void AddBroken(ISheet sheet, BlockItem blockItem, IBlock content, IBlock selected, bool select, double thickness);

        void Remove(ISheet sheet, IEnumerable<IPoint> points);
        void Remove(ISheet sheet, IEnumerable<ILine> lines);
        void Remove(ISheet sheet, IEnumerable<IRectangle> rectangles);
        void Remove(ISheet sheet, IEnumerable<IEllipse> ellipses);
        void Remove(ISheet sheet, IEnumerable<IText> texts);
        void Remove(ISheet sheet, IEnumerable<IImage> images);
        void Remove(ISheet sheet, IEnumerable<IBlock> blocks);
        void Remove(ISheet sheet, IBlock block);
        void RemoveSelected(ISheet sheet, IBlock parent, IBlock selected);

        void MoveDelta(double dx, double dy, IPoint point);
        void MoveDelta(double dx, double dy, IEnumerable<IPoint> points);
        void MoveDelta(double dx, double dy, IEnumerable<ILine> lines);
        void MoveDeltaStart(double dx, double dy, ILine line);
        void MoveDeltaEnd(double dx, double dy, ILine line);
        void MoveDelta(double dx, double dy, IRectangle rectangle);
        void MoveDelta(double dx, double dy, IEnumerable<IRectangle> rectangles);
        void MoveDelta(double dx, double dy, IEllipse ellipse);
        void MoveDelta(double dx, double dy, IEnumerable<IEllipse> ellipses);
        void MoveDelta(double dx, double dy, IText text);
        void MoveDelta(double dx, double dy, IEnumerable<IText> texts);
        void MoveDelta(double dx, double dy, IImage image);
        void MoveDelta(double dx, double dy, IEnumerable<IImage> images);
        void MoveDelta(double dx, double dy, IBlock block);
        void MoveDelta(double dx, double dy, IEnumerable<IBlock> blocks);

        void Deselect(IPoint point);
        void Deselect(ILine line);
        void Deselect(IRectangle rectangle);
        void Deselect(IEllipse ellipse);
        void Deselect(IText text);
        void Deselect(IImage image);
        void Deselect(IBlock parent);

        void Select(IPoint point);
        void Select(ILine line);
        void Select(IRectangle rectangle);
        void Select(IEllipse ellipse);
        void Select(IText text);
        void Select(IImage image);
        void Select(IBlock parent);

        void SelectContent(IBlock selected, IBlock content);
        void DeselectContent(IBlock selected);

        bool HaveSelected(IBlock selected);
        bool HaveOnePointSelected(IBlock selected);
        bool HaveOneLineSelected(IBlock selected);
        bool HaveOneRectangleSelected(IBlock selected);
        bool HaveOneEllipseSelected(IBlock selected);
        bool HaveOneTextSelected(IBlock selected);
        bool HaveOneImageSelected(IBlock selected);
        bool HaveOneBlockSelected(IBlock selected);

        bool HitTest(IEnumerable<IPoint> points, IBlock selected, XImmutableRect rect, bool onlyFirst, bool select, object relativeTo);
        bool HitTest(IEnumerable<ILine> lines, IBlock selected, XImmutableRect rect, bool onlyFirst, bool select);
        bool HitTest(IEnumerable<IRectangle> rectangles, IBlock selected, XImmutableRect rect, bool onlyFirst, bool select, object relativeTo);
        bool HitTest(IEnumerable<IEllipse> ellipses, IBlock selected, XImmutableRect rect, bool onlyFirst, bool select, object relativeTo);
        bool HitTest(IEnumerable<IText> texts, IBlock selected, XImmutableRect rect, bool onlyFirst, bool select, object relativeTo);
        bool HitTest(IEnumerable<IImage> images, IBlock selected, XImmutableRect rect, bool onlyFirst, bool select, object relativeTo);
        bool HitTest(IEnumerable<IBlock> blocks, IBlock selected, XImmutableRect rect, bool onlyFirst, bool select, bool selectInsideBlock, object relativeTo);
        bool HitTest(IBlock parent, IBlock selected, XImmutableRect rect, bool onlyFirst, bool selectInsideBlock, object relativeTo);

        bool HitTestClick(ISheet sheet, IBlock parent, IBlock selected, XImmutablePoint p, double size, bool selectInsideBlock, bool resetSelected);
        bool HitTestForBlocks(ISheet sheet, IBlock parent, IBlock selected, XImmutablePoint p, double size);
        void HitTestSelectionRect(ISheet sheet, IBlock parent, IBlock selected, XImmutableRect rect, bool resetSelected);

        void ToggleFill(IRectangle rectangle);
        void ToggleFill(IEllipse ellipse);
        void ToggleFill(IPoint point);

        IBlock ShallowCopy(IBlock original);
    }

    #endregion

    #region IBlockFactory

    public interface IBlockFactory
    {
        IThumb CreateThumb(double x, double y);
        IThumb CreateThumb(double x, double y, ILine line, Action<ILine, IThumb, double, double> drag);
        IThumb CreateThumb(double x, double y, IElement element, Action<IElement, IThumb, double, double> drag);
        IPoint CreatePoint(double thickness, double x, double y, bool isVisible);
        ILine CreateLine(double thickness, double x1, double y1, double x2, double y2, ItemColor stroke);
        ILine CreateLine(double thickness, IPoint start, IPoint end, ItemColor stroke);
        IRectangle CreateRectangle(double thickness, double x, double y, double width, double height, bool isFilled, ItemColor stroke, ItemColor fill);
        IEllipse CreateEllipse(double thickness, double x, double y, double width, double height, bool isFilled, ItemColor stroke, ItemColor fill);
        IText CreateText(string text, double x, double y, double width, double height, int halign, int valign, double fontSize, ItemColor backgroud, ItemColor foreground);
        IImage CreateImage(double x, double y, double width, double height, byte[] data);
        IBlock CreateBlock(int id, double x, double y, double width, double height, int dataId, string name, ItemColor backgroud);
    }

    #endregion

    #region IBlockHelper

    public interface IBlockHelper
    {
        bool HitTest(IElement element, XImmutableRect rect);
        bool HitTest(IElement element, XImmutableRect rect, object relativeTo);

        void SetIsSelected(IElement element, bool value);
        bool GetIsSelected(IElement element);

        bool IsSelected(IPoint point);
        bool IsSelected(ILine line);
        bool IsSelected(IRectangle rectangle);
        bool IsSelected(IEllipse ellipse);
        bool IsSelected(IText text);
        bool IsSelected(IImage image);

        void Deselect(IPoint point);
        void Deselect(ILine line);
        void Deselect(IRectangle rectangle);
        void Deselect(IEllipse ellipse);
        void Deselect(IText text);
        void Deselect(IImage image);

        void Select(IPoint point);
        void Select(ILine line);
        void Select(IRectangle rectangle);
        void Select(IEllipse ellipse);
        void Select(IText text);
        void Select(IImage image);

        void SetZIndex(IElement element, int index);

        void ToggleFill(IRectangle rectangle);
        void ToggleFill(IEllipse ellipse);
        void ToggleFill(IPoint point);

        double GetLeft(IElement element);
        double GetTop(IElement element);
        double GetWidth(IElement element);
        double GetHeight(IElement element);
        void SetLeft(IElement element, double left);
        void SetTop(IElement element, double top);
        void SetWidth(IElement element, double width);
        void SetHeight(IElement element, double height);

        double GetX1(ILine line);
        double GetY1(ILine line);
        double GetX2(ILine line);
        double GetY2(ILine line);
        ItemColor GetStroke(ILine line);
        void SetX1(ILine line, double x1);
        void SetY1(ILine line, double y1);
        void SetX2(ILine line, double x2);
        void SetY2(ILine line, double y2);
        void SetStrokeThickness(ILine line, double thickness);
        double GetStrokeThickness(ILine line);

        ItemColor GetStroke(IRectangle rectangle);
        ItemColor GetFill(IRectangle rectangle);
        bool IsTransparent(IRectangle rectangle);
        void SetStrokeThickness(IRectangle rectangle, double thickness);
        double GetStrokeThickness(IRectangle rectangle);

        ItemColor GetStroke(IEllipse ellipse);
        ItemColor GetFill(IEllipse ellipse);
        bool IsTransparent(IEllipse ellipse);
        void SetStrokeThickness(IEllipse ellipse, double thickness);
        double GetStrokeThickness(IEllipse ellipse);

        ItemColor GetBackground(IText text);
        ItemColor GetForeground(IText text);

        string GetText(IText text);
        int GetHAlign(IText text);
        int GetVAlign(IText text);
        double GetSize(IText text);

        byte[] GetData(IImage image);
    }

    #endregion

    #region IPointController

    public interface IPointController
    {
        void ConnectStart(IPoint point, ILine line);
        void ConnectEnd(IPoint point, ILine line);
        void UpdateDependencies(List<IBlock> blocks, List<IPoint> points, List<ILine> lines);
    }

    #endregion

    #region IPageFactory

    public interface IPageFactory
    {
        void CreateLine(ISheet sheet, IList<ILine> lines, double thickness, double x1, double y1, double x2, double y2, ItemColor stroke);
        void CreateText(ISheet sheet, IList<IText> texts, string content, double x, double y, double width, double height, int halign, int valign, double size, ItemColor foreground);
        void CreateFrame(ISheet sheet, IBlock block, double size, double thickness, ItemColor stroke);
        void CreateGrid(ISheet sheet, IBlock block, double startX, double startY, double width, double height, double size, double thickness, ItemColor stroke);
        
        IRectangle CreateSelectionRectangle(double thickness, double x, double y, double width, double height);
    }

    #endregion

    #region IZoomController

    public interface IZoomController
    {
        int ZoomIndex { get; set; }
        double Zoom { get; set; }
        double PanX { get; set; }
        double PanY { get; set; }
        void AutoFit();
        void ActualSize();
    }

    #endregion

    #region ICursorController

    public enum SheetCursor
    {
        Unknown,
        Normal,
        Move,
        Pan
    }

    public interface ICursorController
    {
        void Set(SheetCursor cursor);
        SheetCursor Get();
    }

    #endregion

    #region IDataReader

    public interface IDataReader
    {
        IEnumerable<string[]> Read(string path);
    }

    #endregion

    #region ISheet

    public interface ISheet
    {
        double Width { get; set; }
        double Height { get; set; }
        bool IsCaptured { get; }
        object GetParent();
        void SetParent(object parent);
        void Add(IElement element);
        void Remove(IElement element);
        void Add(object element);
        void Remove(object element);
        void Capture();
        void ReleaseCapture();
    }

    #endregion

    #region IHistoryController

    public interface IHistoryController
    {
        void Register(string message);
        void Reset();
        void Undo();
        void Redo();
    } 

    #endregion

    #region ILibraryController

    public interface ILibraryController
    {
        BlockItem GetSelected();
        void SetSelected(BlockItem block);
        IEnumerable<BlockItem> GetSource();
        void SetSource(IEnumerable<BlockItem> source);
    } 

    #endregion

    #region IDatabaseController

    public interface IDatabaseController
    {
        string Name { get; set; }
        string[] Columns { get; set; }
        List<string[]> Data { get; set; }
        string[] Get(int index);
        bool Update(int index, string[] item);
        int Add(string[] item);
    }

    #endregion

    #region ITextController

    public interface ITextController
    {
        void Set(Action<string> ok, Action cancel, string title, string label, string text);
    }

    #endregion

    #region ISheetController

    public interface ISheetController
    {
        // Properties
        IHistoryController HistoryController { get; set; }
        ILibraryController LibraryController { get; set; }
        IZoomController ZoomController { get; set; }
        ICursorController CursorController { get; set; }
        SheetOptions Options { get; set; }
        ISheet EditorSheet { get; set; }
        ISheet BackSheet { get; set; }
        ISheet ContentSheet { get; set; }
        ISheet OverlaySheet { get; set; }
        ISheetView View { get; set; }
        double LastFinalWidth { get; set; }
        double LastFinalHeight { get; set; }

        // Blocks
        IBlock GetSelected();
        IBlock GetContent();
        
        // Mode
        SheetMode GetMode();
        void SetMode(SheetMode mode);

        // Init
        void Init();

        // Clipboard
        void CutText();
        void CopyText();
        void PasteText();
        void CutJson();
        void CopyJson();
        void PasteJson();

        // Delete
        void Delete(IBlock block);
        void Delete();

        // Select All
        void SelecteAll();

        // Toggle Fill
        void ToggleFill();

        // Insert Mode
        void CreateBlock();
        void BreakBlock();

        // Move Mode
        void MoveUp();
        void MoveDown();
        void MoveLeft();
        void MoveRight();

        // Pan & Zoom Mode
        void SetAutoFitSize(double finalWidth, double finalHeight);
        void AdjustBackThickness(double zoom);
        void AdjustPageThickness(double zoom);

        // Data Binding
        bool BindDataToBlock(XImmutablePoint p, DataItem dataItem);
        bool BindDataToBlock(IBlock block, DataItem dataItem);
        void TryToBindData(XImmutablePoint p, DataItem dataItem);

        // New Page
        void NewPage();

        // Open Page
        Task OpenTextPage(string path);
        Task OpenJsonPage(string path);
        void OpenPage();

        // Save Page
        void SaveTextPage(string path);
        void SaveJsonPage(string path);
        void SavePage();

        // Export Page
        void Export(IEnumerable<BlockItem> blocks);
        void Export(SolutionEntry solution);
        void ExportPage();

        // Library
        void Insert(XImmutablePoint p);
        IBlock Insert(BlockItem blockItem, XImmutablePoint p, bool select);
        Task LoadLibrary(string path);
        void LoadLibrary();

        // Input
        void LeftDown(InputArgs args);
        void LeftUp(InputArgs args);
        void Move(InputArgs args);
        void RightDown(InputArgs args);
        void RightUp(InputArgs args);
        void Wheel(int delta, XImmutablePoint position);
        void Down(InputArgs args);

        // Page
        void SetPage(string text);
        string GetPage();
        void ExportPage(string text);
        void ExportPages(IEnumerable<string> texts);
        BlockItem SerializePage();
        void DeserializePage(BlockItem page);
        void ResetPage();
        void ResetPageContent();

        // Plugins
        void InvertSelectedLineStart();
        void InvertSelectedLineEnd();
    }

    #endregion
}
