using System;

namespace WB.Core.SharedKernels.SurveyManagement.Commands
{
    public class DeleteQuestionnaire : QuestionnaireCommand
    {
        public DeleteQuestionnaire(Guid questionnaireId, long questionnaireVersion, Guid? responsibleId)
            : base(questionnaireId, questionnaireId)
        {
            this.ResponsibleId = responsibleId;
            this.QuestionnaireVersion = questionnaireVersion;
        }

        public long QuestionnaireVersion { get; set; }
        public Guid? ResponsibleId { get; set; }
    }
}
