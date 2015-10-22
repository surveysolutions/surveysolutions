﻿using System;
using System.Collections.Generic;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities.Question;
using Moq;
using Ncqrs.Eventing;
using Ncqrs.Eventing.ServiceModel.Bus;
using NUnit.Framework;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.ValueObjects;
using WB.Core.SharedKernels.DataCollection.Views;
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire;
using WB.Core.SharedKernels.SurveyManagement.EventHandler;
using WB.Core.SharedKernels.SurveyManagement.Views.DataExport;
using WB.Core.SharedKernels.SurveyManagement.Views.Interview;
using WB.Core.SharedKernels.SurveyManagement.Views.InterviewHistory;
using It = Moq.It;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.EventHandlers.Interview.InterviewHistoryDenormalizerTests
{
    [Subject(typeof(InterviewHistoryDenormalizer))]
    internal class InterviewHistoryDenormalizerTestContext
    {
        protected static InterviewHistoryDenormalizer CreateInterviewHistoryDenormalizer(IReadSideRepositoryWriter<InterviewHistoryView> interviewHistoryViewWriter=null,
            IReadSideRepositoryWriter<InterviewSummary> interviewSummaryWriter = null, IReadSideRepositoryWriter<UserDocument> userDocumentWriter = null,
            QuestionnaireExportStructure questionnaire = null)
        {
            return new InterviewHistoryDenormalizer(
                interviewHistoryViewWriter ?? Mock.Of<IReadSideRepositoryWriter<InterviewHistoryView>>(),
                interviewSummaryWriter ??
                Mock.Of<IReadSideRepositoryWriter<InterviewSummary>>(
                    _ => _.GetById(It.IsAny<string>()) == new InterviewSummary()),
                userDocumentWriter ?? Mock.Of<IReadSideRepositoryWriter<UserDocument>>(),
                Mock.Of<IReadSideKeyValueStorage<QuestionnaireExportStructure>>(
                    _ =>
                        _.GetById(It.IsAny<string>()) == (questionnaire ?? new QuestionnaireExportStructure())), new InterviewDataExportSettings("", false, 10000, 100));
        }

        protected static InterviewHistoryView CreateInterviewHistoryView(Guid? interviewId=null)
        {
            return new InterviewHistoryView(interviewId??Guid.NewGuid(), new List<InterviewHistoricalRecordView>(), Guid.NewGuid(), 1);
        }

        protected static IPublishedEvent<T> CreatePublishableEvent<T>(Func<T> eventCreator, Guid? eventSourceId = null)
        {
            var publishableEventMock = new Mock<IPublishedEvent<T>>();

            publishableEventMock.Setup(x => x.Payload).Returns(eventCreator());
            publishableEventMock.Setup(x => x.EventSourceId).Returns(eventSourceId ?? Guid.NewGuid());

            return publishableEventMock.Object;
        }

        protected static QuestionnaireExportStructure CreateQuestionnaireExportStructure(Guid questionId, string variableName)
        {
            return new QuestionnaireExportStructure()
            {
                HeaderToLevelMap =
                    new Dictionary<ValueVector<Guid>, HeaderStructureForLevel>
                    {
                        {
                            new ValueVector<Guid>(),
                            new HeaderStructureForLevel()
                            {
                                HeaderItems =
                                    new Dictionary<Guid, ExportedHeaderItem>()
                                    {
                                        {questionId, new ExportedHeaderItem() {VariableName = variableName}}
                                    }
                            }
                        }
                    }
            };
        }

        protected static void PublishEventsOnOnInterviewExportedDataDenormalizer(List<object> eventsToPublish, InterviewHistoryView interviewHistoryView, InterviewHistoryDenormalizer interviewExportedDataDenormalizer)
        {
            foreach (var eventToPublish in eventsToPublish)
            {
                var publishedEventClosedType = typeof(PublishedEvent<>).MakeGenericType(eventToPublish.GetType());
                var handleMethod = typeof(InterviewHistoryDenormalizer).GetMethod("Update", new[] { typeof(InterviewHistoryView), publishedEventClosedType });

                var publishedEvent =
                    (PublishedEvent)
                        Activator.CreateInstance(publishedEventClosedType,
                            new CommittedEvent(Guid.NewGuid(), "", Guid.NewGuid(), interviewHistoryView.InterviewId, 1, DateTime.Now, 0, eventToPublish));

                interviewHistoryView = handleMethod.Invoke(interviewExportedDataDenormalizer, new object[] { interviewHistoryView, publishedEvent }) as InterviewHistoryView;
            }
        }
    }
}
