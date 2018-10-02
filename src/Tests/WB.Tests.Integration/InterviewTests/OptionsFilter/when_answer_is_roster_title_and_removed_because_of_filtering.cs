using System;
using System.Collections.Generic;
using System.Linq;
using AppDomainToolkit;
using FluentAssertions;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Ncqrs.Spec;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Events.Interview.Dtos;
using WB.Tests.Abc;

namespace WB.Tests.Integration.InterviewTests.OptionsFilter
{
    internal class when_answer_is_roster_title_and_removed_because_of_filtering : InterviewTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            appDomainContext = AppDomainContext.Create();
            BecauseOf();
        }

        public void BecauseOf() =>
            results = Execute.InStandaloneAppDomain(appDomainContext.Domain, () =>
            {
                SetUp.MockedServiceLocator();


                var rosterTitleQuestionIdentity = Create.Identity(rosterTitleQuestionId,
                    Create.RosterVector(0));
                var rosterIdentity = Create.Identity(rosterId, Create.RosterVector(0));

                var options = new List<Answer>
                {
                    Create.Entity.Option("1", "Option 1"),
                    Create.Entity.Option("2", "Option 2")
                };

                var questionnaire = Create.Entity.QuestionnaireDocumentWithOneChapter(questionnaireId,
                    Create.Entity.NumericIntegerQuestion(numericQuestionId, "trigger"),
                    Create.Entity.NumericRoster(rosterId, rosterSizeQuestionId: numericQuestionId, rosterTitleQuestionId: rosterTitleQuestionId, variable: "num1", children: new IComposite[]
                    {
                        Create.Entity.SingleQuestion(rosterTitleQuestionId, "singleopt", optionsFilter: "trigger == 1", options: options)
                    }),
                    Create.Entity.NumericRoster(rosterWithMultioptionsTitle, variable: "num2", rosterSizeQuestionId: numericQuestionId, rosterTitleQuestionId: multioptionsQuestionId, children: new IComposite[]
                    {
                        Create.Entity.MultyOptionsQuestion(multioptionsQuestionId, variable: "multiopt", optionsFilter: "trigger == 1", options: options)
                    }),
                    Create.Entity.NumericRoster(rosterWithYesNoTitle, rosterSizeQuestionId: numericQuestionId, rosterTitleQuestionId: multioptionsQuestionId, children: new IComposite[]
                    {
                        Create.Entity.MultyOptionsQuestion(yesNoQuestionId, variable: "yesno", yesNoView: true, optionsFilter: "trigger == 1", options: options)
                    })
                );

                var interview = SetupInterview(questionnaire);

                interview.AnswerNumericIntegerQuestion(userId, numericQuestionId, RosterVector.Empty, DateTime.Today, 1);
                interview.AnswerSingleOptionQuestion(userId, rosterTitleQuestionIdentity.Id, rosterTitleQuestionIdentity.RosterVector, DateTime.Today, 1);
                interview.AnswerMultipleOptionsQuestion(userId, multioptionsQuestionId, Create.RosterVector(0), DateTime.Today, new[] {1});
                interview.AnswerYesNoQuestion(new AnswerYesNoQuestion(interview.EventSourceId, userId, yesNoQuestionId, Create.RosterVector(0), new List<AnsweredYesNoOption> { new AnsweredYesNoOption(1, true) }));

                var result = new InvokeResults();
                using (var eventContext = new EventContext())
                {
                    interview.AnswerNumericIntegerQuestion(userId, numericQuestionId, RosterVector.Empty, DateTime.Now, 2);

                    result.RosterTitleHasChanged = eventContext.AnyEvent<RosterInstancesTitleChanged>(x => x.ChangedInstances.Any(c => c.RosterInstance.GroupId == rosterIdentity.Id));
                    result.RosterTitleBecomesEmpty = eventContext.AnyEvent<RosterInstancesTitleChanged>(x => x
                        .ChangedInstances.Any(c => c.RosterInstance.GroupId == rosterIdentity.Id && c.Title == null));

                    result.MultioptionRosterTriggeredTitleChanged = eventContext.AnyEvent<RosterInstancesTitleChanged>(x => x
                        .ChangedInstances.Any(c => c.RosterInstance.GroupId == rosterWithMultioptionsTitle && string.IsNullOrEmpty(c.Title)));

                    result.YesNoRosterTriggeredRosterTitleChanged = eventContext.AnyEvent<RosterInstancesTitleChanged>(x => x
                        .ChangedInstances.Any(c => c.RosterInstance.GroupId == rosterWithYesNoTitle && string.IsNullOrEmpty(c.Title)));
                }

                return result;
            });

        [NUnit.Framework.Test] public void should_raise_roster_titles_changed_for_roster () => results.RosterTitleHasChanged.Should().BeTrue();

        [NUnit.Framework.Test] public void should_calculate_new_roster_title_as_empty_string () => results.RosterTitleBecomesEmpty.Should().BeTrue();

        [NUnit.Framework.Test] public void should_trigger_roster_title_changed_for_roster_with_multioptions_roster_title_question () => results.MultioptionRosterTriggeredTitleChanged.Should().BeTrue();

        [NUnit.Framework.Test] public void should_trigger_roster_title_changed_for_roster_with_yesno_roster_title_question () => results.YesNoRosterTriggeredRosterTitleChanged.Should().BeTrue();

        static readonly Guid userId = Guid.NewGuid();

        static AppDomainContext<AssemblyTargetLoader, PathBasedAssemblyResolver> appDomainContext;
        static InvokeResults results;

        private static readonly Guid numericQuestionId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
        private static readonly Guid rosterId = Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");
        private static readonly Guid rosterTitleQuestionId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
        private static readonly Guid questionnaireId = Guid.Parse("DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD");

        private static readonly Guid rosterWithMultioptionsTitle = Guid.Parse("11111111111111111111111111111111");
        private static readonly Guid multioptionsQuestionId = Guid.Parse("22222222222222222222222222222222");

        private static readonly Guid rosterWithYesNoTitle = Guid.Parse("33333333333333333333333333333333");
        private static readonly Guid yesNoQuestionId = Guid.Parse("44444444444444444444444444444444");

        [Serializable]
        internal class InvokeResults
        {
            public bool RosterTitleHasChanged { get; set; }
            public bool RosterTitleBecomesEmpty { get; set; }
            public bool MultioptionRosterTriggeredTitleChanged { get; set; }
            public bool YesNoRosterTriggeredRosterTitleChanged { get; set; }
        }
    }
}
