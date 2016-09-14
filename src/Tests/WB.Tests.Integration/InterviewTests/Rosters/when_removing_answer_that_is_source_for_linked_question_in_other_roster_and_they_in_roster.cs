using System;
using System.Collections.Generic;
using System.Linq;
using AppDomainToolkit;
using Machine.Specifications;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Ncqrs.Spec;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.SurveySolutions.Documents;

namespace WB.Tests.Integration.InterviewTests.Rosters
{
    internal class when_removing_answer_that_is_source_for_linked_question_in_other_roster_and_they_in_roster : InterviewTestsContext
    {
        Establish context = () =>
        {
            appDomainContext = AppDomainContext.Create();
        };

        Because of = () =>
            results = Execute.InStandaloneAppDomain(appDomainContext.Domain, () =>
            {
                Setup.MockedServiceLocator();

                var userId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");

                var questionnaireId = Guid.Parse("77778888000000000000000000000000");
                var sourceQuestionId = Guid.Parse("11111111111111111111111111111111");
                var linkedOutsideId = Guid.Parse("22222222222222222222222222222222");
                var linkedId = Guid.Parse("33333333333333333333333333333333");
                var parentRosterId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
                var rosterId = Guid.Parse("DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD");
                var fixedRosterId = Guid.Parse("EEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEE");

                var questionnaireDocument = Create.QuestionnaireDocumentWithOneChapter(questionnaireId,
                    Create.Roster(
                        id: parentRosterId,
                        rosterSizeSourceType: RosterSizeSourceType.FixedTitles,
                        fixedRosterTitles: new FixedRosterTitle[] {
                                Create.FixedRosterTitle(1, "Parent 1"),
                                Create.FixedRosterTitle(2, "Parent 2") },
                        children: new IComposite[] {
                            Create.Roster(
                                id: rosterId,
                                rosterSizeSourceType: RosterSizeSourceType.FixedTitles,
                                fixedRosterTitles: new FixedRosterTitle[] {
                                    Create.FixedRosterTitle(3, "Roster 1"),
                                    Create.FixedRosterTitle(4, "Roster 2") },
                                variable: "fixed_source",
                                children: new IComposite[]
                                {
                                    Create.NumericIntegerQuestion(sourceQuestionId, variable: "source")
                                }),

                            Create.Roster(
                                id: fixedRosterId,
                                rosterSizeSourceType: RosterSizeSourceType.FixedTitles,
                                fixedRosterTitles: new FixedRosterTitle[] {
                                    Create.FixedRosterTitle(5, "Item 1"),
                                    Create.FixedRosterTitle(6, "Item 2") },
                                variable: "fix",
                                children: new IComposite[]
                                {
                                    Create.MultyOptionsQuestion(linkedId, variable: "linked", linkedToQuestionId: sourceQuestionId)
                                }),
                            Create.MultyOptionsQuestion(linkedOutsideId, variable: "linkedOutside", linkedToQuestionId: sourceQuestionId)
                        })
                    );

                var result = new InvokeResults();

                var interview = SetupInterview(questionnaireDocument, new List<object>() { });
                interview.AnswerNumericIntegerQuestion(userId, sourceQuestionId, new decimal[] { 1, 3 }, DateTime.Now, 17);
                interview.AnswerNumericIntegerQuestion(userId, sourceQuestionId, new decimal[] { 1, 4 }, DateTime.Now, 66);

                interview.AnswerMultipleOptionsLinkedQuestion(userId, linkedId, new decimal[] { 1, 5 }, DateTime.Now, new decimal[][] { new decimal[] { 1, 3 }, new decimal[] { 1, 4 } });
                interview.AnswerMultipleOptionsLinkedQuestion(userId, linkedOutsideId, new decimal[] { 1 }, DateTime.Now, new decimal[][] { new decimal[] { 1, 3 } });

                using (var eventContext = new EventContext())
                {
                    interview.RemoveAnswer(sourceQuestionId, new decimal[] { 1, 3 }, userId, DateTime.Now);

                    result.AnswerForLinkedQuestionWasCleared = eventContext.AnyEvent<AnswersRemoved>(q => q.Questions.Any(x => x.Id == linkedId));
                    result.AnswerForLinkedQuestionOutsideRosterWasCleared = eventContext.AnyEvent<AnswersRemoved>(q => q.Questions.Any(x => x.Id == linkedOutsideId));
                }

                return result;
            });

        It should_remove_answer_for_linked_question_in_roster = () =>
            results.AnswerForLinkedQuestionWasCleared.ShouldBeTrue();

        It should_remove_answer_for_linked_question_outside_roster = () =>
            results.AnswerForLinkedQuestionOutsideRosterWasCleared.ShouldBeTrue();

        Cleanup stuff = () =>
        {
            appDomainContext.Dispose();
            appDomainContext = null;
        };

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