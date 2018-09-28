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
    internal class when_answering_question_that_affects_multi_yesno_question_with_filter_and_3_nested_rosters : InterviewTestsContext
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
                    Create.Entity.Option("1", "Option 1"),
                    Create.Entity.Option("2", "Option 2"),
                    Create.Entity.Option("3", "Option 3"),
                    Create.Entity.Option("12", "Option 12")
                };

                var questionnaireDocument = Create.Entity.QuestionnaireDocumentWithOneChapter(questionnaireId, new IComposite[]
                {
                    Create.Entity.NumericIntegerQuestion(q1Id, "q1"),
                    Create.Entity.MultyOptionsQuestion(q2Id, variable: "q2", options: options, optionsFilter: "@optioncode < q1", yesNoView: true),
                    Create.Entity.Roster(roster1Id, variable:"r1", rosterSizeQuestionId: q2Id, rosterSizeSourceType: RosterSizeSourceType.Question, children: new IComposite[]
                    {
                        Create.Entity.NumericIntegerQuestion(q3Id, "q3")
                    }),
                    Create.Entity.Roster(roster2Id, variable:"r2", rosterSizeQuestionId: q2Id, rosterSizeSourceType: RosterSizeSourceType.Question, children: new IComposite[]
                    {
                        Create.Entity.Roster(roster3Id, variable:"fixed_nested_r", rosterSizeSourceType: RosterSizeSourceType.FixedTitles, 
                            fixedRosterTitles: new []{ IntegrationCreate.FixedTitle(1, "Hello")}, children: new IComposite[]
                        {
                            Create.Entity.Roster(roster4Id, variable:"num_nested2_r", rosterSizeQuestionId: q3Id, rosterSizeSourceType: RosterSizeSourceType.Question, children: new IComposite[]
                            {
                                Create.Entity.NumericRealQuestion(q5Id, "q5")
                            })
                        })
                    }),
                    Create.Entity.SingleQuestion(q4Id, "q4", options: options, enablementCondition: "r1.SelectMany(x => x.fixed_nested_r.SelectMany(y=> y.num_nested2_r)).Count() > 1")
                });

                var interview = SetupInterview(questionnaireDocument);

                interview.AnswerNumericIntegerQuestion(userId, q1Id, RosterVector.Empty, DateTime.Now, 10);
                interview.AnswerYesNoQuestion(
                    Create.Command.AnswerYesNoQuestion(questionId: q2Id, rosterVector: RosterVector.Empty,
                        answeredOptions: new[]
                        {
                            Create.Entity.AnsweredYesNoOption(1m, true),
                            Create.Entity.AnsweredYesNoOption(2m, true),
                            Create.Entity.AnsweredYesNoOption(3m, true)
                        }
                        ));
                interview.AnswerNumericIntegerQuestion(userId, q3Id, Create.Entity.RosterVector(new[] {3}), DateTime.Now, 2);

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

        [NUnit.Framework.Test] public void should_disable_q4 () =>
            results.QuestionsQ4Disabled.Should().BeTrue();

        [NUnit.Framework.Test] public void should_have_one_option_answer_q2 () =>
            results.QuestionqQ2HasEmptyAnswer.Should().BeTrue();

        [NUnit.Framework.Test] public void should_remove_rosters_8 () =>
            results.RosterInstancesRemovedCount.Should().Be(8);

        [NUnit.Framework.OneTimeTearDown] public void CleanUp()
        {
            appDomainContext.Dispose();
            appDomainContext = null;
        }

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
