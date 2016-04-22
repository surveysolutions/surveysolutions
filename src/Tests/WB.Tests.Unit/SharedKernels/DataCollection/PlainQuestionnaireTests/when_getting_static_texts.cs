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
            questionnaireDocument = Create.QuestionnaireDocumentWithOneChapter(children: new IComposite[]
            {
                Create.StaticText(publicKey: staticTextId),
                Create.Group(children: new[]
                {
                    Create.StaticText(publicKey: staticTextInSubgroupId)
                }),
                Create.Roster(children: new[]
                {
                    Create.StaticText(publicKey: staticTextInRosterId)
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
        private static readonly Guid staticTextId = Guid.Parse("111111111111111111111111111111");
        private static readonly Guid staticTextInSubgroupId = Guid.Parse("222222222222222222222222222222");
        private static readonly Guid staticTextInRosterId = Guid.Parse("333333333333333333333333333333");
        private static QuestionnaireDocument questionnaireDocument;
    }
}