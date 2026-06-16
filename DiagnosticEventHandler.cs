using System;
using System.Linq;
using System.Collections.Generic;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace RevitVisibilityDiagnostic_R24
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
        public DiagnosticResult PendingFix { get; set; }

        public void Execute(UIApplication app)
        {
            if (OnResultReady == null) return;

            UIDocument uidoc = app.ActiveUIDocument;
            if (uidoc == null) return;

            Document doc = uidoc.Document;

            View viewToCheck = TargetView ?? doc.ActiveView;
            if (viewToCheck == null) return;

            if (PendingFix != null)
            {
                using (Transaction t = new Transaction(doc, "Fix Visibility Issue"))
                {
                    t.Start();
                    try
                    {
                        if (PendingFix.ActionType == FixActionType.UnhideElement)
                        {
                            viewToCheck.UnhideElements(new List<ElementId> { PendingFix.TargetId });
                        }
                        else if (PendingFix.ActionType == FixActionType.UnhideCategory)
                        {
                            viewToCheck.SetCategoryHidden(PendingFix.TargetId, false);
                        }
                        else if (PendingFix.ActionType == FixActionType.UnhideWorkset)
                        {
                            viewToCheck.SetWorksetVisibility(PendingFix.TargetWorksetId, WorksetVisibility.Visible);
                        }
                        else if (PendingFix.ActionType == FixActionType.DisableTemporaryHide)
                        {
                            if (viewToCheck.IsTemporaryHideIsolateActive())
                            {
                                viewToCheck.DisableTemporaryViewMode(TemporaryViewMode.TemporaryHideIsolate);
                            }
                        }
                        else if (PendingFix.ActionType == FixActionType.UnhideFilter)
                        {
                            viewToCheck.SetFilterVisibility(PendingFix.TargetId, true);
                        }
                        t.Commit();
                    }
                    catch (Exception)
                    {
                        t.RollBack();
                    }
                }
                PendingFix = null;
            }

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
