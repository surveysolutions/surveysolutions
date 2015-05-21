using System;
using Machine.Specifications;
using Main.Core.Events.Questionnaire;
using Microsoft.Practices.ServiceLocation;
using Moq;
using Ncqrs;
using Ncqrs.Spec;
using WB.Core.BoundedContexts.Designer.Aggregates;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.BoundedContexts.Designer.CreateQuestionnaireCommandTests
{
    [Subject(typeof (Questionnaire))]
    public class when_creating_new_questionnaire
    {
        Establish context = () =>
        {
            questionnaireId = Guid.NewGuid();
            questionnaireTitle = "questionnaire title";
            createdBy = Guid.NewGuid();
            eventContext = new EventContext();
            isPublic = true;
        };

        Because of = () => questionnaire = new Questionnaire(questionnaireId, questionnaireTitle, createdBy, isPublic);

        It should_raise_questionnaire_created_event = () => 
            eventContext.ShouldContainEvent<NewQuestionnaireCreated>();

        It should_raise_questionnaire_created_event_with_given_id = () =>
            eventContext.ShouldContainEvent<NewQuestionnaireCreated>(e => e.PublicKey == questionnaireId);

        It should_raise_questionnaire_created_event_with_given_Title = () =>
            eventContext.ShouldContainEvent<NewQuestionnaireCreated>(e => e.Title == questionnaireTitle);

        It should_raise_questionnaire_created_event_with_given_IsPublic_flag = () =>
            eventContext.ShouldContainEvent<NewQuestionnaireCreated>(e => e.IsPublic == isPublic);

        It should_raise_questionnaire_created_event_with_given_CreatedBy = () =>
            eventContext.ShouldContainEvent<NewQuestionnaireCreated>(e => e.CreatedBy == createdBy);

        It should_raise_new_group_added_event = () => eventContext.ShouldContainEvent<NewGroupAdded>(e => e.ParentGroupPublicKey == null && e.GroupText == "New Section");


        static Questionnaire questionnaire;
        static Guid questionnaireId;
        static string questionnaireTitle;
        static EventContext eventContext;
        static Guid createdBy;
        static bool isPublic;
    }
}

