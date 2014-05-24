using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sheet
{
    #region IDataReader

    public interface IDataReader
    {
        IEnumerable<string[]> Read(string path);
    }

    #endregion

    #region IJsonSerializer

    public interface IJsonSerializer
    {
        string Serialize(object value);
        T Deerialize<T>(string value);
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
}
