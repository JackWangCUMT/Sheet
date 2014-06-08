using Sheet.Item.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sheet.Item
{
    public interface IItemSerializer
    {
        string SerializeContents(BlockItem block, ItemSerializeOptions options);
        string SerializeContents(BlockItem block);
        BlockItem DeserializeContents(string model, ItemSerializeOptions options);
        BlockItem DeserializeContents(string model);
    }
}
