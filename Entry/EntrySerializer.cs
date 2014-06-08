using Sheet.Entry.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sheet.Entry
{
    public class EntrySerializer : IEntrySerializer
    {
        #region IoC

        private readonly IEntryFactory _entryFactory;

        public EntrySerializer(IEntryFactory entryFactory)
        {
            this._entryFactory = entryFactory;
        }

        #endregion

        #region Fields

        private static char[] entryNameSeparator = { '/' };

        #endregion

        #region Add

        public void AddDocumentEntry(ZipArchive zip, string document)
        {
            var name = string.Concat(document, '/');
            var entry = zip.CreateEntry(name);
        }

        public void AddPageEntry(ZipArchive zip, string document, string page, string content)
        {
            var name = string.Concat(document, '/', page);
            var entry = zip.CreateEntry(name);
            using (var writer = new StreamWriter(entry.Open()))
            {
                writer.Write(content);
            }
        }

        #endregion

        #region Empty

        public void CreateEmpty(string path)
        {
            using (FileStream fs = new FileStream(path, FileMode.Create))
            {
                using (ZipArchive zip = new ZipArchive(fs, ZipArchiveMode.Update))
                {
                    AddPageEntry(zip, "Document0", "Page", "");
                }
            }
        }

        #endregion

        #region Serialize

        public void Serialize(SolutionEntry solution, string path)
        {
            using (FileStream fs = new FileStream(path, FileMode.Create))
            {
                using (ZipArchive zip = new ZipArchive(fs, ZipArchiveMode.Update))
                {
                    foreach (var document in solution.Documents)
                    {
                        if (document.Pages.Count <= 0)
                        {
                            AddDocumentEntry(zip, document.Name);
                        }

                        foreach (var page in document.Pages)
                        {
                            AddPageEntry(zip, document.Name, page.Name, page.Content);
                        }
                    }
                }
            }
        }

        #endregion

        #region Deserialize

        public SolutionEntry Deserialize(string path)
        {
            string solutionName = System.IO.Path.GetFileNameWithoutExtension(path);

            var dict = new Dictionary<string, List<Tuple<string, string>>>();
            var solution = new SolutionEntry() { Name = solutionName, Documents = new ObservableCollection<DocumentEntry>() };

            using (var zip = ZipFile.Open(path, ZipArchiveMode.Read))
            {
                foreach (var entry in zip.Entries)
                {
                    var e = entry.FullName.Split(entryNameSeparator);
                    if (e.Length == 1)
                    {
                        string key = e[0];

                        if (!dict.ContainsKey(key))
                        {
                            dict.Add(key, new List<Tuple<string, string>>());
                        }
                    }
                    else if (e.Length == 2)
                    {
                        string key = e[0];
                        string data = e[1];
                        string content = null;

                        using (var reader = new StreamReader(entry.Open()))
                        {
                            content = reader.ReadToEnd();
                        }

                        if (!dict.ContainsKey(key))
                        {
                            dict.Add(key, new List<Tuple<string, string>>());
                        }

                        dict[key].Add(new Tuple<string, string>(data, content));
                    }
                }
            }

            foreach (var item in dict)
            {
                var document = _entryFactory.CreateDocument(solution, item.Key);
                solution.Documents.Add(document);
                foreach (var tuple in item.Value)
                {
                    var page = _entryFactory.CreatePage(document, tuple.Item2, tuple.Item1);
                    document.Pages.Add(page);
                }
            }

            return solution;
        }

        #endregion
    }
}
