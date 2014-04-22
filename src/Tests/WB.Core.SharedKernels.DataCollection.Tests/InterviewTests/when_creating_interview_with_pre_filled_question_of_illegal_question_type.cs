using System;
using System.Collections.Generic;
using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using Microsoft.Practices.ServiceLocation;
using Moq;
using Ncqrs.Spec;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Exceptions;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Repositories;
using WB.Core.SharedKernels.DataCollection.Repositories;
using It = Machine.Specifications.It;

namespace WB.Core.SharedKernels.DataCollection.Tests.InterviewTests
{
    internal class when_creating_interview_with_pre_filled_question_of_illegal_question_type : InterviewTestsContext
    {
        private Establish context = () =>
        {
            interviewId = Guid.Parse("11111111111111111111111111111111");
            questionnaireId = Guid.Parse("22220000000000000000000000000000");
            userId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
            supervisorId = Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");
            prefilledQuestionId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
            answersToFeaturedQuestions = new Dictionary<Guid, object>();
            prefilledQuestionAnswer = "answer";
            answersToFeaturedQuestions.Add(prefilledQuestionId, prefilledQuestionAnswer);
            answersTime = new DateTime(2013, 09, 01);

            var questionaire = Mock.Of<IQuestionnaire>(_
                => _.GetQuestionType(prefilledQuestionId) == QuestionType.QRBarcode
                && _.HasQuestion(prefilledQuestionId) == true);

            var questionnaireRepository = Mock.Of<IQuestionnaireRepository>(repository
                => repository.GetHistoricalQuestionnaire(questionnaireId, Moq.It.IsAny<long>()) == questionaire);

            Mock.Get(ServiceLocator.Current)
                .Setup(locator => locator.GetInstance<IQuestionnaireRepository>())
                .Returns(questionnaireRepository);

            eventContext = new EventContext();
        };

        private Because of = () =>
            exception = Catch.Exception(() =>
            new Interview(interviewId, userId, questionnaireId, 1, answersToFeaturedQuestions, answersTime, supervisorId));

        Cleanup stuff = () =>
        {
            eventContext.Dispose();
            eventContext = null;
        };

        private It should_raise_InterviewException = () =>
            exception.ShouldBeOfType<InterviewException>();
        
        It should_throw_exception_with_message_containting__has_type__ = () =>
            exception.Message.ToLower().ShouldContain("has type");

        It should_throw_exception_with_message_containting__not_supported__ = () =>
            exception.Message.ToLower().ShouldContain("not supported");        

        private static EventContext eventContext;
        private static Guid interviewId;
        private static Guid userId;
        private static Guid questionnaireId;
        private static Dictionary<Guid, object> answersToFeaturedQuestions;
        private static DateTime answersTime;
        private static Guid supervisorId;
        private static Guid prefilledQuestionId;
        private static string prefilledQuestionAnswer;
        private static Exception exception;
    }
}
