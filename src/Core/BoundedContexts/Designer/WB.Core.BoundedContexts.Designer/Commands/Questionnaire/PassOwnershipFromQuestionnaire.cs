using System;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Base;

namespace WB.Core.BoundedContexts.Designer.Commands.Questionnaire
{
    [Serializable]
    public class PassOwnershipFromQuestionnaire : QuestionnaireCommand
    {
        public PassOwnershipFromQuestionnaire(
            Guid questionnaireId, 
            Guid ownerId, Guid newOwnerId,
            string ownerEmail, string newOwnerEmail)
            : base(questionnaireId, ownerId)
        {
            NewOwnerId = newOwnerId;
            OwnerEmail = ownerEmail;
            NewOwnerEmail = newOwnerEmail;
        }

        public Guid NewOwnerId { get; }
        public string OwnerEmail { get; }
        public string NewOwnerEmail { get; }
    }
}
