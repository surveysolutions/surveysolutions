using System;
using FluentAssertions;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Tests.Abc;

namespace WB.Tests.Unit.SharedKernels.DataCollection.PlainQuestionnaireTests
{
    internal class when_checking_should_static_text_be_hidden_if_it_is_disabled : PlainQuestionnaireTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            QuestionnaireDocument questionnaireDocument = Create.Entity.QuestionnaireDocumentWithOneChapter(children: new IComposite[]
            {
                Create.Entity.StaticText(publicKey: staticTextId),
            });

            plainQuestionnaire = Create.Entity.PlainQuestionnaire(document: questionnaireDocument);
        }

        public void BecauseOf() =>
            result = plainQuestionnaire.ShouldBeHiddenIfDisabled(staticTextId);

        [NUnit.Framework.Test] public void should_return_false () =>
            result.Should().Be(false);

        private static PlainQuestionnaire plainQuestionnaire;
        private static Guid staticTextId = Guid.Parse("11111111111111111111111111111111");
        private static bool result;
    }
}