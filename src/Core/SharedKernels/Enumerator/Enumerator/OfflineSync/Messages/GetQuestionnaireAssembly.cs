using WB.Core.SharedKernels.DataCollection.Implementation.Entities;

namespace WB.Core.SharedKernels.Enumerator.OfflineSync.Messages
{
    public class GetQuestionnaireAssemblyRequest : ICommunicationMessage
    {
        public QuestionnaireIdentity QuestionnaireId { get; set; }

        public GetQuestionnaireAssemblyRequest(QuestionnaireIdentity questionnaireId)
        {
            QuestionnaireId = questionnaireId;
        }
    }

    public class GetQuestionnaireAssemblyResponse : ICommunicationMessage
    {
        public byte[] Content { get; set; }
    }
}
