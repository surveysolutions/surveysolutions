using System;
using System.Collections.Generic;
using System.Linq;
using AppDomainToolkit;
using Machine.Specifications;
using Main.Core.Entities.Composite;
using Ncqrs.Spec;
using NUnit.Framework;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.QuestionnaireEntities;

namespace WB.Tests.Integration.InterviewTests
{
    [TestFixture]
    public class EnablementAndValidnessTests: InterviewTestsContext
    {
        [Test]
        public void when_answering_integer_question_with_dependency_on_one_variable()
        {
            var userId = Guid.Parse("11111111111111111111111111111111");

            var questionnaireId = Guid.Parse("77778888000000000000000000000000");
            var questionId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
            var variableId = Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");

            var appDomainContext = AppDomainContext.Create();

            var results = Execute.InStandaloneAppDomain(appDomainContext.Domain, () =>
            {
                Setup.MockedServiceLocator();

                var questionnaireDocument = Abc.Create.Entity.QuestionnaireDocumentWithOneChapter(questionnaireId,
                    Abc.Create.Entity.NumericIntegerQuestion(questionId, variable: "i", validationConditions: new []
                    {
                        new ValidationCondition("v == 5", "v == 5"), 
                    }),
                    Abc.Create.Entity.Variable(variableId, VariableType.LongInteger, "v", "i")
                );

                var interview = SetupInterviewWithExpressionStorage(questionnaireDocument, new List<object>
                {
                    Abc.Create.Event.NumericIntegerQuestionAnswered(
                        questionId: questionId, answer: 1
                    )
                });

                using (var eventContext = new EventContext())
                {
                    interview.AnswerNumericIntegerQuestion(userId, questionId, RosterVector.Empty, DateTime.Now, 1);

                    return new
                    {
                        AnswersDeclaredInvalidEvent = GetFirstEventByType<AnswersDeclaredInvalid>(eventContext.Events)
                    };
                }
            });

            Assert.That(results.AnswersDeclaredInvalidEvent.Questions.Single().Id, Is.EqualTo(questionId));
            Assert.That(results.AnswersDeclaredInvalidEvent.FailedValidationConditions.Keys.Single().Id, Is.EqualTo(questionId));
            Assert.That(results.AnswersDeclaredInvalidEvent.FailedValidationConditions.Values.Single().Single().FailedConditionIndex, Is.EqualTo(0));


            appDomainContext.Dispose();
            appDomainContext = null;
        }


        [Test]
        public void when_answering_integer_question_with_dependency_on_many_variables()
        {
            var userId = Guid.Parse("11111111111111111111111111111111");

            var questionnaireId  = Guid.Parse("22222222222222222222222222222222");
            var questionFirstId  = Guid.Parse("55555555555555555555555555555555");
            var questionSecondId = Guid.Parse("66666666666666666666666666666666");
            var questionAgeId    = Guid.Parse("77777777777777777777777777777777");
            var variableIsOldId       = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
            var variableIsYoungId     = Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");
            var variableMsgOldId      = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
            var variableMsgYoungOldId = Guid.Parse("DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD");

            var appDomainContext = AppDomainContext.Create();

            var results = Execute.InStandaloneAppDomain(appDomainContext.Domain, () =>
            {
                Setup.MockedServiceLocator();

                var questionnaireDocument = Abc.Create.Entity.QuestionnaireDocumentWithOneChapter(questionnaireId,
                    Abc.Create.Entity.TextQuestion(questionFirstId, variable: "name"),                   
                    Abc.Create.Entity.TextQuestion(questionSecondId, variable: "s_name"),                   
                    Abc.Create.Entity.NumericIntegerQuestion(questionAgeId, variable: "age", validationConditions: new []
                    {
                        new ValidationCondition("!(bool)IsYoung", "%msgYoung%"),
                        new ValidationCondition("!(bool)IsOld", "%msgOld%"), 
                    }),
                    Abc.Create.Entity.Variable(variableIsOldId, VariableType.Boolean, "IsOld", "age>100"),
                    Abc.Create.Entity.Variable(variableIsYoungId, VariableType.Boolean, "IsYoung", "age<1"),
                    Abc.Create.Entity.Variable(variableMsgOldId, VariableType.String, "msgOld", "s_name + name + \" is too old\""),
                    Abc.Create.Entity.Variable(variableMsgYoungOldId, VariableType.String, "msgYoung", "s_name + name + \" is too young\"")
                );

                var interview = SetupInterviewWithExpressionStorage(questionnaireDocument, new List<object>
                {
                    Abc.Create.Event.TextQuestionAnswered(
                        questionId: questionFirstId, answer: "John"
                    ),
                    Abc.Create.Event.TextQuestionAnswered(
                        questionId: questionSecondId, answer: "Smith"
                    )
                });

                using (var eventContext = new EventContext())
                {
                    interview.AnswerNumericIntegerQuestion(userId, questionAgeId, RosterVector.Empty, DateTime.Now, 1000);

                    return new
                    {
                        AnswersDeclaredInvalidEvent = GetFirstEventByType<AnswersDeclaredInvalid>(eventContext.Events)
                    };
                }
            });

            Assert.That(results.AnswersDeclaredInvalidEvent.Questions.Single().Id, Is.EqualTo(questionAgeId));
            Assert.That(results.AnswersDeclaredInvalidEvent.FailedValidationConditions.Keys.Single().Id, Is.EqualTo(questionAgeId));
            Assert.That(results.AnswersDeclaredInvalidEvent.FailedValidationConditions.Values.Single().Single().FailedConditionIndex, Is.EqualTo(1));

            appDomainContext.Dispose();
            appDomainContext = null;
        }
    }
}