using System;
using WB.Core.SharedKernels.Enumerator.Models.Questionnaire;
using WB.Core.SharedKernels.Enumerator.Models.Questionnaire.Questions;

namespace WB.Tests.Unit.BoundedContexts.QuestionnaireTester.ViewModels.SingleOptionLinkedQuestionViewModelTests
{
    public class SingleOptionLinkedQuestionViewModelTestsContext
    {
        protected static QuestionnaireModel SetupQuestionnaireModelWithSingleOptionQuestionLinkedToTextQuestion(Guid questionId, Guid linkedToQuestionId)
        {
            return Create.QuestionnaireModel(questions: new BaseQuestionModel[]
            {
                new LinkedSingleOptionQuestionModel { Id = questionId, LinkedToQuestionId = linkedToQuestionId },
                new TextQuestionModel { Id = linkedToQuestionId },
            });
        }
    }
}