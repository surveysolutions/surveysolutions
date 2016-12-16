using System;
using AppDomainToolkit;
using Machine.Specifications;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Ncqrs.Spec;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;

namespace WB.Tests.Integration.InterviewTests.LinkedQuestions
{
    internal class when_answering_numeric_question_that_affects_filter_of_linked_multioption_question : InterviewTestsContext
    {
        Establish context = () =>
        {
            appDomainContext = AppDomainContext.Create();
        };

        private Because of = () =>
            results = Execute.InStandaloneAppDomain(appDomainContext.Domain, () =>
            {
                Setup.MockedServiceLocator();

                var questionnaireDocument = Create.QuestionnaireDocumentWithOneChapter(questionnaireId, children: new IComposite[]
                {
                    Create.NumericIntegerQuestion(q1Id, variable: "q1"),
                    Create.Roster(rosterId, variable:"r1", rosterSizeQuestionId: q1Id, rosterTitleQuestionId: q2Id, 
                        rosterSizeSourceType: RosterSizeSourceType.Question, children: new IComposite[]
                    {
                        Create.TextQuestion(q2Id, variable: "q2"),
                    }),
                    Create.NumericIntegerQuestion(q4Id, variable: "q4"),

                    Create.MultyOptionsQuestion(q3Id, variable: "q3", linkedToQuestionId: q2Id, linkedFilter:"q4>1")
                });
                
                ILatestInterviewExpressionState interviewState = GetInterviewExpressionState(questionnaireDocument, false);

                var interview = SetupStatefullInterview(questionnaireDocument, precompiledState: interviewState);

                interview.AnswerNumericIntegerQuestion(userId, q1Id, RosterVector.Empty, DateTime.Now, 3);
                interview.AnswerTextQuestion(userId, q2Id, Create.RosterVector(0), DateTime.Now, "test");
                interview.AnswerNumericIntegerQuestion(userId, q4Id, RosterVector.Empty, DateTime.Now, 3);
                interview.AnswerMultipleOptionsLinkedQuestion(userId, q3Id, RosterVector.Empty, DateTime.Now, new decimal[][] {new []{0.0m}});


                var result = new InvokeResults();

                using (var eventContext = new EventContext())
                {
                    interview.AnswerNumericIntegerQuestion(userId, q4Id, RosterVector.Empty, DateTime.Now, 1);

                    result.QuestionsToRemoveAnswer = eventContext.GetSingleEvent<AnswersRemoved>().Questions;
                }

                return result;
            });

        It should_raise_AnswersRemoved_for_linked = () =>
            results.QuestionsToRemoveAnswer.ShouldContain(new Identity(q3Id, RosterVector.Empty));

        Cleanup stuff = () =>
        {
            appDomainContext.Dispose();
            appDomainContext = null;
        };

        private static InvokeResults results;
        private static AppDomainContext<AssemblyTargetLoader, PathBasedAssemblyResolver> appDomainContext;
        private static readonly Guid questionnaireId = Guid.Parse("99999999999999999999999999999999");
        private static readonly Guid rosterId = Guid.Parse("88888888888888888888888888888888");
        private static readonly Guid q1Id = Guid.Parse("22222222222222222222222222222222");
        private static readonly Guid q2Id = Guid.Parse("33333333333333333333333333333333");
        private static readonly Guid q3Id = Guid.Parse("44444444444444444444444444444444");
        private static readonly Guid q4Id = Guid.Parse("54444444444444444444444444444444");
        private static readonly Guid userId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");

        [Serializable]
        internal class InvokeResults
        {
            public Identity[] QuestionsToRemoveAnswer { get; set; }
        }
    }
}