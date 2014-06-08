using Sheet.Block.Core;
using Sheet.Item.Model;
using Sheet.UI.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sheet.Controller.Core
{
    public class SheetState
    {
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

        public SheetMode Mode { get; set; }
        public SheetMode TempMode { get; set; }

        public IBlock SelectedBlock { get; set; }
        public IBlock ContentBlock { get; set; }
        public IBlock FrameBlock { get; set; }
        public IBlock GridBlock { get; set; }

        public IRectangle TempSelectionRect { get; set; }
        public bool IsFirstMove { get; set; }
        public ImmutablePoint PanStartPoint { get; set; }
        public ImmutablePoint SelectionStartPoint { get; set; }
        public ItemType SelectedType { get; set; }
        public ILine SelectedLine { get; set; }
        public IThumb LineThumbStart { get; set; }
        public IThumb LineThumbEnd { get; set; }
        public IElement SelectedElement { get; set; }
        public IThumb ThumbTopLeft { get; set; }
        public IThumb ThumbTopRight { get; set; }
        public IThumb ThumbBottomLeft { get; set; }
        public IThumb ThumbBottomRight { get; set; }
    }

}
