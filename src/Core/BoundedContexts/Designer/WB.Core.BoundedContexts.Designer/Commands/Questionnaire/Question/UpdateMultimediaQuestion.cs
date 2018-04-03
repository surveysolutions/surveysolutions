using System;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Base;

namespace WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Question
{
    [Serializable]
    public class UpdateMultimediaQuestion: AbstractUpdateQuestionCommand
    {
        public UpdateMultimediaQuestion(Guid questionnaireId, 
            Guid questionId, 
            Guid responsibleId,
            CommonQuestionParameters commonQuestionParameters,
            QuestionScope scope)
            : base(
                responsibleId: responsibleId, questionnaireId: questionnaireId, questionId: questionId, 
                commonQuestionParameters: commonQuestionParameters)
        {
            this.Scope = scope;
        }

        public QuestionScope Scope { get; set; }
        public bool IsSignature { get; set; }
    }
}
