using System;

namespace WB.Core.SharedKernel.Structures.Synchronization.SurveyManagement
{
    public class PostPackageRequest
    {
        public Guid InterviewId { get; set; }
        public string SynchronizationPackage { get; set; }
    }
}