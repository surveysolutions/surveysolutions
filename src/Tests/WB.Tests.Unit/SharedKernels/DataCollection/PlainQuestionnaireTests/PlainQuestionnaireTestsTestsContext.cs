using System;
using Main.Core.Entities;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Designer.Implementation.Factories;

namespace WB.Tests.Unit.SharedKernels.DataCollection.PlainQuestionnaireTests
{
    internal class PlainQuestionnaireTestsContext
    {
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