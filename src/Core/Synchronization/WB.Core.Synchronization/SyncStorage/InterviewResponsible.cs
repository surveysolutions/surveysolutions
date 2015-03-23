using System;
using WB.Core.SharedKernels.SurveySolutions.Documents;

namespace WB.Core.Synchronization.SyncStorage
{
    public class InterviewResponsible : IView
    {
        public Guid InterviewId { get; set; }

        public Guid UserId { get; set; }
    }
}