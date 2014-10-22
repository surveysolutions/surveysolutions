using System;
using System.Collections.Generic;
using System.Linq;
using AppDomainToolkit;
using Machine.Specifications;
using Main.Core.Entities.Composite;
using Ncqrs.Spec;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Events.Interview.Dtos;

namespace WB.Tests.Integration.InterviewTests.EnablementAndValidness
{
    internal class when_answering_integer_question_A_and_that_answer_disables_group_GB_with_question_B_that_disables_group_GC : InterviewTestsContext
    {
        Establish context = () =>
        {
            appDomainContext = AppDomainContext.Create();
        };

        Because of = () =>
            results = Execute.InStandaloneAppDomain(appDomainContext.Domain, () =>
            {
                var emptyRosterVector = new decimal[] { };
                var userId = Guid.Parse("11111111111111111111111111111111");

                var questionnaireId = Guid.Parse("77778888000000000000000000000000");
                var questionAId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
                var questionBId = Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");
                var groupGBId = Guid.Parse("DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD");
                var groupGCId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");

                Setup.SetupMockedServiceLocator();

                var questionnaireDocument = Create.QuestionnaireDocument(questionnaireId,
                    Create.NumericIntegerQuestion(questionAId, "a"),
                    Create.Group(groupGBId, enablementCondition: "a > 0", children: new IComposite[] {
                        Create.NumericIntegerQuestion(questionBId, "b")
                    }),
                    Create.Group(groupGCId, enablementCondition: "b > 0")
                );

                var interview = SetupInterview(questionnaireDocument, new List<object>
                {
                    new QuestionsEnabled(new[]{ new Identity(questionAId, emptyRosterVector), new Identity(questionBId, emptyRosterVector)}),
                    new GroupsEnabled(new [] { new Identity(groupGBId, emptyRosterVector), new Identity(groupGCId, emptyRosterVector) }),
                    new NumericIntegerQuestionAnswered(userId, questionBId, emptyRosterVector, DateTime.Now, 1)
                });

                using (var eventContext = new EventContext())
                {
                    interview.AnswerNumericIntegerQuestion(userId, questionAId, emptyRosterVector, DateTime.Now, 0);

                    return new InvokeResults()
                    {
                        GroupGBDisabled = GetFirstEventByType<GroupsDisabled>(eventContext.Events).Groups.FirstOrDefault(g => g.Id == groupGBId) != null,
                        GroupGCDisabled = GetFirstEventByType<GroupsDisabled>(eventContext.Events).Groups.FirstOrDefault(g => g.Id == groupGCId) != null
                    };
                }
            });

        It should_disable_question_B = () =>
            results.GroupGBDisabled.ShouldBeTrue();

        It should_disable_question_C = () =>
            results.GroupGCDisabled.ShouldBeTrue();

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
            public bool GroupGBDisabled { get; set; }
            public bool GroupGCDisabled { get; set; }
        }
    }
}