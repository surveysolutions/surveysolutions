using System;
using Ncqrs.Eventing.Storage;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;

namespace WB.Core.BoundedContexts.Tester.Services
{
    public interface ITesterEventStore : IEventStore
    {
        QuestionnaireIdentity RetireInterview(Guid interviewId);
    }
}
