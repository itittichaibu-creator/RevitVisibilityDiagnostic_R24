using System;
using System.Linq;
using System.Collections.Generic;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace VisibilityDiagnostic
{
    public class DiagnosticResultData
    {
        public string Header { get; set; }
        public List<DiagnosticResult> Results { get; set; }
    }

    public class DiagnosticEventHandler : IExternalEventHandler
    {
        public Action<DiagnosticResultData> OnResultReady { get; set; }
        public View TargetView { get; set; }

        public void Execute(UIApplication app)
        {
            if (OnResultReady == null) return;

            UIDocument uidoc = app.ActiveUIDocument;
            if (uidoc == null) return;

            Document doc = uidoc.Document;

            View viewToCheck = TargetView ?? doc.ActiveView;
            if (viewToCheck == null) return;

            ICollection<ElementId> selectedIds = uidoc.Selection.GetElementIds();

            if (selectedIds.Count == 0)
            {
                OnResultReady(new DiagnosticResultData { Header = "No element selected. Please select an element to diagnose." });
                return;
            }

            if (selectedIds.Count > 1)
            {
                OnResultReady(new DiagnosticResultData { Header = "Multiple elements selected. Please select only one element." });
                return;
            }

            ElementId id = selectedIds.First();
            Element elem = doc.GetElement(id);

            if (elem == null)
            {
                OnResultReady(new DiagnosticResultData { Header = "Selected element is invalid." });
                return;
            }

            string header = $"Diagnosing Element {elem.Id.Value} ({elem.Name})\nin View '{viewToCheck.ViewType}: {viewToCheck.Name}'...\n";
            List<DiagnosticResult> results = VisibilityChecker.DiagnoseVisibility(doc, viewToCheck, elem);

            OnResultReady(new DiagnosticResultData { Header = header, Results = results });
        }

        public string GetName()
        {
            return "Visibility Diagnostic Event Handler";
        }
    }
}
