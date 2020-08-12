using System;
using WB.Core.SharedKernels.DataCollection.Aggregates;

namespace WB.UI.Headquarters.PdfInterview
{
    public interface IPdfInterviewGenerator
    {
        byte[] Generate(Guid interviewId);
    }
}