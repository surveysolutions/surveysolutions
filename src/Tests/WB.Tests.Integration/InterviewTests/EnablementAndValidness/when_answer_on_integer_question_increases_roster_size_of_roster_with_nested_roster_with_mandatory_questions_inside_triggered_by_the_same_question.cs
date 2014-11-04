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
    internal class when_answer_on_integer_question_increases_roster_size_of_roster_with_nested_roster_with_mandatory_questions_inside_triggered_by_the_same_question : InterviewTestsContext
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
                var nestedRosterMandatoryQuestionId = Guid.Parse("33333333333333333333333333333333");
                var rosterId = Guid.Parse("DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD");
                var nestedRosterId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");

                var questionnaireDocument = Create.QuestionnaireDocument(questionnaireId,
                    Create.NumericIntegerQuestion(rosterSizeQuestionId),
                    Create.Roster(rosterId, rosterSizeSourceType: RosterSizeSourceType.Question,
                        rosterSizeQuestionId: rosterSizeQuestionId, children: new IComposite[]
                        {
                            Create.Question(rosterMandatoryQuestionId, isMandatory: true),
                            Create.Roster(nestedRosterId, rosterSizeSourceType: RosterSizeSourceType.Question,
                                rosterSizeQuestionId: rosterSizeQuestionId, children: new[]
                                {
                                    Create.Question(nestedRosterMandatoryQuestionId, isMandatory: true)
                                })
                        })
                    );

                var interview = SetupInterview(questionnaireDocument);

                using (var eventContext = new EventContext())
                {
                    interview.AnswerNumericIntegerQuestion(userId, rosterSizeQuestionId, new decimal[0], DateTime.Now, 1);

                    return new InvokeResults()
                    {
                        NestedRosterInstanceRemoved =
                            HasEvent<RosterInstancesRemoved>(eventContext.Events,
                                where => where.Instances.Any(instance => instance.GroupId == nestedRosterId)),
                        RosterAndNestedRosterInstancesAdded =
                            HasEvent<RosterInstancesAdded>(eventContext.Events, where
                                =>
                                where.Instances.Count(
                                    instance =>
                                        instance.GroupId == rosterId && instance.RosterInstanceId == 0 &&
                                        instance.OuterRosterVector.Length == 0) == 1
                                &&
                                where.Instances.Count(
                                    instance =>
                                        instance.GroupId == nestedRosterId && instance.RosterInstanceId == 0 &&
                                        instance.OuterRosterVector.SequenceEqual(new decimal[] {0})) == 1),
                        AnswersDeclaredInvalidForMandatoryQuestionsInsideRosters =
                            HasEvent<AnswersDeclaredInvalid>(eventContext.Events,
                                @where =>
                                    where.Questions.Count(
                                        instance =>
                                            instance.Id == rosterMandatoryQuestionId &&
                                            instance.RosterVector.SequenceEqual(new decimal[] {0})) == 1 &&
                                    where.Questions.Count(
                                        instance =>
                                            instance.Id == nestedRosterMandatoryQuestionId &&
                                            instance.RosterVector.SequenceEqual(new decimal[] {0, 0})) == 1)
                    };
                }
            });

        It should_raise_RosterInstancesAdded_event_for_roster_and_nested_roster = () =>
            results.RosterAndNestedRosterInstancesAdded.ShouldBeTrue();

        It should_raise_AnswersDeclaredInvalid_event_for_questions_inside_roster_and_nested_roster = () =>
            results.AnswersDeclaredInvalidForMandatoryQuestionsInsideRosters.ShouldBeTrue();

        It should_not_raise_RosterInstancesRemoved_event_for_nested_roster = () =>
            results.NestedRosterInstanceRemoved.ShouldBeFalse();

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
            public bool RosterAndNestedRosterInstancesAdded { get; set; }
            public bool NestedRosterInstanceRemoved { get; set; }
            public bool AnswersDeclaredInvalidForMandatoryQuestionsInsideRosters { get; set; }
        }
    }
}