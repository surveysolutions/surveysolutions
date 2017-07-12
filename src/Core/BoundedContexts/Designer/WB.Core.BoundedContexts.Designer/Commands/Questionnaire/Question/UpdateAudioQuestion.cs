using System;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Base;

namespace WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Question
{
    public class UpdateAudioQuestion : AbstractUpdateQuestionCommand
    {
        public UpdateAudioQuestion(Guid questionnaireId,
            Guid questionId,
            Guid responsibleId,
            CommonQuestionParameters commonQuestionParameters,
            QuestionScope scope,
            Quality quality)
            : base(
                responsibleId: responsibleId, questionnaireId: questionnaireId, questionId: questionId, 
                commonQuestionParameters: commonQuestionParameters)
        {
            this.Scope = scope;
            this.Quality = quality;
        }

        public Quality Quality { get; set; }
        public QuestionScope Scope { get; set; }
    }
}