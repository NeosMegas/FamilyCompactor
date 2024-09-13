using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace FamilyCompactor.FamilyCleaner.Open
{
    public class FamilyOpener
    {
        public static Document OpenFamily(string path, Application app)
        {
            var doc = app.OpenDocumentFile(path);
            return doc;
        }
    }
}