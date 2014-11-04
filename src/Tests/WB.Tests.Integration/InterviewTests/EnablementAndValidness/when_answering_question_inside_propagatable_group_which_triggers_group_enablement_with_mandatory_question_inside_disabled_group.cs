using System;
using System.Linq;
using AppDomainToolkit;
using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using Ncqrs.Spec;
using WB.Core.SharedKernels.DataCollection.Events.Interview;

namespace WB.Tests.Integration.InterviewTests.EnablementAndValidness
{
    internal class when_answering_question_inside_propagatable_group_which_triggers_group_enablement_with_mandatory_question_inside_disabled_group : InterviewTestsContext
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
                var disabledNestedGroupMandtoryQuestionId = Guid.Parse("FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF");
                var nestedGroupRealQuestionId = Guid.Parse("22222222222222222222222222222222");
                var rosterId = Guid.Parse("DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD");
                var nestedGroupId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
                var nestedDisabledGroupId = Guid.Parse("EEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEE");

                var questionnaireDocument = Create.QuestionnaireDocument(questionnaireId,
                    Create.NumericIntegerQuestion(rosterSizeQuestionId, "a"),
                    Create.Roster(rosterId, rosterSizeSourceType: RosterSizeSourceType.Question,
                        rosterSizeQuestionId: rosterSizeQuestionId, children: new[]
                        {
                            Create.Group(nestedGroupId, children: new[]
                            {
                                Create.NumericRealQuestion(nestedGroupRealQuestionId, "b")
                            }),
                            Create.Group(nestedDisabledGroupId, enablementCondition: "b > 0", children: new[]
                            {
                                Create.Question(disabledNestedGroupMandtoryQuestionId, isMandatory: true)
                            }),
                        })
                    );

                var interview = SetupInterview(questionnaireDocument);
                interview.AnswerNumericIntegerQuestion(userId, rosterSizeQuestionId, new decimal[] {}, DateTime.Now, 1);

                using (var eventContext = new EventContext())
                {
                    interview.AnswerNumericRealQuestion(userId, nestedGroupRealQuestionId, new decimal[] {0},
                        DateTime.Now, 1);

                    return new InvokeResults()
                    {
                        MandtoryQuestionInvalid =
                            HasEvent<AnswersDeclaredInvalid>(eventContext.Events,
                                where =>
                                    where.Questions.Any(
                                        question =>
                                            question.Id == disabledNestedGroupMandtoryQuestionId &&
                                            question.RosterVector.Length == 1 && question.RosterVector[0] == 0)),
                        MandtoryQuestionValid = HasEvent<AnswersDeclaredValid>(eventContext.Events, where =>
                            where.Questions.Any(
                                question =>
                                    question.Id == disabledNestedGroupMandtoryQuestionId &&
                                    question.RosterVector.Length == 1 && question.RosterVector[0] == 0))
                    };
                }
            });
        
        It should_be_mandatory_question_invalid = () =>
            results.MandtoryQuestionInvalid.ShouldBeTrue();

        It should_not_be_mandatory_question_valid = () =>
            results.MandtoryQuestionValid.ShouldBeFalse();

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
            public bool MandtoryQuestionInvalid { get; set; }
            public bool MandtoryQuestionValid { get; set; }
        }
    }
}