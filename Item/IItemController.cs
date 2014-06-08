using Sheet.Item.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sheet.Item
{
    public interface IItemController
    {
        Task<string> OpenText(string fileName);
        void SaveText(string fileName, string text);

        void ResetPosition(BlockItem block, double originX, double originY, double width, double height);

        double Snap(double val, double snap);
    }
}
