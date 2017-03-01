using System;
using System.Linq;
using AppDomainToolkit;
using Machine.Specifications;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using Ncqrs.Spec;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Events.Interview;

namespace WB.Tests.Integration.InterviewTests.EnablementAndValidness
{
    internal class when_condition_uses_two_levels_of_nested_rosters : InterviewTestsContext
    {
        Establish context = () =>
        {
            appDomainContext = AppDomainContext.Create();
        };

        Because of = () =>
           result = Execute.InStandaloneAppDomain(appDomainContext.Domain, () =>
           {
               Setup.MockedServiceLocator();

               var userId = Guid.NewGuid();

               var questionnaireId = Guid.Parse("11111111111111111111111111111111");
               var numericQuestionId = Guid.Parse("22222222222222222222222222222222");
               var textQuestionId = Guid.Parse("33333333333333333333333333333333");
               var petsQuestionId = Guid.Parse("44444444444444444444444444444444");
               var familyRosterId = Guid.Parse("55555555555555555555555555555555");
               var petsRosterId = Guid.Parse("66666666666666666666666666666666");
               var petsAgeQuestionId = Guid.Parse("77777777777777777777777777777777");
               var finalQuestionId = Guid.Parse("88888888888888888888888888888888");


               var questionnaireDocument = Abc.Create.Entity.QuestionnaireDocumentWithOneChapter(questionnaireId,
                   Abc.Create.Entity.NumericIntegerQuestion(numericQuestionId, variable: "num"),
                   Abc.Create.Entity.Roster(familyRosterId, variable: "fam",
                       rosterSizeSourceType: RosterSizeSourceType.Question,
                       rosterSizeQuestionId: numericQuestionId,
                       rosterTitleQuestionId: textQuestionId,
                       children: new IComposite[]
                       {
                           Abc.Create.Entity.TextQuestion(questionId: textQuestionId, variable: "title"),
                           Abc.Create.Entity.NumericIntegerQuestion(petsQuestionId, variable: "pet"),
                           Abc.Create.Entity.Roster(petsRosterId, variable: "frnd",
                               rosterSizeSourceType: RosterSizeSourceType.Question,
                               rosterSizeQuestionId: petsQuestionId,
                               rosterTitleQuestionId: textQuestionId,
                               children: new IComposite[]
                               {
                                   Abc.Create.Entity.NumericIntegerQuestion(petsAgeQuestionId, variable: "pet_age")
                               })
                       }),
                   Abc.Create.Entity.NumericIntegerQuestion(finalQuestionId, variable: "fin", enablementCondition: "fam.Sum(y => y.frnd.Sum(z => z.pet_age)) > 10"));


               var interview = SetupInterview(questionnaireDocument);

               interview.AnswerNumericIntegerQuestion(userId, numericQuestionId, RosterVector.Empty, DateTime.Now, 1);
               interview.AnswerNumericIntegerQuestion(userId, petsQuestionId, new decimal[] { 0 }, DateTime.Now, 2);

               using (var eventContext = new EventContext())
               {
                   interview.AnswerNumericIntegerQuestion(userId, petsAgeQuestionId, new[] { 0m, 0m }, DateTime.Now, 11);

                   return new InvokeResult
                   {
                       FinalQuestionWasEnabled = eventContext.AnyEvent<QuestionsEnabled>(x => x.Questions.Any(g => g.Id == finalQuestionId))
                   };
               }
           });

        Cleanup stuff = () =>
        {
            appDomainContext.Dispose();
            appDomainContext = null;
        };

        It should_enable_final_question = () =>
            result.FinalQuestionWasEnabled.ShouldBeTrue();

        static AppDomainContext<AssemblyTargetLoader, PathBasedAssemblyResolver> appDomainContext;
        static InvokeResult result;

        [Serializable]
        private class InvokeResult
        {
            public bool FinalQuestionWasEnabled { get; set; }
        }
    }
}

