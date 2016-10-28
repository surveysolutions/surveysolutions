using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Machine.Specifications;
using Microsoft.Practices.ServiceLocation;
using Moq;
using Ncqrs.Spec;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities.Answers;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Implementation.Repositories;
using WB.Core.SharedKernels.DataCollection.Repositories;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.DataCollection.InterviewTests
{
    internal class when_creating_interview : InterviewTestsContext
    {
        Establish context = () =>
        {
            eventContext = new EventContext();

            questionnaireId = Guid.Parse("10000000000000000000000000000000");
            userId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
            responsibleSupervisorId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAA00");
            questionnaireVersion = 18;
            answersToFeaturedQuestions = new Dictionary<Guid, AbstractAnswer>();
            
            var questionnaireRepository = Setup.QuestionnaireRepositoryWithOneQuestionnaire(questionnaireId, _
                => _.Version == questionnaireVersion);

            interview = Create.AggregateRoot.Interview(questionnaireRepository: questionnaireRepository);
        };

        Because of = () =>
            interview.CreateInterview(questionnaireId, questionnaireVersion, responsibleSupervisorId, answersToFeaturedQuestions, DateTime.Now, userId);

        It should_raise_InterviewCreated_event = () =>
            eventContext.ShouldContainEvent<InterviewCreated>();

        It should_provide_questionnaire_id_in_InterviewCreated_event = () =>
            eventContext.GetEvent<InterviewCreated>()
                .QuestionnaireId.ShouldEqual(questionnaireId);

        It should_provide_questionnaire_verstion_in_InterviewCreated_event = () =>
            eventContext.GetEvent<InterviewCreated>()
                .QuestionnaireVersion.ShouldEqual(questionnaireVersion);

        Cleanup stuff = () =>
        {
            eventContext.Dispose();
            eventContext = null;
        };

        private static EventContext eventContext;
        private static Guid questionnaireId;
        private static long questionnaireVersion;
        private static Guid userId;
        private static Guid responsibleSupervisorId;
        private static Dictionary<Guid, AbstractAnswer> answersToFeaturedQuestions;
        private static Interview interview;
    }
}