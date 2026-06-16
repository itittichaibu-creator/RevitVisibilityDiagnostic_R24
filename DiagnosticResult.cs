using System;
using System.Collections.Generic;

namespace RevitVisibilityDiagnostic_R24
{
    public enum FixActionType
    {
        None,
        UnhideElement,
        UnhideCategory,
        UnhideWorkset,
        DisableTemporaryHide,
        UnhideFilter
    }

    public class DiagnosticResult
    {
        public string CheckName { get; set; }
        public string Message { get; set; }
        public bool IsHiddenReason { get; set; }
        public bool IsSkipped { get; set; }

        // Auto-Fix Properties
        public bool CanFix { get; set; }
        public FixActionType ActionType { get; set; }
        public Autodesk.Revit.DB.ElementId TargetId { get; set; }
        public Autodesk.Revit.DB.WorksetId TargetWorksetId { get; set; }

        public DiagnosticResult(string checkName, string message, bool isHiddenReason, bool isSkipped = false)
        {
            CheckName = checkName;
            Message = message;
            IsHiddenReason = isHiddenReason;
            IsSkipped = isSkipped;
            CanFix = false;
            ActionType = FixActionType.None;
            TargetId = Autodesk.Revit.DB.ElementId.InvalidElementId;
            TargetWorksetId = Autodesk.Revit.DB.WorksetId.InvalidWorksetId;
        }
    }
}
