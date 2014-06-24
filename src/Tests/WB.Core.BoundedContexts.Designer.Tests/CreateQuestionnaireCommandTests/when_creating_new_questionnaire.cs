using System;
using Machine.Specifications;
using Main.Core.Events.Questionnaire;
using Microsoft.Practices.ServiceLocation;
using Moq;
using Ncqrs;
using Ncqrs.Spec;
using WB.Core.BoundedContexts.Designer.Aggregates;
using It = Machine.Specifications.It;

namespace WB.Core.BoundedContexts.Designer.Tests.CreateQuestionnaireCommandTests
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
            currentDate = new DateTime(2010, 2, 3);

            NcqrsEnvironment.SetDefault(Mock.Of<IClock>(x => x.UtcNow() == currentDate));
        };

        Because of = () => questionnaire = new Questionnaire(questionnaireId, questionnaireTitle, createdBy, isPublic);

        It should_raise_questionnaire_created_event = () => 
            eventContext.ShouldContainEvent<NewQuestionnaireCreated>(e => e.PublicKey == questionnaireId && 
                e.Title == questionnaireTitle &&
                e.IsPublic == isPublic &&
                e.CreatedBy == createdBy &&
                e.CreationDate == currentDate);

        It should_raise_new_group_added_event = () => eventContext.ShouldContainEvent<NewGroupAdded>(e => e.ParentGroupPublicKey == null && e.GroupText == "New Chapter");


        static Questionnaire questionnaire;
        static Guid questionnaireId;
        static string questionnaireTitle;
        static EventContext eventContext;
        static Guid createdBy;
        static bool isPublic;
        private static DateTime currentDate;
    }
}

