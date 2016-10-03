using System;
using AppDomainToolkit;
using Machine.Specifications;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Ncqrs.Spec;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;

namespace WB.Tests.Integration.InterviewTests.LinkedQuestions
{
    internal class when_answering_text_question_that_is_source_for_linked_question_and_in_numeric_roster : InterviewTestsContext
    {
        Establish context = () =>
        {
            appDomainContext = AppDomainContext.Create();
        };

        Because of = () =>
            results = Execute.InStandaloneAppDomain(appDomainContext.Domain, () =>
            {
                Setup.MockedServiceLocator();

                var questionnaireDocument = Create.QuestionnaireDocumentWithOneChapter(questionnaireId, children: new IComposite[]
                {
                    Create.NumericIntegerQuestion(q1Id, variable: "q1"),
                    Create.Roster(rosterId, variable:"r1", rosterSizeQuestionId: q1Id, rosterTitleQuestionId: q2Id, rosterSizeSourceType: RosterSizeSourceType.Question, children: new IComposite[]
                    {
                        Create.TextQuestion(q2Id, variable: "q2"),
                    }),
                    Create.SingleQuestion(q3Id, variable: "q3", linkedToRosterId: rosterId)
                });

                ILatestInterviewExpressionState interviewState = GetInterviewExpressionState(questionnaireDocument);

                var interview = SetupStatefullInterview(questionnaireDocument, precompiledState: interviewState);

                interview.AnswerNumericIntegerQuestion(userId, q1Id, RosterVector.Empty, DateTime.Now, 3);

                var result = new InvokeResults();

                using (var eventContext = new EventContext())
                {
                    interview.AnswerTextQuestion(userId, q2Id, Create.RosterVector(2), DateTime.Now, "hello");

                    result.OptionsCountForQuestion3 = GetChangedOptions(eventContext, q3Id, RosterVector.Empty)?.Length ?? 0;
                }

                return result;
            });

        It should_return_1_option_for_linked_question = () =>
            results.OptionsCountForQuestion3.ShouldEqual(1);

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
        private static readonly Guid userId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");

        [Serializable]
        internal class InvokeResults
        {
            public int OptionsCountForQuestion3 { get; set; }
        }
    }
}