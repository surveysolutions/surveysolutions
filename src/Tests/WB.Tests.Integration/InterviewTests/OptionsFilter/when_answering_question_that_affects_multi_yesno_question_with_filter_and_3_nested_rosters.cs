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
    internal class when_answering_question_that_affects_multi_yesno_question_with_filter_and_3_nested_rosters : InterviewTestsContext
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
                    Create.Option(value: "1", text: "Option 1"),
                    Create.Option(value: "2", text: "Option 2"),
                    Create.Option(value: "3", text: "Option 3"),
                    Create.Option(value: "12", text: "Option 12"),
                };

                var questionnaireDocument = Create.QuestionnaireDocumentWithOneChapter(questionnaireId, children: new IComposite[]
                {
                    Create.NumericIntegerQuestion(q1Id, variable: "q1"),
                    Create.MultyOptionsQuestion(q2Id, variable: "q2", options: options, optionsFilter: "@optioncode < q1", yesNo: true),
                    Create.Roster(roster1Id, variable:"r1", rosterSizeQuestionId: q2Id, rosterSizeSourceType: RosterSizeSourceType.Question, children: new IComposite[]
                    {
                        Create.NumericIntegerQuestion(q3Id, variable: "q3"),
                    }),
                    Create.Roster(roster2Id, variable:"r2", rosterSizeQuestionId: q2Id, rosterSizeSourceType: RosterSizeSourceType.Question, children: new IComposite[]
                    {
                        Create.Roster(roster3Id, variable:"fixed_nested_r", rosterSizeSourceType: RosterSizeSourceType.FixedTitles, 
                            fixedRosterTitles: new []{ Create.FixedRosterTitle(1, "Hello")}, children: new IComposite[]
                        {
                            Create.Roster(roster4Id, variable:"num_nested2_r", rosterSizeQuestionId: q3Id, rosterSizeSourceType: RosterSizeSourceType.Question, children: new IComposite[]
                            {
                                Create.NumericRealQuestion(q5Id, variable: "q5"),
                            }),
                        })
                    }),
                    Create.SingleQuestion(q4Id, variable: "q4", options: options, enablementCondition: "r1.SelectMany(x => x.fixed_nested_r.SelectMany(y=> y.num_nested2_r)).Count() > 1")
                });

                ILatestInterviewExpressionState interviewState = GetInterviewExpressionState(questionnaireDocument);

                var interview = SetupInterview(questionnaireDocument, precompiledState: interviewState);

                interview.AnswerNumericIntegerQuestion(userId, q1Id, RosterVector.Empty, DateTime.Now, 10);
                interview.AnswerYesNoQuestion(Create.Command.AnswerYesNoQuestion(q2Id, RosterVector.Empty,
                    Create.AnsweredYesNoOption(1m, yes: true),
                    Create.AnsweredYesNoOption(2m, yes: true),
                    Create.AnsweredYesNoOption(3m, yes: true)
                    ));
                interview.AnswerNumericIntegerQuestion(userId, q3Id, Create.RosterVector(3), DateTime.Now, 2);

                var result = new InvokeResults();

                using (var eventContext = new EventContext())
                {
                    interview.AnswerNumericIntegerQuestion(userId, q1Id, RosterVector.Empty, DateTime.Now, 2);

                    result.QuestionsQ4Disabled = eventContext.AnyEvent<QuestionsDisabled>(x => x.Questions.Any(q => q.Id == q4Id));

                    result.QuestionqQ2HasEmptyAnswer = eventContext.AnyEvent<YesNoQuestionAnswered>(x => x.QuestionId == q2Id && x.AnsweredOptions.Length == 1);

                    result.RosterInstancesRemovedCount = eventContext.GetEvents<RosterInstancesRemoved>().SelectMany(x=>x.Instances).Count();
                }

                return result;
            });

        It should_disable_q4 = () =>
            results.QuestionsQ4Disabled.ShouldBeTrue();

        It should_have_one_option_answer_q2 = () =>
            results.QuestionqQ2HasEmptyAnswer.ShouldBeTrue();

        It should_remove_rosters_8 = () =>
            results.RosterInstancesRemovedCount.ShouldEqual(8);

        Cleanup stuff = () =>
        {
            appDomainContext.Dispose();
            appDomainContext = null;
        };

        private static InvokeResults results;
        private static AppDomainContext<AssemblyTargetLoader, PathBasedAssemblyResolver> appDomainContext;
        private static readonly Guid questionnaireId = Guid.Parse("99999999999999999999999999999999");
        private static readonly Guid roster1Id = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
        private static readonly Guid roster2Id = Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");
        private static readonly Guid roster3Id = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
        private static readonly Guid roster4Id = Guid.Parse("DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD");
        private static readonly Guid q1Id = Guid.Parse("11111111111111111111111111111111");
        private static readonly Guid q2Id = Guid.Parse("22222222222222222222222222222222");
        private static readonly Guid q3Id = Guid.Parse("33333333333333333333333333333333");
        private static readonly Guid q4Id = Guid.Parse("44444444444444444444444444444444");
        private static readonly Guid q5Id = Guid.Parse("55555555555555555555555555555555");
        private static readonly Guid userId = Guid.Parse("07777777777777777777777777777770");

        [Serializable]
        internal class InvokeResults
        {
            public bool QuestionsQ4Disabled { get; set; }
            public bool QuestionqQ2HasEmptyAnswer { get; set; }
            public int RosterInstancesRemovedCount { get; set; }
        }
    }
}