using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sheet
{
    #region IInterfaceLocator

    public interface IInterfaceLocator
    {
        T GetInterface<T>();
    }

    #endregion

    #region IJsonSerializer

    public interface IJsonSerializer
    {
        string Serialize(object value);
        T Deerialize<T>(string value);
    }

    #endregion

    #region IBlockHelper

    public interface IBlockHelper
    {
        bool HitTest(XElement element, XBlockRect rect);
        bool HitTest(XElement element, XBlockRect rect, object relativeTo);

        void SetIsSelected(XElement element, bool value);
        bool GetIsSelected(XElement element);

        bool IsSelected(XPoint point);
        bool IsSelected(XLine line);
        bool IsSelected(XRectangle rectangle);
        bool IsSelected(XEllipse ellipse);
        bool IsSelected(XText text);
        bool IsSelected(XImage image);

        void Deselect(XPoint point);
        void Deselect(XLine line);
        void Deselect(XRectangle rectangle);
        void Deselect(XEllipse ellipse);
        void Deselect(XText text);
        void Deselect(XImage image);

        void Select(XPoint point);
        void Select(XLine line);
        void Select(XRectangle rectangle);
        void Select(XEllipse ellipse);
        void Select(XText text);
        void Select(XImage image);

        void SetZIndex(XElement element, int index);

        void ToggleFill(XRectangle rectangle);
        void ToggleFill(XEllipse ellipse);
        void ToggleFill(XPoint point);

        double GetLeft(XElement element);
        double GetTop(XElement element);
        double GetWidth(XElement element);
        double GetHeight(XElement element);
        void SetLeft(XElement element, double left);
        void SeTop(XElement element, double top);

        double GetX1(XLine line);
        double GetY1(XLine line);
        double GetX2(XLine line);
        double GetY2(XLine line);
        ItemColor GetStroke(XLine line);
        void SetX1(XLine line, double x1);
        void SetY1(XLine line, double y1);
        void SetX2(XLine line, double x2);
        void SetY2(XLine line, double y2);

        ItemColor GetStroke(XRectangle rectangle);
        ItemColor GetFill(XRectangle rectangle);
        bool IsTransparent(XRectangle rectangle);

        ItemColor GetStroke(XEllipse ellipse);
        ItemColor GetFill(XEllipse ellipse);
        bool IsTransparent(XEllipse ellipse);

        ItemColor GetBackground(XText text);
        ItemColor GetForeground(XText text);

        string GetText(XText text);
        int GetHAlign(XText text);
        int GetVAlign(XText text);
        double GetSize(XText text);

        byte[] GetData(XImage image);
    }

    #endregion

    #region IBlockFactory

    public interface IBlockFactory
    {
        XPoint CreatePoint(double thickness, double x, double y, bool isVisible);
        XLine CreateLine(double thickness, double x1, double y1, double x2, double y2, ItemColor stroke);
        XLine CreateLine(double thickness, XPoint start, XPoint end, ItemColor stroke);
        XRectangle CreateRectangle(double thickness, double x, double y, double width, double height, bool isFilled);
        XEllipse CreateEllipse(double thickness, double x, double y, double width, double height, bool isFilled);
        XText CreateText(string text, double x, double y, double width, double height, int halign, int valign, double fontSize, ItemColor backgroud, ItemColor foreground);
        XImage CreateImage(double x, double y, double width, double height, byte[] data);
    }
    
    #endregion

    #region IPageFactory

    public interface IPageFactory
    {
        void CreateLine(ISheet sheet, List<XLine> lines, double thickness, double x1, double y1, double x2, double y2, ItemColor stroke);
        void CreateText(ISheet sheet, List<XText> texts, string content, double x, double y, double width, double height, int halign, int valign, double size, ItemColor foreground);
        void CreateFrame(ISheet sheet, XBlock block, double size, double thickness, ItemColor stroke);
        void CreateGrid(ISheet sheet, XBlock block, double startX, double startY, double width, double height, double size, double thickness, ItemColor stroke);
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

    #region IBlockSerializer

    public interface IBlockSerializer
    {
        PointItem Serialize(XPoint point);
        LineItem Serialize(XLine line);
        RectangleItem Serialize(XRectangle rectangle);
        EllipseItem Serialize(XEllipse ellipse);
        TextItem Serialize(XText text);
        ImageItem Serialize(XImage image);
        BlockItem Serialize(XBlock parent);
        BlockItem SerializerContents(XBlock parent, int id, double x, double y, double width, double height, int dataId, string name);
        XPoint Deserialize(ISheet sheet, XBlock parent, PointItem pointItem, double thickness);
        XLine Deserialize(ISheet sheet, XBlock parent, LineItem lineItem, double thickness);
        XRectangle Deserialize(ISheet sheet, XBlock parent, RectangleItem rectangleItem, double thickness);
        XEllipse Deserialize(ISheet sheet, XBlock parent, EllipseItem ellipseItem, double thickness);
        XText Deserialize(ISheet sheet, XBlock parent, TextItem textItem);
        XImage Deserialize(ISheet sheet, XBlock parent, ImageItem imageItem);
        XBlock Deserialize(ISheet sheet, XBlock parent, BlockItem blockItem, double thickness);
    }

    #endregion

    #region IBlockController

    public interface IBlockController
    {
        List<XPoint> Add(ISheet sheet, IEnumerable<PointItem> pointItems, XBlock parent, XBlock selected, bool select, double thickness);
        List<XLine> Add(ISheet sheet, IEnumerable<LineItem> lineItems, XBlock parent, XBlock selected, bool select, double thickness);
        List<XRectangle> Add(ISheet sheet, IEnumerable<RectangleItem> rectangleItems, XBlock parent, XBlock selected, bool select, double thickness);
        List<XEllipse> Add(ISheet sheet, IEnumerable<EllipseItem> ellipseItems, XBlock parent, XBlock selected, bool select, double thickness);
        List<XText> Add(ISheet sheet, IEnumerable<TextItem> textItems, XBlock parent, XBlock selected, bool select, double thickness);
        List<XImage> Add(ISheet sheet, IEnumerable<ImageItem> imageItems, XBlock parent, XBlock selected, bool select, double thickness);
        List<XBlock> Add(ISheet sheet, IEnumerable<BlockItem> blockItems, XBlock parent, XBlock selected, bool select, double thickness);
        void AddContents(ISheet sheet, BlockItem blockItem, XBlock content, XBlock selected, bool select, double thickness);
        void AddBroken(ISheet sheet, BlockItem blockItem, XBlock content, XBlock selected, bool select, double thickness);

        void Remove(ISheet sheet, IEnumerable<XPoint> points);
        void Remove(ISheet sheet, IEnumerable<XLine> lines);
        void Remove(ISheet sheet, IEnumerable<XRectangle> rectangles);
        void Remove(ISheet sheet, IEnumerable<XEllipse> ellipses);
        void Remove(ISheet sheet, IEnumerable<XText> texts);
        void Remove(ISheet sheet, IEnumerable<XImage> images);
        void Remove(ISheet sheet, IEnumerable<XBlock> blocks);
        void Remove(ISheet sheet, XBlock block);
        void RemoveSelected(ISheet sheet, XBlock parent, XBlock selected);

        void Move(double x, double y, XPoint point);
        void Move(double x, double y, IEnumerable<XPoint> points);
        void Move(double x, double y, IEnumerable<XLine> lines);
        void MoveStart(double x, double y, XLine line);
        void MoveEnd(double x, double y, XLine line);
        void Move(double x, double y, XRectangle rectangle);
        void Move(double x, double y, IEnumerable<XRectangle> rectangles);
        void Move(double x, double y, XEllipse ellipse);
        void Move(double x, double y, IEnumerable<XEllipse> ellipses);
        void Move(double x, double y, XText text);
        void Move(double x, double y, IEnumerable<XText> texts);
        void Move(double x, double y, XImage image);
        void Move(double x, double y, IEnumerable<XImage> images);
        void Move(double x, double y, XBlock block);
        void Move(double x, double y, IEnumerable<XBlock> blocks);

        void Deselect(XPoint point);
        void Deselect(XLine line);
        void Deselect(XRectangle rectangle);
        void Deselect(XEllipse ellipse);
        void Deselect(XText text);
        void Deselect(XImage image);
        void Deselect(XBlock parent);

        void Select(XPoint point);
        void Select(XLine line);
        void Select(XRectangle rectangle);
        void Select(XEllipse ellipse);
        void Select(XText text);
        void Select(XImage image);
        void Select(XBlock parent);

        void SelectContent(XBlock selected, XBlock content);
        void DeselectContent(XBlock selected);

        bool HaveSelected(XBlock selected);
        bool HaveOnePointSelected(XBlock selected);
        bool HaveOneLineSelected(XBlock selected);
        bool HaveOneRectangleSelected(XBlock selected);
        bool HaveOneEllipseSelected(XBlock selected);
        bool HaveOneTextSelected(XBlock selected);
        bool HaveOneImageSelected(XBlock selected);
        bool HaveOneBlockSelected(XBlock selected);

        bool HitTest(IEnumerable<XPoint> points, XBlock selected, XBlockRect rect, bool onlyFirst, bool select, object relativeTo);
        bool HitTest(IEnumerable<XLine> lines, XBlock selected, XBlockRect rect, bool onlyFirst, bool select);
        bool HitTest(IEnumerable<XRectangle> rectangles, XBlock selected, XBlockRect rect, bool onlyFirst, bool select, object relativeTo);
        bool HitTest(IEnumerable<XEllipse> ellipses, XBlock selected, XBlockRect rect, bool onlyFirst, bool select, object relativeTo);
        bool HitTest(IEnumerable<XText> texts, XBlock selected, XBlockRect rect, bool onlyFirst, bool select, object relativeTo);
        bool HitTest(IEnumerable<XImage> images, XBlock selected, XBlockRect rect, bool onlyFirst, bool select, object relativeTo);
        bool HitTest(IEnumerable<XBlock> blocks, XBlock selected, XBlockRect rect, bool onlyFirst, bool select, bool selectInsideBlock, object relativeTo);
        bool HitTest(XBlock parent, XBlock selected, XBlockRect rect, bool onlyFirst, bool selectInsideBlock, object relativeTo);

        bool HitTestClick(ISheet sheet, XBlock parent, XBlock selected, XBlockPoint p, double size, bool selectInsideBlock, bool resetSelected);
        bool HitTestForBlocks(ISheet sheet, XBlock parent, XBlock selected, XBlockPoint p, double size);
        void HitTestSelectionRect(ISheet sheet, XBlock parent, XBlock selected, XBlockRect rect, bool resetSelected);

        void ToggleFill(XRectangle rectangle);
        void ToggleFill(XEllipse ellipse);
        void ToggleFill(XPoint point);

        XBlock ShallowCopy(XBlock original);
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

    #region IDataReader

    public interface IDataReader
    {
        IEnumerable<string[]> Read(string path);
    }

    #endregion

    #region ISheet

    public interface ISheet
    {
        object GetParent();
        void Add(XElement element);
        void Remove(XElement element);
        void Capture();
        void ReleaseCapture();
        bool IsCaptured { get; }
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

    #region IPageController

    public interface IPageController
    {
        IHistoryController History { get; set; }
        ILibraryController Library { get; set; }
        IPanAndZoomController PanAndZoom { get; set; }
        void SetPage(string text);
        string GetPage();
        void ExportPage(string text);
        void ExportPages(IEnumerable<string> texts);
        BlockItem SerializePage();
        void DeserializePage(BlockItem page);
        void ResetPage();
        void ResetPageContent();
    }

    #endregion

    #region IPanAndZoomController

    public interface IPanAndZoomController
    {
        int ZoomIndex { get; set; }
        double Zoom { get; set; }
        double PanX { get; set; }
        double PanY { get; set; }
        void AutoFit();
        void ActualSize();
    }
    
    #endregion

    #region IPointController

    public interface IPointController
    {
        void ConnectStart(XPoint point, XLine line);
        void ConnectEnd(XPoint point, XLine line);
        void UpdateDependencies(List<XBlock> blocks, List<XPoint> points, List<XLine> lines);
    }

    #endregion
}
