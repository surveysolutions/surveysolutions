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
    internal class when_answering_question_that_disables_group_and_roster_inside_it : InterviewTestsContext
    {
        Establish context = () =>
        {
            appDomainContext = AppDomainContext.Create();
        };

        Because of = () =>
            results = Execute.InStandaloneAppDomain(appDomainContext.Domain, () =>
            {
                Setup.MockedServiceLocator();

                var questionnaireDocument = Abc.Create.Entity.QuestionnaireDocumentWithOneChapter(QuestionnaireId,
                    Abc.Create.Entity.NumericIntegerQuestion(q1Id, variable: "q1"),
                    Abc.Create.Entity.Group(groupId, "Group X", null, "q1 == 1", false, new IComposite[]
                    {
                        Abc.Create.Entity.Roster(
                            rosterId: rosterId,
                            rosterSizeSourceType: RosterSizeSourceType.Question,
                            rosterSizeQuestionId: q1Id,
                            variable: "r",
                            children: new IComposite[]
                            {
                                Abc.Create.Entity.NumericIntegerQuestion(q2Id, variable: "q2"),
                            })
                    })
                );

                var result = new InvokeResults();

                var interview = SetupInterview(questionnaireDocument);

                interview.AnswerNumericIntegerQuestion(UserId, q1Id, RosterVector.Empty, DateTime.Now, 1);
                interview.AnswerNumericIntegerQuestion(UserId, q2Id, Create.RosterVector(0), DateTime.Now, 1);

                using (var eventContext = new EventContext())
                {
                    interview.AnswerNumericIntegerQuestion(UserId, q1Id, RosterVector.Empty, DateTime.Now, 2);

                    result.QuestionsQ2Disabled = eventContext.AnyEvent<QuestionsDisabled>(x => x.Questions.Any(q => q.Id == q2Id));
                    result.RosterDisabled = eventContext.AnyEvent<GroupsDisabled>(x => x.Groups.Any(g => g.Id == rosterId));
                }

                return result;
            });

        It should_declare_question_q2_as_disabled = () =>
            results.QuestionsQ2Disabled.ShouldBeTrue();

        It should_declare_roster_as_disabled = () =>
            results.RosterDisabled.ShouldBeTrue();

        Cleanup stuff = () =>
        {
            appDomainContext.Dispose();
            appDomainContext = null;
        };

        private static InvokeResults results;
        private static AppDomainContext<AssemblyTargetLoader, PathBasedAssemblyResolver> appDomainContext;
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
            public bool QuestionsQ2Disabled { get; set; }
            public bool RosterDisabled { get; set; }
        }
    }
}