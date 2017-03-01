using System;
using System.Collections.Generic;
using System.Linq;
using AppDomainToolkit;
using Machine.Specifications;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Ncqrs.Spec;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;

namespace WB.Tests.Integration.InterviewTests.OptionsFilter
{
    internal class when_answering_multioption_question_removes_rosters_and_answer_for_linked_multi_question : InterviewTestsContext
    {
        Establish context = () =>
        {
            appDomainContext = AppDomainContext.Create();
        };

        Because of = () =>
            results = Execute.InStandaloneAppDomain(appDomainContext.Domain, () =>
            {
                Setup.MockedServiceLocator();

                var options = new List<Answer>
                {
                    Create.Option("2"), Create.Option("22")
                };

                var questionnaireDocument = Create.QuestionnaireDocumentWithOneChapter(questionnaireId, children: new IComposite[]
                {
                    Create.MultyOptionsQuestion(q2Id, variable: "q2", options: options),
                    Create.Roster(rosterId, variable:"r1", rosterSizeQuestionId: q2Id, rosterSizeSourceType: RosterSizeSourceType.Question, children: new IComposite[]
                    {
                        Abc.Create.Entity.NumericIntegerQuestion(q3Id, variable: "age", enablementCondition:"@rowindex == 1"),
                    }),
                    Create.MultyOptionsQuestion(q4Id, variable: "q4", linkedToRosterId: rosterId),
                    
                });

                ILatestInterviewExpressionState interviewState = GetInterviewExpressionState(questionnaireDocument);

                var interview = SetupInterview(questionnaireDocument, precompiledState: interviewState);

                interview.AnswerMultipleOptionsQuestion(userId, q2Id, RosterVector.Empty, DateTime.Now, new[] { 2 });
                interview.AnswerMultipleOptionsQuestion(userId, q2Id, RosterVector.Empty, DateTime.Now, new[] { 2, 22 });
               
                
                var result = new InvokeResults();

                using (var eventContext = new EventContext())
                {
                    interview.AnswerMultipleOptionsQuestion(userId, q2Id, RosterVector.Empty, DateTime.Now, new[] { 22 });

                    result.HasOption = eventContext.AnyEvent<LinkedOptionsChanged>(@event
                        => @event.ChangedLinkedQuestions.Count(q => q.QuestionId.Id == q4Id && q.Options.Length == 1) == 1);
                }

                return result;
            });

        It should_disable_q5 = () =>
            results.HasOption.ShouldBeTrue();

        Cleanup stuff = () =>
        {
            appDomainContext.Dispose();
            appDomainContext = null;
        };

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
            public bool HasOption { get; set; }
        }
    }
}