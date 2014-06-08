using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sheet.Controller
{
    public static class FileDialogSettings
    {
        #region Extensions

        public static string SolutionExtension = ".solution";
        public static string DocumentExtension = ".document";
        public static string PageExtension = ".page";
        public static string LibraryExtension = ".library";

        public static string DatabaseExtension = ".csv";

        public static string JsonSolutionExtension = ".jsolution";
        public static string JsonDocumentExtension = ".jdocument";
        public static string JsonPageExtension = ".jpage";
        public static string JsonLibraryExtension = ".jlibrary";

        #endregion

        #region Filters

        public static string SolutionFilter = "Solution Files (*.solution)|*.solution|Json Solution Files (*.jsolution)|*.jsolution|All Files (*.*)|*.*";
        public static string DocumentFilter = "Document Files (*.document)|*.document|Json Document Files (*.jdocument)|*.jdocument|All Files (*.*)|*.*";
        public static string PageFilter = "Page Files (*.page)|*.page|Json Page Files (*.jpage)|*.jpage|All Files (*.*)|*.*";
        public static string LibraryFilter = "Library Files (*.library)|*.library|Json Library Files (*.jlibrary)|*.jlibrary|All Files (*.*)|*.*";

        public static string DatabaseFilter = "Csv Files (*.csv)|*.csv|All Files (*.*)|*.*";
        public static string ImageFilter = "Supported Files (*.png;*.jpg;*.jpeg)|*.png;*.jpg;*.jpeg|Png Files (*.png)|*.png|Jpg Files (*.jpg)|*.jpg|Jpeg Files (*.jpeg)|*.jpeg|All Files (*.*)|*.*";
        public static string ExportFilter = "Pdf Documents (*.pdf)|*.pdf|Dxf Documents (*.dxf)|*.dxf|All Files (*.*)|*.*";

        #endregion
    }
}
