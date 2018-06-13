using System;
using System.Linq;
using FluentAssertions;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Designer.Aggregates;


namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.CreateQuestionnaireCommandTests
{
    internal class when_creating_new_questionnaire
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            questionnaireId = Guid.NewGuid();
            questionnaireTitle = "questionnaire title";
            createdBy = Guid.NewGuid();
            isPublic = true;
            questionnaire = Create.Questionnaire();
            BecauseOf();
        }

        private void BecauseOf() => questionnaire.CreateQuestionnaire(Create.Command.CreateQuestionnaire(questionnaireId, questionnaireTitle, createdBy, isPublic));

        [NUnit.Framework.Test] public void should_contains_questionnaire_with_given_id () =>
            questionnaire.QuestionnaireDocument.PublicKey.Should().Be(questionnaireId);

        [NUnit.Framework.Test] public void should_contains_questionnaire_with_given_Title () =>
            questionnaire.QuestionnaireDocument.Title.Should().Be(questionnaireTitle);

        [NUnit.Framework.Test] public void should_contains_questionnaire_with_given_IsPublic_flag () =>
            questionnaire.QuestionnaireDocument.IsPublic.Should().Be(isPublic);

        [NUnit.Framework.Test] public void should_contains_questionnaire_with_given_CreatedBy () =>
            questionnaire.QuestionnaireDocument.CreatedBy.Should().Be(createdBy);

        [NUnit.Framework.Test] public void should_raise_new_group_added_event () 
        {
            var group = (IGroup)questionnaire.QuestionnaireDocument.Children.Single();
            group.GetParent().Should().Be(questionnaire.QuestionnaireDocument);
            group.Title.Should().Be("New Section");
        }


        static Questionnaire questionnaire;
        static Guid questionnaireId;
        static string questionnaireTitle;
        static Guid createdBy;
        static bool isPublic;
    }
}

