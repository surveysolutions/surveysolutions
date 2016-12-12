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

namespace WB.Tests.Integration.InterviewTests.LinkedQuestions
{
    internal class when_answering_question_that_change_options_list_of_linked_that_is_different_from_source_question_scope : InterviewTestsContext
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
                    Create.Option("1"), Create.Option("2"), Create.Option("3"), Create.Option("12")
                };

                var questionnaireDocument = Create.QuestionnaireDocumentWithOneChapter(questionnaireId, children: new IComposite[]
                {
                    Create.MultyOptionsQuestion(q2Id, variable: "q2", options: options),
                    Create.Roster(rosterId, variable:"r", rosterSizeQuestionId: q2Id, rosterSizeSourceType: RosterSizeSourceType.Question, children: new IComposite[]
                    {
                        Create.NumericIntegerQuestion(q3Id, variable: "age")
                    }),
                    Create.Roster(roster1Id, variable:"r1", fixedTitles: new [] { Create.FixedTitle(1),  Create.FixedTitle(2)}, rosterSizeSourceType: RosterSizeSourceType.FixedTitles, children: new IComposite[]
                    {
                        Create.NumericIntegerQuestion(q5Id, variable: "ageFilter"),
                        Create.SingleQuestion(q4Id, variable: "q4", linkedToQuestionId: q3Id, linkedFilter: "age > current.ageFilter")
                    })
                });

                ILatestInterviewExpressionState interviewState = GetInterviewExpressionState(questionnaireDocument);

                var interview = SetupInterview(questionnaireDocument, precompiledState: interviewState);

                interview.AnswerMultipleOptionsQuestion(userId, q2Id, RosterVector.Empty, DateTime.Now, new[] { 1, 2, 3 });
                interview.AnswerNumericIntegerQuestion(userId, q3Id, Create.RosterVector(1), DateTime.Now, 20);
                interview.AnswerNumericIntegerQuestion(userId, q3Id, Create.RosterVector(2), DateTime.Now, 15);
                interview.AnswerNumericIntegerQuestion(userId, q5Id, Create.RosterVector(1), DateTime.Now, 18);
                interview.AnswerNumericIntegerQuestion(userId, q5Id, Create.RosterVector(2), DateTime.Now, 12);

                var result = new InvokeResults();

                using (var eventContext = new EventContext())
                {
                    interview.AnswerNumericIntegerQuestion(userId, q3Id, Create.RosterVector(3), DateTime.Now, 35);

                    result.OptionsCountForQuestion4InFixedRoster1 = GetChangedOptions(eventContext, q4Id, Create.RosterVector(1))?.Length ?? 0;
                    result.OptionsCountForQuestion4InFixedRoster2 = GetChangedOptions(eventContext, q4Id, Create.RosterVector(2))?.Length ?? 0;
                }

                return result;
            });

        It should_return_2_options_for_linked_question_in_1_roster = () =>
            results.OptionsCountForQuestion4InFixedRoster1.ShouldEqual(2);

        It should_return_3_options_for_linked_question_in_2_roster = () =>
            results.OptionsCountForQuestion4InFixedRoster2.ShouldEqual(3);

        Cleanup stuff = () =>
        {
            appDomainContext.Dispose();
            appDomainContext = null;
        };

        private static InvokeResults results;
        private static AppDomainContext<AssemblyTargetLoader, PathBasedAssemblyResolver> appDomainContext;
        private static readonly Guid questionnaireId = Guid.Parse("99999999999999999999999999999999");
        private static readonly Guid rosterId = Guid.Parse("88888888888888888888888888888888");
        private static readonly Guid roster1Id = Guid.Parse("77777777777777777777777777777777");
        private static readonly Guid q2Id = Guid.Parse("22222222222222222222222222222222");
        private static readonly Guid q3Id = Guid.Parse("33333333333333333333333333333333");
        private static readonly Guid q4Id = Guid.Parse("44444444444444444444444444444444");
        private static readonly Guid q5Id = Guid.Parse("55555555555555555555555555555555");
        private static readonly Guid userId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");

        [Serializable]
        internal class InvokeResults
        {
            public int OptionsCountForQuestion4InFixedRoster1 { get; set; }
            public int OptionsCountForQuestion4InFixedRoster2 { get; set; }
        }
    }
}