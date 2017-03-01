﻿using System;
using System.Linq;
using AppDomainToolkit;
using Machine.Specifications;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Ncqrs.Spec;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.QuestionnaireEntities;
using System.Collections.Generic;

namespace WB.Tests.Integration.InterviewTests.EnablementAndValidness
{
    internal class when_answering_integer_question_on_2nd_level_of_nested_roster_with_multiple_conditions : InterviewTestsContext
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


               var questionnaireDocument = Create.QuestionnaireDocumentWithOneChapter(questionnaireId,
                   Abc.Create.Entity.NumericIntegerQuestion(numericQuestionId, variable: "num"),
                   Abc.Create.Entity.Roster(familyRosterId, variable: "fam",
                       rosterSizeSourceType: RosterSizeSourceType.Question,
                       rosterSizeQuestionId: numericQuestionId,
                       rosterTitleQuestionId: textQuestionId,
                       children: new IComposite[]
                       {
                           Create.TextQuestion(textQuestionId, variable: "title"),
                           Abc.Create.Entity.NumericIntegerQuestion(petsQuestionId, variable: "pet"),
                           Abc.Create.Entity.Roster(petsRosterId, variable: "frnd",
                               rosterSizeSourceType: RosterSizeSourceType.Question,
                               rosterSizeQuestionId: petsQuestionId,
                               rosterTitleQuestionId: textQuestionId,
                               children: new IComposite[]
                               {
                                   Abc.Create.Entity.NumericIntegerQuestion(id: petsAgeQuestionId, 
                                                                 variable: "pet_age",
                                                                 validationConditions: new List<ValidationCondition>()
                                                                    {
                                                                         new ValidationCondition("pet_age > 10", "pet_age > 10"),
                                                                         new ValidationCondition("pet_age > 20", "pet_age > 20"),
                                                                         new ValidationCondition("pet_age > 30", "pet_age > 30"),
                                                                    }
                                                                 )
                               })
                       })
                   );

               var interview = SetupInterview(questionnaireDocument);

               interview.AnswerNumericIntegerQuestion(userId, numericQuestionId, Empty.RosterVector, DateTime.Now, 1);
               interview.AnswerNumericIntegerQuestion(userId, petsQuestionId, new decimal[] { 0 }, DateTime.Now, 2);

               using (var eventContext = new EventContext())
               {
                   interview.AnswerNumericIntegerQuestion(userId, petsAgeQuestionId, new[] { 0m, 0m }, DateTime.Now, 11);

                   return new InvokeResult
                   {
                       CountInvalidQuestions = eventContext.Count<AnswersDeclaredInvalid>(),
                       CountInvalidValidations = eventContext.GetSingleEvent<AnswersDeclaredInvalid>().FailedValidationConditions.Values.First().Count()
                   };
               }
           });

        Cleanup stuff = () =>
        {
            appDomainContext.Dispose();
            appDomainContext = null;
        };

        It should_invalid_question_count_equal_1 = () =>
            result.CountInvalidQuestions.ShouldEqual(1);

        It should_invalid_validation_count_equal_2 = () =>
            result.CountInvalidValidations.ShouldEqual(2);

        static AppDomainContext<AssemblyTargetLoader, PathBasedAssemblyResolver> appDomainContext;
        static InvokeResult result;

        [Serializable]
        private class InvokeResult
        {
            public int CountInvalidQuestions { get; set; }
            public int CountInvalidValidations { get; set; }

        }
    }
}

