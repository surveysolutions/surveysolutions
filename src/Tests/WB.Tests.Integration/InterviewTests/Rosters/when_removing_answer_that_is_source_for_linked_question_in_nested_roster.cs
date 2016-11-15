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
    internal class when_removing_answer_that_is_source_for_linked_question_in_nested_roster : InterviewTestsContext
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
                var commonRosterId = Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");
                var parentLinkedSourceRosterId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
                var linkedSourceRosterId = Guid.Parse("DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD");
                var linkedRosterId = Guid.Parse("FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF");

                var questionnaireDocument = Create.QuestionnaireDocumentWithOneChapter(questionnaireId,
                Create.Roster(id: commonRosterId, rosterSizeSourceType: RosterSizeSourceType.FixedTitles, fixedTitles: new [] { Create.FixedTitle(1), Create.FixedTitle(2) }, children: new IComposite[] 
                {
                    Create.Roster(id: parentLinkedSourceRosterId, fixedTitles: new [] { Create.FixedTitle(3), Create.FixedTitle(4) },  children: new IComposite[]
                    {
                        Create.Roster(id: linkedSourceRosterId, fixedTitles: new [] { Create.FixedTitle(5), Create.FixedTitle(6) }, children: new IComposite[]
                        {
                            Create.NumericIntegerQuestion(sourceQuestionId, variable: "source"),
                            Create.Roster(id: linkedRosterId, fixedTitles: new [] { Create.FixedTitle(7), Create.FixedTitle(8) }, children: new IComposite[]
                            {
                                Create.MultyOptionsQuestion(linkedId, variable: "linked", linkedToQuestionId: sourceQuestionId)
                            }),
                            Create.MultyOptionsQuestion(linkedOutsideId, variable: "linkedOutside", linkedToQuestionId: sourceQuestionId)
                        }),
                    })
                }));

                var result = new InvokeResults();

                var interview = SetupInterview(questionnaireDocument, new List<object>() { });
                interview.AnswerNumericIntegerQuestion(userId, sourceQuestionId, new decimal[] { 1, 3, 5 }, DateTime.Now, 17);
                interview.AnswerNumericIntegerQuestion(userId, sourceQuestionId, new decimal[] { 1, 3, 6 }, DateTime.Now, 66);
                interview.AnswerNumericIntegerQuestion(userId, sourceQuestionId, new decimal[] { 1, 4, 5 }, DateTime.Now, 66);
                interview.AnswerNumericIntegerQuestion(userId, sourceQuestionId, new decimal[] { 1, 4, 6 }, DateTime.Now, 66);

                interview.AnswerMultipleOptionsLinkedQuestion(userId, linkedId, new decimal[] { 1, 3, 5, 7 }, DateTime.Now, new decimal[][] { new decimal[] { 1, 3, 5 }, new decimal[] { 1, 3, 6 } });
                interview.AnswerMultipleOptionsLinkedQuestion(userId, linkedOutsideId, new decimal[] { 1, 3, 5 }, DateTime.Now, new decimal[][] { new decimal[] { 1, 3, 5 } });

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

        It should_remove_answer_for_linked_question_in_roster = () =>
            results.AnswerForLinkedQuestionWasCleared.ShouldBeTrue();

        It should_remove_answer_for_linked_question_outside_roster = () =>
            results.AnswerForLinkedQuestionOutsideRosterWasCleared.ShouldBeTrue();

        It should_update_options_for_linked_question_outside_roster = () =>
            results.OptionsForLinkedQuestionOutsideRosterWasUpdated.ShouldBeTrue();

        It should_update_options_for_linked_question_in_roster = () =>
          results.OptionsForLinkedQuestionWasUpdated.ShouldBeTrue();

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
            public bool OptionsForLinkedQuestionOutsideRosterWasUpdated { get; set; }
            public bool OptionsForLinkedQuestionWasUpdated { get; set; }
        }
    }
}