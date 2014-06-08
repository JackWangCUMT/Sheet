using Sheet.Item.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sheet.Item
{
    public static class ItemColors
    {
        public static ItemColor Transparent { get { return new ItemColor() { Alpha = 0, Red = 255, Green = 255, Blue = 255 }; } }
        public static ItemColor White { get { return new ItemColor() { Alpha = 255, Red = 255, Green = 255, Blue = 255 }; } }
        public static ItemColor Black { get { return new ItemColor() { Alpha = 255, Red = 0, Green = 0, Blue = 0 }; } }
        public static ItemColor Red { get { return new ItemColor() { Alpha = 255, Red = 255, Green = 0, Blue = 0 }; } }
        public static ItemColor LightGray { get { return new ItemColor() { Alpha = 255, Red = 211, Green = 211, Blue = 211 }; } }
        public static ItemColor DarkGray { get { return new ItemColor() { Alpha = 255, Red = 169, Green = 169, Blue = 169 }; } }
    }
}
