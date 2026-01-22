using System.Linq;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Events;

namespace FamilyCompactor.FamilyCleaner.Open
{
    public class FailureProcessor
    {
        public void ApplicationOnFailuresProcessing(object sender, FailuresProcessingEventArgs e)
        {
            FailuresAccessor accessor = e.GetFailuresAccessor();

  
            accessor.DeleteAllWarnings();
            
            accessor.ResolveFailures(accessor.GetFailureMessages());
            
            ElementId[] elementIds = accessor.GetFailureMessages()
                .SelectMany(item => item.GetFailingElementIds())
                .ToArray();


            if (elementIds.Length > 0)
            {
                accessor.DeleteElements(elementIds);
                e.SetProcessingResult(FailureProcessingResult.ProceedWithCommit); 
            }
            else
            {
                e.SetProcessingResult(FailureProcessingResult.Continue); 
            }
        }
        
    }
}