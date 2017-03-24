using System;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Tests.Abc;

namespace WB.Tests.Unit.SharedKernels.DataCollection.PlainQuestionnaireTests
{
    internal class when_checking_should_static_text_be_hidden_if_it_is_disabled : PlainQuestionnaireTestsContext
    {
        Establish context = () =>
        {
            QuestionnaireDocument questionnaireDocument = Create.Entity.QuestionnaireDocumentWithOneChapter(children: new IComposite[]
            {
                Create.Entity.StaticText(publicKey: staticTextId),
            });

            plainQuestionnaire = Create.Entity.PlainQuestionnaire(document: questionnaireDocument);
        };

        Because of = () =>
            result = plainQuestionnaire.ShouldBeHiddenIfDisabled(staticTextId);

        It should_return_false = () =>
            result.ShouldEqual(false);

        private static PlainQuestionnaire plainQuestionnaire;
        private static Guid staticTextId = Guid.Parse("11111111111111111111111111111111");
        private static bool result;
    }
}