using System;
using System.Linq;
using FluentAssertions;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Designer.Implementation.Services;
using WB.Core.BoundedContexts.Designer.ValueObjects;
using WB.UI.Designer.Code;
using WB.UI.Designer.Models;

namespace WB.Tests.Unit.Designer.Applications.VerificationErrorsMapperTests
{
    internal class when_enriching_errors_with_titles_from_questionnaire : VerificationErrorsMapperTestContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            mapper = CreateVerificationErrorsMapper();
            verificationMessages = CreateQuestionnaireVerificationErrors(Guid.Parse(questionId), Guid.Parse(groupId), Guid.Parse(rosterId));

            document = Create.QuestionnaireDocument(children: new IComposite[]
            {
                Create.Group(Guid.Parse(groupId), title: groupTitle, children: new []
                {
                    Create.TextQuestion(Guid.Parse(questionId), text: questionTitle)
                }),
                Create.Roster(Guid.Parse(rosterId), title: rosterTitle, rosterType: RosterSizeSourceType.FixedTitles, fixedRosterTitles: new []
                {
                    Create.FixedRosterTitle(1, "Hello"), Create.FixedRosterTitle(2, "World")
                })
            });
            BecauseOf();
        }

        private void BecauseOf() =>
            result = mapper.EnrichVerificationErrors(verificationMessages, document.AsReadOnly());

        [NUnit.Framework.Test] public void should_return_2_errors () => 
            result.Length.Should().Be(3);

        [NUnit.Framework.Test] public void should_return_first_error_with_same_Code_as_input_error_has () =>
            result.ElementAt(0).Code.Should().Be(verificationMessages.ElementAt(0).Code);

        [NUnit.Framework.Test] public void should_return_first_error_with_same_Message_as_input_error_has () =>
            result.ElementAt(0).Message.Should().Be(verificationMessages.ElementAt(0).Message);

        [NUnit.Framework.Test] public void should_return_first_error_with_same_References_count_as_input_error_has () =>
            result.ElementAt(0).Errors.SelectMany(e => e.References).Count().Should().Be(2);

        [NUnit.Framework.Test] public void should_return_first_error_that_references_question_with_questionId () =>
            result.ElementAt(0).Errors.First().References.ElementAt(0).ItemId.Should().Be(questionId);

        [NUnit.Framework.Test] public void should_return_first_error_that_references_group_with_groupId () =>
            result.ElementAt(0).Errors.Second().References.ElementAt(0).ItemId.Should().Be(groupId);

        [NUnit.Framework.Test] public void should_return_first_error_that_references_question_with_questionTitle () =>
            result.ElementAt(0).Errors.First().References.ElementAt(0).Title.Should().Be(questionTitle);

        [NUnit.Framework.Test] public void should_return_first_error_with_IsGroupOfErrors_field_set_in_true () =>
            result.ElementAt(0).IsGroupedMessage.Should().BeTrue();

        [NUnit.Framework.Test] public void should_return_last_error_with_same_Code_as_input_error_has () =>
            result.ElementAt(1).Code.Should().Be(verificationMessages.ElementAt(1).Code);

        [NUnit.Framework.Test] public void should_return_last_error_with_same_Message_as_input_error_has () =>
            result.ElementAt(1).Message.Should().Be(verificationMessages.ElementAt(1).Message);

        [NUnit.Framework.Test] public void should_return_last_error_with_same_References_count_as_input_error_has () =>
            result.ElementAt(1).Errors.First().References.Count.Should().Be(verificationMessages.ElementAt(1).References.Count());

        [NUnit.Framework.Test] public void should_return_last_error_that_references_question_with_questionId () =>
            result.ElementAt(1).Errors.First().References.ElementAt(0).ItemId.Should().Be(groupId);

        [NUnit.Framework.Test] public void should_return_last_error_that_references_question_with_groupTitle () =>
            result.ElementAt(1).Errors.First().References.ElementAt(0).Title.Should().Be(groupTitle);

        [NUnit.Framework.Test] public void should_return_last_error_with_IsGroupOfErrors_field_set_in_true () =>
            result.ElementAt(1).IsGroupedMessage.Should().BeTrue();

        [NUnit.Framework.Test] public void should_return_third_error_with_same_Code_as_input_error_has () =>
            result.ElementAt(2).Code.Should().Be(verificationMessages.ElementAt(2).Code);

        [NUnit.Framework.Test] public void should_return_third_error_with_same_Message_as_input_error_has () =>
            result.ElementAt(2).Message.Should().Be(verificationMessages.ElementAt(2).Message);

        [NUnit.Framework.Test] public void should_return_third_error_with_same_References_count_as_input_error_has () =>
            result.ElementAt(2).Errors.First().References.Count.Should().Be(3);

        [NUnit.Framework.Test] public void should_return_third_error_that_references_question_group_and_roster () 
        {
            var references = result.ElementAt(2).Errors.First().References;
            references.ElementAt(0).ItemId.Should().Be(questionId);
            references.ElementAt(0).Title.Should().Be(questionTitle);

            references.ElementAt(1).ItemId.Should().Be(groupId);
            references.ElementAt(1).Title.Should().Be(groupTitle);

            references.ElementAt(2).ItemId.Should().Be(rosterId);
            references.ElementAt(2).Title.Should().Be(rosterTitle);

        }

        [NUnit.Framework.Test] public void should_return_third_error_with_IsGroupOfErrors_field_set_in_true () =>
            result.ElementAt(2).IsGroupedMessage.Should().BeFalse();


        private static IVerificationErrorsMapper mapper;
        private static QuestionnaireVerificationMessage[] verificationMessages;
        private static QuestionnaireDocument document;
        private static VerificationMessage[] result;
        private static string questionId = "11111111111111111111111111111111";
        private static string groupId = "22222222222222222222222222222222";
        private static string rosterId = "33333333333333333333333333333333";
        private static string groupTitle = "Group Title";
        private static string questionTitle = "Question Title";
        private static readonly string rosterTitle = "Roster";
    }
}
