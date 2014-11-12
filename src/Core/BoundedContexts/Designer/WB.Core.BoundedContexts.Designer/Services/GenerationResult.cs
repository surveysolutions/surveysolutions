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
            this.Diagnostics =
                diagnostics.Select(d => new GenerationDiagnostic(d.GetMessage(), d.Category, d.Location.SourceTree.FilePath, 
                    (GenerationDiagnosticSeverity)d.Severity)).ToList();
        }
    }

    public class GenerationDiagnostic
    {
        private readonly string message;
        private readonly GenerationDiagnosticSeverity severity;
        private readonly string category;
        private readonly string location;

        public string Message
        {
            get { return this.message; }
        }

        public string Category
        {
            get { return this.category; }
        }

        public string Location
        {
            get { return this.location; }
        }

        public GenerationDiagnosticSeverity Severity
        {
            get { return this.severity; }
        }

        public GenerationDiagnostic(string message, string category, string location,GenerationDiagnosticSeverity severity)
        {
            this.message = message;
            this.location = location;
            this.severity = severity;
            this.category = category;
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