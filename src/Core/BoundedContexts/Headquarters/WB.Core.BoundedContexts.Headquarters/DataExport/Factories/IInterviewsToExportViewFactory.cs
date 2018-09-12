using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WB.Core.BoundedContexts.Headquarters.DataExport.Views;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;

namespace WB.Core.BoundedContexts.Headquarters.DataExport.Factories
{
    public interface IInterviewsToExportViewFactory
    {
        List<InterviewToExport> GetInterviewsToExport(QuestionnaireIdentity questionnaireIdentity,
            InterviewStatus? status, DateTime? fromDate, DateTime? toDate);
    }
}
