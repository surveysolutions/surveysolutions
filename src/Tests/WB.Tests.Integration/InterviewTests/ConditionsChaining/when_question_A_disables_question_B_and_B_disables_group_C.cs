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
                var questionnaireId = Guid.Parse("00000000000000000000000000000000");
                var q1 = Guid.Parse("11111111111111111111111111111111");
                var q2 = Guid.Parse("22222222222222222222222222222222");
                var q3 = Guid.Parse("33333333333333333333333333333333");

                var g1 = Guid.Parse("C1111111111111111111111111111111");
                var g2 = Guid.Parse("C2222222222222222222222222222222");

                var actorId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");

                Setup.SetupMockedServiceLocator();

                var questionnaireDocument = Create.QuestionnaireDocument(questionnaireId,
                    Create.NumericIntegerQuestion(q1, "q1"),
                    Create.Group(g1, "g1", null, children: new[]
                    {
                        Create.NumericIntegerQuestion(q2, "q2", "q1 > 0")
                    }),
                    Create.Group(g2, "g2", "q1 > 0 && q2 == 1", children: new[]
                    {
                        Create.NumericIntegerQuestion(q3, "q3", "q2 > 20 && q1 > 0")
                    })
               );

                var interview = SetupInterview(questionnaireDocument, new List<object>() { });

                var result = new InvokeResults();

                interview.AnswerNumericIntegerQuestion(actorId, q1, new decimal[] { }, DateTime.Now, 1);
                interview.Apply(new GroupDisabled(g2, new decimal[]{}));

                using (var eventContext = new EventContext())
                {                    
                    interview.AnswerNumericIntegerQuestion(actorId, q2, new decimal[] { }, DateTime.Now, 1);

                    var group2Enabled = GetFirstEventByType<GroupsEnabled>(eventContext.Events).Groups.FirstOrDefault(q => q.Id == g2) != null;

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