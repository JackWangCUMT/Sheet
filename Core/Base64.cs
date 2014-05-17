using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sheet
{
    #region Base64

    public static class Base64
    {
        public static string ToBase64(byte[] bytes)
        {
            if (bytes != null)
            {
                return Convert.ToBase64String(bytes);
            }
            return null;
        }

        public static MemoryStream ToStream(byte[] bytes)
        {
            if (bytes != null)
            {
                return new MemoryStream(bytes, 0, bytes.Length);
            }
            return null;
        }

        public static byte[] ToBytes(string base64)
        {
            if (!string.IsNullOrEmpty(base64))
            {
                return Convert.FromBase64String(base64);
            }
            return null;
        }

        public static MemoryStream ToStream(string base64)
        {
            if (!string.IsNullOrEmpty(base64))
            {
                byte[] bytes = ToBytes(base64);
                if (bytes != null)
                {
                    return new MemoryStream(bytes, 0, bytes.Length);
                }
                return null;
            }
            return null;
        }

        public static byte[] ReadAllBytes(string path)
        {
            if (!string.IsNullOrEmpty(path))
            {
                return System.IO.File.ReadAllBytes(path);
            }
            return null;
        }

        public static string FromFileToBase64(string path)
        {
            if (!string.IsNullOrEmpty(path))
            {
                byte[] bytes = ReadAllBytes(path);
                if (bytes != null)
                {
                    return ToBase64(bytes);
                }
                return null;
            }
            return null;
        }

        public static MemoryStream FromFileToStream(string path)
        {
            if (!string.IsNullOrEmpty(path))
            {
                byte[] bytes = ReadAllBytes(path);
                if (bytes != null)
                {
                    return new MemoryStream(bytes, 0, bytes.Length);
                }
                return null;
            }
            return null;
        }
    }

    #endregion
}
