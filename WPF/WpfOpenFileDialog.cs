using Microsoft.Win32;
using Sheet.Util.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sheet.WPF
{
    public class WpfOpenFileDialog : IOpenFileDialog
    {
        private readonly OpenFileDialog dlg;

        public WpfOpenFileDialog()
        {
            dlg = new OpenFileDialog();
        }

        public string Filter { get; set; }
        public string FileName { get; set; }
        public string[] FileNames { get; set; }
        public int FilterIndex { get; set; }

        public bool ShowDialog()
        {
            FileNames = null;

            dlg.Filter = Filter;
            dlg.FilterIndex = FilterIndex;
            dlg.FileName = FileName;

            var result = dlg.ShowDialog();
            if (result.HasValue && result.Value == true)
            {
                FileName = dlg.FileName;
                FileNames = dlg.FileNames;
                FilterIndex = dlg.FilterIndex;
                return true;
            }
            return false;
        }
    }
}
