using System;
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.UI;
using Autodesk.Revit.DB;
using FamilyCompactor.FamilyCleaner.Open;

namespace FamilyCompactor.FamilyCleaner.Worker
{
    public class Worker
    {

        private static Application _app;
        private readonly FailureProcessor _failureProcessor;
    
        public Worker(Application app, string path)
        {
            _failureProcessor = new FailureProcessor();
            _app = app;
        }

        public void Execute(string pathDownload, string pathSave)
        {
            _app.FailuresProcessing += _failureProcessor.ApplicationOnFailuresProcessing;
            var doc = FamilyOpener.OpenFamily(pathDownload, Worker._app);
        
            try
            {
                var t = new Transaction(doc, "CleaningFamily");
                
                var cm = new CleaningManager.CleaningManager();

                t.Start();

                cm.CleaningFamily(doc);

                t.Commit();
                

                cm.DeleteUnused(doc);

                doc.SaveAs(pathSave);
                doc.Close(false);
                _app.FailuresProcessing -= _failureProcessor.ApplicationOnFailuresProcessing;
            }
            catch (Exception e)
            {
                TaskDialog.Show("Err", e.ToString());
                doc.Close(false);
                _app.FailuresProcessing -= _failureProcessor.ApplicationOnFailuresProcessing;
            }
        }
    }
}