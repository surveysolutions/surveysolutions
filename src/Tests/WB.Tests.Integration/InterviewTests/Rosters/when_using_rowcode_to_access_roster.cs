using System;
using System.Collections.Generic;
using System.Linq;
using AppDomainToolkit;
using FluentAssertions;
using Main.Core.Entities.SubEntities;
using Ncqrs.Spec;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Tests.Abc;

namespace WB.Tests.Integration.InterviewTests.Rosters
{
    internal class when_using_rowcode_to_access_roster : InterviewTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            appDomainContext = AppDomainContext.Create();
            BecauseOf();
        }

        [NUnit.Framework.OneTimeTearDown] public void CleanUp()
        {
            appDomainContext.Dispose();
            appDomainContext = null;
        }

        protected static AppDomainContext<AssemblyTargetLoader, PathBasedAssemblyResolver> appDomainContext;

        public void BecauseOf() =>
            results = Execute.InStandaloneAppDomain(appDomainContext.Domain, () =>
            {
                SetUp.MockedServiceLocator();

                var options = new List<Answer>{ Create.Entity.Option(20), Create.Entity.Option(30) };

                var questionnaireDocument = Create.Entity.QuestionnaireDocumentWithOneChapter(questionnaireId,
                    Create.Entity.MultyOptionsQuestion(rosterSizeQuestionId, variable: "multi", yesNoView: true, options: options),
                    Create.Entity.MultiRoster(rosterId: rosterId, rosterSizeQuestionId: rosterSizeQuestionId, variable: "r1"),
                    Create.Entity.TextQuestion(questionId, enablementCondition: "r1[30]!=null")
                );

                var result = new InvokeResults();

                var interview = SetupInterview(questionnaireDocument);

                using (var eventContext = new EventContext())
                {
                    interview.AnswerYesNoQuestion(Create.Command.AnswerYesNoQuestion(questionId: rosterSizeQuestionId, answeredOptions: new[] { Yes(30) }));

                    result.WasTextQuestionEnabled = eventContext.GetSingleEvent<QuestionsEnabled>().Questions.Any(x => x.Id == questionId);
                }

                return result;
            });

        [NUnit.Framework.Test] public void should_enable_text_question () =>
            results.WasTextQuestionEnabled.Should().BeTrue();

        private static InvokeResults results;

        private static readonly Guid questionnaireId = Guid.Parse("77778888000000000000000000000000");
        private static readonly Guid rosterSizeQuestionId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
        private static readonly Guid questionId = Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");
        private static readonly Guid rosterId = Guid.Parse("11111111111111111111111111111111");

        [Serializable]
        internal class InvokeResults
        {
            public bool WasTextQuestionEnabled;
        }
    }
}
