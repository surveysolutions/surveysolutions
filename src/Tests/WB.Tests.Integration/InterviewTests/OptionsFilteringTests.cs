using System;
using System.Linq;
using Main.Core.Entities.SubEntities.Question;
using Ncqrs.Spec;
using NUnit.Framework;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Exceptions;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities;
using WB.Core.SharedKernels.Enumerator.Views;
using WB.Core.SharedKernels.Questionnaire.Documents;
using WB.Tests.Abc;

namespace WB.Tests.Integration.InterviewTests
{
    [TestFixture]
    public class OptionsFilteringTests : InterviewTestsContext
    {
        [Test]
        public void when_answer_question_enable_section_with_roster_but_roster_triget_is_disabled()
        {
            var userId = Guid.Parse("11111111111111111111111111111111");

            var questionnaireId = Guid.Parse("77778888000000000000000000000000");
            var numeric1Id = Guid.Parse("11111111111111111111111111111111");
            var numeric2Id = Guid.Parse("22222222222222222222222222222222");
            var singleId = Guid.Parse("33333333333333333333333333333333");

            var appDomainContext = AppDomainContext.Create();

            var results = Execute.InStandaloneAppDomain(appDomainContext.Domain, () =>
            {
                SetUp.MockedServiceLocator();

                var questionnaireDocument = Abc.Create.Entity.QuestionnaireDocumentWithOneChapter(questionnaireId,
                    Create.Entity.NumericIntegerQuestion(numeric1Id, variable: "i1"),
                    Create.Entity.NumericIntegerQuestion(numeric2Id, variable: "i2", enablementCondition: "i1 == 1"),
                    Create.Entity.SingleOptionQuestion(singleId, optionsFilterExpression: "@optioncode==1 && i2==1", answerCodes: new int[]{ 1, 2, 3})
                );

                var interview = SetupStatefullInterview(appDomainContext.AssemblyLoadContext, questionnaireDocument);

                interview.AnswerNumericIntegerQuestion(userId, numeric1Id, RosterVector.Empty, DateTime.UtcNow, 1);
                interview.AnswerNumericIntegerQuestion(userId, numeric2Id, RosterVector.Empty, DateTime.UtcNow, 1);
                interview.AnswerSingleOptionQuestion(userId, singleId, RosterVector.Empty, DateTime.UtcNow, 1);

                using (var eventContext = new EventContext())
                {
                    interview.AnswerNumericIntegerQuestion(userId, numeric1Id, RosterVector.Empty, DateTime.UtcNow, 2);

                    return new
                    {
                        AnswersRemovedEvent = GetFirstEventByType<AnswersRemoved>(eventContext.Events)
                    };
                }
            });

            Assert.That(results, Is.Not.Null);
            Assert.That(results.AnswersRemovedEvent, Is.Not.Null);
            Assert.That(results.AnswersRemovedEvent.Questions.Length, Is.EqualTo(1));
            Assert.That(results.AnswersRemovedEvent.Questions.Select(e => e.Id).Contains(singleId), Is.True);


            appDomainContext.Dispose();
            appDomainContext = null;
        }      
        
