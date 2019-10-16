using System;
using FluentAssertions;
using Main.Core.Documents;
using Moq;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.PlainStorage;


namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.QuestionnaireInfoFactoryTests
{
    internal class when_getting_numeric_roster_edit_view : QuestionnaireInfoFactoryTestContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            questionDetailsReaderMock = new Mock<IDesignerQuestionnaireStorage>();
            questionnaireView = CreateQuestionnaireDocument();
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

        [NUnit.Framework.Test] public void should_return_roster_with_RosterFixedTitles_count_equals_to_0 () =>
            result.FixedRosterTitles.Length.Should().Be(0);

        [NUnit.Framework.Test] public void should_return_roster_with_RosterSizeMultiQuestionId_equals_g3_RosterSizeQuestionId () =>
            result.RosterSizeMultiQuestionId.Should().Be(q2Id.FormatGuid());

        [NUnit.Framework.Test] public void should_return_roster_with_RosterSizeListQuestionId_equals_g3_RosterSizeQuestionId () =>
            result.RosterSizeListQuestionId.Should().BeNull();

        [NUnit.Framework.Test] public void should_return_roster_with_RosterSizeNumericQuestionId_equals_g3_RosterSizeQuestionId () =>
            result.RosterSizeNumericQuestionId.Should().BeNull();

        [NUnit.Framework.Test] public void should_return_roster_with_RosterSizeSourceType_equals_g3_RosterSizeSourceType () =>
            result.Type.Should().Be(RosterType.Multi);
        

        private static QuestionnaireInfoFactory factory;
        private static NewEditRosterView result;
        private static QuestionnaireDocument questionnaireView;
        private static Mock<IDesignerQuestionnaireStorage> questionDetailsReaderMock;
        private static Guid rosterId = g2Id;
    }
}
