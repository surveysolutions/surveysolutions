using System;
using System.Linq;
using AppDomainToolkit;
using Machine.Specifications;
using Ncqrs.Spec;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Generated;
using It = Machine.Specifications.It;

namespace WB.Tests.Integration.InterviewTests.LanguageTests.Cases.KP4586
{
    internal class when_condition_uses_two_levels_of_nested_rosters : InterviewTestsContext
    {
        Establish context = () =>
        {
            appDomainContext = AppDomainContext.Create();
            finalQuestion = Guid.Parse("ddc77110-aea0-ded6-0527-86e7177d468e");
        };

        Because of = () =>
           result = Execute.InStandaloneAppDomain(appDomainContext.Domain, () =>
           {
               Setup.MockedServiceLocator();

               var petsAgeQuestionId = Guid.Parse("9fe10ba6-a07a-a779-3097-78e97cd5816b");

               var interview = SetupInterview(
                   Resources.KP4586Questionnaire,
                   new object[]{}, 
                   new InterviewExpressionState_7d59f35ebecf45cb9a890d03c56759bc());

               interview.AnswerNumericIntegerQuestion(Guid.NewGuid(), Guid.Parse("3fdcd883-5ebf-4451-1f6c-7db452fa289f"), Empty.RosterVector, DateTime.Now, 1);
               interview.AnswerNumericIntegerQuestion(Guid.NewGuid(), Guid.Parse("1b8fc07e-70cd-9a2a-9405-ac6fc08ba02e"), new decimal[]{0}, DateTime.Now, 1);

               using (var eventContext = new EventContext())
               {
                   interview.AnswerNumericIntegerQuestion(Guid.NewGuid(), petsAgeQuestionId, new[] { 0m, 0m }, DateTime.Now, 11);

                   return new when_condition_uses_two_levels_of_nested_rosters.InvokeResult
                   {
                       QuestionsEnabledQuestionIds = eventContext.Events.OfType<QuestionsEnabled>()
                                                                        .SelectMany(x => x.Questions)
                                                                        .Select(identity => identity.Id)
                                                                        .ToArray()
                   };
               }
           });

        Cleanup stuff = () =>
        {
            appDomainContext.Dispose();
            appDomainContext = null;
        };

        It should_enable_final_question = () => result.QuestionsEnabledQuestionIds.ShouldContain(finalQuestion);

        static AppDomainContext appDomainContext;
        static InvokeResult result;
        static Guid finalQuestion;

        [Serializable]
        private class InvokeResult
        {
            public Guid[] QuestionsEnabledQuestionIds { get; set; }
        }
    }
}

