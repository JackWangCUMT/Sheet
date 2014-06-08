using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sheet.Util.Core
{
    public interface IBase64
    {
        string ToBase64(byte[] bytes);
        MemoryStream ToStream(byte[] bytes);
        byte[] ToBytes(string base64);
        MemoryStream ToStream(string base64);
        byte[] ReadAllBytes(string path);
        string FromFileToBase64(string path);
        MemoryStream FromFileToStream(string path);
    }
}
