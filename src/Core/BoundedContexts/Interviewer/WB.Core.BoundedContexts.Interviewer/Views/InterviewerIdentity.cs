using System;
using WB.Core.BoundedContexts.Interviewer.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;

namespace WB.Core.BoundedContexts.Interviewer.Views
{
    public class InterviewerIdentity : IInterviewerPrincipal, IPlainStorageEntity
    {
        public string Name { get;  set; }
        public string Password { get; set; }
        public Guid UserId { get; set; }
        public Guid SupervisorId { get; set; }
        public string Id { get; set; }
    }
}