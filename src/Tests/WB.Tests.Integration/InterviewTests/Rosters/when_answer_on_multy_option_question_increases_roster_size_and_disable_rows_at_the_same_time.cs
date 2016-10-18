using System;
using System.Collections.Generic;
using AppDomainToolkit;
using Machine.Specifications;
using Main.Core.Entities.Composite;
using Ncqrs.Spec;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using It = Machine.Specifications.It;

namespace WB.Tests.Integration.InterviewTests.Rosters
{
    internal class when_answer_on_multy_option_question_increases_roster_size_and_disable_rows_at_the_same_time : InterviewTestsContext
    {
        Establish context = () =>
        {
            appDomainContext = AppDomainContext.Create();
        };

        Because of = () =>
           results = Execute.InStandaloneAppDomain(appDomainContext.Domain, () =>
           {
               var questionnaireId = Guid.Parse("10000000000000000000000000000000");
               userId = Guid.Parse("FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF");

               numericQuestionInsideRoster = Guid.Parse("33333333333333333333333333333333");
               rosterGroupId = Guid.Parse("11111111111111111111111111111111");
               multyOptionRosterSizeId = Guid.Parse("22222222222222222222222222222222");

               var questionnaireDocument = Create.QuestionnaireDocumentWithOneChapter(questionnaireId,
                   Create.MultyOptionsQuestion(id: multyOptionRosterSizeId, variable:"q1", options: new[] { Create.Option("1"), Create.Option("2"), Create.Option("3") }),
                   Create.Roster(id: rosterGroupId, enablementCondition: "!q1.Contains(2)", rosterSizeQuestionId: multyOptionRosterSizeId, children: new IComposite[]
                   {
                       Create.NumericIntegerQuestion(id: numericQuestionInsideRoster),
                   })
               );

               var result = new InvokeResults();
               var interview = SetupInterview(questionnaireDocument, new List<object>());

               interview.AnswerMultipleOptionsQuestion(userId, multyOptionRosterSizeId, new decimal[0], DateTime.Now, new decimal[] { 1 });
               interview.AnswerMultipleOptionsQuestion(userId, multyOptionRosterSizeId, new decimal[0], DateTime.Now, new decimal[] { 1, 2 });
               interview.AnswerMultipleOptionsQuestion(userId, multyOptionRosterSizeId, new decimal[0], DateTime.Now, new decimal[] { 2 });
               interview.AnswerMultipleOptionsQuestion(userId, multyOptionRosterSizeId, new decimal[0], DateTime.Now, new decimal[0]);
               interview.AnswerMultipleOptionsQuestion(userId, multyOptionRosterSizeId, new decimal[0], DateTime.Now, new decimal[] { 1 });

               using (var eventContext = new EventContext())
               {
                   interview.AnswerNumericIntegerQuestion(userId, numericQuestionInsideRoster, new decimal[] { 1 }, DateTime.Now, 18);

                   result.WasIntegerQuestionAnswered = eventContext.AnyEvent<NumericIntegerQuestionAnswered>(x => x.Answer == 18 && x.QuestionId == numericQuestionInsideRoster);
               }

               return result;
           });

        Cleanup stuff = () =>
        {
            appDomainContext.Dispose();
            appDomainContext = null;
        };

        It should_raise_NumericIntegerQuestionAnswered_event = () =>
            results.WasIntegerQuestionAnswered.ShouldBeTrue();

        private static Guid userId;
        private static Guid multyOptionRosterSizeId;
        private static Guid numericQuestionInsideRoster;
        private static Guid rosterGroupId;

        private static InvokeResults results;
        private static AppDomainContext<AssemblyTargetLoader, PathBasedAssemblyResolver> appDomainContext;

        [Serializable]
        internal class InvokeResults
        {
            public bool WasIntegerQuestionAnswered;
        }
    }
}
