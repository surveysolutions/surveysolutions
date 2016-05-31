using System;
using System.Collections.Generic;
using System.Linq;
using AppDomainToolkit;
using Machine.Specifications;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Ncqrs.Spec;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.QuestionnaireEntities;

namespace WB.Tests.Integration.InterviewTests.Rosters
{
    [Ignore("KP-6853")]
    internal class when_roster_with_condition_and_question_outside_depends_on_rosters_questions : InterviewTestsContext
    {
        Establish context = () =>
        {
            appDomainContext = AppDomainContext.Create();
        };

        Because of = () =>
            results = Execute.InStandaloneAppDomain(appDomainContext.Domain, () =>
            {
                Setup.MockedServiceLocator();

                var questionnaireDocument = Create.QuestionnaireDocument(QuestionnaireId,
                    Create.NumericIntegerQuestion(q1Id, variable: "q1"),
                    Create.Roster(
                            id: rosterId,
                            rosterSizeSourceType: RosterSizeSourceType.Question,
                            rosterSizeQuestionId: q1Id,
                            variable: "r",
                            enablementCondition: "q1 == 1",
                            children: new IComposite[]
                            {
                                Create.NumericIntegerQuestion(q2Id, variable: "q2"),
                            }),
                    Create.NumericIntegerQuestion(q3Id, variable: "q3", validationConditions: new List<ValidationCondition>
                    {
                        Create.ValidationCondition("r.Any(r=>IsAnswered(r.q2))")
                    }));

                var result = new InvokeResults();

                var interview = SetupInterview(questionnaireDocument);

                interview.AnswerNumericIntegerQuestion(UserId, q1Id, RosterVector.Empty, DateTime.Now, 1);
                interview.AnswerNumericIntegerQuestion(UserId, q2Id, Create.RosterVector(0), DateTime.Now, 1);

                using (var eventContext = new EventContext())
                {
                    interview.AnswerNumericIntegerQuestion(UserId, q1Id, RosterVector.Empty, DateTime.Now, 2);

                    result.QuestionQ3BecomeValid = eventContext.AnyEvent<AnswersDeclaredValid>(x => x.Questions.Any(q => q.Id == q3Id));
                }

                return result;
            });

        It should_declare_question_q3_as_valid = () =>
            results.QuestionQ3BecomeValid.ShouldBeTrue();

        Cleanup stuff = () =>
        {
            appDomainContext.Dispose();
            appDomainContext = null;
        };

        private static InvokeResults results;
        private static AppDomainContext appDomainContext;
        private static readonly Guid QuestionnaireId = Guid.Parse("77778888000000000000000000000000");
        private static readonly Guid q1Id = Guid.Parse("11111111111111111111111111111111");
        private static readonly Guid UserId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
        private static readonly Guid q2Id = Guid.Parse("22222222222222222222222222222222");
        private static readonly Guid q3Id = Guid.Parse("33333333333333333333333333333333");
        private static readonly Guid rosterId = Guid.Parse("44444444444444444444444444444444");

        [Serializable]
        internal class InvokeResults
        {
            public bool QuestionQ3BecomeValid { get; set; }
        }
    }

    [Ignore("KP-6853")]
    internal class when_roster_is_inside_group_with_condition : InterviewTestsContext
    {
        Establish context = () =>
        {
            appDomainContext = AppDomainContext.Create();
        };

        Because of = () =>
            results = Execute.InStandaloneAppDomain(appDomainContext.Domain, () =>
            {
                Setup.MockedServiceLocator();

                var questionnaireDocument = Create.QuestionnaireDocument(QuestionnaireId,
                    Create.NumericIntegerQuestion(q1Id, variable: "q1"),
                    Create.Group(groupId, enablementCondition: "q1 == 1", children: new IComposite[]
                    {
                        Create.Roster(
                            id: rosterId,
                            rosterSizeSourceType: RosterSizeSourceType.Question,
                            rosterSizeQuestionId: q1Id,
                            variable: "r",
                            children: new IComposite[]
                            {
                                Create.NumericIntegerQuestion(q2Id, variable: "q2"),
                            })
                    }),
                    Create.NumericIntegerQuestion(q3Id, variable: "q3", validationConditions: new List<ValidationCondition> { Create.ValidationCondition("r.Any(r=>IsAnswered(r.q2))") })
                    );

                var result = new InvokeResults();

                var interview = SetupInterview(questionnaireDocument);

                interview.AnswerNumericIntegerQuestion(UserId, q1Id, RosterVector.Empty, DateTime.Now, 1);
                interview.AnswerNumericIntegerQuestion(UserId, q2Id, Create.RosterVector(0), DateTime.Now, 1);

                using (var eventContext = new EventContext())
                {
                    interview.AnswerNumericIntegerQuestion(UserId, q1Id, RosterVector.Empty, DateTime.Now, 2);

                    result.QuestionQ3BecomeValid = eventContext.AnyEvent<AnswersDeclaredValid>(x => x.Questions.Any(q => q.Id == q3Id));
                }

                return result;
            });

        It should_declare_question_q3_as_valid = () =>
            results.QuestionQ3BecomeValid.ShouldBeTrue();

        Cleanup stuff = () =>
        {
            appDomainContext.Dispose();
            appDomainContext = null;
        };

        private static InvokeResults results;
        private static AppDomainContext appDomainContext;
        private static readonly Guid QuestionnaireId = Guid.Parse("77778888000000000000000000000000");
        private static readonly Guid q1Id = Guid.Parse("11111111111111111111111111111111");
        private static readonly Guid UserId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
        private static readonly Guid q2Id = Guid.Parse("22222222222222222222222222222222");
        private static readonly Guid q3Id = Guid.Parse("33333333333333333333333333333333");
        private static readonly Guid rosterId = Guid.Parse("44444444444444444444444444444444");
        private static readonly Guid groupId = Guid.Parse("99999999999999999999999999999999");

        [Serializable]
        internal class InvokeResults
        {
            public bool QuestionQ3BecomeValid { get; set; }
        }
    }
}