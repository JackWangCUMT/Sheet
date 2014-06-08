using Sheet.Entry.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sheet.Entry
{
    public class EntryFactory : IEntryFactory
    {
        #region Create

        public PageEntry CreatePage(DocumentEntry document, string content, string name = null)
        {
            var page = new PageEntry()
            {
                Name = name == null ? "Page" : name,
                Content = content,
                Document = document
            };
            return page;
        }

        public DocumentEntry CreateDocument(SolutionEntry solution, string name = null)
        {
            var document = new DocumentEntry()
            {
                Name = name == null ? string.Concat("Document", solution.Documents.Count) : name,
                Pages = new ObservableCollection<PageEntry>(),
                Solution = solution
            };
            return document;
        }

        #endregion
    }
}
