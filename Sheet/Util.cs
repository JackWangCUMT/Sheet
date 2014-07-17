using Microsoft.VisualBasic.FileIO;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sheet.Util
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

    public interface IDataReader
    {
        IEnumerable<string[]> Read(string path);
    }

    public interface IClipboard
    {
        void Set(string text);
        string Get();
    }

    public interface IJsonSerializer
    {
        string Serialize(object value);
        T Deerialize<T>(string value);
    }

    public interface IDatabaseController
    {
        string Name { get; set; }
        string[] Columns { get; set; }
        List<string[]> Data { get; set; }
        string[] Get(int index);
        bool Update(int index, string[] item);
        int Add(string[] item);
    }

    public interface IOpenFileDialog
    {
        string Filter { get; set; }
        string FileName { get; set; }
        string[] FileNames { get; set; }
        int FilterIndex { get; set; }
        bool ShowDialog();
    }

    public interface ISaveFileDialog
    {
        string Filter { get; set; }
        string FileName { get; set; }
        string[] FileNames { get; set; }
        int FilterIndex { get; set; }
        bool ShowDialog();
    }

    public class Base64 : IBase64
    {
        public string ToBase64(byte[] bytes)
        {
            if (bytes != null)
            {
                return Convert.ToBase64String(bytes);
            }
            return null;
        }

        public MemoryStream ToStream(byte[] bytes)
        {
            if (bytes != null)
            {
                return new MemoryStream(bytes, 0, bytes.Length);
            }
            return null;
        }

        public byte[] ToBytes(string base64)
        {
            if (!string.IsNullOrEmpty(base64))
            {
                return Convert.FromBase64String(base64);
            }
            return null;
        }

        public MemoryStream ToStream(string base64)
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

        public byte[] ReadAllBytes(string path)
        {
            if (!string.IsNullOrEmpty(path))
            {
                return System.IO.File.ReadAllBytes(path);
            }
            return null;
        }

        public string FromFileToBase64(string path)
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

        public MemoryStream FromFileToStream(string path)
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

    public class CsvDataReader : IDataReader
    {
        #region IDataReader

        public IEnumerable<string[]> Read(string path)
        {
            // reference Microsoft.VisualBasic, namespace Microsoft.VisualBasic.FileIO
            using (TextFieldParser parser = new TextFieldParser(path))
            {
                parser.CommentTokens = new string[] { "#" };
                parser.SetDelimiters(new string[] { ";" });
                parser.HasFieldsEnclosedInQuotes = true;
                while (!parser.EndOfData)
                {
                    string[] fields = parser.ReadFields();
                    yield return fields;
                }
            }
        }

        #endregion
    }

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

    public class CsvDatabaseController : IDatabaseController
    {
        #region Properties

        private string name = null;
        private string[] columns = null;
        private List<string[]> data = null;

        public string Name
        {
            get { return name; }
            set
            {
                name = value;
            }
        }

        public string[] Columns
        {
            get { return columns; }
            set
            {
                columns = value;
            }
        }

        public List<string[]> Data
        {
            get { return data; }
            set
            {
                data = value;
            }
        }

        #endregion

        #region Constructor

        public CsvDatabaseController(string name)
        {
            Name = name;
        }

        #endregion

        #region IDatabaseController

        public string[] Get(int index)
        {
            return data.Where(x => int.Parse(x[0]) == index).FirstOrDefault();
        }

        public bool Update(int index, string[] item)
        {
            for (int i = 0; i < data.Count; i++)
            {
                if (int.Parse(data[i][0]) == index)
                {
                    data[i] = item;
                    return true;
                }
            }
            return false;
        }

        public int Add(string[] item)
        {
            int index = data.Max((x) => int.Parse(x[0])) + 1;
            item[0] = index.ToString();
            data.Add(item);
            return index;
        }

        #endregion
    }
}
