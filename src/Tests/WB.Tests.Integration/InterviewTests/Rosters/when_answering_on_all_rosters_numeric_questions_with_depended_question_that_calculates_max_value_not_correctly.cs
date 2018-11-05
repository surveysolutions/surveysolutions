using System;
using System.Collections.Generic;
using AppDomainToolkit;
using FluentAssertions;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Ncqrs.Spec;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Events.Interview;

namespace WB.Tests.Integration.InterviewTests.Rosters
{
    internal class when_answering_on_all_rosters_numeric_questions_with_depended_question_that_calculates_max_value_not_correctly : InterviewTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            appDomainContext = AppDomainContext.Create();
            BecauseOf();
        }

        public void BecauseOf() =>
            results = Execute.InStandaloneAppDomain(appDomainContext.Domain, () =>
            {
                SetUp.MockedServiceLocator();

                var userId = Guid.Parse("11111111111111111111111111111111");

                var questionnaireId = Guid.Parse("77778888000000000000000000000000");
                var rosterSizeQuestionId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
                var rosterAgeQuestionId = Guid.Parse("11111111111111111111111111111111");
                var rosterValidation = Guid.Parse("22222222222222222222222222222222");
                var rosterId = Guid.Parse("DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD");

                var questionnaireDocument = Abc.Create.Entity.QuestionnaireDocumentWithOneChapter(questionnaireId,
                    Abc.Create.Entity.NumericIntegerQuestion(id: rosterSizeQuestionId),

                    Abc.Create.Entity.Roster(
                        rosterId: rosterId, 
                        rosterSizeSourceType: RosterSizeSourceType.Question,
                        rosterSizeQuestionId: rosterSizeQuestionId,
                        variable: "varRoster",
                        children: new IComposite[]
                        {
                            Abc.Create.Entity.NumericIntegerQuestion(rosterAgeQuestionId, variable: "age")
                        }),

                    Abc.Create.Entity.NumericIntegerQuestion(
                        id: rosterValidation,
                        enablementCondition: "varRoster.Select(x => x.age).Max() > 65",
                        variable: null)
                );

                var result = new InvokeResults();

                var interview = SetupInterview(questionnaireDocument, new List<object>() { });

                using (var eventContext = new EventContext())
                {                    
                    interview.AnswerNumericIntegerQuestion(userId, rosterSizeQuestionId, RosterVector.Empty, DateTime.Now, 2);

                    interview.AnswerNumericIntegerQuestion(userId, rosterAgeQuestionId, new decimal[1] { 0 }, DateTime.Now, 24);
                    interview.AnswerNumericIntegerQuestion(userId, rosterAgeQuestionId, new decimal[1] { 1 }, DateTime.Now, 25);

                    result.RosterValidationQuestionEnabled = HasEvent<QuestionsEnabled>(eventContext.Events);
                }

                return result;
            });

        [NUnit.Framework.Test] public void should_keep_question_disabled () =>
            results.RosterValidationQuestionEnabled.Should().BeFalse();

        [NUnit.Framework.OneTimeTearDown] public void CleanUp()
        {
            appDomainContext.Dispose();
            appDomainContext = null;
        }

        private static InvokeResults results;
        private static AppDomainContext<AssemblyTargetLoader, PathBasedAssemblyResolver> appDomainContext;

        [Serializable]
        internal class InvokeResults
        {
            public bool RosterValidationQuestionEnabled { get; set; }
        }          
    }
}
