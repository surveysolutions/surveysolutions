using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace WB.Core.BoundedContexts.Designer.Services
{
    public class GenerationResult
    {
        public bool Success { get; set; }

        public List<GenerationDiagnostic> Diagnostics { get; set; }

        public GenerationResult()
        {
            Success = false;
            Diagnostics = new List<GenerationDiagnostic>();
        }

        public GenerationResult(bool success, IEnumerable<Diagnostic> diagnostics)
        {
            this.Success = success;
            this.Diagnostics = diagnostics
                .Select(d => new GenerationDiagnostic(
                    message: d.GetMessage(),
                    location: d.Location.SourceTree?.FilePath ?? String.Empty,
                    severity: (GenerationDiagnosticSeverity)d.Severity))
                .ToList();
        }
    }

    public class GenerationDiagnostic
    {
        public string Message { get; }

        public string Location { get; }

        public GenerationDiagnosticSeverity Severity { get; }

        public GenerationDiagnostic(string message, string location, GenerationDiagnosticSeverity severity)
        {
            this.Message = message;
            this.Location = location;
            this.Severity = severity;
        }
    }

    public enum GenerationDiagnosticSeverity
    {
        Hidden,
        Info,
        Warning,
        Error,
    }
}
