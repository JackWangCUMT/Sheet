using Sheet.Item.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sheet.Test
{
    public class TestLibraryController : ILibraryController
    {
        public BlockItem GetSelected()
        {
            return null;
        }

        public void SetSelected(BlockItem block)
        {
        }

        public IEnumerable<BlockItem> GetSource()
        {
            return null;
        }

        public void SetSource(IEnumerable<BlockItem> source)
        {
        }
    }
}
