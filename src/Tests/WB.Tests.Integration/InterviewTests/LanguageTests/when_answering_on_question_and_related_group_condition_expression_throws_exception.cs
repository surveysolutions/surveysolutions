using System;
using System.Collections.Generic;
using System.Linq;
using AppDomainToolkit;
using Machine.Specifications;
using Ncqrs.Spec;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Events.Interview.Dtos;

namespace WB.Tests.Integration.InterviewTests.LanguageTests
{
    internal class when_answering_on_question_and_related_group_condition_expression_throws_exception : InterviewTestsContext
    {
        Establish context = () =>
        {
            appDomainContext = AppDomainContext.Create();
        };

        Because of = () =>
            results = Execute.InStandaloneAppDomain(appDomainContext.Domain, () =>
            {
                var questionnaireId = Guid.Parse("00000000000000000000000000000000");
                var actorId = Guid.Parse("99999999999999999999999999999999");
                var questionId = Guid.Parse("11111111111111111111111111111111");
                var groupId = Guid.Parse("22222222222222222222222222222222");

                Setup.SetupMockedServiceLocator();

                var questionnaireDocument = Create.QuestionnaireDocument(questionnaireId,
                    Create.NumericIntegerQuestion(questionId, "q1"),
                    Create.Group(groupId, "g1", "1/q1 == 1")
               );

                var interview = SetupInterview(questionnaireDocument, new List<object>()
                {
                    new NumericIntegerQuestionAnswered(actorId, questionId, new decimal[0], DateTime.Now, 1),
                    new GroupsEnabled(new[]{ new Identity(groupId, new decimal[0])})
                });

                var result = new InvokeResults();

                using (var eventContext = new EventContext())
                {
                    interview.AnswerNumericIntegerQuestion(actorId, questionId, new decimal[0], DateTime.Now, 0);

                    result.GroupDisabledEventWasFound  =
                        eventContext.Events.Any(b => b.Payload is GroupsDisabled) &&
                        GetFirstEventByType<GroupsDisabled>(eventContext.Events).Groups.Any(g => g.Id == groupId);

                    result.GroupEnabledEventWasFound =
                        eventContext.Events.Any(b => b.Payload is GroupsEnabled) &&
                        GetFirstEventByType<GroupsEnabled>(eventContext.Events).Groups.Any(g => g.Id == groupId);
                }

                return result;
            });

        It should_not_enable_groupId = () =>
            results.GroupEnabledEventWasFound.ShouldBeTrue();

        It should_disable_groupId_because_of_calculation_error = () =>
            results.GroupDisabledEventWasFound.ShouldBeFalse();

        Cleanup stuff = () =>
        {
            appDomainContext.Dispose();
            appDomainContext = null;
        };

        private static InvokeResults results;
        private static AppDomainContext appDomainContext;

        [Serializable]
        internal class InvokeResults
        {
            public bool GroupEnabledEventWasFound { get; set; }
            public bool GroupDisabledEventWasFound { get; set; }
        } 
    }
}