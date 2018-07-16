using WB.Core.SharedKernels.DataCollection.Implementation.Entities;

namespace WB.Core.SharedKernels.Enumerator.OfflineSync.Messages
{
    public class GetQuestionnaireRequest : ICommunicationMessage
    {
        public QuestionnaireIdentity QuestionnaireId { get; set; }

        public GetQuestionnaireRequest(QuestionnaireIdentity questionnaireId)
        {
            QuestionnaireId = questionnaireId;
        }
    }

    public class GetQuestionnaireResponse : ICommunicationMessage
    {
        public string QuestionnaireDocument { get; set; }
    }
}
