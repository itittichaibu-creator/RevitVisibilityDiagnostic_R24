using System;
using System.Collections.Generic;

namespace RevitVisibilityDiagnostic_R24
{
    public class DiagnosticResult
    {
        public string CheckName { get; set; }
        public string Message { get; set; }
        public bool IsHiddenReason { get; set; }
        public bool IsSkipped { get; set; }

        public DiagnosticResult(string checkName, string message, bool isHiddenReason, bool isSkipped = false)
        {
            CheckName = checkName;
            Message = message;
            IsHiddenReason = isHiddenReason;
            IsSkipped = isSkipped;
        }
    }
}
