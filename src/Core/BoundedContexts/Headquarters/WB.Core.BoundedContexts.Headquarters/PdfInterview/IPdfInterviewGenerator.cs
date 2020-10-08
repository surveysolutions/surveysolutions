#nullable enable

using System;
using System.IO;
using Main.Core.Entities.SubEntities;

namespace WB.Core.BoundedContexts.Headquarters.PdfInterview
{
    public interface IPdfInterviewGenerator
    {
        Stream? Generate(Guid interviewId);
        Stream? Generate(Guid interviewId, PdfView pdfView);
    }
}