using System;
using System.IO;
using System.Security.Principal;
using WB.Core.SharedKernels.DataCollection.Aggregates;

namespace WB.UI.Headquarters.PdfInterview
{
    public interface IPdfInterviewGenerator
    {
        Stream Generate(Guid interviewId, IPrincipal user);
    }
}