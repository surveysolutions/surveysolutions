using System;
using System.Collections.Generic;
using System.Linq;
using AppDomainToolkit;
using Ncqrs.Spec;
using NUnit.Framework;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.QuestionnaireEntities;
using WB.Tests.Abc;

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

                var interview = SetupInterviewWithExpressionStorage(questionnaireDocument, new List<object>());

                using (var eventContext = new EventContext())
                {
                    interview.AnswerNumericIntegerQuestion(userId, questionId, RosterVector.Empty, DateTime.Now, 1);

                    return new
                    {
                        AnswersDeclaredInvalidEvent = GetFirstEventByType<AnswersDeclaredInvalid>(eventContext.Events)
                    };
                }
            });

            Assert.That(results, Is.Not.Null);
            Assert.That(results.AnswersDeclaredInvalidEvent, Is.Not.Null);
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

            Assert.That(results, Is.Not.Null);
            Assert.That(results.AnswersDeclaredInvalidEvent, Is.Not.Null);
            Assert.That(results.AnswersDeclaredInvalidEvent.Questions.Single().Id, Is.EqualTo(questionAgeId));
            Assert.That(results.AnswersDeclaredInvalidEvent.FailedValidationConditions.Keys.Single().Id, Is.EqualTo(questionAgeId));
            Assert.That(results.AnswersDeclaredInvalidEvent.FailedValidationConditions.Values.Single().Single().FailedConditionIndex, Is.EqualTo(1));

            appDomainContext.Dispose();
            appDomainContext = null;
        }

        [Test]
        public void when_answering_integer_question_as_roster_triger_should_execute_condition_for_new_element_inside_roster()
        {
            var userId = Guid.Parse("11111111111111111111111111111111");

            var questionnaireId = Guid.Parse("77778888000000000000000000000000");
            var questionId      = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
            var rosterId        = Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");
            var textQuestionId  = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");

            var appDomainContext = AppDomainContext.Create();

            var results = Execute.InStandaloneAppDomain(appDomainContext.Domain, () =>
            {
                Setup.MockedServiceLocator();

                var questionnaireDocument = Abc.Create.Entity.QuestionnaireDocumentWithOneChapter(questionnaireId,
                    Abc.Create.Entity.NumericIntegerQuestion(questionId, variable: "i"),
                    Abc.Create.Entity.Roster(rosterId, variable: "r", rosterSizeQuestionId: questionId, children: new []
                    {
                        Abc.Create.Entity.TextQuestion(textQuestionId, "i != 5")
                    })
                );

                var interview = SetupInterviewWithExpressionStorage(questionnaireDocument, new List<object>());

                using (var eventContext = new EventContext())
                {
                    interview.AnswerNumericIntegerQuestion(userId, questionId, RosterVector.Empty, DateTime.Now, 5);

                    return new
                    {
                        DisabledEvent = GetFirstEventByType<QuestionsDisabled>(eventContext.Events)
                    };
                }
            });

            Assert.That(results, Is.Not.Null);
            Assert.That(results.DisabledEvent, Is.Not.Null);
            Assert.That(results.DisabledEvent.Questions.Length, Is.EqualTo(5));
            Assert.That(results.DisabledEvent.Questions.Select(e => e.Id).ToArray(), Is.EqualTo(new[] { textQuestionId, textQuestionId , textQuestionId , textQuestionId , textQuestionId }));


            appDomainContext.Dispose();
            appDomainContext = null;
        }


        [Test]
        [Ignore("Fixed on server side")]
        public void when_complete_previously_rejected_interview_with_re_answered_text_list_question_as_roster_triger_should_publish_disabled_questions_event_by_questions_inside_new_roster_instance()
        {
            var userId = Guid.Parse("11111111111111111111111111111111");


            var questionnaireId = Guid.Parse("77778888000000000000000000000000");
            var numericId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
            var textListId = Guid.Parse("EEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEE");
            var rosterId = Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");
            var textQuestionId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");

            var appDomainContext = AppDomainContext.Create();

            var results = Execute.InStandaloneAppDomain(appDomainContext.Domain, () =>
            {
                Setup.MockedServiceLocator();

                var questionnaireDocument = Abc.Create.Entity.QuestionnaireDocumentWithOneChapter(questionnaireId,
                    Abc.Create.Entity.NumericIntegerQuestion(numericId, variable: "i"),
                    Abc.Create.Entity.TextListQuestion(textListId),
                    Abc.Create.Entity.Roster(rosterId, variable: "r", rosterSizeQuestionId: textListId, children: new[]
                    {
                        Abc.Create.Entity.TextQuestion(textQuestionId, "i != 5")
                    })
                );

                var interview = SetupStatefullInterview(questionnaireDocument, new List<object>());

                interview.Synchronize(Create.Command.Synchronize(userId,
                    Create.Entity.InterviewSynchronizationDto(questionnaireId, 1, userId, null, new[]
                    {
                        Create.Entity.AnsweredQuestionSynchronizationDto(numericId, RosterVector.Empty, 5),
                        Create.Entity.AnsweredQuestionSynchronizationDto(textListId, RosterVector.Empty,
                            new[] {new Tuple<decimal, string>(1, "1"), new Tuple<decimal, string>(2, "2")})
                    })));

                interview.AnswerTextListQuestion(userId, textListId, RosterVector.Empty, DateTime.UtcNow,
                    new[] { new Tuple<decimal, string>(1, "1") });

                interview.AnswerTextListQuestion(userId, textListId, RosterVector.Empty, DateTime.UtcNow,
                    new[] { new Tuple<decimal, string>(1, "1"), new Tuple<decimal, string>(2, "2") });

                using (var eventContext = new EventContext())
                {
                    
                    interview.Complete(userId, "complete", DateTime.UtcNow);

                    return new
                    {
                        DisabledEvent = GetFirstEventByType<QuestionsDisabled>(eventContext.Events)
                    };
                }
            });

            Assert.That(results, Is.Not.Null);
            Assert.That(results.DisabledEvent, Is.Not.Null);
            Assert.That(results.DisabledEvent.Questions.Length, Is.EqualTo(1));
            Assert.That(results.DisabledEvent.Questions.Select(e => e.Id).ToArray(), Is.EquivalentTo(new[] { textQuestionId }));


            appDomainContext.Dispose();
            appDomainContext = null;
        }

        [Test]
        [Ignore("Fixed on server side")]
        public void when_complete_previously_rejected_interview_with_re_answered_numeric_question_as_roster_triger_after_reject_should_publish_disabled_questions_event_by_questions_inside_new_roster_instance()
        {
            var userId = Guid.Parse("11111111111111111111111111111111");

            var questionnaireId = Guid.Parse("77778888000000000000000000000000");
            var numericId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
            var rosterSourceId = Guid.Parse("EEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEE");
            var rosterId = Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");
            var textQuestionId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");

            var appDomainContext = AppDomainContext.Create();

            var results = Execute.InStandaloneAppDomain(appDomainContext.Domain, () =>
            {
                Setup.MockedServiceLocator();

                var questionnaireDocument = Abc.Create.Entity.QuestionnaireDocumentWithOneChapter(questionnaireId,
                    Abc.Create.Entity.NumericIntegerQuestion(numericId, variable: "i"),
                    Abc.Create.Entity.NumericIntegerQuestion(rosterSourceId),
                    Abc.Create.Entity.Roster(rosterId, variable: "r", rosterSizeQuestionId: rosterSourceId, children: new[]
                    {
                        Abc.Create.Entity.TextQuestion(textQuestionId, "i != 5")
                    })
                );

                var interview = SetupStatefullInterview(questionnaireDocument, new List<object>());

                interview.Synchronize(Create.Command.Synchronize(userId,
                    Create.Entity.InterviewSynchronizationDto(questionnaireId, 1, userId, null, new[]
                    {
                        Create.Entity.AnsweredQuestionSynchronizationDto(numericId, RosterVector.Empty, 5),
                        Create.Entity.AnsweredQuestionSynchronizationDto(rosterSourceId, RosterVector.Empty, 2)
                    })));

                interview.AnswerNumericIntegerQuestion(userId, rosterSourceId, RosterVector.Empty, DateTime.UtcNow, 1);
                interview.AnswerNumericIntegerQuestion(userId, rosterSourceId, RosterVector.Empty, DateTime.UtcNow, 2);

                using (var eventContext = new EventContext())
                {

                    interview.Complete(userId, "complete", DateTime.UtcNow);

                    return new
                    {
                        DisabledEvent = GetFirstEventByType<QuestionsDisabled>(eventContext.Events)
                    };
                }
            });

            Assert.That(results, Is.Not.Null);
            Assert.That(results.DisabledEvent, Is.Not.Null);
            Assert.That(results.DisabledEvent.Questions.Length, Is.EqualTo(1));
            Assert.That(results.DisabledEvent.Questions.Select(e => e.Id).ToArray(), Is.EquivalentTo(new[] { textQuestionId }));


            appDomainContext.Dispose();
            appDomainContext = null;
        }
    }
}