        [Test]
        public void when_answer_single_question_with_category_options_and_active_options_filter()
        {
            var userId = Guid.Parse("11111111111111111111111111111111");

            var questionnaireId = Guid.Parse("77778888000000000000000000000000");
            var questionnaireIdentity = Create.Entity.QuestionnaireIdentity(questionnaireId, 7);
            var numericId = Guid.Parse("11111111111111111111111111111111");
            var singleId = Guid.Parse("33333333333333333333333333333333");
            var categoryId = Guid.Parse("77777777777777777777777777777777");

            var appDomainContext = AppDomainContext.Create();

            var results = Execute.InStandaloneAppDomain(appDomainContext.Domain, () =>
            {
                SetUp.MockedServiceLocator();

                var plainStorage = Create.Storage.SqliteInmemoryStorage<OptionView, int?>(
                    Create.Entity.OptionView(questionnaireIdentity, 11, "111", null, categoryId),
                    Create.Entity.OptionView(questionnaireIdentity, 22, "222", null, categoryId),
                    Create.Entity.OptionView(questionnaireIdentity, 33, "333", null, categoryId)
                    );
                var questionOptionRepository = Create.Storage.QuestionOptionsRepository(plainStorage);

                var questionnaireDocument = Abc.Create.Entity.QuestionnaireDocumentWithOneChapter(
                    Guid.NewGuid(), questionnaireId, 
                    Create.Entity.NumericIntegerQuestion(numericId, variable: "i1"),
                    Create.Entity.SingleOptionQuestion(singleId, optionsFilterExpression: "@optioncode==i1", categoryId: categoryId)
                );

                var interview = SetupStatefullInterview(appDomainContext.AssemblyLoadContext, questionnaireDocument,
                    questionnaireIdentity: questionnaireIdentity,
                    questionOptionsRepository: questionOptionRepository);

                interview.AnswerNumericIntegerQuestion(userId, numericId, RosterVector.Empty, DateTime.UtcNow, 11);
                interview.AnswerSingleOptionQuestion(userId, singleId, RosterVector.Empty, DateTime.UtcNow, 11);
                interview.AnswerNumericIntegerQuestion(userId, numericId, RosterVector.Empty, DateTime.UtcNow, 22);

                using (var eventContext = new EventContext())
                {
                    interview.AnswerSingleOptionQuestion(userId, singleId, RosterVector.Empty, DateTime.UtcNow, 22);

                    return new
                    {
                        SingleOptionQuestionAnsweredEvent = GetFirstEventByType<SingleOptionQuestionAnswered>(eventContext.Events)
                    };
                }
            });

            Assert.That(results, Is.Not.Null);
            Assert.That(results.SingleOptionQuestionAnsweredEvent, Is.Not.Null);
            Assert.That(results.SingleOptionQuestionAnsweredEvent.SelectedValue, Is.EqualTo(22));


            appDomainContext.Dispose();
            appDomainContext = null;
        }
        
        [Test]
        public void when_answer_single_question_with_category_options_and_try_answer_option_disabled_by_filter()
        {
            var userId = Guid.Parse("11111111111111111111111111111111");

            var questionnaireId = Guid.Parse("77778888000000000000000000000000");
            var questionnaireIdentity = Create.Entity.QuestionnaireIdentity(questionnaireId, 7);
            var numericId = Guid.Parse("11111111111111111111111111111111");
            var singleId = Guid.Parse("33333333333333333333333333333333");
            var categoryId = Guid.Parse("77777777777777777777777777777777");

            var appDomainContext = AppDomainContext.Create();

            var results = Execute.InStandaloneAppDomain(appDomainContext.Domain, () =>
            {
                SetUp.MockedServiceLocator();

                var plainStorage = Create.Storage.SqliteInmemoryStorage<OptionView, int?>(
                    Create.Entity.OptionView(questionnaireIdentity, 11, "111", null, categoryId),
                    Create.Entity.OptionView(questionnaireIdentity, 22, "222", null, categoryId),
                    Create.Entity.OptionView(questionnaireIdentity, 33, "333", null, categoryId)
                    );
                var questionOptionRepository = Create.Storage.QuestionOptionsRepository(plainStorage);

                var questionnaireDocument = Abc.Create.Entity.QuestionnaireDocumentWithOneChapter(
                    Guid.NewGuid(), questionnaireId, 
                    Create.Entity.NumericIntegerQuestion(numericId, variable: "i1"),
                    Create.Entity.SingleOptionQuestion(singleId, optionsFilterExpression: "@optioncode!=i1", categoryId: categoryId)
                );

                var interview = SetupStatefullInterview(appDomainContext.AssemblyLoadContext, questionnaireDocument,
                    questionnaireIdentity: questionnaireIdentity,
                    questionOptionsRepository: questionOptionRepository);

                interview.AnswerNumericIntegerQuestion(userId, numericId, RosterVector.Empty, DateTime.UtcNow, 22);

                using (var eventContext = new EventContext())
                {
                    interview.AnswerSingleOptionQuestion(userId, singleId, RosterVector.Empty, DateTime.UtcNow, 22);

                    return new
                    {
                        CountOfEvents = eventContext.Events.Count(),
                        ValueOfSingleQuestion = interview.GetQuestion(Create.Identity(singleId)).GetAnswerAsString(),
                        IsAnswered = interview.GetQuestion(Create.Identity(singleId)).IsAnswered()
                    };
                }
            });

            Assert.That(results, Is.Not.Null);
            Assert.That(results.CountOfEvents, Is.EqualTo(0));
            Assert.That(results.IsAnswered, Is.EqualTo(false));
            Assert.That(results.ValueOfSingleQuestion, Is.Null);


            appDomainContext.Dispose();
            appDomainContext = null;
        }
    }
}
