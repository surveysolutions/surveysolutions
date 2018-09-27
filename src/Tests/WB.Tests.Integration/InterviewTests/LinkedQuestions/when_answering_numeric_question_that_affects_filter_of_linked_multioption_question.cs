using System;
using System.Linq;
using AppDomainToolkit;
using FluentAssertions;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Ncqrs.Spec;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Events.Interview;

namespace WB.Tests.Integration.InterviewTests.LinkedQuestions
{
    internal class when_answering_numeric_question_that_affects_filter_of_linked_multioption_question : InterviewTestsContext
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
                    Abc.Create.Entity.Roster(rosterId, variable:"r1", rosterSizeQuestionId: q1Id, rosterTitleQuestionId: q2Id, 
                        rosterSizeSourceType: RosterSizeSourceType.Question, children: new IComposite[]
                    {
                        Abc.Create.Entity.TextQuestion(questionId: q2Id, variable: "q2"),
                    }),
                    Abc.Create.Entity.NumericIntegerQuestion(q4Id, variable: "q4"),

                    Abc.Create.Entity.MultyOptionsQuestion(q3Id, variable: "q3", linkedToQuestionId: q2Id, linkedFilter:"q4>1")
                });
                
                var interview = SetupStatefullInterview(questionnaireDocument);

                interview.AnswerNumericIntegerQuestion(userId, q1Id, RosterVector.Empty, DateTime.Now, 3);
                interview.AnswerTextQuestion(userId, q2Id, Abc.Create.Entity.RosterVector(new[] {0}), DateTime.Now, "test");
                interview.AnswerNumericIntegerQuestion(userId, q4Id, RosterVector.Empty, DateTime.Now, 3);
                interview.AnswerMultipleOptionsLinkedQuestion(userId, q3Id, RosterVector.Empty, DateTime.Now, new RosterVector[] {new []{0.0m}});


                var result = new InvokeResults();

                using (var eventContext = new EventContext())
                {
                    interview.AnswerNumericIntegerQuestion(userId, q4Id, RosterVector.Empty, DateTime.Now, 1);

                    result.HasLinkedQuestionToRemoveAnswer = eventContext.GetSingleEvent<AnswersRemoved>().Questions.Any(x => x.Id == q3Id && x.RosterVector == RosterVector.Empty);
                }

                return result;
            });

        [NUnit.Framework.Test] public void should_raise_AnswersRemoved_for_linked () =>
            results.HasLinkedQuestionToRemoveAnswer.Should().BeTrue();

        [NUnit.Framework.OneTimeTearDown] public void CleanUp()
        {
            appDomainContext.Dispose();
            appDomainContext = null;
        }

        static InvokeResults results;
        static AppDomainContext<AssemblyTargetLoader, PathBasedAssemblyResolver> appDomainContext;
        static readonly Guid questionnaireId = Guid.Parse("99999999999999999999999999999999");
        static readonly Guid rosterId = Guid.Parse("88888888888888888888888888888888");
        static readonly Guid q1Id = Guid.Parse("22222222222222222222222222222222");
        static readonly Guid q2Id = Guid.Parse("33333333333333333333333333333333");
        static readonly Guid q3Id = Guid.Parse("44444444444444444444444444444444");
        static readonly Guid q4Id = Guid.Parse("54444444444444444444444444444444");
        static readonly Guid userId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");

        [Serializable]
        internal class InvokeResults
        {
            public bool HasLinkedQuestionToRemoveAnswer { get; set; }
        }
    }
}
