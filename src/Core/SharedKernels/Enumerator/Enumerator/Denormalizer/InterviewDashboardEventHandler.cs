using System;
using System.Collections.Generic;
using System.Linq;
using Main.Core.Entities.SubEntities;
using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.EventBus;
using WB.Core.Infrastructure.EventBus.Lite;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.DataTransferObjects.Synchronization;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;
using WB.Core.SharedKernels.Enumerator.Views;
using WB.Core.SharedKernels.Questionnaire.Documents;

namespace WB.Core.SharedKernels.Enumerator.Denormalizer
{
    public class InterviewDashboardEventHandler : BaseDenormalizer,
                                         ILitePublishedEventHandler<InterviewCreated>,
                                         ILitePublishedEventHandler<SynchronizationMetadataApplied>,
                                         ILitePublishedEventHandler<InterviewSynchronized>,
                                         ILitePublishedEventHandler<InterviewStatusChanged>,
                                         ILitePublishedEventHandler<InterviewHardDeleted>,
                                         ILitePublishedEventHandler<InterviewerAssigned>,
                                         ILitePublishedEventHandler<SupervisorAssigned>,

                                         ILitePublishedEventHandler<TextQuestionAnswered>,
                                         ILitePublishedEventHandler<MultipleOptionsQuestionAnswered>,
                                         ILitePublishedEventHandler<SingleOptionQuestionAnswered>,
                                         ILitePublishedEventHandler<NumericRealQuestionAnswered>,
                                         ILitePublishedEventHandler<NumericIntegerQuestionAnswered>,
                                         ILitePublishedEventHandler<DateTimeQuestionAnswered>,
                                         ILitePublishedEventHandler<GeoLocationQuestionAnswered>,
                                         ILitePublishedEventHandler<QRBarcodeQuestionAnswered>,
                                         ILitePublishedEventHandler<YesNoQuestionAnswered>,
                                         ILitePublishedEventHandler<TextListQuestionAnswered>,
                                         ILitePublishedEventHandler<PictureQuestionAnswered>,
                                         ILitePublishedEventHandler<AudioQuestionAnswered>,
                                         ILitePublishedEventHandler<AnswersRemoved>,
                                         ILitePublishedEventHandler<InterviewOnClientCreated>,
                                         ILitePublishedEventHandler<InterviewFromPreloadedDataCreated>,
                                         ILitePublishedEventHandler<AnswerRemoved>,

