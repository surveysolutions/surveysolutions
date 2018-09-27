using System;
using System.Collections.Generic;
using System.Linq;
using AppDomainToolkit;
using Main.Core.Entities.Composite;
using Ncqrs.Spec;
using NUnit.Framework;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.QuestionnaireEntities;
using WB.Tests.Abc;

namespace WB.Tests.Integration.InterviewTests
{
    [TestFixture]
    public class EnablementAndValidnessTests : InterviewTestsContext
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
                SetUp.MockedServiceLocator();

                var questionnaireDocument = Abc.Create.Entity.QuestionnaireDocumentWithOneChapter(questionnaireId,
                    Abc.Create.Entity.NumericIntegerQuestion(questionId, variable: "i", validationConditions: new[]
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
            Assert.That(results.AnswersDeclaredInvalidEvent.FailedValidationConditions.Keys.Single().Id,
                Is.EqualTo(questionId));
            Assert.That(
                results.AnswersDeclaredInvalidEvent.FailedValidationConditions.Values.Single().Single()
                    .FailedConditionIndex, Is.EqualTo(0));


            appDomainContext.Dispose();
            appDomainContext = null;
        }


        [Test]
        public void when_answering_integer_question_with_dependency_on_many_variables()
        {
            var userId = Guid.Parse("11111111111111111111111111111111");

            var questionnaireId = Guid.Parse("22222222222222222222222222222222");
            var questionFirstId = Guid.Parse("55555555555555555555555555555555");
            var questionSecondId = Guid.Parse("66666666666666666666666666666666");
            var questionAgeId = Guid.Parse("77777777777777777777777777777777");
            var variableIsOldId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
            var variableIsYoungId = Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");
            var variableMsgOldId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
            var variableMsgYoungOldId = Guid.Parse("DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD");

            var appDomainContext = AppDomainContext.Create();

            var results = Execute.InStandaloneAppDomain(appDomainContext.Domain, () =>
            {
                SetUp.MockedServiceLocator();

                var questionnaireDocument = Abc.Create.Entity.QuestionnaireDocumentWithOneChapter(questionnaireId,
                    Abc.Create.Entity.TextQuestion(questionFirstId, variable: "name"),
                    Abc.Create.Entity.TextQuestion(questionSecondId, variable: "s_name"),
                    Abc.Create.Entity.NumericIntegerQuestion(questionAgeId, variable: "age", validationConditions: new[]
                    {
                        new ValidationCondition("!(bool)IsYoung", "%msgYoung%"),
                        new ValidationCondition("!(bool)IsOld", "%msgOld%"),
                    }),
                    Abc.Create.Entity.Variable(variableIsOldId, VariableType.Boolean, "IsOld", "age>100"),
                    Abc.Create.Entity.Variable(variableIsYoungId, VariableType.Boolean, "IsYoung", "age<1"),
                    Abc.Create.Entity.Variable(variableMsgOldId, VariableType.String, "msgOld",
                        "s_name + name + \" is too old\""),
                    Abc.Create.Entity.Variable(variableMsgYoungOldId, VariableType.String, "msgYoung",
                        "s_name + name + \" is too young\"")
                );

                var interview = SetupStatefullInterviewWithExpressionStorage(questionnaireDocument);
                interview.AnswerTextQuestion(userId, questionFirstId, RosterVector.Empty, DateTime.UtcNow, "John");
                interview.AnswerTextQuestion(userId, questionSecondId, RosterVector.Empty, DateTime.UtcNow, "Smith");

                using (var eventContext = new EventContext())
                {
                    interview.AnswerNumericIntegerQuestion(userId, questionAgeId, RosterVector.Empty, DateTime.Now,
                        1000);

                    return new
                    {
                        AnswersDeclaredInvalidEvent = GetFirstEventByType<AnswersDeclaredInvalid>(eventContext.Events),
                        ValidationErrorMessage =
                        interview.GetFailedValidationMessages(Abc.Create.Identity(questionAgeId), null)
                    };
                }
            });

            Assert.That(results, Is.Not.Null);
            Assert.That(results.AnswersDeclaredInvalidEvent, Is.Not.Null);
            Assert.That(results.AnswersDeclaredInvalidEvent.Questions.Single().Id, Is.EqualTo(questionAgeId));
            Assert.That(results.AnswersDeclaredInvalidEvent.FailedValidationConditions.Keys.Single().Id,
                Is.EqualTo(questionAgeId));
            Assert.That(
                results.AnswersDeclaredInvalidEvent.FailedValidationConditions.Values.Single().Single()
                    .FailedConditionIndex, Is.EqualTo(1));

            Assert.That(results.ValidationErrorMessage.Single(), Is.EqualTo("SmithJohn is too old [2]"));

            appDomainContext.Dispose();
            appDomainContext = null;
        }

        [Test]
        public void
            when_answering_integer_question_as_roster_triger_should_execute_condition_for_new_element_inside_roster()
        {
            var userId = Guid.Parse("11111111111111111111111111111111");

            var questionnaireId = Guid.Parse("77778888000000000000000000000000");
            var questionId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
            var rosterId = Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");
            var textQuestionId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");

            var appDomainContext = AppDomainContext.Create();

            var results = Execute.InStandaloneAppDomain(appDomainContext.Domain, () =>
            {
                SetUp.MockedServiceLocator();

                var questionnaireDocument = Abc.Create.Entity.QuestionnaireDocumentWithOneChapter(questionnaireId,
                    Abc.Create.Entity.NumericIntegerQuestion(questionId, variable: "i"),
                    Abc.Create.Entity.Roster(rosterId, variable: "r", rosterSizeQuestionId: questionId, children: new[]
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
            Assert.That(results.DisabledEvent.Questions.Select(e => e.Id).ToArray(),
                Is.EqualTo(new[] {textQuestionId, textQuestionId, textQuestionId, textQuestionId, textQuestionId}));


            appDomainContext.Dispose();
            appDomainContext = null;
        }

        [Test]
        public void when_answering_integer_question_should_execute_roster_enablement_condition()
        {
            var userId = Guid.Parse("11111111111111111111111111111111");

            var questionnaireId = Guid.Parse("77778888000000000000000000000000");
            var questionId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
            var rosterId = Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");
            var textQuestionId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");

            var appDomainContext = AppDomainContext.Create();

            var results = Execute.InStandaloneAppDomain(appDomainContext.Domain, () =>
            {
                SetUp.MockedServiceLocator();

                var questionnaireDocument = Abc.Create.Entity.QuestionnaireDocumentWithOneChapter(questionnaireId,
                    Abc.Create.Entity.NumericIntegerQuestion(questionId, variable: "i"),
                    Abc.Create.Entity.Roster(rosterId, variable: "r", rosterSizeQuestionId: questionId,
                        enablementCondition: "i != 5", children: new[]
                        {
                            Abc.Create.Entity.TextQuestion(textQuestionId)
                        })
                );

                var interview = SetupInterviewWithExpressionStorage(questionnaireDocument, new List<object>());

                using (var eventContext = new EventContext())
                {
                    interview.AnswerNumericIntegerQuestion(userId, questionId, RosterVector.Empty, DateTime.Now, 5);

                    return new
                    {
                        DisabledQuestionEvent = GetFirstEventByType<QuestionsDisabled>(eventContext.Events)
                    };
                }
            });

            Assert.That(results, Is.Not.Null);
            Assert.That(results.DisabledQuestionEvent, Is.Not.Null);
            Assert.That(results.DisabledQuestionEvent.Questions.Length, Is.EqualTo(5));
            Assert.That(results.DisabledQuestionEvent.Questions.Select(e => e.Id).ToArray(),
                Is.EqualTo(new[] {textQuestionId, textQuestionId, textQuestionId, textQuestionId, textQuestionId}));


            appDomainContext.Dispose();
            appDomainContext = null;
        }

        [Test]
        public void when_answering_single_question_should_filter_options_in_other_question()
        {
            var userId = Guid.Parse("11111111111111111111111111111111");

            var questionnaireId = Guid.Parse("77778888000000000000000000000000");
            var intQuestionId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
            var singleQuestionId = Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");
            var singleWithFilterQuestionId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
            var groupId = Guid.Parse("99999999999999999999999999999999");

            var appDomainContext = AppDomainContext.Create();

            var results = Execute.InStandaloneAppDomain(appDomainContext.Domain, () =>
            {
                SetUp.MockedServiceLocator();

                var questionnaireDocument = Abc.Create.Entity.QuestionnaireDocumentWithOneChapter(questionnaireId,
                    Abc.Create.Entity.SingleOptionQuestion(singleQuestionId, variable: "sq",
                        answerCodes: new decimal[] {1, 2, 3}),
                    Abc.Create.Entity.NumericIntegerQuestion(intQuestionId, variable: "i"),
                    Abc.Create.Entity.Group(groupId, enablementCondition: "i == 5", children: new[]
                    {
                        Abc.Create.Entity.SingleOptionQuestion(singleWithFilterQuestionId, variable: "sf",
                            answerCodes: new decimal[] {1, 2, 3},
                            optionsFilterExpression:
                            "sq.InList(2, 3) && @optioncode!=1 || !sq.InList(2, 3) && @optioncode >= 1")
                    })
                );

                var interview = SetupInterviewWithExpressionStorage(questionnaireDocument, new List<object>());

                using (var eventContext = new EventContext())
                {
                    interview.AnswerSingleOptionQuestion(userId, singleQuestionId, RosterVector.Empty, DateTime.Now, 2);

                    return new
                    {
                        EnabledQuestionEvent = GetFirstEventByType<QuestionsEnabled>(eventContext.Events)
                    };
                }
            });

            Assert.That(results, Is.Not.Null);
            Assert.That(results.EnabledQuestionEvent, Is.Null);

            appDomainContext.Dispose();
            appDomainContext = null;
        }

        [Test]
        public void when_answering_list_question_with_trigger_roster()
        {
            var userId = Guid.Parse("11111111111111111111111111111111");

            var questionnaireId = Guid.Parse("77778888000000000000000000000000");
            var textListQuestionId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
            var singleQuestionId = Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");
            var singleEnabledForSectionId = Guid.Parse("EEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEE");
            var singleInRosterQuestionId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
            var multiInRosterQuestionId = Guid.Parse("DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD");
            var rosterId = Guid.Parse("99999999999999999999999999999999");
            var sectionId = Guid.Parse("88888888888888888888888888888888");

            var appDomainContext = AppDomainContext.Create();

            var results = Execute.InStandaloneAppDomain(appDomainContext.Domain, () =>
            {
                SetUp.MockedServiceLocator();

                var questionnaireDocument = Abc.Create.Entity.QuestionnaireDocumentWithOneChapter(questionnaireId,
                    Abc.Create.Entity.TextListQuestion(textListQuestionId, variable: "tl"),
                    Abc.Create.Entity.SingleOptionQuestion(singleEnabledForSectionId, variable: "sos",
                        answerCodes: new decimal[] {1, 2}),
                    Abc.Create.Entity.Group(sectionId, enablementCondition: "sos == 1", children: new IComposite[]
                    {
                        Abc.Create.Entity.SingleOptionQuestion(singleQuestionId, variable: "so",
                            answerCodes: new decimal[] {1, 2}),
                        Abc.Create.Entity.ListRoster(rosterId, rosterSizeQuestionId: textListQuestionId,
                            enablementCondition: "so == 1", children: new IComposite[]
                            {
                                Abc.Create.Entity.SingleOptionQuestion(singleInRosterQuestionId, variable: "sor",
                                    answerCodes: new decimal[] {1, 2},
                                    title: "DID %rostertitle% OBTAIN"),
                                Abc.Create.Entity.MultipleOptionsQuestion(multiInRosterQuestionId,
                                    enablementCondition: "so == 1", answers: new int[] {1, 2, 3})
                            })
                    })
                );

                var interview = SetupInterviewWithExpressionStorage(questionnaireDocument, new List<object>());

                using (var eventContext = new EventContext())
                {
                    interview.AnswerTextListQuestion(userId, textListQuestionId, RosterVector.Empty, DateTime.Now,
                        new[] {new Tuple<decimal, string>(1, "name"),});

                    return new
                    {
                        DisabledGroupsEvent = GetFirstEventByType<GroupsDisabled>(eventContext.Events),
                        DisabledQuestionsEvent = GetFirstEventByType<QuestionsDisabled>(eventContext.Events)
                    };
                }
            });

            Assert.That(results, Is.Not.Null);
            Assert.That(results.DisabledQuestionsEvent, Is.Not.Null);
            Assert.That(results.DisabledQuestionsEvent.Questions, Is.EqualTo(new[]
            {
                Abc.Create.Identity(singleInRosterQuestionId, new int[] {1}),
                Abc.Create.Identity(multiInRosterQuestionId, new int[] {1}),
            }));

            Assert.That(results.DisabledGroupsEvent, Is.Not.Null);
            Assert.That(results.DisabledGroupsEvent.Groups, Is.EqualTo(new[]
            {
                Abc.Create.Identity(rosterId, new int[] {1}),
            }));

            appDomainContext.Dispose();
            appDomainContext = null;
        }

        [Test]
        public void when_answering_list_question_with_trigger_roster2()
        {
            var userId = Guid.Parse("11111111111111111111111111111111");

            var questionnaireId = Guid.Parse("77778888000000000000000000000000");
            var textListQuestionId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
            var singleEnabledForSectionId = Guid.Parse("EEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEE");
            var singleInRosterQuestionId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
            var rosterId = Guid.Parse("99999999999999999999999999999999");
            var sectionId = Guid.Parse("88888888888888888888888888888888");

            var appDomainContext = AppDomainContext.Create();

            var results = Execute.InStandaloneAppDomain(appDomainContext.Domain, () =>
            {
                SetUp.MockedServiceLocator();

                var questionnaireDocument = Abc.Create.Entity.QuestionnaireDocumentWithOneChapter(questionnaireId,
                    Abc.Create.Entity.TextListQuestion(textListQuestionId, variable: "tl"),
                    Abc.Create.Entity.SingleOptionQuestion(singleEnabledForSectionId, variable: "sos",
                        answerCodes: new decimal[] {1, 2}),
                    Abc.Create.Entity.Group(sectionId, enablementCondition: "sos == 1", children: new IComposite[]
                    {
                        Abc.Create.Entity.ListRoster(rosterId, rosterSizeQuestionId: textListQuestionId,
                            children: new IComposite[]
                            {
                                Abc.Create.Entity.SingleOptionQuestion(singleInRosterQuestionId, variable: "sor",
                                    answerCodes: new decimal[] {1, 2},
                                    title: "DID %rostertitle% OBTAIN")
                            })
                    })
                );

                var interview = SetupInterviewWithExpressionStorage(questionnaireDocument, new List<object>());

                using (var eventContext = new EventContext())
                {
                    interview.AnswerTextListQuestion(userId, textListQuestionId, RosterVector.Empty, DateTime.Now,
                        new[] {new Tuple<decimal, string>(1, "name"),});

                    return new
                    {
                        DisabledGroupsEvent = GetFirstEventByType<GroupsDisabled>(eventContext.Events),
                        DisabledQuestionsEvent = GetFirstEventByType<QuestionsDisabled>(eventContext.Events)
                    };
                }
            });

            Assert.That(results, Is.Not.Null);
            Assert.That(results.DisabledQuestionsEvent, Is.Not.Null);
            Assert.That(results.DisabledQuestionsEvent.Questions, Is.EqualTo(new[]
            {
                Abc.Create.Identity(singleInRosterQuestionId, new int[] {1}),
            }));

            Assert.That(results.DisabledGroupsEvent, Is.Not.Null);
            Assert.That(results.DisabledGroupsEvent.Groups, Is.EqualTo(new[]
            {
                Abc.Create.Identity(rosterId, new int[] {1}),
            }));

            appDomainContext.Dispose();
            appDomainContext = null;
        }

        [Test]
        public void when_answering_datetime_question_with_in_roster()
        {
            var userId = Guid.Parse("11111111111111111111111111111111");

            var questionnaireId = Guid.Parse("77778888000000000000000000000000");
            var textListQuestionId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
            var singleEnablementQuestionId = Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");
            var singleQuestion1Id = Guid.Parse("55555555555555555555555555555555");
            var singleQuestion2Id = Guid.Parse("44444444444444444444444444444444");
            var intQuestionId = Guid.Parse("FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF");
            var roster1Id = Guid.Parse("99999999999999999999999999999999");
            var roster2Id = Guid.Parse("88888888888888888888888888888888");
            var section1Id = Guid.Parse("77777777777777777777777777777777");
            var section2Id = Guid.Parse("66666666666666666666666666666666");

            var appDomainContext = AppDomainContext.Create();

            var results = Execute.InStandaloneAppDomain(appDomainContext.Domain, () =>
            {
                SetUp.MockedServiceLocator();

                var questionnaireDocument = Abc.Create.Entity.QuestionnaireDocumentWithOneChapter(questionnaireId,
                    Abc.Create.Entity.SingleOptionQuestion(singleEnablementQuestionId, variable: "soe",
                        answerCodes: new decimal[] {1, 2}),
                    Abc.Create.Entity.Group(section1Id, enablementCondition: "soe == 1", children: new IComposite[]
                    {
                        Abc.Create.Entity.TextListQuestion(textListQuestionId, variable: "tl"),
                        Abc.Create.Entity.ListRoster(roster1Id, variable: "lr1",
                            rosterSizeQuestionId: textListQuestionId, children: new IComposite[]
                            {
                                Abc.Create.Entity.NumericIntegerQuestion(intQuestionId, variable: "inn"),
                                Abc.Create.Entity.SingleOptionQuestion(singleQuestion1Id, variable: "so",
                                    enablementCondition: "inn > 4", answerCodes: new decimal[] {1, 2}),
                            }),
                    }),
                    Abc.Create.Entity.Group(section2Id, enablementCondition: "soe == 1", children: new IComposite[]
                    {
                        Abc.Create.Entity.ListRoster(roster2Id, variable: "lr2", enablementCondition: "so == 1",
                            rosterSizeQuestionId: textListQuestionId, children: new IComposite[]
                            {
                                Abc.Create.Entity.SingleOptionQuestion(singleQuestion2Id, variable: "so2",
                                    answerCodes: new decimal[] {1, 2}),
                            })
                    })
                );

                var interview = SetupInterviewWithExpressionStorage(questionnaireDocument, new List<object>());
                interview.AnswerSingleOptionQuestion(userId, singleEnablementQuestionId, RosterVector.Empty,
                    DateTime.Now, 1);
                interview.AnswerTextListQuestion(userId, textListQuestionId, RosterVector.Empty, DateTime.Now,
                    new[] {new Tuple<decimal, string>(1, "name"),});
                interview.AnswerNumericIntegerQuestion(userId, intQuestionId, Abc.Create.RosterVector(1), DateTime.Now,
                    5);
                interview.AnswerSingleOptionQuestion(userId, singleQuestion1Id, Abc.Create.RosterVector(1),
                    DateTime.Now, 1);
                interview.AnswerSingleOptionQuestion(userId, singleQuestion2Id, Abc.Create.RosterVector(1),
                    DateTime.Now, 1);
                interview.AnswerSingleOptionQuestion(userId, singleEnablementQuestionId, RosterVector.Empty,
                    DateTime.Now, 2);

                using (var eventContext = new EventContext())
                {
                    interview.AnswerSingleOptionQuestion(userId, singleEnablementQuestionId, RosterVector.Empty,
                        DateTime.Now, 1);

                    return new
                    {
                        GroupsEnabledEvent = GetFirstEventByType<GroupsEnabled>(eventContext.Events),
                        QuestionsEnabledEvent = GetFirstEventByType<QuestionsEnabled>(eventContext.Events)
                    };
                }
            });

            Assert.That(results, Is.Not.Null);
            Assert.That(results.QuestionsEnabledEvent, Is.Not.Null);
//            Assert.That(results.QuestionsEnabledEvent.Questions, Is.EqualTo(new[]
//            {
//                Abc.Create.Identity(singleInRosterQuestionId, new int[] { 1 }),
//                Abc.Create.Identity(multiInRosterQuestionId, new int[] { 1 }),
//            }));

            Assert.That(results.GroupsEnabledEvent, Is.Not.Null);
            Assert.That(results.GroupsEnabledEvent.Groups.Any(i => i == Abc.Create.Identity(roster2Id, new int[] {1})),
                Is.True);

            appDomainContext.Dispose();
            appDomainContext = null;
        }


        [Test]
        [Ignore("Fixed on server side")]
        public void
            when_complete_previously_rejected_interview_with_re_answered_text_list_question_as_roster_triger_should_publish_disabled_questions_event_by_questions_inside_new_roster_instance()
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
                SetUp.MockedServiceLocator();

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
                    new[] {new Tuple<decimal, string>(1, "1")});

                interview.AnswerTextListQuestion(userId, textListId, RosterVector.Empty, DateTime.UtcNow,
                    new[] {new Tuple<decimal, string>(1, "1"), new Tuple<decimal, string>(2, "2")});

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
            Assert.That(results.DisabledEvent.Questions.Select(e => e.Id).ToArray(),
                Is.EquivalentTo(new[] {textQuestionId}));


            appDomainContext.Dispose();
            appDomainContext = null;
        }

        [Test]
        [Ignore("Fixed on server side")]
        public void
            when_complete_previously_rejected_interview_with_re_answered_numeric_question_as_roster_triger_after_reject_should_publish_disabled_questions_event_by_questions_inside_new_roster_instance()
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
                SetUp.MockedServiceLocator();

                var questionnaireDocument = Abc.Create.Entity.QuestionnaireDocumentWithOneChapter(questionnaireId,
                    Abc.Create.Entity.NumericIntegerQuestion(numericId, variable: "i"),
                    Abc.Create.Entity.NumericIntegerQuestion(rosterSourceId),
                    Abc.Create.Entity.Roster(rosterId, variable: "r", rosterSizeQuestionId: rosterSourceId,
                        children: new[]
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
            Assert.That(results.DisabledEvent.Questions.Select(e => e.Id).ToArray(),
                Is.EquivalentTo(new[] {textQuestionId}));


            appDomainContext.Dispose();
            appDomainContext = null;
        }


        [Test]
        public void when_enable_section_with_roster_but_roster_triget_is_disabled()
        {
            var userId = Guid.Parse("11111111111111111111111111111111");

            var questionnaireId = Guid.Parse("77778888000000000000000000000000");
            var numeric1Id = Guid.Parse("11111111111111111111111111111111");
            var numeric2Id = Guid.Parse("22222222222222222222222222222222");
            var rosterSourceId = Guid.Parse("EEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEE");
            var rosterId = Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");
            var groupId = Guid.Parse("DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD");
            var textQuestionId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");

            var appDomainContext = AppDomainContext.Create();

            var results = Execute.InStandaloneAppDomain(appDomainContext.Domain, () =>
            {
                SetUp.MockedServiceLocator();

                var questionnaireDocument = Abc.Create.Entity.QuestionnaireDocumentWithOneChapter(questionnaireId,
                    Create.Entity.NumericIntegerQuestion(numeric1Id, variable: "i"),
                    Create.Entity.NumericIntegerQuestion(numeric2Id, variable: "i2"),
                    Create.Entity.NumericIntegerQuestion(rosterSourceId, enablementCondition: "i2 == 1"),
                    Create.Entity.Group(groupId, enablementCondition: "i == 1", children: new []{
                        Create.Entity.Roster(rosterId, variable: "r", rosterSizeQuestionId: rosterSourceId,
                            children: new[]
                            {
                                Abc.Create.Entity.TextQuestion(textQuestionId)
                            })
                        }
                    )
                );

                var interview = SetupStatefullInterview(questionnaireDocument);

                interview.AnswerNumericIntegerQuestion(userId, numeric1Id, RosterVector.Empty, DateTime.UtcNow, 1);
                interview.AnswerNumericIntegerQuestion(userId, numeric2Id, RosterVector.Empty, DateTime.UtcNow, 1);
                interview.AnswerNumericIntegerQuestion(userId, rosterSourceId, RosterVector.Empty, DateTime.UtcNow, 1);
                interview.AnswerTextQuestion(userId, textQuestionId, new RosterVector(new[] { 0 }), DateTime.UtcNow, "1");
                interview.AnswerNumericIntegerQuestion(userId, numeric1Id, RosterVector.Empty, DateTime.UtcNow, 2);
                interview.AnswerNumericIntegerQuestion(userId, numeric2Id, RosterVector.Empty, DateTime.UtcNow, 2);

                using (var eventContext = new EventContext())
                {
                    interview.AnswerNumericIntegerQuestion(userId, numeric1Id, RosterVector.Empty, DateTime.UtcNow, 1);

                    return new
                    {
                        GroupsEnabledEvent = GetFirstEventByType<GroupsEnabled>(eventContext.Events)
                    };
                }
            });

            Assert.That(results, Is.Not.Null);
            Assert.That(results.GroupsEnabledEvent, Is.Not.Null);
            Assert.That(results.GroupsEnabledEvent.Groups.Length, Is.EqualTo(1));
            Assert.That(results.GroupsEnabledEvent.Groups.Select(e => e.Id).Contains(groupId), Is.True);
            Assert.That(results.GroupsEnabledEvent.Groups.Select(e => e.Id).Contains(rosterId), Is.False);


            appDomainContext.Dispose();
            appDomainContext = null;
        }


        [Test]
        public void when_set_roster_title_to_empty_and_linked_question_set_to_this_value_and_exists_question_with_condition_on_linked_question()
        {
            var userId = Guid.Parse("11111111111111111111111111111111");

            var questionnaireId = Guid.Parse("77778888000000000000000000000000");
            var intQuestionId = Guid.Parse("11111111111111111111111111111111");
            var rosterTitleQuestionId = Guid.Parse("22222222222222222222222222222222");
            var linkedQuestionId = Guid.Parse("33333333333333333333333333333333");
            var questionWithConditionId = Guid.Parse("44444444444444444444444444444444");

            var appDomainContext = AppDomainContext.Create();

            var results = Execute.InStandaloneAppDomain(appDomainContext.Domain, () =>
            {
                SetUp.MockedServiceLocator();

                var questionnaireDocument = Abc.Create.Entity.QuestionnaireDocumentWithOneChapter(questionnaireId,
                    Create.Entity.NumericIntegerQuestion(intQuestionId, variable: "rsq"),
                    Create.Entity.NumericRoster(rosterSizeQuestionId: intQuestionId, rosterTitleQuestionId: rosterTitleQuestionId, variable: "r1", children: new IComposite[]
                    {
                        Create.Entity.TextQuestion(rosterTitleQuestionId, variable: "rtq")
                    }),
                    Create.Entity.SingleOptionQuestion(linkedQuestionId, variable: "lq", linkedToQuestionId: rosterTitleQuestionId),
                    Create.Entity.TextQuestion(questionWithConditionId, variable: "tbq", enablementCondition: "lq == new[] { 0 }")
                );

                var interview = SetupStatefullInterview(questionnaireDocument);

                interview.AnswerNumericIntegerQuestion(userId, intQuestionId, RosterVector.Empty, DateTime.UtcNow, 1);
                interview.AnswerTextQuestion(userId, rosterTitleQuestionId, new RosterVector(new[] { 0 }), DateTime.UtcNow, "roster title 0");
                interview.AnswerSingleOptionLinkedQuestion(userId, linkedQuestionId, RosterVector.Empty, DateTime.UtcNow, new decimal[]{0});
                interview.AnswerTextQuestion(userId, questionWithConditionId, RosterVector.Empty, DateTime.UtcNow, "enabled");

                using (var eventContext = new EventContext())
                {
                    interview.RemoveAnswer(rosterTitleQuestionId, new RosterVector(new[] { 0 }), userId, DateTime.UtcNow);

                    return new
                    {
                        AnswersRemovedEvent = GetFirstEventByType<AnswersRemoved>(eventContext.Events),
                        QuestionsDisabledEvent = GetFirstEventByType<QuestionsDisabled>(eventContext.Events)
                    };
                }
            });

            Assert.That(results, Is.Not.Null);
            Assert.That(results.AnswersRemovedEvent, Is.Not.Null);
            Assert.That(results.AnswersRemovedEvent.Questions.Length, Is.EqualTo(2));
            Assert.That(results.AnswersRemovedEvent.Questions.Select(e => e.Id).Contains(rosterTitleQuestionId), Is.True);
            Assert.That(results.AnswersRemovedEvent.Questions.Select(e => e.Id).Contains(linkedQuestionId), Is.True);

            Assert.That(results.QuestionsDisabledEvent, Is.Not.Null);
            Assert.That(results.QuestionsDisabledEvent.Questions.Length, Is.EqualTo(1));
            Assert.That(results.QuestionsDisabledEvent.Questions.Select(e => e.Id).Contains(questionWithConditionId), Is.True);


            appDomainContext.Dispose();
            appDomainContext = null;
        }


        [Test]
        public void when_set_roster_title_to_empty_and_linked_question_set_to_this_value_and_exists_question_with_condition_on_linked_question_2()
        {
            var userId = Guid.Parse("11111111111111111111111111111111");

            var questionnaireId = Guid.Parse("77778888000000000000000000000000");
            var intQuestionId = Guid.Parse("11111111111111111111111111111111");
            var rosterTitleQuestionId = Guid.Parse("22222222222222222222222222222222");
            var linkedQuestionId = Guid.Parse("33333333333333333333333333333333");
            var questionWithConditionId = Guid.Parse("44444444444444444444444444444444");

            var appDomainContext = AppDomainContext.Create();

            var results = Execute.InStandaloneAppDomain(appDomainContext.Domain, () =>
            {
                SetUp.MockedServiceLocator();

                var questionnaireDocument = Abc.Create.Entity.QuestionnaireDocumentWithOneChapter(questionnaireId,
                    Create.Entity.NumericIntegerQuestion(intQuestionId, variable: "rsq"),
                    Create.Entity.NumericRoster(rosterSizeQuestionId: intQuestionId, rosterTitleQuestionId: rosterTitleQuestionId, variable: "r1", children: new IComposite[]
                    {
                        Create.Entity.TextQuestion(rosterTitleQuestionId, variable: "rtq")
                    }),
                    Create.Entity.SingleOptionQuestion(linkedQuestionId, variable: "lq", linkedToQuestionId: rosterTitleQuestionId),
                    Create.Entity.TextQuestion(questionWithConditionId, variable: "tbq", enablementCondition: "lq == new[] { 0 }")
                );

                var interview = SetupStatefullInterview(questionnaireDocument);

                interview.AnswerNumericIntegerQuestion(userId, intQuestionId, RosterVector.Empty, DateTime.UtcNow, 1);
                interview.AnswerTextQuestion(userId, rosterTitleQuestionId, new RosterVector(new[] { 0 }), DateTime.UtcNow, "roster title 0");
                interview.AnswerSingleOptionLinkedQuestion(userId, linkedQuestionId, RosterVector.Empty, DateTime.UtcNow, new decimal[]{0});
                interview.AnswerTextQuestion(userId, questionWithConditionId, RosterVector.Empty, DateTime.UtcNow, "enabled");

                using (var eventContext = new EventContext())
                {
                    interview.RemoveAnswer(intQuestionId, RosterVector.Empty, userId, DateTime.UtcNow);

                    return new
                    {
                        AnswersRemovedEvent = GetFirstEventByType<AnswersRemoved>(eventContext.Events),
                        QuestionsDisabledEvent = GetFirstEventByType<QuestionsDisabled>(eventContext.Events)
                    };
                }
            });

            Assert.That(results, Is.Not.Null);
            Assert.That(results.AnswersRemovedEvent, Is.Not.Null);
            Assert.That(results.AnswersRemovedEvent.Questions.Length, Is.EqualTo(3));
            Assert.That(results.AnswersRemovedEvent.Questions.Select(e => e.Id).Contains(rosterTitleQuestionId), Is.True);
            Assert.That(results.AnswersRemovedEvent.Questions.Select(e => e.Id).Contains(linkedQuestionId), Is.True);
            Assert.That(results.AnswersRemovedEvent.Questions.Select(e => e.Id).Contains(intQuestionId), Is.True);

            Assert.That(results.QuestionsDisabledEvent, Is.Not.Null);
            Assert.That(results.QuestionsDisabledEvent.Questions.Length, Is.EqualTo(1));
            Assert.That(results.QuestionsDisabledEvent.Questions.Select(e => e.Id).Contains(questionWithConditionId), Is.True);


            appDomainContext.Dispose();
            appDomainContext = null;
        }

        [Test]
        public void when_set_roster_title_to_empty_and_linked_question_set_to_this_value_and_exists_question_with_condition_on_linked_question_3()
        {
            var userId = Guid.Parse("11111111111111111111111111111111");

            var questionnaireId = Guid.Parse("77778888000000000000000000000000");
            var intQuestionId = Guid.Parse("11111111111111111111111111111111");
            var rosterTitleQuestionId = Guid.Parse("22222222222222222222222222222222");
            var linkedQuestionId = Guid.Parse("33333333333333333333333333333333");
            var questionWithConditionId = Guid.Parse("44444444444444444444444444444444");
            var rosterId = Guid.Parse("55555555555555555555555555555555");

            var appDomainContext = AppDomainContext.Create();

            var results = Execute.InStandaloneAppDomain(appDomainContext.Domain, () =>
            {
                SetUp.MockedServiceLocator();

                var questionnaireDocument = Create.Entity.QuestionnaireDocumentWithOneChapter(questionnaireId,
                    Create.Entity.NumericIntegerQuestion(intQuestionId, variable: "rsq"),
                    Create.Entity.NumericRoster(rosterId, rosterSizeQuestionId: intQuestionId, rosterTitleQuestionId: rosterTitleQuestionId, variable: "r1", children: new IComposite[]
                    {
                        Create.Entity.TextQuestion(rosterTitleQuestionId, variable: "rtq")
                    }),
                    Create.Entity.SingleOptionQuestion(linkedQuestionId, variable: "lq", linkedToRosterId: rosterId),
                    Create.Entity.TextQuestion(questionWithConditionId, variable: "tbq", enablementCondition: "lq == new[] { 0 }")
                );

                var interview = SetupStatefullInterview(questionnaireDocument);

                interview.AnswerNumericIntegerQuestion(userId, intQuestionId, RosterVector.Empty, DateTime.UtcNow, 1);
                interview.AnswerTextQuestion(userId, rosterTitleQuestionId, new RosterVector(new[] { 0 }), DateTime.UtcNow, "roster title 0");
                interview.AnswerSingleOptionLinkedQuestion(userId, linkedQuestionId, RosterVector.Empty, DateTime.UtcNow, new decimal[]{0});
                interview.AnswerTextQuestion(userId, questionWithConditionId, RosterVector.Empty, DateTime.UtcNow, "enabled");

                using (var eventContext = new EventContext())
                {
                    interview.RemoveAnswer(rosterTitleQuestionId, new RosterVector(new[] { 0 }), userId, DateTime.UtcNow);

                    return new
                    {
                        AnswersRemovedEvent = GetFirstEventByType<AnswersRemoved>(eventContext.Events),
                        QuestionsDisabledEvent = GetFirstEventByType<QuestionsDisabled>(eventContext.Events)
                    };
                }
            });

            Assert.That(results, Is.Not.Null);
            Assert.That(results.AnswersRemovedEvent, Is.Not.Null);
            Assert.That(results.AnswersRemovedEvent.Questions.Length, Is.EqualTo(2));
            Assert.That(results.AnswersRemovedEvent.Questions.Select(e => e.Id).Contains(rosterTitleQuestionId), Is.True);
            Assert.That(results.AnswersRemovedEvent.Questions.Select(e => e.Id).Contains(linkedQuestionId), Is.True);

            Assert.That(results.QuestionsDisabledEvent, Is.Not.Null);
            Assert.That(results.QuestionsDisabledEvent.Questions.Length, Is.EqualTo(1));
            Assert.That(results.QuestionsDisabledEvent.Questions.Select(e => e.Id).Contains(questionWithConditionId), Is.True);


            appDomainContext.Dispose();
            appDomainContext = null;
        }



        [Test]
        public void when_set_roster_title_to_empty_and_linked_question_set_to_this_value_and_exists_question_with_condition_on_linked_question_4()
        {
            var userId = Guid.Parse("11111111111111111111111111111111");

            var questionnaireId = Guid.Parse("77778888000000000000000000000000");
            var intQuestionId = Guid.Parse("11111111111111111111111111111111");
            var rosterTitleQuestionId = Guid.Parse("22222222222222222222222222222222");
            var linkedQuestionId = Guid.Parse("33333333333333333333333333333333");
            var questionWithConditionId = Guid.Parse("44444444444444444444444444444444");
            var rosterId = Guid.Parse("55555555555555555555555555555555");

            var appDomainContext = AppDomainContext.Create();

            var results = Execute.InStandaloneAppDomain(appDomainContext.Domain, () =>
            {
                SetUp.MockedServiceLocator();

                var questionnaireDocument = Abc.Create.Entity.QuestionnaireDocumentWithOneChapter(questionnaireId,
                    Create.Entity.NumericIntegerQuestion(intQuestionId, variable: "rsq"),
                    Create.Entity.NumericRoster(rosterId, rosterSizeQuestionId: intQuestionId, rosterTitleQuestionId: rosterTitleQuestionId, variable: "r1", children: new IComposite[]
                    {
                        Create.Entity.TextQuestion(rosterTitleQuestionId, variable: "rtq")
                    }),
                    Create.Entity.SingleOptionQuestion(linkedQuestionId, variable: "lq", linkedToRosterId: rosterId),
                    Create.Entity.TextQuestion(questionWithConditionId, variable: "tbq", enablementCondition: "lq == new[] { 0 }")
                );

                var interview = SetupStatefullInterview(questionnaireDocument);

                interview.AnswerNumericIntegerQuestion(userId, intQuestionId, RosterVector.Empty, DateTime.UtcNow, 1);
                interview.AnswerTextQuestion(userId, rosterTitleQuestionId, new RosterVector(new[] { 0 }), DateTime.UtcNow, "roster title 0");
                interview.AnswerSingleOptionLinkedQuestion(userId, linkedQuestionId, RosterVector.Empty, DateTime.UtcNow, new decimal[] { 0 });
                interview.AnswerTextQuestion(userId, questionWithConditionId, RosterVector.Empty, DateTime.UtcNow, "enabled");

                using (var eventContext = new EventContext())
                {
                    interview.RemoveAnswer(intQuestionId, RosterVector.Empty, userId, DateTime.UtcNow);

                    return new
                    {
                        AnswersRemovedEvent = GetFirstEventByType<AnswersRemoved>(eventContext.Events),
                        QuestionsDisabledEvent = GetFirstEventByType<QuestionsDisabled>(eventContext.Events)
                    };
                }
            });

            Assert.That(results, Is.Not.Null);
            Assert.That(results.AnswersRemovedEvent, Is.Not.Null);
            Assert.That(results.AnswersRemovedEvent.Questions.Length, Is.EqualTo(3));
            Assert.That(results.AnswersRemovedEvent.Questions.Select(e => e.Id).Contains(rosterTitleQuestionId), Is.True);
            Assert.That(results.AnswersRemovedEvent.Questions.Select(e => e.Id).Contains(linkedQuestionId), Is.True);
            Assert.That(results.AnswersRemovedEvent.Questions.Select(e => e.Id).Contains(intQuestionId), Is.True);

            Assert.That(results.QuestionsDisabledEvent, Is.Not.Null);
            Assert.That(results.QuestionsDisabledEvent.Questions.Length, Is.EqualTo(1));
            Assert.That(results.QuestionsDisabledEvent.Questions.Select(e => e.Id).Contains(questionWithConditionId), Is.True);


            appDomainContext.Dispose();
            appDomainContext = null;
        }

        [Test]
        public void when_exists_condition_on_rowindex()
        {
            var userId = Guid.Parse("11111111111111111111111111111111");

            var questionnaireId = Guid.Parse("77778888000000000000000000000000");
            var intQuestionId = Guid.Parse("11111111111111111111111111111111");
            var rosterTitleQuestionId = Guid.Parse("22222222222222222222222222222222");
            var rosterIntQuestionId = Guid.Parse("33333333333333333333333333333333");
            var rosterId = Guid.Parse("55555555555555555555555555555555");

            var appDomainContext = AppDomainContext.Create();

            var results = Execute.InStandaloneAppDomain(appDomainContext.Domain, () =>
            {
                SetUp.MockedServiceLocator();

                var questionnaireDocument = Abc.Create.Entity.QuestionnaireDocumentWithOneChapter(questionnaireId,
                    Create.Entity.NumericIntegerQuestion(intQuestionId, variable: "rsq"),
                    Create.Entity.NumericRoster(rosterId, rosterSizeQuestionId: intQuestionId, rosterTitleQuestionId: rosterTitleQuestionId, variable: "r1", children: new IComposite[]
                    {
                        Create.Entity.NumericIntegerQuestion(rosterIntQuestionId, variable: "riq"),
                        Create.Entity.TextQuestion(rosterTitleQuestionId, variable: "rtq", enablementCondition: "@rowindex + riq == 5")
                    })
                );

                var interview = SetupStatefullInterview(questionnaireDocument);

                interview.AnswerNumericIntegerQuestion(userId, intQuestionId, RosterVector.Empty, DateTime.UtcNow, 10);

                using (var eventContext = new EventContext())
                {
                    interview.AnswerNumericIntegerQuestion(userId, rosterIntQuestionId, new RosterVector(new[] { 2 }), DateTime.UtcNow, 3);

                    return new
                    {
                        QuestionsEnabledEvent = GetFirstEventByType<QuestionsEnabled>(eventContext.Events)
                    };
                }
            });

            Assert.That(results, Is.Not.Null);
            Assert.That(results.QuestionsEnabledEvent, Is.Not.Null);
            Assert.That(results.QuestionsEnabledEvent.Questions.Length, Is.EqualTo(1));
            Assert.That(results.QuestionsEnabledEvent.Questions.Single(i => i == Identity.Create(rosterTitleQuestionId, new int[] { 2 })), Is.Not.Null);

            appDomainContext.Dispose();
            appDomainContext = null;
        }

        [Test]
        public void when_exists_condition_on_rowcode()
        {
            var userId = Guid.Parse("11111111111111111111111111111111");

            var questionnaireId = Guid.Parse("77778888000000000000000000000000");
            var intQuestionId = Guid.Parse("11111111111111111111111111111111");
            var rosterTitleQuestionId = Guid.Parse("22222222222222222222222222222222");
            var rosterIntQuestionId = Guid.Parse("33333333333333333333333333333333");
            var rosterId = Guid.Parse("55555555555555555555555555555555");

            var appDomainContext = AppDomainContext.Create();

            var results = Execute.InStandaloneAppDomain(appDomainContext.Domain, () =>
            {
                SetUp.MockedServiceLocator();

                var questionnaireDocument = Abc.Create.Entity.QuestionnaireDocumentWithOneChapter(questionnaireId,
                    Create.Entity.NumericIntegerQuestion(intQuestionId, variable: "rsq"),
                    Create.Entity.NumericRoster(rosterId, rosterSizeQuestionId: intQuestionId, rosterTitleQuestionId: rosterTitleQuestionId, variable: "r1", children: new IComposite[]
                    {
                        Create.Entity.NumericIntegerQuestion(rosterIntQuestionId, variable: "riq"),
                        Create.Entity.TextQuestion(rosterTitleQuestionId, variable: "rtq", enablementCondition: "@rowcode + riq == 5")
                    })
                );

                var interview = SetupStatefullInterview(questionnaireDocument);

                interview.AnswerNumericIntegerQuestion(userId, intQuestionId, RosterVector.Empty, DateTime.UtcNow, 10);

                using (var eventContext = new EventContext())
                {
                    interview.AnswerNumericIntegerQuestion(userId, rosterIntQuestionId, new RosterVector(new[] { 2 }), DateTime.UtcNow, 3);

                    return new
                    {
                        QuestionsEnabledEvent = GetFirstEventByType<QuestionsEnabled>(eventContext.Events)
                    };
                }
            });

            Assert.That(results, Is.Not.Null);
            Assert.That(results.QuestionsEnabledEvent, Is.Not.Null);
            Assert.That(results.QuestionsEnabledEvent.Questions.Length, Is.EqualTo(1));
            Assert.That(results.QuestionsEnabledEvent.Questions.Single(i => i == Identity.Create(rosterTitleQuestionId, new int[] { 2 })), Is.Not.Null);

            appDomainContext.Dispose();
            appDomainContext = null;
        }

    }
}
