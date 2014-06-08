using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sheet.Util.Core
{
    public interface IOpenFileDialog
    {
        string Filter { get; set; }
        string FileName { get; set; }
        string[] FileNames { get; set; }
        int FilterIndex { get; set; }
        bool ShowDialog();
    }
}
