using System;
using FluentAssertions;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Moq;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.ChangeHistory;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit;
using WB.Core.GenericSubdomains.Portable;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.QuestionnaireInfoFactoryTests
{
    internal class when_getting_roster_edit_view_referencing_non_existing_question : QuestionnaireInfoFactoryTestContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            questionDetailsReaderMock = new Mock<IDesignerQuestionnaireStorage>();
            questionnaireView = Create.QuestionnaireDocumentWithOneChapter(questionnaireId: docId, chapterId: g1Id, children: new IComposite[]
            {
                Create.Roster(rosterId:g2Id, title:"Roster 1.1", rosterSizeQuestionId: q2Id, rosterTitleQuestionId: q3Id, rosterType: RosterSizeSourceType.Question)
            });
            questionDetailsReaderMock
                .Setup(x => x.Get(questionnaireId))
                .Returns(questionnaireView);

            factory = CreateQuestionnaireInfoFactory(questionDetailsReaderMock.Object);
            BecauseOf();
        }

        private void BecauseOf() =>
            result = factory.GetRosterEditView(questionnaireId, rosterId);

        [NUnit.Framework.Test] public void should_return_not_null_view () =>
            result.Should().NotBeNull();

        [NUnit.Framework.Test] public void should_return_roster_with_ItemId_equals_groupId () =>
            result.ItemId.Should().Be(rosterId.FormatGuid());
        
        [NUnit.Framework.Test] public void should_return_roster_with_RosterSizeSourceType_equals_g3_RosterSizeSourceType () =>
            result.Type.Should().Be(RosterType.Numeric);
        

        private static QuestionnaireInfoFactory factory;
        private static NewEditRosterView result;
        private static QuestionnaireDocument questionnaireView;
        private static Mock<IDesignerQuestionnaireStorage> questionDetailsReaderMock;

        private static Guid rosterId = g2Id;
    }
}
