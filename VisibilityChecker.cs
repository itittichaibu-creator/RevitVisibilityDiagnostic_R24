using System;
using System.Collections.Generic;
using Autodesk.Revit.DB;

namespace RevitVisibilityDiagnostic_R24
{
    public static class VisibilityChecker
    {
        public static List<DiagnosticResult> DiagnoseVisibility(Document doc, View view, Element element)
        {
            List<DiagnosticResult> results = new List<DiagnosticResult>();

            // 1. Check if Element is permanently hidden in the view
            if (element.IsHidden(view))
            {
                results.Add(new DiagnosticResult("Hide in View (Element)", "The element is explicitly hidden in this view.", true)
                {
                    CanFix = true,
                    ActionType = FixActionType.UnhideElement,
                    TargetId = element.Id
                });
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
                    results.Add(new DiagnosticResult("Visibility/Graphics (Category)", $"The category '{cat.Name}' is turned off in V/G.", true)
                    {
                        CanFix = true,
                        ActionType = FixActionType.UnhideCategory,
                        TargetId = cat.Id
                    });
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
                        results.Add(new DiagnosticResult("Worksets", $"The workset '{wsName}' is hidden in this view.", true)
                        {
                            CanFix = true,
                            ActionType = FixActionType.UnhideWorkset,
                            TargetWorksetId = worksetId
                        });
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
                    results.Add(new DiagnosticResult("Temporary Hide/Isolate", "The element is hidden by the Temporary Hide/Isolate tool (glasses icon).", true)
                    {
                        CanFix = true,
                        ActionType = FixActionType.DisableTemporaryHide
                    });
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
                            results.Add(new DiagnosticResult("View Filters", $"The element is caught by filter '{filter.Name}' which has visibility turned off.", true)
                            {
                                CanFix = true,
                                ActionType = FixActionType.UnhideFilter,
                                TargetId = filterId
                            });
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

            // 7. Check Detail Level
            try
            {
                ViewDetailLevel viewDl = view.DetailLevel;
                OverrideGraphicSettings elemOverride = null;
                OverrideGraphicSettings catOverride = null;

                if (view.AreGraphicsOverridesAllowed())
                {
                    try { elemOverride = view.GetElementOverrides(element.Id); } catch { }
                    if (element.Category != null)
                    {
                        try { catOverride = view.GetCategoryOverrides(element.Category.Id); } catch { }
                    }
                }

                string detailLevelMsg = $"View Detail Level: {viewDl}.";
                if (elemOverride != null && elemOverride.DetailLevel != ViewDetailLevel.Undefined)
                {
                    detailLevelMsg += $" Element Override: {elemOverride.DetailLevel}.";
                }
                else if (catOverride != null && catOverride.DetailLevel != ViewDetailLevel.Undefined)
                {
                    detailLevelMsg += $" Category Override: {catOverride.DetailLevel}.";
                }
                results.Add(new DiagnosticResult("Detail Level", detailLevelMsg, false));
            }
            catch (Exception ex)
            {
                results.Add(new DiagnosticResult("Detail Level", $"Could not check: {ex.Message}", false, true));
            }

            // 8. Check Visual Style
            try
            {
                DisplayStyle viewStyle = view.DisplayStyle;
                results.Add(new DiagnosticResult("Visual Style", $"View Visual Style is set to: {viewStyle}.", false));
            }
            catch (Exception ex)
            {
                results.Add(new DiagnosticResult("Visual Style", $"Could not check: {ex.Message}", false, true));
            }

            // 9. Check Phasing
            try
            {
                Parameter viewPhaseParam = view.get_Parameter(BuiltInParameter.VIEW_PHASE);
                Parameter viewPhaseFilterParam = view.get_Parameter(BuiltInParameter.VIEW_PHASE_FILTER);

                string viewPhaseName = "None";
                if (viewPhaseParam != null && viewPhaseParam.AsElementId() != ElementId.InvalidElementId)
                {
                    var pElem = doc.GetElement(viewPhaseParam.AsElementId());
                    if (pElem != null) viewPhaseName = pElem.Name;
                }

                string phaseFilterName = "None";
                if (viewPhaseFilterParam != null && viewPhaseFilterParam.AsElementId() != ElementId.InvalidElementId)
                {
                    var pfElem = doc.GetElement(viewPhaseFilterParam.AsElementId());
                    if (pfElem != null) phaseFilterName = pfElem.Name;
                }

                ElementId createdPhaseId = element.CreatedPhaseId;
                ElementId demolishedPhaseId = element.DemolishedPhaseId;

                string createdPhaseName = "None";
                if (createdPhaseId != null && createdPhaseId != ElementId.InvalidElementId)
                {
                    var cElem = doc.GetElement(createdPhaseId);
                    if (cElem != null) createdPhaseName = cElem.Name;
                }

                string demolishedPhaseName = "None";
                if (demolishedPhaseId != null && demolishedPhaseId != ElementId.InvalidElementId)
                {
                    var dElem = doc.GetElement(demolishedPhaseId);
                    if (dElem != null) demolishedPhaseName = dElem.Name;
                }

                string phaseMsg = $"View Phase: {viewPhaseName} (Filter: {phaseFilterName}).\nElement Created: {createdPhaseName}, Demolished: {demolishedPhaseName}.";
                bool isHiddenByPhase = false;

                PhaseArray phases = doc.Phases;
                int viewPhaseIndex = -1;
                int createdPhaseIndex = -1;
                int demolishedPhaseIndex = -1;

                if (phases != null)
                {
                    for (int i = 0; i < phases.Size; i++)
                    {
                        Phase p = phases.get_Item(i);
                        if (viewPhaseParam != null && p.Id == viewPhaseParam.AsElementId()) viewPhaseIndex = i;
                        if (createdPhaseId != null && p.Id == createdPhaseId) createdPhaseIndex = i;
                        if (demolishedPhaseId != null && p.Id == demolishedPhaseId) demolishedPhaseIndex = i;
                    }
                }

                if (createdPhaseIndex != -1 && viewPhaseIndex != -1 && viewPhaseIndex < createdPhaseIndex)
                {
                    isHiddenByPhase = true;
                    phaseMsg += "\n(Element created after view phase)";
                }
                else if (demolishedPhaseIndex != -1 && viewPhaseIndex != -1 && viewPhaseIndex >= demolishedPhaseIndex)
                {
                    phaseMsg += "\n(Element demolished in or before view phase)";
                }

                results.Add(new DiagnosticResult("Phasing", phaseMsg, isHiddenByPhase));
            }
            catch (Exception ex)
            {
                results.Add(new DiagnosticResult("Phasing", $"Could not check: {ex.Message}", false, true));
            }

            // 10. Check Discipline
            try
            {
                ViewDiscipline viewDiscipline = view.Discipline;
                string categoryName = element.Category != null ? element.Category.Name : "None";
                bool isHiddenByDiscipline = false;
                string disciplineMsg = $"View Discipline: {viewDiscipline}.\nElement Category: {categoryName}.";

                if (element.Category != null)
                {
                    BuiltInCategory bic = (BuiltInCategory)element.Category.Id.Value;

                    if (viewDiscipline == ViewDiscipline.Structural)
                    {
                        List<BuiltInCategory> archCategories = new List<BuiltInCategory>
                        {
                            BuiltInCategory.OST_Doors, BuiltInCategory.OST_Windows, BuiltInCategory.OST_Furniture,
                            BuiltInCategory.OST_FurnitureSystems, BuiltInCategory.OST_Casework, BuiltInCategory.OST_Planting,
                            BuiltInCategory.OST_Entourage, BuiltInCategory.OST_PlumbingFixtures, BuiltInCategory.OST_SpecialityEquipment,
                            BuiltInCategory.OST_Ceilings, BuiltInCategory.OST_RoofSoffit, BuiltInCategory.OST_Fascia,
                            BuiltInCategory.OST_Gutter, BuiltInCategory.OST_CurtainWallPanels, BuiltInCategory.OST_CurtainWallMullions
                        };

                        if (archCategories.Contains(bic))
                        {
                            isHiddenByDiscipline = true;
                            disciplineMsg += "\n(Architectural elements are typically hidden in Structural views)";
                        }
                        else if (bic == BuiltInCategory.OST_Walls)
                        {
                            Parameter structUsage = element.get_Parameter(BuiltInParameter.WALL_STRUCTURAL_USAGE_PARAM);
                            if (structUsage != null && structUsage.AsInteger() == 0) // 0 is NonBearing
                            {
                                isHiddenByDiscipline = true;
                                disciplineMsg += "\n(Non-bearing walls are typically hidden in Structural views)";
                            }
                        }
                    }
                }

                results.Add(new DiagnosticResult("Discipline", disciplineMsg, isHiddenByDiscipline));
            }
            catch (Exception ex)
            {
                results.Add(new DiagnosticResult("Discipline", $"Could not check: {ex.Message}", false, true));
            }

            return results;
        }
    }
}
