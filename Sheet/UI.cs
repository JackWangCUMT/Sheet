using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sheet.UI
{
    public interface IView : IDisposable
    {
        bool Focus();
        bool IsFocused { get; }
    }

    public interface IWindow : IDisposable
    {
        void Show();
        bool? ShowDialog();
    }

    public interface ISheetView : IView
    {
    }

    public interface ILibraryView : IView
    {
    }

    public interface IDatabaseView : IView
    {
    }

    public interface ITextView : IView
    {
    }

    public interface ISolutionView : IView
    {
    }

    public interface IMainWindow : IWindow
    {
    }
}
