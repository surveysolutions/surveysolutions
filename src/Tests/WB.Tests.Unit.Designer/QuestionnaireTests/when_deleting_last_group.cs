using System;
using FluentAssertions;
using NUnit.Framework;
using WB.Core.BoundedContexts.Designer.Aggregates;

namespace WB.Tests.Unit.Designer.BoundedContexts.QuestionnaireTests
{
    internal class when_deleting_last_remaining_group_in_questionnaire : QuestionnaireTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            responsible = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
            rootSectionId = Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");
            questionnaire = Create.Questionnaire(
                responsible,
                Create.QuestionnaireDocumentWithoutChildren(Guid.NewGuid(), children: Create.Group(groupId: rootSectionId, title: "last section")));
            BecauseOf();
        }

        private void BecauseOf() => exception = Assert.Throws<QuestionnaireException>(() => questionnaire.DeleteGroup(rootSectionId, responsible));

        [NUnit.Framework.Test] public void should_not_allow_delete_last_remaining_root_section () 
        {
            exception.Should().NotBeNull();
            exception.Message.Should().Be("Last existing section cannot be removed from questionnaire");
        }

        static Questionnaire questionnaire;
        static Guid responsible;
        static Guid rootSectionId;
        static QuestionnaireException exception;
    }
}
