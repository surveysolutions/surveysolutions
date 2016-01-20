using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace WB.Core.BoundedContexts.Designer.Services
{
    public class GenerationResult
    {
        public bool Success { get; set; }

        public List<GenerationDiagnostic> Diagnostics { get; set; }

        public GenerationResult() {}

        public GenerationResult(bool success, ImmutableArray<Diagnostic> diagnostics)
        {
            this.Success = success;
            this.Diagnostics = diagnostics
                .Select(d => new GenerationDiagnostic(
                    message: d.GetMessage(),
                    location: d.Location.SourceTree.FilePath,
                    severity: (GenerationDiagnosticSeverity)d.Severity))
                .ToList();
        }
    }

    public class GenerationDiagnostic
    {
        private readonly string message;
        private readonly GenerationDiagnosticSeverity severity;
        private readonly string location;

        public string Message
        {
            get { return this.message; }
        }

        public string Location
        {
            get { return this.location; }
        }

        public GenerationDiagnosticSeverity Severity
        {
            get { return this.severity; }
        }

        public GenerationDiagnostic(string message, string location, GenerationDiagnosticSeverity severity)
        {
            this.message = message;
            this.location = location;
            this.severity = severity;
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