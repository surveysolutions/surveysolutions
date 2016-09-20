using System;
using System.Linq;
using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using Main.Core.Events.Questionnaire;
using WB.Core.BoundedContexts.Designer.Aggregates;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.CreateQuestionnaireCommandTests
{
    [Subject(typeof (Questionnaire))]
    internal class when_creating_new_questionnaire
    {
        Establish context = () =>
        {
            questionnaireId = Guid.NewGuid();
            questionnaireTitle = "questionnaire title";
            createdBy = Guid.NewGuid();
            isPublic = true;
            questionnaire = Create.Questionnaire();
        };

        Because of = () => questionnaire.CreateQuestionnaire(questionnaireId, questionnaireTitle, createdBy, isPublic);

        It should_contains_questionnaire_with_given_id = () =>
            questionnaire.QuestionnaireDocument.PublicKey.ShouldEqual(questionnaireId);

        It should_contains_questionnaire_with_given_Title = () =>
            questionnaire.QuestionnaireDocument.Title.ShouldEqual(questionnaireTitle);

        It should_contains_questionnaire_with_given_IsPublic_flag = () =>
            questionnaire.QuestionnaireDocument.IsPublic.ShouldEqual(isPublic);

        It should_contains_questionnaire_with_given_CreatedBy = () =>
            questionnaire.QuestionnaireDocument.CreatedBy.ShouldEqual(createdBy);

        It should_raise_new_group_added_event = () =>
        {
            var group = (IGroup)questionnaire.QuestionnaireDocument.Children.Single();
            group.GetParent().ShouldEqual(questionnaire.QuestionnaireDocument);
            group.Title.ShouldEqual("New Section");
        };


        static Questionnaire questionnaire;
        static Guid questionnaireId;
        static string questionnaireTitle;
        static Guid createdBy;
        static bool isPublic;
    }
}

