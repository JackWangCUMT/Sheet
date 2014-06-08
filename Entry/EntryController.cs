using Sheet.Entry.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sheet.Entry
{
    public class EntryController : IEntryController
    {
        #region IoC

        private readonly IEntryFactory _entryFactory;

        public EntryController(IEntryFactory entryFactory)
        {
            this._entryFactory = entryFactory;
        }

        #endregion

        #region Page

        public PageEntry AddPage(DocumentEntry document, string content)
        {
            var page = _entryFactory.CreatePage(document, content);
            document.Pages.Add(page);
            return page;
        }

        public PageEntry AddPageBefore(DocumentEntry document, PageEntry beofore, string content)
        {
            var page = _entryFactory.CreatePage(document, content);
            int index = document.Pages.IndexOf(beofore);
            document.Pages.Insert(index, page);
            return page;
        }

        public PageEntry AddPageAfter(DocumentEntry document, PageEntry after, string content)
        {
            var page = _entryFactory.CreatePage(document, content);
            int index = document.Pages.IndexOf(after);
            document.Pages.Insert(index + 1, page);
            return page;
        }

        public void AddPageAfter(object item)
        {
            if (item != null && item is PageEntry)
            {
                var page = item as PageEntry;
                var document = page.Document;
                if (document != null)
                {
                    AddPageAfter(document, page, "");
                }
            }
        }

        public void AddPageBefore(object item)
        {
            if (item != null && item is PageEntry)
            {
                var page = item as PageEntry;
                var document = page.Document;
                if (document != null)
                {
                    AddPageBefore(document, page, "");
                }
            }
        }

        public void DuplicatePage(object item)
        {
            if (item != null && item is PageEntry)
            {
                var page = item as PageEntry;
                var document = page.Document;
                if (document != null)
                {
                    AddPage(document, page.Content);
                }
            }
        }

        public void RemovePage(object item)
        {
            if (item != null && item is PageEntry)
            {
                var page = item as PageEntry;
                var document = page.Document;
                if (document != null)
                {
                    document.Pages.Remove(page);
                }
            }
        }

        #endregion

        #region Document

        public DocumentEntry AddDocumentBefore(SolutionEntry solution, DocumentEntry after)
        {
            var document = _entryFactory.CreateDocument(solution);
            int index = solution.Documents.IndexOf(after);
            solution.Documents.Insert(index, document);
            return document;
        }

        public DocumentEntry AddDocumentAfter(SolutionEntry solution, DocumentEntry after)
        {
            var document = _entryFactory.CreateDocument(solution);
            int index = solution.Documents.IndexOf(after);
            solution.Documents.Insert(index + 1, document);
            return document;
        }

        public DocumentEntry AddDocument(SolutionEntry solution)
        {
            var document = _entryFactory.CreateDocument(solution);
            solution.Documents.Add(document);
            return document;
        }

        public void DocumentAddPage(object item)
        {
            if (item != null && item is DocumentEntry)
            {
                var document = item as DocumentEntry;
                AddPage(document, "");
            }
        }

        public void AddDocumentAfter(object item)
        {
            if (item != null && item is DocumentEntry)
            {
                var document = item as DocumentEntry;
                var solution = document.Solution;
                if (solution != null)
                {
                    AddDocumentAfter(solution, document);
                }
            }
        }

        public void AddDocumentBefore(object item)
        {
            if (item != null && item is DocumentEntry)
            {
                var document = item as DocumentEntry;
                var solution = document.Solution;
                if (solution != null)
                {
                    AddDocumentBefore(solution, document);
                }
            }
        }

        public void DulicateDocument(object item)
        {
            if (item != null && item is DocumentEntry)
            {
                var document = item as DocumentEntry;
                var solution = document.Solution;
                if (solution != null)
                {
                    var duplicate = AddDocument(solution);
                    foreach (var page in document.Pages)
                    {
                        AddPage(duplicate, page.Content);
                    }
                }
            }
        }

        public void RemoveDocument(object item)
        {
            if (item != null && item is DocumentEntry)
            {
                var document = item as DocumentEntry;
                var solution = document.Solution;
                if (solution != null)
                {
                    solution.Documents.Remove(document);
                }
            }
        }

        #endregion
    }
}
