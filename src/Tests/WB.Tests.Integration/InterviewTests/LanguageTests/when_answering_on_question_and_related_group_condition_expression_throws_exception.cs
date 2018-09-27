using System;
using System.Collections.Generic;
using System.Linq;
using AppDomainToolkit;
using FluentAssertions;
using Ncqrs.Spec;
using NUnit.Framework;
using WB.Core.SharedKernels.DataCollection.Events.Interview;

namespace WB.Tests.Integration.InterviewTests.LanguageTests
{
    [Ignore("KP-4381")]
    internal class when_answering_on_question_and_related_group_condition_expression_throws_exception : InterviewTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            appDomainContext = AppDomainContext.Create();
            BecauseOf();
        }

        public void BecauseOf() =>
            results = Execute.InStandaloneAppDomain(appDomainContext.Domain, () =>
            {
                SetUp.MockedServiceLocator();

                var questionnaireId = Guid.Parse("00000000000000000000000000000000");
                var actorId = Guid.Parse("99999999999999999999999999999999");
                var questionId = Guid.Parse("11111111111111111111111111111111");
                var groupId = Guid.Parse("22222222222222222222222222222222");
                
                var interview = SetupInterview(
                    Abc.Create.Entity.QuestionnaireDocumentWithOneChapter(questionnaireId,
                        Abc.Create.Entity.NumericIntegerQuestion(questionId, "q1"),
                        Abc.Create.Entity.Group(groupId, "g1", null, "1/q1 == 1", false, null)
                    ),
                    events: new List<object>
                    {
                        Abc.Create.Event.NumericIntegerQuestionAnswered(
                            questionId, null, 1, null, null
                        ),
                        Abc.Create.Event.QuestionsEnabled(Abc.Create.Identity(groupId))
                    });

                var result = new InvokeResults();

                using (var eventContext = new EventContext())
                {
                    interview.AnswerNumericIntegerQuestion(actorId, questionId, new decimal[0], DateTime.Now, 0);

                    result.GroupDisabledEventWasFound = eventContext.AnyEvent<GroupsDisabled>(x => x.Groups.Any(g => g.Id == groupId));
                    result.GroupEnabledEventWasFound = eventContext.AnyEvent<GroupsEnabled>(x => x.Groups.Any(g => g.Id == groupId));
                }

                return result;
            });

        [NUnit.Framework.Test] public void should_not_enable_groupId () =>
            results.GroupEnabledEventWasFound.Should().BeTrue();

        [NUnit.Framework.Test] public void should_disable_groupId_because_of_calculation_error () =>
            results.GroupDisabledEventWasFound.Should().BeFalse();

        [NUnit.Framework.OneTimeTearDown] public void CleanUp()
        {
            appDomainContext.Dispose();
            appDomainContext = null;
        }

        private static InvokeResults results;
        private static AppDomainContext<AssemblyTargetLoader, PathBasedAssemblyResolver> appDomainContext;

        [Serializable]
        internal class InvokeResults
        {
            public bool GroupEnabledEventWasFound { get; set; }
            public bool GroupDisabledEventWasFound { get; set; }
        } 
    }
}
