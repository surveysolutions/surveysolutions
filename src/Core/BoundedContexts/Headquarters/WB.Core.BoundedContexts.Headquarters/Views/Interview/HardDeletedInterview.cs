using System;
using WB.Core.SharedKernels.SurveySolutions.Documents;

namespace WB.Core.BoundedContexts.Headquarters.Views.Interview
{
    public class HardDeletedInterview : IView
    {
        public Guid InterviewId { get; set; }
    }
}
