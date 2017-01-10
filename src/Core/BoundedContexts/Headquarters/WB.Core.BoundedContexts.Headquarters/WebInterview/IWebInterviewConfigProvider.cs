using WB.Core.SharedKernels.DataCollection.Implementation.Entities;

namespace WB.Core.BoundedContexts.Headquarters.WebInterview
{
    public interface IWebInterviewConfigProvider
    {
        WebInterviewConfig Get(QuestionnaireIdentity identity);
    }
}