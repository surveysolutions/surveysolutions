﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using Moq;
using Ncqrs.Eventing.ServiceModel.Bus;
using NUnit.Framework;
using WB.Core.BoundedContexts.Headquarters.EventHandler;
using WB.Core.BoundedContexts.Headquarters.Repositories;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.BoundedContexts.Headquarters.Views.Questionnaire;
using WB.Core.BoundedContexts.Headquarters.Views.User;
using WB.Core.GenericSubdomains.Portable.Implementation.Services;
using WB.Core.Infrastructure.EventBus;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Events.Interview.Base;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.Enumerator.Views;
using WB.Core.SharedKernels.Questionnaire.Translations;
using WB.Enumerator.Native.Questionnaire.Impl;
using WB.Infrastructure.Native.Questionnaire;
using WB.Infrastructure.Native.Questionnaire.Impl;
using WB.Infrastructure.Native.Storage;
using WB.Tests.Abc;
using QuestionAnswer = WB.Core.BoundedContexts.Headquarters.Views.Interview.QuestionAnswer;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.EventHandlers.Interview.InterviewSummaryEventHandlerFunctionalTests
{
    [TestFixture]
    internal class InterviewSummaryEventHandlerUpdateAnswerTest
    {
        [Test]
        public void
            Update_When_event_with_answer_on_featured_question_with_reusable_categories_published_Then_answer_value_be_equal_selected_option_text()
        {
            var questionnaireIdentity = new QuestionnaireIdentity(Id.g2, 1);
            var questionId = Guid.Parse("10000000000000000000000000000000");
            var categoriesId = Id.g1;
            var answerText = "answer text";

            var savedInterviewSummary = CreateInterviewSummaryQuestions(new QuestionAnswer
            {
                Questionid = questionId
            });

            savedInterviewSummary.QuestionnaireId = questionnaireIdentity.QuestionnaireId;
            savedInterviewSummary.QuestionnaireVersion = questionnaireIdentity.Version;

            var questionnaireDocument = Create.Entity.QuestionnaireDocument(
                children: Create.Entity.SingleOptionQuestion(questionId, categoryId: categoriesId));
            questionnaireDocument.Categories.Add(Create.Entity.Categories(categoriesId));

            var reusableCategoriesStorage = Create.Storage.InMemoryPlainStorage<ReusableCategoricalOptions>();
            reusableCategoriesStorage.Store(new []
            {
                Create.Entity.ReusableCategoricalOptions(questionnaireIdentity, categoriesId, 1, answerText)
            });

            var questionnaireStorage = new HqQuestionnaireStorage(Mock.Of<IPlainKeyValueStorage<QuestionnaireDocument>>(),
                Mock.Of<ITranslationStorage>(),
                Mock.Of<IQuestionnaireTranslator>(),
                Mock.Of<IReadSideRepositoryWriter<QuestionnaireCompositeItem, int>>(), 
                Mock.Of<INativeReadSideStorage<QuestionnaireCompositeItem, int>>(), 
                new QuestionnaireQuestionOptionsRepository(), 
                Create.Service.SubstitutionService(),
                Create.Service.ExpressionStatePrototypeProvider(),
                new ReusableCategoriesFillerIntoQuestionnaire(new ReusableCategoriesStorage(reusableCategoriesStorage)));
            questionnaireStorage.StoreQuestionnaire(questionnaireIdentity.QuestionnaireId, questionnaireIdentity.Version, questionnaireDocument);

            var interviewSummaryEventHandler = new InterviewSummaryDenormalizer(Mock.Of<IUserViewFactory>(), questionnaireStorage);

            var updatedInterviewSummary =
                interviewSummaryEventHandler.Update(savedInterviewSummary,
                    this.CreatePublishableEvent(new SingleOptionQuestionAnswered(Guid.NewGuid(), questionId, new decimal[0], DateTime.Now, 1)));

            Assert.That(updatedInterviewSummary.AnswersToFeaturedQuestions.First(x => x.Questionid == questionId).Answer, Is.EqualTo(answerText));
        }

        [Test]
        public void
            Update_When_event_with_answer_on_featured_question_with_options_published_Then_answer_value_be_equal_selected_option_text()
        {
            var questionId = Guid.Parse("10000000000000000000000000000000");
            var answerText = "answer text";

            var savedInterviewSummary =
                CreateInterviewSummaryQuestions(
                    new QuestionAnswer()
                    {
                        Questionid = questionId,
                    });

            savedInterviewSummary.QuestionnaireId = Guid.NewGuid();

            var interviewSummaryEventHandler =
                CreateInterviewSummaryEventHandlerFunctional(
                    Create.Entity.QuestionnaireDocument(savedInterviewSummary.QuestionnaireId, children:
                        Create.Entity.SingleQuestion(questionId, options: new List<Answer>(new []{Create.Entity.Answer(answerText, 1)}))));

             var updatedInterviewSummary =
                interviewSummaryEventHandler.Update(savedInterviewSummary,
                    this.CreatePublishableEvent(new SingleOptionQuestionAnswered(Guid.NewGuid(), questionId, new decimal[0], DateTime.Now, 1)));

            Assert.That(updatedInterviewSummary.AnswersToFeaturedQuestions.First(x => x.Questionid == questionId).Answer, Is.EqualTo(answerText));
        }

        [Test]
        public void
            Update_When_event_with_answer_on_featured_question_with_multy_options_published_Then_answer_value_be_equal_selected_options_text
            ()
        {
            var questionId = Guid.Parse("10000000000000000000000000000000");

            var savedInterviewSummary =
                CreateInterviewSummaryQuestions(
                    new QuestionAnswer
                    {
                        Questionid = questionId,
                    });

            savedInterviewSummary.QuestionnaireId = Guid.NewGuid();

            var interviewSummaryEventHandler = CreateInterviewSummaryEventHandlerFunctional(Create.Entity.QuestionnaireDocument(savedInterviewSummary.QuestionnaireId, children:
                        Create.Entity.SingleQuestion(questionId, options: new List<Answer> { Create.Entity.Answer("1", 1), Create.Entity.Answer("3", 3), Create.Entity.Answer("8", 8) })));
            var updatedInterviewSummary =
                interviewSummaryEventHandler.Update(savedInterviewSummary,
                    this.CreatePublishableEvent(new MultipleOptionsQuestionAnswered(Guid.NewGuid(), questionId, new decimal[0], DateTime.Now,
                        new decimal[] { 1, 3, 8 })));
            Assert.That(updatedInterviewSummary.AnswersToFeaturedQuestions.First(x => x.Questionid == questionId).Answer, Is.EqualTo("1,3,8"));
        }

        [Test]
        [SetCulture("en-US")]
        [TestCase(QuestionType.Numeric, 1)]
        [TestCase(QuestionType.Numeric, 1.3)]
        [TestCase(QuestionType.Text, "answer text")]
        [TestCase(QuestionType.QRBarcode, "some answer")]
        public void Update_When_event_with_answer_on_featured_question_published_Then_answer_value_be_equal_passed_answer(QuestionType type,
            object answer)
        {
            var questionId = Id.g1;

            var savedInterviewSummary =
                CreateInterviewSummaryQuestions(
                    new QuestionAnswer()
                    {
                        Questionid = questionId
                    });

            var interviewSummaryEventHandler = CreateInterviewSummaryEventHandlerFunctional();

            var updatedInterviewSummary = this.CallUpdateMethod(interviewSummaryEventHandler, savedInterviewSummary,
                this.CreateQuestionAnsweredEventByQuestionType(questionId, type, answer));

            Assert.That(updatedInterviewSummary.AnswersToFeaturedQuestions.First(x => x.Questionid == questionId).Answer, Is.EqualTo(answer.ToString()));
        }

        [Test]
        public void when_date_time_featured_questions_answered()
        {
            var dateQuestionId = Id.g1;
            var dateTimeQuestionId = Id.g2;

            var savedInterviewSummary =
                CreateInterviewSummaryQuestions(
                    new QuestionAnswer()
                    {
                        Questionid = dateQuestionId
                    },
                    new QuestionAnswer()
                    {
                        Questionid = dateTimeQuestionId
                    });

            savedInterviewSummary.QuestionnaireId = Guid.NewGuid();

            var questionnaire = Create.Entity.QuestionnaireDocumentWithOneChapter(
                Create.Entity.DateTimeQuestion(questionId: dateQuestionId, isTimestamp: false),
                Create.Entity.DateTimeQuestion(questionId: dateTimeQuestionId, isTimestamp: true)
            );

            var interviewSummaryEventHandler = CreateInterviewSummaryEventHandlerFunctional(questionnaire);

            var dateAnswer = new DateTime(2010, 5, 17);
            var timestampAnswer = new DateTime(2011, 1, 17, 20, 10, 38);

            // act
            var updatedInterviewSummary =  interviewSummaryEventHandler.Update(savedInterviewSummary,
                this.CreatePublishableEvent(Create.Event.DateTimeQuestionAnswered(dateQuestionId, dateAnswer)));
            updatedInterviewSummary =  interviewSummaryEventHandler.Update(savedInterviewSummary,
                this.CreatePublishableEvent(Create.Event.DateTimeQuestionAnswered(dateTimeQuestionId, timestampAnswer)));

            // assert
            Assert.That(updatedInterviewSummary.AnswersToFeaturedQuestions.First(x => x.Questionid == dateQuestionId).Answer, Is.EqualTo(dateAnswer.ToString(DateTimeFormat.DateFormat)));
            Assert.That(updatedInterviewSummary.AnswersToFeaturedQuestions.First(x => x.Questionid == dateTimeQuestionId).Answer, Is.EqualTo(timestampAnswer.ToString(DateTimeFormat.DateWithTimeFormat)));
        }

        [Test]
        [TestCase(QuestionType.Text, "answer text")]
        public void Update_When_event_SynchronizationMetadataApplied_Then_featured_answer_value_be_equal_passed_valuesr(QuestionType type,
            object answer)
        {
            var questionnaireId = Guid.Parse("10000000000000000000000000000000");
            var questionId = Guid.Parse("10000000000000000000000000000000");
            var userId = Guid.Parse("20000000000000000000000000000002");

            var questionnaire = Create.Entity.QuestionnaireDocumentWithOneChapter(children:Create.Entity.TextQuestion(questionId));
            var savedInterviewSummary = CreateInterviewSummaryQuestions(
                    new QuestionAnswer()
                    {
                        Questionid = questionId
                    });

            savedInterviewSummary.WasCreatedOnClient = true;


            var featuredQuestionsMeta = new []{ Create.Entity.AnsweredQuestionSynchronizationDto(questionId, new decimal[0], answer ) };

            var interviewSummaryEventHandler = CreateInterviewSummaryEventHandlerFunctional(questionnaire);

            var synchronizationMetadataApplied = new SynchronizationMetadataApplied(userId, questionnaireId, 1, InterviewStatus.Created, featuredQuestionsMeta, 
                false, null, null, null, DateTimeOffset.Now);

            var updatedInterviewSummary = this.CallUpdateMethod(interviewSummaryEventHandler, savedInterviewSummary,
                synchronizationMetadataApplied);

            Assert.That(updatedInterviewSummary.AnswersToFeaturedQuestions.First(x => x.Questionid == questionId).Answer, Is.EqualTo(answer.ToString()));
        }

        private QuestionAnswered CreateQuestionAnsweredEventByQuestionType(Guid questionId, QuestionType type, object answer)
        {
            switch (type)
            {
                case QuestionType.DateTime:
                    return new DateTimeQuestionAnswered(Guid.NewGuid(), questionId, new decimal[0], DateTime.Now,
                        DateTime.Parse(answer.ToString()));
                case QuestionType.Numeric:
                    if (answer is int)
                        return new NumericIntegerQuestionAnswered(Guid.NewGuid(), questionId, new decimal[0], DateTime.Now, (int)answer);
                    return new NumericRealQuestionAnswered(Guid.NewGuid(), questionId, new decimal[0], DateTime.Now, Convert.ToDecimal(answer));
                case QuestionType.Text:
                    return new TextQuestionAnswered(Guid.NewGuid(), questionId, new decimal[0], DateTime.Now, (string)answer);
                case QuestionType.QRBarcode:
                    return new QRBarcodeQuestionAnswered(userId: Guid.NewGuid(), questionId: questionId, rosterVector: new decimal[0],
                        originDate: DateTimeOffset.Now, answer: (string) answer);
            }
            return null;
        }

        private InterviewSummary CallUpdateMethod(InterviewSummaryDenormalizer eventHandler, InterviewSummary currentState,
            object updateEvent)
        {
            MethodInfo method = this.GetType().GetMethod("CreatePublishableEvent", BindingFlags.Instance | BindingFlags.NonPublic);
            MethodInfo generic = method.MakeGenericMethod(updateEvent.GetType());
            var publishableEvent = generic.Invoke(this, new object[] { updateEvent });

            var eventType = typeof (IPublishedEvent<>).MakeGenericType(updateEvent.GetType());

            return (InterviewSummary) eventHandler.GetType()
                        .GetMethod("Update", new Type[] { typeof (InterviewSummary), eventType })
                        .Invoke(eventHandler, new object[] { currentState, publishableEvent });
        }

        protected IPublishedEvent<T> CreatePublishableEvent<T>(T payload)
            where T: IEvent
        {
            var publishableEventMock = new Mock<IPublishedEvent<T>>();
            publishableEventMock.Setup(x => x.Payload).Returns(payload);
            return publishableEventMock.Object;
        }

        protected static InterviewSummaryDenormalizer CreateInterviewSummaryEventHandlerFunctional(QuestionnaireDocument questionnaire = null)
        {
            PlainQuestionnaire plainQuestionnaire =
                Create.Entity.PlainQuestionnaire(questionnaire ?? Create.Entity.QuestionnaireDocument(), 1,
                    questionOptionsRepository: Create.Storage.QuestionnaireQuestionOptionsRepository());

            return new InterviewSummaryDenormalizer(
                new Mock<IUserViewFactory>().Object,
                Mock.Of<IQuestionnaireStorage>(_ => _.GetQuestionnaire(Moq.It.IsAny<QuestionnaireIdentity>(), Moq.It.IsAny<string>()) == plainQuestionnaire));
        }

        protected static InterviewSummary CreateInterviewSummaryQuestions(params QuestionAnswer[] questions)
        {
            var interviewSummary = new InterviewSummary();
            foreach (var questionAnswer in questions)
            {
                interviewSummary.AnswersToFeaturedQuestions.Add(questionAnswer);
            }
            return interviewSummary;
        }
    }
}
