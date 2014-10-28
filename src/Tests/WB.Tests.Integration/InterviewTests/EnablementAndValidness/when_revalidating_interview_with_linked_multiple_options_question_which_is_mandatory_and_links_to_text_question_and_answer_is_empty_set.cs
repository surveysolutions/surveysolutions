using System;
using System.Linq;
using AppDomainToolkit;
using Machine.Specifications;
using Ncqrs.Spec;
using WB.Core.SharedKernels.DataCollection.Events.Interview;

namespace WB.Tests.Integration.InterviewTests.EnablementAndValidness
{
    internal class when_revalidating_interview_with_linked_multiple_options_question_which_is_mandatory_and_links_to_text_question_and_answer_is_empty_set : InterviewTestsContext
    {
        Establish context = () =>
        {
            appDomainContext = AppDomainContext.Create();
        };

        Because of = () =>
            results = Execute.InStandaloneAppDomain(appDomainContext.Domain, () =>
            {
                Setup.MockedServiceLocator();
                
                var questionnaireId = Guid.Parse("00000000000000000000000000000000");
                var categoricalMultiOptionMandatoryQuestionId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
                var rosterTextQuestionId = Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");
                var rosterId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");

                var linkedOption1Vector = new decimal[] { 0 };
                var linkedOption2Vector = new decimal[] { 1 };
                var linkedOption3Vector = new decimal[] { 2 };

                var questionnaireDocument = Create.QuestionnaireDocument(questionnaireId,
                    Create.Roster(rosterId, fixedTitles: new []{ "roster row 1", "roster row 2", "roster row 3"},
                        rosterTitleQuestionId: rosterTextQuestionId,
                        children: new[]
                        {
                            Create.Question(rosterTextQuestionId)
                        }),
                    Create.MultyOptionsQuestion(categoricalMultiOptionMandatoryQuestionId, isMandatory: true,
                        linkedToQuestionId: rosterTextQuestionId)
                    );

                var interview = SetupInterview(questionnaireDocument, new object[]
                {
                    Create.Event.TextQuestionAnswered(rosterTextQuestionId, propagationVector: linkedOption1Vector, answer: "linked option 1"),
                    Create.Event.TextQuestionAnswered(rosterTextQuestionId, propagationVector: linkedOption2Vector, answer: "linked option 2"),
                    Create.Event.TextQuestionAnswered(rosterTextQuestionId, propagationVector: linkedOption3Vector, answer: "linked option 3"),
                    Create.Event.MultipleOptionsLinkedQuestionAnswered(categoricalMultiOptionMandatoryQuestionId,
                        selectedValues: new[] {linkedOption3Vector, linkedOption2Vector}),
                    Create.Event.MultipleOptionsLinkedQuestionAnswered(categoricalMultiOptionMandatoryQuestionId,
                        selectedValues: new decimal[][] {})
                });
                

                using (var eventContext = new EventContext())
                {
                    interview.ReevaluateSynchronizedInterview();

                    return new InvokeResults()
                    {
                        MandatoryQuestionNotDeclaredAsInvalid =
                            !HasEvent<AnswersDeclaredInvalid>(eventContext.Events,
                                where =>
                                    where.Questions.Any(
                                        question => question.Id == categoricalMultiOptionMandatoryQuestionId))
                    };
                }
            });

        It should_not_raise_AnswersDeclaredInvalid_event = () =>
            results.MandatoryQuestionNotDeclaredAsInvalid.ShouldBeTrue();
        
        Cleanup stuff = () =>
        {
            appDomainContext.Dispose();
            appDomainContext = null;
        };

        private static InvokeResults results;
        private static AppDomainContext appDomainContext;

        [Serializable]
        internal class InvokeResults
        {
            public bool MandatoryQuestionNotDeclaredAsInvalid { get; set; }
        }
    }
}