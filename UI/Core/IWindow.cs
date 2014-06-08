using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sheet.UI.Core
{
    public interface IWindow : IDisposable
    {
        void Show();
        bool? ShowDialog();
    }
}