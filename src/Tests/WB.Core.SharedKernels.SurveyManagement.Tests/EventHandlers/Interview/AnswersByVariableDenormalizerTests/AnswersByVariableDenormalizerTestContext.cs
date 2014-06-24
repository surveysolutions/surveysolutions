using System;
using System.Collections.Generic;
using Moq;
using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.SurveyManagement.EventHandler;
using WB.Core.SharedKernels.SurveyManagement.Views.Interview;
using WB.Core.SharedKernels.SurveyManagement.Views.Questionnaire;

namespace WB.Core.SharedKernels.SurveyManagement.Tests.EventHandlers.Interview.AnswersByVariableDenormalizerTests
{
    internal class AnswersByVariableDenormalizerTestContext
    {
        protected static IPublishedEvent<T> ToPublishedEvent<T>(T @event, int? eventSequence = null, Guid? eventSourceId = null) where T : class
        {
            return Mock.Of<IPublishedEvent<T>>(publishedEvent => publishedEvent.Payload == @event
                && publishedEvent.EventSourceId == (eventSourceId ?? Guid.Parse("33333333333333333333333333333333"))
                && publishedEvent.EventSequence == (eventSequence ?? 1));
        }

        protected static IPublishedEvent<AnswerRemoved> CreateAnswerRemovedEvent(Guid interviewId, Guid? questionId = null, decimal[] propagationVector = null)
        {
            return ToPublishedEvent(new AnswerRemoved(questionId ?? Guid.NewGuid(), propagationVector ?? new decimal[0]), 10, interviewId);
        }

        protected static IPublishedEvent<GeoLocationQuestionAnswered> CreateGeoLocationQuestionAnsweredEvent(Guid interviewId, Guid? userId = null, Guid? questionId = null,
            decimal[] propagationVector = null, DateTime? answerTime = null, double? latitude = null, double? longitude = null,
            double? accuracy = null, double? altitude = null, DateTimeOffset? timestamp = null)
        {
            return ToPublishedEvent(new GeoLocationQuestionAnswered(
                userId ?? Guid.NewGuid(),
                questionId ?? Guid.NewGuid(),
                propagationVector ?? new decimal[0],
                answerTime ?? DateTime.Now,
                latitude ?? 0,
                longitude ?? 0,
                accuracy ?? 0,
                altitude ?? 0,
                timestamp ?? new DateTimeOffset(DateTime.Now)
                ), 1, interviewId);
        }


        protected static AnswersByVariableCollection CreateAnswersByVariableCollectionWithOneAnswer(Guid interviewId, string propagationVectorKey = "#", string answer = "11;11")
        {
            return new AnswersByVariableCollection()
            {
                Answers = new Dictionary<Guid, Dictionary<string, string>>()
                {
                    {
                        interviewId, new Dictionary<string, string>()
                        {
                            { propagationVectorKey, answer }
                        }
                    }
                }
            };
        }

        protected static AnswersByVariableDenormalizer CreateAnswersByVariableDenormalizer(
            IReadSideRepositoryWriter<InterviewBrief> interviewBriefStorage = null,
            IReadSideRepositoryWriter<QuestionnaireQuestionsInfo> variablesStorage = null,
            IReadSideRepositoryWriter<AnswersByVariableCollection> answersByVariableStorage = null)
        {
            return new AnswersByVariableDenormalizer(
                interviewBriefStorage ?? Mock.Of<IReadSideRepositoryWriter<InterviewBrief>>(),
                variablesStorage ?? Mock.Of<IReadSideRepositoryWriter<QuestionnaireQuestionsInfo>>(),
                answersByVariableStorage ?? Mock.Of<IReadSideRepositoryWriter<AnswersByVariableCollection>>()
                );
        }

    }
}
