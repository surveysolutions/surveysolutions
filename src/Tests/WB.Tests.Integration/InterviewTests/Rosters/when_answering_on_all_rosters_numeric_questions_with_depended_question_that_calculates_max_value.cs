using System;
using System.Collections.Generic;
using System.Linq;
using AppDomainToolkit;
using Machine.Specifications;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Ncqrs.Spec;
using WB.Core.SharedKernels.DataCollection.Events.Interview;

namespace WB.Tests.Integration.InterviewTests.Rosters
{
    internal class when_answering_on_all_rosters_numeric_questions_with_depended_question_that_calculates_max_value : InterviewTestsContext
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
                var rosterAgeQuestionId = Guid.Parse("11111111111111111111111111111111");
                var rosterValidation = Guid.Parse("22222222222222222222222222222222");
                var rosterId = Guid.Parse("DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD");

                var questionnaireDocument = Create.QuestionnaireDocument(questionnaireId,
                    Create.NumericIntegerQuestion(id: rosterSizeQuestionId),

                    Create.Roster(
                        id: rosterId, 
                        rosterSizeSourceType: RosterSizeSourceType.Question,
                        rosterSizeQuestionId: rosterSizeQuestionId,
                        variable: "varRoster",
                        children: new IComposite[]
                        {
                            Create.NumericIntegerQuestion(rosterAgeQuestionId, variable: "age")
                        }),

                    Create.NumericIntegerQuestion(
                        id: rosterValidation,
                        enablementCondition: "varRoster.Select(x => x.age).Max() > 65")
                );

                var result = new InvokeResults();

                var interview = SetupInterview(questionnaireDocument, new List<object>() { });

                using (var eventContext = new EventContext())
                {                    
                    interview.AnswerNumericIntegerQuestion(userId, rosterSizeQuestionId, Empty.RosterVector, DateTime.Now, 2);

                    interview.AnswerNumericIntegerQuestion(userId, rosterAgeQuestionId, new decimal[1] { 0 }, DateTime.Now, 17);
                    interview.AnswerNumericIntegerQuestion(userId, rosterAgeQuestionId, new decimal[1] { 1 }, DateTime.Now, 66);

                    result.RosterValidationQuestionEnabled = GetFirstEventByType<QuestionsEnabled>(eventContext.Events).Questions.Any(q => q.Id == rosterValidation);
                }

                return result;
            });

        It should_enable_question = () =>
            results.RosterValidationQuestionEnabled.ShouldBeTrue();

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
            public bool RosterValidationQuestionEnabled { get; set; }
        }          
    }
}