using System;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace RevitVisibilityDiagnostic_R24
{
    [Transaction(TransactionMode.Manual)]
    public class Command : IExternalCommand
    {
        public Result Execute(
            ExternalCommandData commandData, 
            ref string message, 
            ElementSet elements)
        {
            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Document doc = uidoc.Document;
            View activeView = doc.ActiveView;

            try
            {
                // Set up the event handler and external event
                DiagnosticEventHandler handler = new DiagnosticEventHandler();
                ExternalEvent exEvent = ExternalEvent.Create(handler);

                // Show the modeless window
                UI.DiagnosticWindow window = new UI.DiagnosticWindow(doc, exEvent, handler);
                window.Show();

                return Result.Succeeded;
            }
            catch (Exception ex)
            {
                message = ex.Message;
                return Result.Failed;
            }
        }
    }
}
