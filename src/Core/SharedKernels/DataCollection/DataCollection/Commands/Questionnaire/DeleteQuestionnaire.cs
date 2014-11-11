using System;
using Ncqrs.Commanding;

namespace WB.Core.SharedKernels.DataCollection.Commands.Questionnaire
{
    [Serializable]
    public class DeleteQuestionnaire : CommandBase
    {
        public DeleteQuestionnaire(Guid questionnaireId, long questionnaireVersion, Guid? responsibleId)
            : base(questionnaireId)
        {
            this.ResponsibleId = responsibleId;
            this.QuestionnaireId = questionnaireId;
            this.QuestionnaireVersion = questionnaireVersion;
        }

        public Guid QuestionnaireId { get; set; }
        public long QuestionnaireVersion { get; set; }
        public Guid? ResponsibleId { get; set; }
    }
}
