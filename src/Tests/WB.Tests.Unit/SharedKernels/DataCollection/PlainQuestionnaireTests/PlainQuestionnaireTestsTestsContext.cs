using System;
using Main.Core.Documents;
using Main.Core.Entities;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Designer.Implementation.Factories;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;

namespace WB.Tests.Unit.SharedKernels.DataCollection.PlainQuestionnaireTests
{
    internal class PlainQuestionnaireTestsContext
    {
        protected static PlainQuestionnaire CreatePlainQuestionnaire(QuestionnaireDocument questionnaireDocument, long version = 1)
        {
            return new PlainQuestionnaire(questionnaireDocument, version);
        }

        protected static IQuestion CreateTextListQuestion(Guid questionId)
        {
            IQuestionnaireEntityFactory questionnaireEntityFactory = new QuestionnaireEntityFactory();

            return questionnaireEntityFactory.CreateQuestion(
                new QuestionData(
                    questionId,
                    QuestionType.TextList,
                    QuestionScope.Interviewer,
                    "",
                    "",
                    "",
                    "",
                    "",
                    "",
                    Order.AZ,
                    false,
                    false,
                    false,
                    "",
                    "",
                    null,
                    null,
                    null,
                    null,
                    false,
                    null,
                    null,
                    null,
                    null,
                    false,
                    null
            ));
        }

        protected static IQuestion CreateSingleOptionQuestion(Guid questionId, Answer[] answers)
        {
            IQuestionnaireEntityFactory questionnaireEntityFactory = new QuestionnaireEntityFactory();

            return questionnaireEntityFactory.CreateQuestion(
                new QuestionData(
                    questionId,
                    QuestionType.SingleOption,
                    QuestionScope.Interviewer,
                    "",
                    "",
                    "",
                    "",
                    "",
                    "",
                    Order.AZ,
                    false,
                    false,
                    false,
                    "",
                    "",
                    null,
                    null,
                    answers,
                    null,
                    false,
                    null,
                    null,
                    null,
                    null,
                    false,
                    null
            ));
        }
    }
}