using System;
using System.Collections.Generic;
using System.Linq;
using AppDomainToolkit;
using FluentAssertions;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Ncqrs.Spec;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Tests.Abc;

namespace WB.Tests.Integration.InterviewTests.OptionsFilter
{
    internal class when_answering_question_removes_rosters_and_answer_for_linked_single_question : InterviewTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            appDomainContext = AppDomainContext.Create();
            BecauseOf();
        }

        public void BecauseOf() =>
            results = Execute.InStandaloneAppDomain(appDomainContext.Domain, () =>
            {
                SetUp.MockedServiceLocator();

                var options = new List<Answer>
                {
                    Create.Entity.Option("1"),
                    Create.Entity.Option("2"),
                    Create.Entity.Option("3"),
                    Create.Entity.Option("12")
                };

                var questionnaireDocument = Create.Entity.QuestionnaireDocumentWithOneChapter(questionnaireId, new IComposite[]
                {
                    Create.Entity.NumericIntegerQuestion(q1Id, "q1"),
                    Create.Entity.MultyOptionsQuestion(q2Id, variable: "q2", options: options, optionsFilter: "@optioncode < q1"),
                    Create.Entity.Roster(rosterId, variable:"r1", rosterSizeQuestionId: q2Id, rosterSizeSourceType: RosterSizeSourceType.Question, children: new IComposite[]
                    {
                        Create.Entity.NumericIntegerQuestion(q3Id, "age")
                    }),
                    Create.Entity.SingleQuestion(q4Id, "q4", linkedToQuestionId: q3Id),
                    Create.Entity.TextQuestion(q5Id, variable: "q5",
                        enablementCondition: "q4.Length > 0")
                });

                var interview = SetupInterview(questionnaireDocument);

                interview.AnswerNumericIntegerQuestion(userId, q1Id, RosterVector.Empty, DateTime.Now, 10);
                interview.AnswerMultipleOptionsQuestion(userId, q2Id, RosterVector.Empty, DateTime.Now, new[] { 1, 2, 3 });
                interview.AnswerNumericIntegerQuestion(userId, q3Id, Create.Entity.RosterVector(new[] {1}), DateTime.Now, 20);
                interview.AnswerNumericIntegerQuestion(userId, q3Id, Create.Entity.RosterVector(new[] {2}), DateTime.Now, 15);
                interview.AnswerNumericIntegerQuestion(userId, q3Id, Create.Entity.RosterVector(new[] {3}), DateTime.Now, 35);
                interview.AnswerSingleOptionLinkedQuestion(userId, q4Id, RosterVector.Empty, DateTime.Now, new [] { 3m });
                var result = new InvokeResults();

                using (var eventContext = new EventContext())
                {
                    interview.AnswerNumericIntegerQuestion(userId, q1Id, RosterVector.Empty, DateTime.Now, 2);

                    result.QuestionsQ5Disabled = eventContext.AnyEvent<QuestionsDisabled>(x => x.Questions.Any(q => q.Id == q5Id));
                }

                return result;
            });

        [NUnit.Framework.Test] public void should_disable_q5 () =>
            results.QuestionsQ5Disabled.Should().BeTrue();

        [NUnit.Framework.OneTimeTearDown] public void CleanUp()
        {
            appDomainContext.Dispose();
            appDomainContext = null;
        }

        private static InvokeResults results;
        private static AppDomainContext<AssemblyTargetLoader, PathBasedAssemblyResolver> appDomainContext;
        private static readonly Guid questionnaireId = Guid.Parse("99999999999999999999999999999999");
        private static readonly Guid rosterId = Guid.Parse("88888888888888888888888888888888");
        private static readonly Guid q1Id = Guid.Parse("11111111111111111111111111111111");
        private static readonly Guid q2Id = Guid.Parse("22222222222222222222222222222222");
        private static readonly Guid q3Id = Guid.Parse("33333333333333333333333333333333");
        private static readonly Guid q4Id = Guid.Parse("44444444444444444444444444444444");
        private static readonly Guid q5Id = Guid.Parse("55555555555555555555555555555555");
        private static readonly Guid userId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");

        [Serializable]
        internal class InvokeResults
        {
            public bool QuestionsQ5Disabled { get; set; }
        }
    }
}
