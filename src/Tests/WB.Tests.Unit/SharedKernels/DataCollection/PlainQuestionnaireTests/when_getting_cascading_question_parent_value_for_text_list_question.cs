using System;
using FluentAssertions;
using Main.Core.Entities.Composite;
using NUnit.Framework;
using WB.Core.SharedKernels.DataCollection.Exceptions;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Tests.Abc;

namespace WB.Tests.Unit.SharedKernels.DataCollection.PlainQuestionnaireTests
{
    internal class when_getting_cascading_question_parent_value_for_text_list_question : PlainQuestionnaireTestsContext
    {
        [Test] public void should_throw_exception_with_message_containing__type____not____support () {
            var questionnaireDocument = Create.Entity.QuestionnaireDocumentWithOneChapter(children: new IComposite[]
            {
                Create.Entity.TextListQuestion(questionId)
            });

            plainQuestionnaire = Create.Entity.PlainQuestionnaire(questionnaireDocument, 0);

            var exception =
                Assert.Throws<QuestionnaireException>(() => plainQuestionnaire.GetCascadingParentValue(questionId, 1m));
            exception.Message.ToLower().ToSeparateWords().Should().Contain("type", "not", "support");
        }

        private static PlainQuestionnaire plainQuestionnaire;
        private static readonly Guid questionId = Guid.Parse("00000000000000000000000000000000");
    }
}
