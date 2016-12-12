using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using AppDomainToolkit;

using Machine.Specifications;

using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;

using Ncqrs.Spec;

using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.SurveySolutions.Documents;

namespace WB.Tests.Integration.InterviewTests.Rosters
{
    internal class when_removing_answer_that_is_source_for_linked_question_in_other_roster : InterviewTestsContext
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
                var rosterId = Guid.Parse("DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD");
                var fixedRosterId = Guid.Parse("EEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEE");

                var questionnaireDocument = Create.QuestionnaireDocumentWithOneChapter(questionnaireId,
                    Create.Roster(
                        id: rosterId,
                        rosterSizeSourceType: RosterSizeSourceType.FixedTitles,
                        fixedTitles: new FixedRosterTitle[] {
                            Create.FixedTitle(1, "Roster 1"),
                            Create.FixedTitle(2, "Roster 2") },
                        variable: "fixed_source",
                        children: new IComposite[]
                        {
                            Create.NumericIntegerQuestion(sourceQuestionId, variable: "source")
                        }),

                    Create.Roster(
                        id: fixedRosterId,
                        rosterSizeSourceType: RosterSizeSourceType.FixedTitles,
                        fixedTitles: new FixedRosterTitle[] {
                            Create.FixedTitle(3, "Item 1"),
                            Create.FixedTitle(4, "Item 2") },
                        variable: "fix",
                        children: new IComposite[]
                        {
                            Create.MultyOptionsQuestion(linkedId, variable: "linked", linkedToQuestionId: sourceQuestionId)
                        }),
                    Create.MultyOptionsQuestion(linkedOutsideId, variable: "linkedOutside", linkedToQuestionId: sourceQuestionId)
                );

                var result = new InvokeResults();

                var interview = SetupInterview(questionnaireDocument, new List<object>() { });
                interview.AnswerNumericIntegerQuestion(userId, sourceQuestionId, new decimal[1] { 1 }, DateTime.Now, 17);
                interview.AnswerNumericIntegerQuestion(userId, sourceQuestionId, new decimal[1] { 2 }, DateTime.Now, 66);

                interview.AnswerMultipleOptionsLinkedQuestion(userId, linkedId, new decimal[1] { 3 }, DateTime.Now, new decimal[][] { new decimal[] { 1 }, new decimal[] { 2 } });
                interview.AnswerMultipleOptionsLinkedQuestion(userId, linkedOutsideId, new decimal[0], DateTime.Now, new decimal[][] { new decimal[] { 1 } });

                using (var eventContext = new EventContext())
                {
                    interview.RemoveAnswer(sourceQuestionId, new decimal[1] { 1 }, userId, DateTime.Now);

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
