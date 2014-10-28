using System;
using System.Linq;
using AppDomainToolkit;
using Machine.Specifications;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Ncqrs.Spec;
using WB.Core.SharedKernels.DataCollection.Events.Interview;

namespace WB.Tests.Integration.InterviewTests.EnablementAndValidness
{
    internal class when_answering_on_numeric_roster_size_question_which_propagate_roster_with_mandatory_question : InterviewTestsContext
    {
        Establish context = () =>
        {
            appDomainContext = AppDomainContext.Create();
        };

        Because of = () =>
            results = Execute.InStandaloneAppDomain(appDomainContext.Domain, () =>
            {
                Setup.MockedServiceLocator();

                var userId = Guid.Parse("11111111111111111111111111111111");

                var questionnaireId = Guid.Parse("77778888000000000000000000000000");
                var rosterSizeQuestionId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
                var rosterMandatoryQuestionId = Guid.Parse("22222222222222222222222222222222");
                var rosterId = Guid.Parse("DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD");

                var questionnaireDocument = Create.QuestionnaireDocument(questionnaireId,
                    Create.NumericIntegerQuestion(rosterSizeQuestionId),
                    Create.Roster(rosterId, rosterSizeSourceType: RosterSizeSourceType.Question,
                        rosterSizeQuestionId: rosterSizeQuestionId, children: new IComposite[]
                        {
                            Create.Question(rosterMandatoryQuestionId, isMandatory: true)
                        })
                    );

                var interview = SetupInterview(questionnaireDocument);

                using (var eventContext = new EventContext())
                {
                    interview.AnswerNumericIntegerQuestion(userId, rosterSizeQuestionId, new decimal[0], DateTime.Now, 1);

                    return new InvokeResults()
                    {
                        AnswersDeclaredInvalidForMandatoryQuestionInsideRoster =
                            HasEvent<AnswersDeclaredInvalid>(eventContext.Events,
                                where =>
                                    where.Questions.Count(
                                        instance =>
                                            instance.Id == rosterMandatoryQuestionId &&
                                            instance.RosterVector.SequenceEqual(new decimal[] {0})) == 1),
                        AnswersDeclaredValidForMandatoryQuestionInsideRoster =
                            HasEvent<AnswersDeclaredValid>(eventContext.Events,
                                where =>
                                    where.Questions.Count(
                                        instance =>
                                            instance.Id == rosterMandatoryQuestionId &&
                                            instance.RosterVector.SequenceEqual(new decimal[] {0})) == 1)
                    };
                }
            });

        It should_not_raise_AnswersDeclaredValid_event_for_question_inside_roster = () =>
            results.AnswersDeclaredValidForMandatoryQuestionInsideRoster.ShouldBeFalse();

        It should_raise_AnswersDeclaredInvalid_event_for_question_inside_roster = () =>
            results.AnswersDeclaredInvalidForMandatoryQuestionInsideRoster.ShouldBeTrue();

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
            public bool AnswersDeclaredValidForMandatoryQuestionInsideRoster { get; set; }
            public bool AnswersDeclaredInvalidForMandatoryQuestionInsideRoster { get; set; }
        }
    }
}