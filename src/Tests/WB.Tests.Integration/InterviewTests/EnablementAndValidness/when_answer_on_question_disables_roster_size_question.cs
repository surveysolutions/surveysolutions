using System;
using System.Linq;
using AppDomainToolkit;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Ncqrs.Spec;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;

namespace WB.Tests.Integration.InterviewTests.EnablementAndValidness
{
    [Ignore("unignore when KP-6084 will be fixed")]
    internal class when_answer_on_question_disables_roster_size_question : InterviewTestsContext
    {
        Establish context = () =>
        {
            appDomainContext = AppDomainContext.Create();
            var questionnaireId = Guid.Parse("10000000000000000000000000000000");
            userId = Guid.Parse("FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF");
            rosterId = Guid.Parse("21111111111111111111111111111111");
            questionWhichIncreasesRosterSizeId = Guid.Parse("22222222222222222222222222222222");
            questionWhichDisablesRosterSizeQuestion = Guid.NewGuid();
            QuestionnaireDocument questionnaire = Create.QuestionnaireDocumentWithOneChapter(id: questionnaireId,
                children: new IComposite[]
                {
                    Create.NumericIntegerQuestion(id: questionWhichDisablesRosterSizeQuestion, variable: "num_disable"),
                    Create.NumericIntegerQuestion(id: questionWhichIncreasesRosterSizeId, variable: "num_trigger", enablementCondition:"!num_disable.HasValue"),
                    Create.Roster(id: rosterId, variable: "ros",
                        rosterSizeQuestionId: questionWhichIncreasesRosterSizeId,
                        rosterSizeSourceType: RosterSizeSourceType.Question)
                });
            interview = SetupInterview(questionnaireDocument: questionnaire);
           
        };

        Cleanup stuff = () =>
        {
            eventContext.Dispose();
            eventContext = null;
        };

        Because of = () =>
        {
            results = Execute.InStandaloneAppDomain(appDomainContext.Domain, () =>
            {
                interview.AnswerNumericIntegerQuestion(userId, questionWhichIncreasesRosterSizeId, new decimal[0],
                    DateTime.Now, 2);

                eventContext = new EventContext();
                interview.AnswerNumericIntegerQuestion(userId, questionWhichDisablesRosterSizeQuestion, new decimal[0],
                    DateTime.Now, 2);
                return true;
            });
        };

        It should_raise_RosterInstancesRemoved_event_for_first_roster_row = () =>
            eventContext.ShouldContainEvent<RosterInstancesRemoved>(@event
                => @event.Instances.Any(instance => instance.GroupId == rosterId && instance.RosterInstanceId == 0));

        It should_raise_RosterInstancesRemoved_event_for_second_roster_row = () =>
          eventContext.ShouldContainEvent<RosterInstancesRemoved>(@event
              => @event.Instances.Any(instance => instance.GroupId == rosterId && instance.RosterInstanceId == 1));

        private static EventContext eventContext;
        private static Interview interview;
        private static Guid userId;
        private static Guid questionWhichIncreasesRosterSizeId;
        private static Guid questionWhichDisablesRosterSizeQuestion;
        private static Guid rosterId;
        private static AppDomainContext<AssemblyTargetLoader, PathBasedAssemblyResolver> appDomainContext;  
        private static bool results;
    }
}