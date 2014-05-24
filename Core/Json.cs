using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sheet
{
    #region Newtonsoft IJsonSerializer

    public class NewtonsoftJsonSerializer : IJsonSerializer
    {
        public string Serialize(object value)
        {
            var settings = new JsonSerializerSettings() { ReferenceLoopHandling = ReferenceLoopHandling.Ignore };
            return JsonConvert.SerializeObject(value, Formatting.Indented, settings);
        }

        public T Deerialize<T>(string value)
        {
            return JsonConvert.DeserializeObject<T>(value);
        }
    } 

    #endregion
}
