using System;
using System.Collections.Generic;
using Autodesk.Revit.DB;

namespace VisibilityDiagnostic
{
    public static class VisibilityChecker
    {
        public static List<DiagnosticResult> DiagnoseVisibility(Document doc, View view, Element element)
        {
            List<DiagnosticResult> results = new List<DiagnosticResult>();

            // 1. Check if Element is permanently hidden in the view
            if (element.IsHidden(view))
            {
                results.Add(new DiagnosticResult("Hide in View (Element)", "The element is explicitly hidden in this view.", true));
            }
            else
            {
                results.Add(new DiagnosticResult("Hide in View (Element)", "Not hidden by element.", false));
            }

            // 2. Check Category Visibility
            Category cat = element.Category;
            if (cat != null)
            {
                if (view.GetCategoryHidden(cat.Id))
                {
                    results.Add(new DiagnosticResult("Visibility/Graphics (Category)", $"The category '{cat.Name}' is turned off in V/G.", true));
                }
                else
                {
                    results.Add(new DiagnosticResult("Visibility/Graphics (Category)", $"Category '{cat.Name}' is visible in V/G.", false));
                }
            }
            else
            {
                results.Add(new DiagnosticResult("Visibility/Graphics (Category)", "Element has no category.", false, true));
            }

            // 3. Check Workset Visibility
            if (doc.IsWorkshared)
            {
                WorksetId worksetId = element.WorksetId;
                if (worksetId != WorksetId.InvalidWorksetId)
                {
                    WorksetVisibility wsVis = view.GetWorksetVisibility(worksetId);
                    if (wsVis == WorksetVisibility.Hidden)
                    {
                        Workset workset = doc.GetWorksetTable().GetWorkset(worksetId);
                        string wsName = workset != null ? workset.Name : worksetId.ToString();
                        results.Add(new DiagnosticResult("Worksets", $"The workset '{wsName}' is hidden in this view.", true));
                    }
                    else
                    {
                        results.Add(new DiagnosticResult("Worksets", "Workset is visible.", false));
                    }
                }
                else
                {
                    results.Add(new DiagnosticResult("Worksets", "Element does not belong to a valid workset.", false, true));
                }
            }
            else
            {
                results.Add(new DiagnosticResult("Worksets", "Project is not workshared. Skipped.", false, true));
            }

            // 4. Check Temporary Hide/Isolate
            if (view.IsTemporaryViewPropertiesModeEnabled() || view.IsTemporaryHideIsolateActive())
            {
                if (view.IsElementVisibleInTemporaryViewMode(TemporaryViewMode.TemporaryHideIsolate, element.Id))
                {
                    results.Add(new DiagnosticResult("Temporary Hide/Isolate", "Not hidden by Temporary Hide.", false));
                }
                else
                {
                    results.Add(new DiagnosticResult("Temporary Hide/Isolate", "The element is hidden by the Temporary Hide/Isolate tool (glasses icon).", true));
                }
            }
            else
            {
                results.Add(new DiagnosticResult("Temporary Hide/Isolate", "Temporary Hide/Isolate is not active.", false, true));
            }

            // 5. Check View Filters (simplified check)
            ICollection<ElementId> filters = view.GetFilters();
            bool hiddenByFilter = false;
            foreach (ElementId filterId in filters)
            {
                if (!view.GetFilterVisibility(filterId))
                {
                    ParameterFilterElement filter = doc.GetElement(filterId) as ParameterFilterElement;
                    if (filter != null && filter.GetElementFilter() != null)
                    {
                        if (filter.GetElementFilter().PassesFilter(doc, element.Id))
                        {
                            results.Add(new DiagnosticResult("View Filters", $"The element is caught by filter '{filter.Name}' which has visibility turned off.", true));
                            hiddenByFilter = true;
                        }
                    }
                }
            }
            if (!hiddenByFilter)
            {
                results.Add(new DiagnosticResult("View Filters", "Not hidden by any View Filter.", false));
            }

            // 6. Check View Crop / View Range
            if (view.CropBoxActive)
            {
                BoundingBoxXYZ viewCrop = view.CropBox;
                BoundingBoxXYZ elemBox = element.get_BoundingBox(null);
                
                if (elemBox != null && viewCrop != null)
                {
                    Transform t = viewCrop.Transform;
                    XYZ elemCenter = (elemBox.Min + elemBox.Max) / 2.0;
                    XYZ elemCenterInView = t.Inverse.OfPoint(elemCenter);

                    if (elemCenterInView.X < viewCrop.Min.X || elemCenterInView.X > viewCrop.Max.X ||
                        elemCenterInView.Y < viewCrop.Min.Y || elemCenterInView.Y > viewCrop.Max.Y)
                    {
                        results.Add(new DiagnosticResult("Crop Region", "The element's center appears to be outside the view's crop box.", true));
                    }
                    else
                    {
                        results.Add(new DiagnosticResult("Crop Region", "Element center is within the crop region.", false));
                    }
                }
                else
                {
                    results.Add(new DiagnosticResult("Crop Region", "Could not calculate bounding box.", false, true));
                }
            }
            else
            {
                results.Add(new DiagnosticResult("Crop Region", "Crop Region is not active. Skipped.", false, true));
            }

            return results;
        }
    }
}
