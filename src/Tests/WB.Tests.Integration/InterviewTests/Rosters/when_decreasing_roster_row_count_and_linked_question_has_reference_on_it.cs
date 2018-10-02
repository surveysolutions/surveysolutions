using System;
using System.Collections.Generic;
using AppDomainToolkit;
using FluentAssertions;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Ncqrs.Spec;
using WB.Core.SharedKernels.DataCollection.Events.Interview;

namespace WB.Tests.Integration.InterviewTests.Rosters
{
    internal class when_decreasing_roster_row_count_and_linked_question_has_reference_on_it : InterviewTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            appDomainContext = AppDomainContext.Create();
            BecauseOf();
        }

        public void BecauseOf() =>
            results = Execute.InStandaloneAppDomain(appDomainContext.Domain, () =>
            {
                SetUp.MockedServiceLocator();

                var userId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");

                var questionnaireId = Guid.Parse("77778888000000000000000000000000");
                var rosterSizeQuestionId = Guid.Parse("EEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEE");
                var sourceQuestionId = Guid.Parse("11111111111111111111111111111111");
                var linkedOutsideId = Guid.Parse("22222222222222222222222222222222");
                var rosterId = Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");

                var questionnaireDocument = Abc.Create.Entity.QuestionnaireDocumentWithOneChapter(questionnaireId,
                         children: new IComposite[]
                         {
                             Abc.Create.Entity.NumericIntegerQuestion(rosterSizeQuestionId, variable: "trigger"),
                             Abc.Create.Entity.Roster(rosterId:rosterId, rosterSizeSourceType:RosterSizeSourceType.Question, rosterSizeQuestionId:rosterSizeQuestionId, variable: "ros",
                             children: new IComposite[]
                             {
                                 Abc.Create.Entity.TextQuestion(questionId: sourceQuestionId, variable: "source")
                             }),
                             Abc.Create.Entity.MultyOptionsQuestion(linkedOutsideId, variable: "linked", linkedToQuestionId: sourceQuestionId)
                         }
                    );

                var result = new InvokeResults();

                var interview = SetupInterview(questionnaireDocument, new List<object>() { });
                interview.AnswerNumericIntegerQuestion(userId, rosterSizeQuestionId, new decimal[0],DateTime.Now, 3);

                interview.AnswerTextQuestion(userId, sourceQuestionId, new decimal[] {0}, DateTime.Now, "a");
                interview.AnswerTextQuestion(userId, sourceQuestionId, new decimal[] { 1 }, DateTime.Now, "b");
                interview.AnswerTextQuestion(userId, sourceQuestionId, new decimal[] { 2 }, DateTime.Now, "c");

                using (var eventContext = new EventContext())
                {
                    interview.AnswerNumericIntegerQuestion(userId, rosterSizeQuestionId, new decimal[0], DateTime.Now, 2);
                    result.OptionsForLinkedQuestionWasUpdated = HasEvent<LinkedOptionsChanged>(eventContext.Events);
                }

                return result;
            });

        [NUnit.Framework.Test] public void should_update_options_for_linked_question_in_roster () =>
          results.OptionsForLinkedQuestionWasUpdated.Should().BeTrue();

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
            public bool OptionsForLinkedQuestionWasUpdated { get; set; }
        }
    }
}
