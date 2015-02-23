using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WB.Core.SharedKernels.DataCollection.Commands.Questionnaire
{
    public class PrepareQuestionnaireForDelete : QuestionnaireCommand
    {
        public PrepareQuestionnaireForDelete(Guid questionnaireId, long questionnaireVersion, Guid? responsibleId)
            : base(questionnaireId, questionnaireId)
        {
            this.ResponsibleId = responsibleId;
            this.QuestionnaireVersion = questionnaireVersion;
        }

        public long QuestionnaireVersion { get; set; }
        public Guid? ResponsibleId { get; set; }
    }
}
