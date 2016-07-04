﻿using System;
using System.Linq;
using AppDomainToolkit;
using Machine.Specifications;
using Ncqrs.Spec;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.QuestionnaireEntities;
using System.Collections.Generic;

namespace WB.Tests.Integration.InterviewTests.EnablementAndValidness
{
    internal class when_answering_integer_question_on_questionnaire_level_with_multiple_conditions : InterviewTestsContext
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


               var questionnaireDocument = Create.QuestionnaireDocument(questionnaireId,
                   Create.NumericIntegerQuestion(id: petsAgeQuestionId,
                                                                 variable: "pet_age",
                                                                 validationExpression: new List<ValidationCondition>()
                                                                    {
                                                                         new ValidationCondition("pet_age > 10", "pet_age > 10"),
                                                                         new ValidationCondition("pet_age > 20", "pet_age > 20"),
                                                                         new ValidationCondition("pet_age > 30", "pet_age > 30"),
                                                                    }
                                                                 )
                   );

               var interview = SetupInterview(questionnaireDocument);

               using (var eventContext = new EventContext())
               {
                   interview.AnswerNumericIntegerQuestion(userId, petsAgeQuestionId, new decimal[0], DateTime.Now, 11);

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

