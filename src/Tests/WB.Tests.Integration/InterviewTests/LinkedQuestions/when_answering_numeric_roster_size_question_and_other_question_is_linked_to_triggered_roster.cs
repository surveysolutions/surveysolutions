using System;
using AppDomainToolkit;
using FluentAssertions;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Ncqrs.Spec;
using WB.Core.SharedKernels.DataCollection;

namespace WB.Tests.Integration.InterviewTests.LinkedQuestions
{
    internal class when_answering_numeric_roster_size_question_and_other_question_is_linked_to_triggered_roster : InterviewTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            appDomainContext = AppDomainContext.Create();
            BecauseOf();
        }

        public void BecauseOf() =>
            results = Execute.InStandaloneAppDomain(appDomainContext.Domain, () =>
            {
                SetUp.MockedServiceLocator();

                var questionnaireDocument = Abc.Create.Entity.QuestionnaireDocumentWithOneChapter(questionnaireId, children: new IComposite[]
                {
                    Abc.Create.Entity.NumericIntegerQuestion(q1Id, variable: "q1"),
                    Abc.Create.Entity.Roster(rosterId, variable:"r1", rosterSizeQuestionId: q1Id, rosterTitleQuestionId: q2Id, rosterSizeSourceType: RosterSizeSourceType.Question, children: new IComposite[]
                    {
                        Abc.Create.Entity.TextQuestion(questionId: q2Id, variable: "q2"),
                    }),
                    Abc.Create.Entity.SingleQuestion(q3Id, variable: "q3", linkedToRosterId: rosterId)
                });

                var interview = SetupStatefullInterview(questionnaireDocument);

                var result = new InvokeResults();

                using (var eventContext = new EventContext())
                {
                    interview.AnswerNumericIntegerQuestion(userId, q1Id, RosterVector.Empty, DateTime.Now, 3);
                    
                    result.NoLinkedQuestionOptionsChangesEventShoulBeRised = !eventContext.AnyEvent<LinkedQuestionOptionsChanges>();
                }

                return result;
            });

        [NUnit.Framework.Test] public void should_not_any_options_for_linked_question () =>
            results.NoLinkedQuestionOptionsChangesEventShoulBeRised.Should().BeTrue();

        [NUnit.Framework.OneTimeTearDown] public void CleanUp()
        {
            appDomainContext.Dispose();
            appDomainContext = null;
        }

        private static InvokeResults results;
        private static AppDomainContext<AssemblyTargetLoader, PathBasedAssemblyResolver> appDomainContext;
        private static readonly Guid questionnaireId = Guid.Parse("99999999999999999999999999999999");
        private static readonly Guid rosterId = Guid.Parse("88888888888888888888888888888888");
        private static readonly Guid q1Id = Guid.Parse("22222222222222222222222222222222");
        private static readonly Guid q2Id = Guid.Parse("33333333333333333333333333333333");
        private static readonly Guid q3Id = Guid.Parse("44444444444444444444444444444444");
        private static readonly Guid userId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");

        [Serializable]
        internal class InvokeResults
        {
            public bool NoLinkedQuestionOptionsChangesEventShoulBeRised { get; set; }
        }
    }
}
