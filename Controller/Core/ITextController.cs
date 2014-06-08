using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sheet.Controller.Core
{
    public interface ITextController
    {
        void Set(Action<string> ok, Action cancel, string title, string label, string text);
    }
}
