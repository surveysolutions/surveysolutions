using System;
using WB.Core.SharedKernels.Enumerator.Services;

namespace WB.Core.BoundedContexts.Tester.Implementation.Services;

public class DummyLastCompletionComments: ILastCompletionComments
{
    public DummyLastCompletionComments()
    {
    }

    public void Store(Guid interviewId, string comment)
    {
    }

    public string Get(Guid interviewId)
    {
        return null;
    }

    public void Remove(Guid interviewId)
    {
    }
}
