using Microsoft.VisualBasic.FileIO;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sheet
{
    #region IDataReader

    public interface IDataReader
    {
        IEnumerable<string[]> Read(string path);
    } 

    #endregion

    #region CsvDataReader

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

    #endregion
}
