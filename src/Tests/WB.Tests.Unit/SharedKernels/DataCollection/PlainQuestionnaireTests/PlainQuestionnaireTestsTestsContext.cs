using System;
using System.Collections.Generic;
using Main.Core.Documents;
using Main.Core.Entities;
using Main.Core.Entities.SubEntities;
using NUnit.Framework;
using WB.Core.BoundedContexts.Designer.Implementation.Factories;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.QuestionnaireEntities;

namespace WB.Tests.Unit.SharedKernels.DataCollection.PlainQuestionnaireTests
{
    [TestOf(typeof(PlainQuestionnaire))]
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
                    false,
                    Order.AZ,
                    false,
                    false,
                    "",
                    new QuestionProperties(false, false),
                    "",
                    null,
                    null,
                    null,
                    false,
                    null,
                    null,
                    null,
                    null,
                    false,
                    null,
                    null,
                new List<ValidationCondition>(),
                null,
                false
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
                    false,
                    Order.AZ,
                    false,
                    false,
                    "",
                    new QuestionProperties(false, false),
                    "",
                    answers,
                    null,
                    null,
                    false,
                    null,
                    null,
                    null,
                    null,
                    false,
                    null,
                    null,
                    new List<ValidationCondition>(),
                null,
                false
            ));
        }
    }
}