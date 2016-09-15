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
    internal class when_answering_question_that_change_options_list_of_linked_that_is_one_level_deeper_than_source_question : InterviewTestsContext
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
                        Create.NumericIntegerQuestion(q3Id, variable: "age"),
                        Create.Roster(roster1Id, variable:"r1", fixedRosterTitles: new [] { Create.FixedRosterTitle(1),  Create.FixedRosterTitle(2)}, rosterSizeSourceType: RosterSizeSourceType.FixedTitles, children: new IComposite[]
                        {
                            Create.SingleQuestion(q4Id, variable: "q4", linkedToQuestionId: q3Id, linkedFilter: "age > current.age")
                        }),
                    })
                });

                ILatestInterviewExpressionState interviewState = GetInterviewExpressionState(questionnaireDocument);

                var interview = SetupInterview(questionnaireDocument, precompiledState: interviewState);

                interview.AnswerMultipleOptionsQuestion(userId, q2Id, RosterVector.Empty, DateTime.Now, new[] { 1m, 2m, 3m });
                interview.AnswerNumericIntegerQuestion(userId, q3Id, Create.RosterVector(2), DateTime.Now, 15);
                interview.AnswerNumericIntegerQuestion(userId, q3Id, Create.RosterVector(3), DateTime.Now, 35);

                var result = new InvokeResults();

                using (var eventContext = new EventContext())
                {
                    interview.AnswerNumericIntegerQuestion(userId, q3Id, Create.RosterVector(1), DateTime.Now, 20);

                    result.OptionsCountForQuestion4InRoster1_1 = GetChangedOptions(eventContext, q4Id, Create.RosterVector(1, 1))?.Length ?? 0;
                    result.OptionsCountForQuestion4InRoster2_1 = GetChangedOptions(eventContext, q4Id, Create.RosterVector(2, 1))?.Length ?? 0;
                    result.OptionsCountForQuestion4InRoster3_1 = GetChangedOptions(eventContext, q4Id, Create.RosterVector(3, 1))?.Length ?? 0;
                }

                return result;
            });

        It should_return_1_option_for_linked_question_in_1_1_roster = () =>
            results.OptionsCountForQuestion4InRoster1_1.ShouldEqual(1);

        It should_return_2_options_for_linked_question_in_2_1_roster = () =>
            results.OptionsCountForQuestion4InRoster2_1.ShouldEqual(2);

        It should_not_return_options_for_linked_question_in_3_1_roster = () =>
            results.OptionsCountForQuestion4InRoster3_1.ShouldEqual(0);

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
        private static readonly Guid userId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");

        [Serializable]
        internal class InvokeResults
        {
            public int OptionsCountForQuestion4InRoster1_1 { get; set; }
            public int OptionsCountForQuestion4InRoster2_1 { get; set; }
            public int OptionsCountForQuestion4InRoster3_1 { get; set; }
        }
    }
}