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

namespace WB.Tests.Integration.InterviewTests.Rosters
{
    internal class when_removing_answer_that_is_source_for_linked_question_in_nested_roster : InterviewTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            appDomainContext = AppDomainContext.Create();
            BecauseOf();
        }

        public void BecauseOf() =>
            results = Execute.InStandaloneAppDomain(appDomainContext.Domain, () =>
            {
                SetUp.MockedServiceLocator();

                var userId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");

                var questionnaireId = Guid.Parse("77778888000000000000000000000000");
                var sourceQuestionId = Guid.Parse("11111111111111111111111111111111");
                var linkedOutsideId = Guid.Parse("22222222222222222222222222222222");
                var linkedId = Guid.Parse("33333333333333333333333333333333");
                var commonRosterId = Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");
                var parentLinkedSourceRosterId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
                var linkedSourceRosterId = Guid.Parse("DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD");
                var linkedRosterId = Guid.Parse("FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF");

                var questionnaireDocument = Abc.Create.Entity.QuestionnaireDocumentWithOneChapter(questionnaireId,
                Abc.Create.Entity.Roster(rosterId: commonRosterId, rosterSizeSourceType: RosterSizeSourceType.FixedTitles, fixedRosterTitles: new [] { IntegrationCreate.FixedTitle(1), IntegrationCreate.FixedTitle(2) }, children: new IComposite[] 
                {
                    Abc.Create.Entity.Roster(rosterId: parentLinkedSourceRosterId, fixedRosterTitles: new [] { IntegrationCreate.FixedTitle(3), IntegrationCreate.FixedTitle(4) },  children: new IComposite[]
                    {
                        Abc.Create.Entity.Roster(rosterId: linkedSourceRosterId, fixedRosterTitles: new [] { IntegrationCreate.FixedTitle(5), IntegrationCreate.FixedTitle(6) }, children: new IComposite[]
                        {
                            Abc.Create.Entity.NumericIntegerQuestion(sourceQuestionId, variable: "source"),
                            Abc.Create.Entity.Roster(rosterId: linkedRosterId, fixedRosterTitles: new [] { IntegrationCreate.FixedTitle(7), IntegrationCreate.FixedTitle(8) }, children: new IComposite[]
                            {
                                Abc.Create.Entity.SingleOptionQuestion(linkedId, variable: "linked", linkedToQuestionId: sourceQuestionId)
                            }),
                            Abc.Create.Entity.MultyOptionsQuestion(linkedOutsideId, variable: "linkedOutside", linkedToQuestionId: sourceQuestionId)
                        }),
                    })
                }));

                var result = new InvokeResults();

                var interview = SetupInterview(questionnaireDocument, new List<object>() { });
                interview.AnswerNumericIntegerQuestion(userId, sourceQuestionId, new decimal[] { 1, 3, 5 }, DateTime.Now, 17);
                interview.AnswerNumericIntegerQuestion(userId, sourceQuestionId, new decimal[] { 1, 3, 6 }, DateTime.Now, 66);
                interview.AnswerNumericIntegerQuestion(userId, sourceQuestionId, new decimal[] { 1, 4, 5 }, DateTime.Now, 66);
                interview.AnswerNumericIntegerQuestion(userId, sourceQuestionId, new decimal[] { 1, 4, 6 }, DateTime.Now, 66);

                interview.AnswerSingleOptionLinkedQuestion(userId, linkedId, new decimal[] { 1, 3, 5, 7 }, DateTime.Now, new decimal[] { 1, 3, 5 });
                interview.AnswerMultipleOptionsLinkedQuestion(userId, linkedOutsideId, new decimal[] { 1, 3, 5 }, DateTime.Now, new RosterVector[] { new decimal[] { 1, 3, 5 } });

                using (var eventContext = new EventContext())
                {
                    interview.RemoveAnswer(sourceQuestionId, new decimal[] { 1, 3, 5 }, userId, DateTime.Now);

                    result.AnswerForLinkedQuestionWasCleared = eventContext.AnyEvent<AnswersRemoved>(q => q.Questions.Any(x => x.Id == linkedId));
                    result.AnswerForLinkedQuestionOutsideRosterWasCleared = eventContext.AnyEvent<AnswersRemoved>(q => q.Questions.Any(x => x.Id == linkedOutsideId));
                    result.OptionsForLinkedQuestionWasUpdated = eventContext.AnyEvent<LinkedOptionsChanged>(q => q.ChangedLinkedQuestions.Any(x => x.QuestionId.Id == linkedId));
                    result.OptionsForLinkedQuestionOutsideRosterWasUpdated = eventContext.AnyEvent<LinkedOptionsChanged>(q => q.ChangedLinkedQuestions.Any(x => x.QuestionId.Id == linkedOutsideId));
                }

                return result;
            });

        [NUnit.Framework.Test] public void should_remove_answer_for_linked_question_in_roster () =>
            results.AnswerForLinkedQuestionWasCleared.Should().BeTrue();

        [NUnit.Framework.Test] public void should_remove_answer_for_linked_question_outside_roster () =>
            results.AnswerForLinkedQuestionOutsideRosterWasCleared.Should().BeTrue();

        [NUnit.Framework.Test] public void should_update_options_for_linked_question_outside_roster () =>
            results.OptionsForLinkedQuestionOutsideRosterWasUpdated.Should().BeTrue();

        [NUnit.Framework.Test] public void should_update_options_for_linked_question_in_roster () =>
          results.OptionsForLinkedQuestionWasUpdated.Should().BeTrue();

        [NUnit.Framework.OneTimeTearDown] public void CleanUp()
        {
            appDomainContext.Dispose();
            appDomainContext = null;
        }

        private static InvokeResults results;
        private static AppDomainContext<AssemblyTargetLoader, PathBasedAssemblyResolver> appDomainContext;

        [Serializable]
        internal class InvokeResults
        {
            public bool AnswerForLinkedQuestionOutsideRosterWasCleared { get; set; }
            public bool AnswerForLinkedQuestionWasCleared { get; set; }
            public bool OptionsForLinkedQuestionOutsideRosterWasUpdated { get; set; }
            public bool OptionsForLinkedQuestionWasUpdated { get; set; }
        }
    }
}
