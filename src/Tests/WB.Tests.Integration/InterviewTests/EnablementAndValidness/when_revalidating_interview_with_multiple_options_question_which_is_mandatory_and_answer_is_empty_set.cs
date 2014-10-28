using System;
using System.Linq;
using AppDomainToolkit;
using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using Ncqrs.Spec;
using WB.Core.SharedKernels.DataCollection.Events.Interview;

namespace WB.Tests.Integration.InterviewTests.EnablementAndValidness
{
    internal class when_revalidating_interview_with_multiple_options_question_which_is_mandatory_and_answer_is_empty_set : InterviewTestsContext
    {
        Establish context = () =>
        {
            appDomainContext = AppDomainContext.Create();
        };

        Because of = () =>
            results = Execute.InStandaloneAppDomain(appDomainContext.Domain, () =>
            {
                Setup.MockedServiceLocator();

                var questionnaireId = Guid.Parse("77778888000000000000000000000000");
                var categoricalMultiOptionMandatoryQuestionId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");

                var questionnaireDocument = Create.QuestionnaireDocument(questionnaireId,
                    Create.MultyOptionsQuestion(categoricalMultiOptionMandatoryQuestionId, isMandatory: true,
                        answers:
                            new []
                            {
                                new Answer() {AnswerValue = "1", AnswerText = "title 1"},
                                new Answer() {AnswerValue = "2", AnswerText = "title 2"},
                                new Answer() {AnswerValue = "3", AnswerText = "title 3"}
                            })
                    );

                var interview = SetupInterview(questionnaireDocument, new[]
                {
                    Create.Event.MultipleOptionsQuestionAnswered(categoricalMultiOptionMandatoryQuestionId,
                        selectedValues: new[] {1.0m, 3.0m}),
                    Create.Event.MultipleOptionsQuestionAnswered(categoricalMultiOptionMandatoryQuestionId,
                        selectedValues: new decimal[0])
                });
                

                using (var eventContext = new EventContext())
                {
                    interview.ReevaluateSynchronizedInterview();

                    return new InvokeResults()
                    {
                        MandtoryQuestionNotDeclaredAsInvalid =
                            !HasEvent<AnswersDeclaredInvalid>(eventContext.Events,
                                where =>
                                    where.Questions.Any(
                                        question => question.Id == categoricalMultiOptionMandatoryQuestionId))
                    };
                }
            });

        It should_not_raise_AnswersDeclaredInvalid_event = () =>
            results.MandtoryQuestionNotDeclaredAsInvalid.ShouldBeTrue();

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
            public bool MandtoryQuestionNotDeclaredAsInvalid { get; set; }
        }
    }
}