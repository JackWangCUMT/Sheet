using Sheet.Util.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Sheet.WPF
{
    public class WpfClipboard : IClipboard
    {
        public void Set(string text)
        {
            Clipboard.SetData(DataFormats.UnicodeText, text);
        }

        public string Get()
        {
            return (string)Clipboard.GetData(DataFormats.UnicodeText);
        }
    }
}
