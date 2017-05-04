using WB.Core.SharedKernels.DataCollection.Implementation.Entities;

namespace WB.Core.BoundedContexts.Headquarters.WebInterview
{
    public interface ISampleWebInterviewService
    {
        byte[] Generate(QuestionnaireIdentity questionnaire, string baseUrl);
    }
}