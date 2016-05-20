using System;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;

namespace WB.Tests.Unit.SharedKernels.DataCollection.PlainQuestionnaireTests
{
    internal class when_getting_static_texts : PlainQuestionnaireTestsContext
    {
        Establish context = () =>
        {
            questionnaireDocument = Create.Other.QuestionnaireDocumentWithOneChapter(children: new IComposite[]
            {
                Create.Other.StaticText(publicKey: staticTextId),
                Create.Other.Group(children: new[]
                {
                    Create.Other.StaticText(publicKey: staticTextInSubgroupId)
                }),
                Create.Other.Roster(children: new[]
                {
                    Create.Other.StaticText(publicKey: staticTextInRosterId)
                })
            });
        };

        Because of = () =>
            plainQuestionnaire = new PlainQuestionnaire(questionnaireDocument, 0);

        It should_return_specified_static_texts = () =>
        {
            plainQuestionnaire.GetAllStaticTexts().Count.ShouldEqual(3);
            plainQuestionnaire.GetAllStaticTexts().ShouldEachConformTo(
                    sttid => sttid == staticTextId || sttid == staticTextInSubgroupId || sttid == staticTextInRosterId);
        };

        private static PlainQuestionnaire plainQuestionnaire;
        private static readonly Guid staticTextId = Guid.NewGuid();
        private static readonly Guid staticTextInSubgroupId = Guid.NewGuid();
        private static readonly Guid staticTextInRosterId = Guid.NewGuid();
        private static QuestionnaireDocument questionnaireDocument;
    }
}