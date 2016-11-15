using System;
using System.Collections.Generic;
using AppDomainToolkit;
using Machine.Specifications;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Ncqrs.Spec;
using WB.Core.SharedKernels.DataCollection.Events.Interview;

namespace WB.Tests.Integration.InterviewTests.Rosters
{
    internal class when_decreasing_roster_row_count_and_linked_question_has_reference_on_it : InterviewTestsContext
    {
        Establish context = () =>
        {
            appDomainContext = AppDomainContext.Create();
        };

        Because of = () =>
            results = Execute.InStandaloneAppDomain(appDomainContext.Domain, () =>
            {
                Setup.MockedServiceLocator();

                var userId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");

                var questionnaireId = Guid.Parse("77778888000000000000000000000000");
                var rosterSizeQuestionId = Guid.Parse("EEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEE");
                var sourceQuestionId = Guid.Parse("11111111111111111111111111111111");
                var linkedOutsideId = Guid.Parse("22222222222222222222222222222222");
                var rosterId = Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");

                var questionnaireDocument = Create.QuestionnaireDocumentWithOneChapter(questionnaireId,
                         children: new IComposite[]
                         {
                             Create.NumericIntegerQuestion(rosterSizeQuestionId, variable: "trigger"),
                             Create.Roster(id:rosterId, rosterSizeSourceType:RosterSizeSourceType.Question, rosterSizeQuestionId:rosterSizeQuestionId, variable: "ros",
                             children: new IComposite[]
                             {
                                 Create.TextQuestion(id:sourceQuestionId,variable: "source")
                             }),
                             Create.MultyOptionsQuestion(linkedOutsideId, variable: "linked", linkedToQuestionId: sourceQuestionId)
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

        It should_update_options_for_linked_question_in_roster = () =>
          results.OptionsForLinkedQuestionWasUpdated.ShouldBeTrue();

        Cleanup stuff = () =>
        {
            appDomainContext.Dispose();
            appDomainContext = null;
        };

        private static InvokeResults results;
        private static AppDomainContext<AssemblyTargetLoader, PathBasedAssemblyResolver> appDomainContext;

        [Serializable]
        internal class InvokeResults
        {
            public bool OptionsForLinkedQuestionWasUpdated { get; set; }
        }
    }
}