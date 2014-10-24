using System;
using System.Collections.Generic;
using System.Linq;
using AppDomainToolkit;
using Machine.Specifications;
using Ncqrs.Spec;
using WB.Core.SharedKernels.DataCollection.Events.Interview;

namespace WB.Tests.Integration.InterviewTests.ConditionsChaining
{
    internal class when_question_A_disables_question_B_and_B_disables_group_C : InterviewTestsContext
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
                var questionA = Guid.Parse("11111111111111111111111111111111");
                var questionB = Guid.Parse("22222222222222222222222222222222");
                var questionC = Guid.Parse("33333333333333333333333333333333");

                var groupA = Guid.Parse("C1111111111111111111111111111111");
                var groupC = Guid.Parse("C2222222222222222222222222222222");

                var actorId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");

                var questionnaireDocument = Create.QuestionnaireDocument(questionnaireId,
                    Create.NumericIntegerQuestion(questionA, "questionA"),
                    Create.Group(groupA, "groupA", children: new[]
                    {
                        Create.NumericIntegerQuestion(questionB, "questionB", "questionA > 0")
                    }),
                    Create.Group(groupC, "groupC", "questionA > 0 && questionB == 1", children: new[]
                    {
                        Create.NumericIntegerQuestion(questionC, "questionC", "questionB > 20 && questionA > 0")
                    })
               );

                var interview = SetupInterview(questionnaireDocument, new List<object>()
                {
                    Create.Event.NumericIntegerQuestionAnswered(questionA, 1),
                    Create.Event.QuestionsEnabled(new []
                    {
                        Create.Identity(questionA),
                        Create.Identity(questionB),
                        Create.Identity(questionC)
                    }),
                    Create.Event.GroupsDisabled(Create.Identity(groupC))
                });

                var result = new InvokeResults();

                using (var eventContext = new EventContext())
                {
                    interview.AnswerNumericIntegerQuestion(actorId, questionB, Empty.RosterVector, DateTime.Now, 1);

                    var group2Enabled = GetFirstEventByType<GroupsEnabled>(eventContext.Events).Groups.FirstOrDefault(q => q.Id == groupC) != null;

                    result.Group2Enabled = group2Enabled;
                }

                return result;
            });

        It should_enable_second_group = () =>
            results.Group2Enabled.ShouldBeTrue();

        private static InvokeResults results;
        private static AppDomainContext appDomainContext;

        Cleanup stuff = () =>
        {
            appDomainContext.Dispose();
            appDomainContext = null;
        };

        [Serializable]
        internal class InvokeResults
        {
            public bool Group2Enabled { get; set; }
        } 
    }
}