using System;
using System.Collections.Generic;
using WB.Core.BoundedContexts.Headquarters.DataExport.Views;

namespace WB.Core.BoundedContexts.Headquarters.DataExport.Factories
{
    public interface IInterviewsToExportViewFactory
    {
        List<InterviewApiComment> GetInterviewComments(Guid interviewId);
    }
}
