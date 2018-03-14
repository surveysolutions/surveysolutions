using System;
using WB.Core.BoundedContexts.Headquarters.ValueObjects.PreloadedData;

namespace WB.Core.BoundedContexts.Headquarters.AssignmentImport.Parser
{
    public class InterviewImportException : Exception
    {
        public string Code { get; }
        public InterviewImportReference[] References { get; }

        public InterviewImportException(string code, string message, params InterviewImportReference[] references) :base(message)
        {
            Code = code;
            References = references;
        }
    }
}
