using Sheet.Entry.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sheet.Entry
{
    public interface IEntryFactory
    {
        PageEntry CreatePage(DocumentEntry document, string content, string name = null);
        DocumentEntry CreateDocument(SolutionEntry solution, string name = null);
    }
}
