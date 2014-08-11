using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Emit;

namespace WB.Core.BoundedContexts.Designer.Services
{
    public class GenerationResult
    {
        private readonly bool success;
        private readonly List<GenerationDiagnostic> diagnostics;
        
        public bool Success
        {
            get
            {
                return this.success;
            }
        }

        public List<GenerationDiagnostic> Diagnostics
        {
            get
            {
                return this.diagnostics;
            }
        }

        public GenerationResult(bool success)
        {
            this.success = success;
            this.diagnostics = new List<GenerationDiagnostic>() { new GenerationDiagnostic("Invalid operation", "Generation", GenerationDiagnosticSeverity.Error)};
        }

        public GenerationResult(bool success, ImmutableArray<Diagnostic> diagnostics)
        {
            this.success = success;
            this.diagnostics = diagnostics.Select(d => new GenerationDiagnostic(d.GetMessage(), d.Category, (GenerationDiagnosticSeverity)d.Severity)).ToList();
        }
    }

    public class GenerationDiagnostic 
    {
        private readonly string message;
        private readonly GenerationDiagnosticSeverity severity;
        private readonly string category;

        public string Message
        {
            get
            {
                return this.message;
            }
        }

        public string Category
        {
            get
            {
                return this.category;
            }
        }

        public GenerationDiagnosticSeverity Severity
        {
            get
            {
                return this.severity;
            }
        }

        internal GenerationDiagnostic(string message, string category, GenerationDiagnosticSeverity severity)
        {
            this.message = message;
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