                                         ILitePublishedEventHandler<TranslationSwitched>,
                                         ILitePublishedEventHandler<MultipleOptionsLinkedQuestionAnswered>,
                                         ILitePublishedEventHandler<SingleOptionLinkedQuestionAnswered>,
                                         ILitePublishedEventHandler<AreaQuestionAnswered>,
                                         ILitePublishedEventHandler<InterviewKeyAssigned>
    {
        private readonly IPlainStorage<InterviewView> interviewViewRepository;
        private readonly IPlainStorage<PrefilledQuestionView> prefilledQuestions;
        private readonly IQuestionnaireStorage questionnaireRepository;
        private readonly IAnswerToStringConverter answerToStringConverter;

        public InterviewDashboardEventHandler(IPlainStorage<InterviewView> interviewViewRepository, 
            IPlainStorage<PrefilledQuestionView> prefilledQuestions,
            IQuestionnaireStorage questionnaireRepository,
            ILiteEventRegistry liteEventRegistry,
            IAnswerToStringConverter answerToStringConverter)
        {
            this.interviewViewRepository = interviewViewRepository;
            this.prefilledQuestions = prefilledQuestions;
            this.questionnaireRepository = questionnaireRepository;
            this.answerToStringConverter = answerToStringConverter;

            liteEventRegistry.Subscribe(this);
        }

        public override object[] Writers => new object[] { this.interviewViewRepository };
        public override object[] Readers => new object[] { this.interviewViewRepository};

        public void Handle(IPublishedEvent<SynchronizationMetadataApplied> evnt)
        {
            var payload = evnt.Payload;

            this.AddOrUpdateInterviewToDashboard(payload.QuestionnaireId,
                payload.QuestionnaireVersion,
                evnt.EventSourceId,
                payload.UserId,
                payload.Status,
                payload.Comments,
                payload.FeaturedQuestionsMeta,
                payload.CreatedOnClient,
                canBeDeleted: false,
                assignedDateTime: payload.InterviewerAssignedDateTime,
                startedDateTime: null,
                rejectedDateTime: payload.RejectedDateTime,
                assignmentId: null,
                interviewKey: null);
        }

        public void Handle(IPublishedEvent<InterviewCreated> evnt)
        {
            var payload = evnt.Payload;
            this.AddOrUpdateInterviewToDashboard(payload.QuestionnaireId,
                payload.QuestionnaireVersion,
                evnt.EventSourceId,
                payload.UserId,
                InterviewStatus.InterviewerAssigned,
                comments: null,
                answeredQuestions: new AnsweredQuestionSynchronizationDto[0],
                createdOnClient: true,
                canBeDeleted: true,
                assignedDateTime: evnt.EventTimeStamp,
                startedDateTime: evnt.EventTimeStamp,
                rejectedDateTime: null,
                assignmentId: payload.AssignmentId,
                interviewKey: null);
        }

        public void Handle(IPublishedEvent<InterviewOnClientCreated> evnt)
        {
            this.AddOrUpdateInterviewToDashboard(evnt.Payload.QuestionnaireId,
                evnt.Payload.QuestionnaireVersion,
                evnt.EventSourceId,
                evnt.Payload.UserId,
                InterviewStatus.InterviewerAssigned,
                comments: null,
                answeredQuestions: new AnsweredQuestionSynchronizationDto[0],
                createdOnClient: true,
                canBeDeleted: true,
                assignedDateTime: evnt.EventTimeStamp,
                startedDateTime: evnt.EventTimeStamp,
                rejectedDateTime: null, 
                assignmentId: evnt.Payload.AssignmentId,
                interviewKey: null);
        }

        public void Handle(IPublishedEvent<InterviewFromPreloadedDataCreated> evnt)
        {
            this.AddOrUpdateInterviewToDashboard(evnt.Payload.QuestionnaireId,
                evnt.Payload.QuestionnaireVersion,
                evnt.EventSourceId,
                evnt.Payload.UserId,
                InterviewStatus.InterviewerAssigned,
                comments: null,
                answeredQuestions: new AnsweredQuestionSynchronizationDto[0],
                createdOnClient: true,
                canBeDeleted: true,
                assignedDateTime: evnt.EventTimeStamp,
                startedDateTime: evnt.EventTimeStamp,
                rejectedDateTime: null,
                assignmentId: evnt.Payload.AssignmentId,
                interviewKey: null);
        }
        
        private void AddOrUpdateInterviewToDashboard(Guid questionnaireId, 
            long questionnaireVersion, 
            Guid interviewId, 
            Guid responsibleId, 
            InterviewStatus status, 
            string comments, 
            IEnumerable<AnsweredQuestionSynchronizationDto> answeredQuestions, 
            bool createdOnClient, 
            bool canBeDeleted, 
            DateTime? assignedDateTime, 
            DateTime? startedDateTime, 
            DateTime? rejectedDateTime, 
            int? assignmentId, 
            InterviewKey interviewKey)
        {
            var questionnaireIdentity = new QuestionnaireIdentity(questionnaireId, questionnaireVersion);
            var questionnaireDocumentView = this.questionnaireRepository.GetQuestionnaireDocument(questionnaireIdentity);
            
            if (questionnaireDocumentView == null)
                return;

            var storageInterviewId = interviewId.FormatGuid();
            var interviewView = this.interviewViewRepository.GetById(storageInterviewId) ?? new InterviewView
            {
                Id = storageInterviewId,
                InterviewId = interviewId,
                ResponsibleId = responsibleId,
                QuestionnaireId = questionnaireIdentity.ToString(),
                Census = createdOnClient,
                QuestionnaireTitle = questionnaireDocumentView.Title,
                Status = status,
                StartedDateTime = startedDateTime,
                InterviewerAssignedDateTime = assignedDateTime,
                RejectedDateTime = rejectedDateTime,
                CanBeDeleted = canBeDeleted,
                Assignment = assignmentId,
                LastInterviewerOrSupervisorComment = comments
            };

            var questionnaire = this.questionnaireRepository.GetQuestionnaire(questionnaireIdentity, interviewView.Language);

            var prefilledQuestionsList = new List<PrefilledQuestionView>();
            var featuredQuestions = questionnaireDocumentView.Children
                                                             .TreeToEnumerableDepthFirst(x => x.Children)
                                                             .OfType<IQuestion>()
                                                             .Where(q => q.Featured).ToList();

            InterviewGpsCoordinatesView gpsCoordinates = null;
            Guid? prefilledGpsQuestionId = null;

            int prefilledQuestionSortIndex = 0;
            foreach (var featuredQuestion in featuredQuestions)
            {
                var item = answeredQuestions.FirstOrDefault(q => q.Id == featuredQuestion.PublicKey);

                if (featuredQuestion.QuestionType != QuestionType.GpsCoordinates)
                {
                    var answerOnPrefilledQuestion = this.GetAnswerOnPrefilledQuestion(featuredQuestion.PublicKey, questionnaire, item?.Answer, interviewId);
                    answerOnPrefilledQuestion.SortIndex = prefilledQuestionSortIndex;
                    prefilledQuestionSortIndex++;
                    prefilledQuestionsList.Add(answerOnPrefilledQuestion);
                }
                else
                {
                    prefilledGpsQuestionId = featuredQuestion.PublicKey;

                    var answerOnPrefilledGeolocationQuestion = GetGeoPositionAnswer(item);
                    if (answerOnPrefilledGeolocationQuestion != null)
                    {
                        gpsCoordinates = new InterviewGpsCoordinatesView
                        {
                            Latitude = answerOnPrefilledGeolocationQuestion.Latitude,
                            Longitude = answerOnPrefilledGeolocationQuestion.Longitude
                        };
                    }
                }
            }

            interviewView.LocationQuestionId = prefilledGpsQuestionId;
            interviewView.Assignment = assignmentId;
            interviewView.InterviewKey = interviewKey?.ToString();

            var existingPrefilledForInterview = this.prefilledQuestions.Where(x => x.InterviewId == interviewId).ToList();
            this.prefilledQuestions.Remove(existingPrefilledForInterview);
            this.prefilledQuestions.Store(prefilledQuestionsList);

            if (gpsCoordinates != null)
            {
                interviewView.LocationLatitude = gpsCoordinates.Latitude;
                interviewView.LocationLongitude = gpsCoordinates.Longitude;
            }

            this.interviewViewRepository.Store(interviewView);
        }

        private static GeoPosition GetGeoPositionAnswer(AnsweredQuestionSynchronizationDto item)
        {
            if (item == null)
                return null;

            if (item.Answer is GeoPosition geoPositionAnswer)
                return geoPositionAnswer;

            if (item.Answer is string geoPositionString)
                return GeoPosition.FromString(geoPositionString);

            return null;
        }

        private PrefilledQuestionView GetAnswerOnPrefilledQuestion(Guid prefilledQuestion, IQuestionnaire questionnaire, object answer, Guid interviewId)
        {
            string stringAnswer = this.answerToStringConverter.Convert(answer, prefilledQuestion, questionnaire);

            return new PrefilledQuestionView
            {
                Id = $"{interviewId:N}${prefilledQuestion:N}",
                InterviewId = interviewId,
                QuestionId = prefilledQuestion,
                QuestionText = questionnaire.GetQuestionTitle(prefilledQuestion),
                Answer = stringAnswer
            };
        }

        public void Handle(IPublishedEvent<InterviewSynchronized> evnt)
        {
            var payload = evnt.Payload;

            this.AddOrUpdateInterviewToDashboard(payload.InterviewData.QuestionnaireId,
                payload.InterviewData.QuestionnaireVersion,
                evnt.EventSourceId,
                payload.UserId,
                payload.InterviewData.Status,
                payload.InterviewData.Comments,
                payload.InterviewData.Answers,
                payload.InterviewData.CreatedOnClient,
                canBeDeleted: false,
                assignedDateTime: payload.InterviewData.InterviewerAssignedDateTime,
                startedDateTime: null,
                rejectedDateTime: payload.InterviewData.RejectDateTime,
                assignmentId: payload.InterviewData.AssignmentId,
                interviewKey: payload.InterviewData.InterviewKey);
        }

        public void Handle(IPublishedEvent<InterviewHardDeleted> evnt)
        {
            this.interviewViewRepository.Remove(evnt.EventSourceId.FormatGuid());
        }

        public void Handle(IPublishedEvent<InterviewStatusChanged> evnt)
        {
            if(!this.IsInterviewStatusShouldBeHandled(evnt.Payload.Status))
                return;

            InterviewView interviewView = this.interviewViewRepository.GetById(evnt.EventSourceId.FormatGuid());
            if (interviewView == null)
                return;

            if (evnt.Payload.Status == InterviewStatus.Completed)
            {
                interviewView.CompletedDateTime = evnt.Payload.UtcTime ?? evnt.EventTimeStamp;
            }

            if (evnt.Payload.Status == InterviewStatus.RejectedBySupervisor ||
                evnt.Payload.Status == InterviewStatus.RejectedByHeadquarters)
            {
                interviewView.RejectedDateTime = evnt.Payload.UtcTime ?? evnt.EventTimeStamp;
                interviewView.LastInterviewerOrSupervisorComment = evnt.Payload.Comment;
                interviewView.CanBeDeleted = false;
            }

            if (evnt.Payload.Status == InterviewStatus.ApprovedBySupervisor)
            {
                interviewView.ApprovedDateTimeUtc = evnt.Payload.UtcTime;
            }
            
            interviewView.ReceivedByInterviewerAtUtc = null;
            interviewView.Status = evnt.Payload.Status;
            interviewView.LastInterviewerOrSupervisorComment = evnt.Payload.Comment;

            this.interviewViewRepository.Store(interviewView);
        }

        private bool IsInterviewStatusShouldBeHandled(InterviewStatus status)
        {
            return status == InterviewStatus.Completed || 
                   status == InterviewStatus.Restarted || 
                   status == InterviewStatus.RejectedBySupervisor ||
                   status == InterviewStatus.ApprovedBySupervisor ||
                   status == InterviewStatus.RejectedByHeadquarters;
        }

        private void AnswerQuestion(Guid interviewId, Guid questionId, object answer, DateTime answerTimeUtc)
        {
            this.AnswerOnPrefilledQuestion(interviewId, questionId, answer);
            this.SetStartedDateTimeOnFirstAnswer(interviewId, questionId, answerTimeUtc);
        }

        private readonly HashSet<Guid> interviewsWithExistedStartedDateTime = new HashSet<Guid>();

        private void SetStartedDateTimeOnFirstAnswer(Guid interviewId, Guid questionId, DateTime answerTimeUtc)
        {
            if (this.interviewsWithExistedStartedDateTime.Contains(interviewId))
                return;

            var interviewView = this.interviewViewRepository.GetById(interviewId.FormatGuid());
            if (interviewView == null) return;

            var questionnaireIdentity = QuestionnaireIdentity.Parse(interviewView.QuestionnaireId);
            var questionnaire = this.questionnaireRepository.GetQuestionnaire(questionnaireIdentity, interviewView.Language);
            if (questionnaire.IsPrefilled(questionId))
                return;

            var questionScope = questionnaire.GetQuestionScope(questionId);
            if (questionScope != QuestionScope.Interviewer)
                return;

            this.interviewsWithExistedStartedDateTime.Add(interviewId);

            if (!interviewView.StartedDateTime.HasValue)
            {
                interviewView.StartedDateTime = answerTimeUtc;
            }

            this.interviewViewRepository.Store(interviewView);
        }

        private void AnswerOnPrefilledQuestion(Guid interviewId, Guid questionId, object answer)
        {
            var interviewView = this.interviewViewRepository.GetById(interviewId.FormatGuid());
            if (interviewView == null) return;

            var questionnaireIdentity = QuestionnaireIdentity.Parse(interviewView.QuestionnaireId);

            var questionnaire = this.questionnaireRepository.GetQuestionnaire(questionnaireIdentity, interviewView.Language);
            if (!questionnaire.IsPrefilled(questionId))
                return;
            
            if (questionId == interviewView.LocationQuestionId)
            {
                var gpsCoordinates = (GeoPosition) answer;

                if (gpsCoordinates == null)
                {
                    interviewView.LocationLongitude = interviewView.LocationLatitude = null;
                }
                else
                {
                    interviewView.LocationLongitude = gpsCoordinates.Longitude;
                    interviewView.LocationLatitude = gpsCoordinates.Latitude;
                }
            }
            else
            {
                var newPrefilledQuestionToStore = this.GetAnswerOnPrefilledQuestion(questionId, questionnaire, answer, interviewId);

                var interviewPrefilledQuestion = this.prefilledQuestions.Where(question => question.QuestionId == questionId && question.InterviewId == interviewId).FirstOrDefault()
                       ?? newPrefilledQuestionToStore;
                if (interviewPrefilledQuestion != null)
                {
                    interviewPrefilledQuestion.Answer = newPrefilledQuestionToStore.Answer;
                }

                this.prefilledQuestions.Store(interviewPrefilledQuestion);
            }

            this.interviewViewRepository.Store(interviewView);
        }

        public void Handle(IPublishedEvent<TextQuestionAnswered> evnt)
        {
            this.AnswerQuestion(evnt.EventSourceId, evnt.Payload.QuestionId, evnt.Payload.Answer, evnt.Payload.OriginDate?.UtcDateTime ?? evnt.Payload.AnswerTimeUtc.Value);
        }
        
        public void Handle(IPublishedEvent<MultipleOptionsQuestionAnswered> evnt)
        {
            this.AnswerQuestion(evnt.EventSourceId, evnt.Payload.QuestionId, evnt.Payload.SelectedValues, evnt.Payload.OriginDate?.UtcDateTime ?? evnt.Payload.AnswerTimeUtc.Value);
        }

        public void Handle(IPublishedEvent<SingleOptionQuestionAnswered> evnt)
        {
            this.AnswerQuestion(evnt.EventSourceId, evnt.Payload.QuestionId, evnt.Payload.SelectedValue, evnt.Payload.OriginDate?.UtcDateTime ?? evnt.Payload.AnswerTimeUtc.Value);
        }

        public void Handle(IPublishedEvent<NumericRealQuestionAnswered> evnt)
        {
            this.AnswerQuestion(evnt.EventSourceId, evnt.Payload.QuestionId, evnt.Payload.Answer, evnt.Payload.OriginDate?.UtcDateTime ?? evnt.Payload.AnswerTimeUtc.Value);
        }

        public void Handle(IPublishedEvent<NumericIntegerQuestionAnswered> evnt)
        {
            this.AnswerQuestion(evnt.EventSourceId, evnt.Payload.QuestionId, evnt.Payload.Answer, evnt.Payload.OriginDate?.UtcDateTime ?? evnt.Payload.AnswerTimeUtc.Value);
        }

        public void Handle(IPublishedEvent<DateTimeQuestionAnswered> evnt)
        {
            this.AnswerQuestion(evnt.EventSourceId, evnt.Payload.QuestionId, evnt.Payload.Answer, evnt.Payload.OriginDate?.UtcDateTime ?? evnt.Payload.AnswerTimeUtc.Value);
        }

        public void Handle(IPublishedEvent<YesNoQuestionAnswered> evnt)
        {
            this.AnswerQuestion(evnt.EventSourceId, evnt.Payload.QuestionId, evnt.Payload.AnsweredOptions, evnt.Payload.OriginDate?.UtcDateTime ?? evnt.Payload.AnswerTimeUtc.Value);
        }

        public void Handle(IPublishedEvent<GeoLocationQuestionAnswered> evnt)
        {
            this.AnswerQuestion(evnt.EventSourceId, evnt.Payload.QuestionId,
                new GeoPosition(evnt.Payload.Latitude, evnt.Payload.Longitude,
                    evnt.Payload.Accuracy, evnt.Payload.Altitude,
                    evnt.Payload.Timestamp), 
                evnt.Payload.OriginDate?.UtcDateTime ?? evnt.Payload.AnswerTimeUtc.Value);
        }

        public void Handle(IPublishedEvent<QRBarcodeQuestionAnswered> evnt)
        {
            this.AnswerQuestion(evnt.EventSourceId, evnt.Payload.QuestionId, evnt.Payload.Answer, 
                evnt.Payload.OriginDate?.UtcDateTime ?? evnt.Payload.AnswerTimeUtc.Value);
        }

        public void Handle(IPublishedEvent<TextListQuestionAnswered> evnt)
        {
            this.AnswerQuestion(evnt.EventSourceId, evnt.Payload.QuestionId, evnt.Payload.Answers,
                evnt.Payload.OriginDate?.UtcDateTime ?? evnt.Payload.AnswerTimeUtc.Value);
        }

        public void Handle(IPublishedEvent<PictureQuestionAnswered> evnt)
        {
            this.AnswerQuestion(evnt.EventSourceId, evnt.Payload.QuestionId, evnt.Payload.PictureFileName,
                evnt.Payload.OriginDate?.UtcDateTime ?? evnt.Payload.AnswerTimeUtc.Value);
        }

        public void Handle(IPublishedEvent<MultipleOptionsLinkedQuestionAnswered> evnt)
        {
            this.AnswerQuestion(evnt.EventSourceId, evnt.Payload.QuestionId, evnt.Payload.SelectedRosterVectors,
                evnt.Payload.OriginDate?.UtcDateTime ?? evnt.Payload.AnswerTimeUtc.Value);
        }

        public void Handle(IPublishedEvent<SingleOptionLinkedQuestionAnswered> evnt)
        {
            this.AnswerQuestion(evnt.EventSourceId, evnt.Payload.QuestionId, evnt.Payload.SelectedRosterVector,
                evnt.Payload.OriginDate?.UtcDateTime ?? evnt.Payload.AnswerTimeUtc.Value);
        }

        public void Handle(IPublishedEvent<AnswersRemoved> evnt)
        {
            foreach (var question in evnt.Payload.Questions)
            {
                this.AnswerQuestion(evnt.EventSourceId, question.Id, answer: null, answerTimeUtc: evnt.EventTimeStamp);
            }
        }

        public void Handle(IPublishedEvent<AnswerRemoved> evnt)
        {
            this.AnswerQuestion(evnt.EventSourceId, evnt.Payload.QuestionId, answer: null, answerTimeUtc: evnt.EventTimeStamp);
        }

        public void Handle(IPublishedEvent<TranslationSwitched> @event)
        {
            var interviewView = this.interviewViewRepository.GetById(@event.EventSourceId.FormatGuid());
            if (interviewView == null) return;

            interviewView.Language = @event.Payload.Language;

            this.interviewViewRepository.Store(interviewView);
        }

        public void Handle(IPublishedEvent<AreaQuestionAnswered> evnt)
        {
            this.AnswerQuestion(evnt.EventSourceId, evnt.Payload.QuestionId, new Area(evnt.Payload.Geometry, evnt.Payload.MapName, evnt.Payload.NumberOfPoints,
                evnt.Payload.AreaSize, evnt.Payload.Length, evnt.Payload.Coordinates, evnt.Payload.DistanceToEditor),
                evnt.Payload.OriginDate?.UtcDateTime ?? evnt.Payload.AnswerTimeUtc.Value);
        }

        public void Handle(IPublishedEvent<InterviewKeyAssigned> @event)
        {
            InterviewView interviewView = this.interviewViewRepository.GetById(@event.EventSourceId.FormatGuid());
            if (interviewView == null)
                return;

            interviewView.InterviewKey = @event.Payload.Key.ToString();
            this.interviewViewRepository.Store(interviewView);
        }

        public void Handle(IPublishedEvent<AudioQuestionAnswered> evnt)
        {
            this.AnswerQuestion(evnt.EventSourceId, evnt.Payload.QuestionId, evnt.Payload.Length, 
                evnt.Payload.OriginDate?.UtcDateTime ?? evnt.Payload.AnswerTimeUtc.Value);
        }

        public void Handle(IPublishedEvent<InterviewerAssigned> @event)
        {
            InterviewView interviewView = this.interviewViewRepository.GetById(@event.EventSourceId.FormatGuid());
            if (interviewView == null)
                return;

            if (@event.Payload.InterviewerId.HasValue)
            {
                interviewView.ResponsibleId = @event.Payload.InterviewerId.Value;
            }

            interviewView.InterviewerAssignedDateTime = @event.Payload.AssignTime;
            interviewView.ReceivedByInterviewerAtUtc = null;

            this.interviewViewRepository.Store(interviewView);
        }

        public void Handle(IPublishedEvent<SupervisorAssigned> @event)
        {
            InterviewView interviewView = this.interviewViewRepository.GetById(@event.EventSourceId.FormatGuid());
            if (interviewView == null)
                return;

            interviewView.ResponsibleId = @event.Payload.SupervisorId;
            interviewView.InterviewerAssignedDateTime = @event.Payload.AssignTime;
            this.interviewViewRepository.Store(interviewView);
        }
    }
}
