using System;
using System.Linq;
using FluentAssertions;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using NUnit.Framework;
using WB.Core.SharedKernels.DataCollection.Exceptions;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Tests.Abc;

namespace WB.Tests.Unit.SharedKernels.DataCollection.PlainQuestionnaireTests
{
    internal class when_getting_cascading_question_parent_value_which_is_not_decimal : PlainQuestionnaireTestsContext
    {
        [NUnit.Framework.Test] public void should_throw_exception_type_of_QuestionnaireException () {
            var questionnaireDocument = Create.Entity.QuestionnaireDocumentWithOneChapter(children: new IComposite[]
            {
                Create.Entity.SingleQuestion(id: questionId,
                        options: new[] { new Answer { AnswerValue = "1", ParentValue = "A" },
                                         new Answer { AnswerValue = "2", ParentValue = "2" } }.ToList()),
                });
            

            plainQuestionnaire = Create.Entity.PlainQuestionnaire(questionnaireDocument, 0);

            var exception = Assert.Throws<QuestionnaireException>(() => plainQuestionnaire.GetCascadingParentValue(questionId, 1m));

            exception.Message.ToLower().ToSeparateWords().Should().Contain("no", "parent");

        }

        private static PlainQuestionnaire plainQuestionnaire;
        private static readonly Guid questionId = Guid.Parse("00000000000000000000000000000000");
        private static Exception exception;
    }
}
