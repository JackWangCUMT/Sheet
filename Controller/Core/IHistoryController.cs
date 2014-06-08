using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sheet.Controller.Core
{
    public interface IHistoryController
    {
        void Register(string message);
        void Reset();
        void Undo();
        void Redo();
    }
}
