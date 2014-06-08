using Sheet.Item.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sheet.Controller.Core
{
    public interface ILibraryController
    {
        BlockItem GetSelected();
        void SetSelected(BlockItem block);
        IEnumerable<BlockItem> GetSource();
        void SetSource(IEnumerable<BlockItem> source);
    }
}
