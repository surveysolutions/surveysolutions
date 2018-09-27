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
using WB.Core.SharedKernels.SurveySolutions.Documents;

namespace WB.Tests.Integration.InterviewTests.Rosters
{
    internal class when_removing_answer_that_is_source_for_linked_question_in_other_roster : InterviewTestsContext
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
                var rosterId = Guid.Parse("DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD");
                var fixedRosterId = Guid.Parse("EEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEE");

                var questionnaireDocument = Abc.Create.Entity.QuestionnaireDocumentWithOneChapter(questionnaireId,
                    Abc.Create.Entity.Roster(
                        rosterId: rosterId,
                        rosterSizeSourceType: RosterSizeSourceType.FixedTitles,
                        fixedRosterTitles: new FixedRosterTitle[] {
                            IntegrationCreate.FixedTitle(1, "Roster 1"),
                            IntegrationCreate.FixedTitle(2, "Roster 2") },
                        variable: "fixed_source",
                        children: new IComposite[]
                        {
                            Abc.Create.Entity.NumericIntegerQuestion(sourceQuestionId, variable: "source")
                        }),

                    Abc.Create.Entity.Roster(
                        rosterId: fixedRosterId,
                        rosterSizeSourceType: RosterSizeSourceType.FixedTitles,
                        fixedRosterTitles: new FixedRosterTitle[] {
                            IntegrationCreate.FixedTitle(3, "Item 1"),
                            IntegrationCreate.FixedTitle(4, "Item 2") },
                        variable: "fix",
                        children: new IComposite[]
                        {
                            Abc.Create.Entity.SingleOptionQuestion(linkedId, variable: "linked", linkedToQuestionId: sourceQuestionId)
                        }),
                    Abc.Create.Entity.MultyOptionsQuestion(linkedOutsideId, variable: "linkedOutside", linkedToQuestionId: sourceQuestionId)
                );

                var result = new InvokeResults();

                var interview = SetupInterview(questionnaireDocument, new List<object>() { });
                interview.AnswerNumericIntegerQuestion(userId, sourceQuestionId, new decimal[1] { 1 }, DateTime.Now, 17);
                interview.AnswerNumericIntegerQuestion(userId, sourceQuestionId, new decimal[1] { 2 }, DateTime.Now, 66);

                interview.AnswerSingleOptionLinkedQuestion(userId, linkedId, new decimal[1] { 3 }, DateTime.Now, new decimal[] { 1 } );
                interview.AnswerMultipleOptionsLinkedQuestion(userId, linkedOutsideId, new decimal[0], DateTime.Now, new RosterVector[] { new decimal[] { 1 } });

                using (var eventContext = new EventContext())
                {
                    interview.RemoveAnswer(sourceQuestionId, new decimal[1] { 1 }, userId, DateTime.Now);

                    result.AnswerForLinkedQuestionWasCleared = eventContext.AnyEvent<AnswersRemoved>(q => q.Questions.Any(x => x.Id == linkedId));
                    result.AnswerForLinkedQuestionOutsideRosterWasCleared = eventContext.AnyEvent<AnswersRemoved>(q => q.Questions.Any(x => x.Id == linkedOutsideId));
                }

                return result;
            });

        [NUnit.Framework.Test] public void should_remove_answer_for_linked_question_in_roster () =>
            results.AnswerForLinkedQuestionWasCleared.Should().BeTrue();

        [NUnit.Framework.Test] public void should_remove_answer_for_linked_question_outside_roster () =>
           results.AnswerForLinkedQuestionOutsideRosterWasCleared.Should().BeTrue();

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
        }
    }
}
