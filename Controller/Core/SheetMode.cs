using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sheet.Controller.Core
{
    public enum SheetMode
    {
        None,
        Selection,
        Insert,
        Pan,
        Move,
        Edit,
        Point,
        Line,
        Rectangle,
        Ellipse,
        Text,
        Image,
        TextEditor
    }
}
