using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Main.Core.Entities.SubEntities;
using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.EventBus;
using WB.Core.SharedKernels.DataCollection.DataTransferObjects.Synchronization;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.Utils;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;

namespace WB.Core.BoundedContexts.Interviewer.Views
{
    public class InterviewEventHandler : BaseDenormalizer,
                                         IEventHandler<SynchronizationMetadataApplied>,
                                         IEventHandler<InterviewSynchronized>,
                                         IEventHandler<InterviewStatusChanged>, 
                                         IEventHandler<InterviewHardDeleted>,

                                         IEventHandler<TextQuestionAnswered>,
                                         IEventHandler<MultipleOptionsQuestionAnswered>,
                                         IEventHandler<SingleOptionQuestionAnswered>,
                                         IEventHandler<NumericRealQuestionAnswered>,
                                         IEventHandler<NumericIntegerQuestionAnswered>,
                                         IEventHandler<DateTimeQuestionAnswered>,
                                         IEventHandler<GeoLocationQuestionAnswered>,
                                         IEventHandler<QRBarcodeQuestionAnswered>,
                                         IEventHandler<YesNoQuestionAnswered>,

                                         IEventHandler<AnswersRemoved>,

                                         IEventHandler<InterviewOnClientCreated>,
                                         IEventHandler<AnswerRemoved>
    {
        private readonly IAsyncPlainStorage<InterviewView> interviewViewRepository;
        private readonly IPlainQuestionnaireRepository questionnaireRepository;

        public InterviewEventHandler(IAsyncPlainStorage<InterviewView> interviewViewRepository, IPlainQuestionnaireRepository questionnaireRepository)
        {
            this.interviewViewRepository = interviewViewRepository;
            this.questionnaireRepository = questionnaireRepository;
        }

        public override object[] Writers => new object[] { this.interviewViewRepository };
        public override object[] Readers => new object[] { this.interviewViewRepository};

        public void Handle(IPublishedEvent<SynchronizationMetadataApplied> evnt)
        {
            this.AddOrUpdateInterviewToDashboard(evnt.Payload.QuestionnaireId,
                evnt.Payload.QuestionnaireVersion,
                evnt.EventSourceId,
                evnt.Payload.UserId,
                evnt.Payload.Status,
                evnt.Payload.Comments,
                evnt.Payload.FeaturedQuestionsMeta,
                evnt.Payload.CreatedOnClient,
                false,
                evnt.Payload.InterviewerAssignedDateTime,
                null,
                evnt.Payload.RejectedDateTime);
        }


        public void Handle(IPublishedEvent<InterviewOnClientCreated> evnt)
        {
            this.AddOrUpdateInterviewToDashboard(evnt.Payload.QuestionnaireId,
                evnt.Payload.QuestionnaireVersion,
                evnt.EventSourceId,
                evnt.Payload.UserId,
                InterviewStatus.InterviewerAssigned,
                null,
                new AnsweredQuestionSynchronizationDto[0],
                true,
                true,
                evnt.EventTimeStamp,
                evnt.EventTimeStamp,
                null);
        }


        private void AddOrUpdateInterviewToDashboard(
            Guid questionnaireId, long questionnaireVersion, Guid interviewId, Guid responsibleId, InterviewStatus status,
            string comments, IEnumerable<AnsweredQuestionSynchronizationDto> answeredQuestions,
            bool createdOnClient, bool canBeDeleted,
            DateTime? assignedDateTime, DateTime? startedDateTime, DateTime? rejectedDateTime)
        {
            var questionnaireIdentity = new QuestionnaireIdentity(questionnaireId, questionnaireVersion);
            var questionnaireDocumentView = this.questionnaireRepository.GetQuestionnaireDocument(questionnaireIdentity);

            if (questionnaireDocumentView == null)
                return;

            var prefilledQuestions = new List<InterviewAnswerOnPrefilledQuestionView>();
            var featuredQuestions = questionnaireDocumentView.Find<IQuestion>(q => q.Featured).ToList();

            InterviewGpsCoordinatesView gpsCoordinates = null;
            Guid? prefilledGpsQuestionId = null;

            foreach (var featuredQuestion in featuredQuestions)
            {
                var item = answeredQuestions.FirstOrDefault(q => q.Id == featuredQuestion.PublicKey);

                if (featuredQuestion.QuestionType != QuestionType.GpsCoordinates)
                {
                    prefilledQuestions.Add(this.GetAnswerOnPrefilledQuestion(featuredQuestion, item?.Answer));
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

            var storageInterviewId = interviewId.FormatGuid();
            var interviewView = this.interviewViewRepository.GetById(storageInterviewId) ?? new InterviewView
            {
                Id = storageInterviewId,
                InterviewId = interviewId,
                ResponsibleId = responsibleId,
                QuestionnaireId = questionnaireIdentity.ToString(),
                Census = createdOnClient,
                GpsLocation = new InterviewGpsLocationView
                {
                    PrefilledQuestionId = prefilledGpsQuestionId
                }
            };

            interviewView.Status = status;
            interviewView.AnswersOnPrefilledQuestions = prefilledQuestions.ToArray();
            interviewView.StartedDateTime = startedDateTime;
            interviewView.InterviewerAssignedDateTime = assignedDateTime;
            interviewView.RejectedDateTime = rejectedDateTime;
            interviewView.CanBeDeleted = canBeDeleted;
            interviewView.LastInterviewerOrSupervisorComment = comments;
            interviewView.GpsLocation.Coordinates = gpsCoordinates;
            
            this.interviewViewRepository.StoreAsync(interviewView).Wait();
        }

        private static GeoPosition GetGeoPositionAnswer(AnsweredQuestionSynchronizationDto item)
        {
            if (item == null)
                return null;

            var geoPositionAnswer = item.Answer as GeoPosition;
            if (geoPositionAnswer != null)
                return geoPositionAnswer;

            var geoPositionString = item.Answer as string;
            if (geoPositionString != null)
                return GeoPosition.FromString(geoPositionString);

            return null;
        }

        private InterviewAnswerOnPrefilledQuestionView GetAnswerOnPrefilledQuestion(IQuestion prefilledQuestion, object answer)
        {
            Func<decimal, string> getCategoricalOptionText = null;

            if (answer != null)
            {
                switch (prefilledQuestion.QuestionType)
                {
                    case QuestionType.DateTime:
                        if (answer is string)
                            answer = DateTime.Parse((string)answer).ToLocalTime();
                        break;
                    case QuestionType.MultyOption:
                    case QuestionType.SingleOption:
                        if (answer.GetType().IsArray)
                        {
                            answer = (answer as object[]).Select(x => Convert.ToDecimal(x, CultureInfo.InvariantCulture)).ToArray();
                        }
                        else
                        {
                            answer = Convert.ToDecimal(answer, CultureInfo.InvariantCulture);
                        }
                        getCategoricalOptionText = GetPrefilledCategoricalQuestionOptionText(prefilledQuestion);
                        break;
                }
            }

            return new InterviewAnswerOnPrefilledQuestionView
            {
                QuestionId = prefilledQuestion.PublicKey,
                QuestionText = prefilledQuestion.QuestionText,
                Answer = answer == null ? null : AnswerUtils.AnswerToString(answer, getCategoricalOptionText)
            };
        }

        private static Func<decimal, string> GetPrefilledCategoricalQuestionOptionText(IQuestion prefilledQuestion)
        {
            if(prefilledQuestion.Answers == null) return null;

            var prefilledCategoricalQuestionOptions = prefilledQuestion.Answers.ToDictionary(
                option => decimal.Parse(option.AnswerValue, CultureInfo.InvariantCulture),
                option => option.AnswerText);

            return (optionValue) => prefilledCategoricalQuestionOptions.ContainsKey(optionValue)
                ? prefilledCategoricalQuestionOptions[optionValue]
                : string.Empty;
        }

        public void Handle(IPublishedEvent<InterviewSynchronized> evnt)
        {
            this.AddOrUpdateInterviewToDashboard(evnt.Payload.InterviewData.QuestionnaireId,
                evnt.Payload.InterviewData.QuestionnaireVersion,
                evnt.EventSourceId,
                evnt.Payload.UserId,
                evnt.Payload.InterviewData.Status,
                evnt.Payload.InterviewData.Comments,
                evnt.Payload.InterviewData.Answers,
                evnt.Payload.InterviewData.CreatedOnClient,
                canBeDeleted: false,
                assignedDateTime: evnt.Payload.InterviewData.InterviewerAssignedDateTime,
                startedDateTime: null,
                rejectedDateTime: evnt.Payload.InterviewData.RejectDateTime);
        }

        public void Handle(IPublishedEvent<InterviewHardDeleted> evnt)
        {
            this.interviewViewRepository.RemoveAsync(evnt.EventSourceId.FormatGuid());
        }

        public void Handle(IPublishedEvent<InterviewStatusChanged> evnt)
        {
            if(!this.IsInterviewCompletedOrRestarted(evnt.Payload.Status))
                return;

            InterviewView interviewView = this.interviewViewRepository.GetById(evnt.EventSourceId.FormatGuid());
            if (interviewView == null)
                return;

            if (evnt.Payload.Status == InterviewStatus.Completed)
                interviewView.CompletedDateTime = evnt.EventTimeStamp;

            if (evnt.Payload.Status == InterviewStatus.RejectedBySupervisor)
                interviewView.RejectedDateTime = evnt.EventTimeStamp;

            interviewView.Status = evnt.Payload.Status;
            interviewView.LastInterviewerOrSupervisorComment = evnt.Payload.Comment;

            this.interviewViewRepository.StoreAsync(interviewView).Wait();
        }

        private bool IsInterviewCompletedOrRestarted(InterviewStatus status)
        {
            return status == InterviewStatus.Completed || status == InterviewStatus.Restarted;
        }

        private void AnswerQuestion(Guid interviewId, Guid questionId, object answer, DateTime answerTimeUtc)
        {
            this.AnswerOnPrefilledQuestion(interviewId, questionId, answer, answerTimeUtc);
            this.SetStartedDateTimeOnFirstAnswer(interviewId, answerTimeUtc);
        }

        private readonly HashSet<Guid> interviewsWithExistedStartedDateTime = new HashSet<Guid>();

        private void SetStartedDateTimeOnFirstAnswer(Guid interviewId, DateTime answerTimeUtc)
        {
            if (interviewsWithExistedStartedDateTime.Contains(interviewId))
                return;

            var interviewView = this.interviewViewRepository.GetById(interviewId.FormatGuid());

            if (interviewView == null) return;

            interviewsWithExistedStartedDateTime.Add(interviewId);

            if (!interviewView.StartedDateTime.HasValue)
            {
                interviewView.StartedDateTime = answerTimeUtc;
            }

            this.interviewViewRepository.StoreAsync(interviewView).Wait();
        }

        private readonly Dictionary<Guid, QuestionnaireIdentity> mapInterviewIdToQuestionnaireIdentity = new Dictionary<Guid, QuestionnaireIdentity>();

        private void AnswerOnPrefilledQuestion(Guid interviewId, Guid questionId, object answer, DateTime answerTimeUtc)
        {
            QuestionnaireIdentity questionnaireIdentity;
            if (this.mapInterviewIdToQuestionnaireIdentity.TryGetValue(interviewId, out questionnaireIdentity))
            {
                var questionnaire = this.questionnaireRepository.GetQuestionnaire(questionnaireIdentity);
                if (!questionnaire.IsPrefilled(questionId))
                    return;
            }

            var interviewView = this.interviewViewRepository.GetById(interviewId.FormatGuid());

            if (interviewView == null) return;

            questionnaireIdentity = QuestionnaireIdentity.Parse(interviewView.QuestionnaireId);
            this.mapInterviewIdToQuestionnaireIdentity.Add(interviewId, questionnaireIdentity);

            if (questionId == interviewView.GpsLocation.PrefilledQuestionId)
            {
                var gpsCoordinates = (GeoPosition) answer;

                interviewView.GpsLocation.Coordinates = gpsCoordinates == null
                    ? null
                    : new InterviewGpsCoordinatesView
                    {
                        Latitude = gpsCoordinates.Latitude,
                        Longitude = gpsCoordinates.Longitude
                    };
            }
            else
            {
                var prefilledQuestion = interviewView.AnswersOnPrefilledQuestions?.FirstOrDefault(
                    question => question.QuestionId == questionId);

                if (prefilledQuestion != null)
                {
                    var questionnaire =
                        this.questionnaireRepository.GetQuestionnaireDocument(
                            QuestionnaireIdentity.Parse(interviewView.QuestionnaireId));
                    var questionnairePrefilledQuestion =
                        questionnaire.FirstOrDefault<IQuestion>(question => question.PublicKey == questionId);

                    prefilledQuestion.Answer = AnswerUtils.AnswerToString(answer,
                        GetPrefilledCategoricalQuestionOptionText(questionnairePrefilledQuestion));
                }
            }

            this.interviewViewRepository.StoreAsync(interviewView).Wait();
        }

        public void Handle(IPublishedEvent<TextQuestionAnswered> evnt)
        {
            this.AnswerQuestion(evnt.EventSourceId, evnt.Payload.QuestionId, evnt.Payload.Answer, evnt.Payload.AnswerTimeUtc);
        }
        
        public void Handle(IPublishedEvent<MultipleOptionsQuestionAnswered> evnt)
        {
            this.AnswerQuestion(evnt.EventSourceId, evnt.Payload.QuestionId, evnt.Payload.SelectedValues, evnt.Payload.AnswerTimeUtc);
        }

        public void Handle(IPublishedEvent<SingleOptionQuestionAnswered> evnt)
        {
            this.AnswerQuestion(evnt.EventSourceId, evnt.Payload.QuestionId, evnt.Payload.SelectedValue, evnt.Payload.AnswerTimeUtc);
        }

        public void Handle(IPublishedEvent<NumericRealQuestionAnswered> evnt)
        {
            this.AnswerQuestion(evnt.EventSourceId, evnt.Payload.QuestionId, evnt.Payload.Answer, evnt.Payload.AnswerTimeUtc);
        }

        public void Handle(IPublishedEvent<NumericIntegerQuestionAnswered> evnt)
        {
            this.AnswerQuestion(evnt.EventSourceId, evnt.Payload.QuestionId, evnt.Payload.Answer, evnt.Payload.AnswerTimeUtc);
        }

        public void Handle(IPublishedEvent<DateTimeQuestionAnswered> evnt)
        {
            this.AnswerQuestion(evnt.EventSourceId, evnt.Payload.QuestionId, evnt.Payload.Answer, evnt.Payload.AnswerTimeUtc);
        }

        public void Handle(IPublishedEvent<YesNoQuestionAnswered> evnt)
        {
            this.AnswerQuestion(evnt.EventSourceId, evnt.Payload.QuestionId, evnt.Payload.AnsweredOptions, evnt.Payload.AnswerTimeUtc);
        }

        public void Handle(IPublishedEvent<GeoLocationQuestionAnswered> evnt)
        {
            this.AnswerQuestion(evnt.EventSourceId, evnt.Payload.QuestionId,
                new GeoPosition(latitude: evnt.Payload.Latitude, longitude: evnt.Payload.Longitude,
                    accuracy: evnt.Payload.Accuracy, altitude:evnt.Payload.Altitude,
                    timestamp: evnt.Payload.Timestamp), evnt.Payload.AnswerTimeUtc);
        }

        public void Handle(IPublishedEvent<QRBarcodeQuestionAnswered> evnt)
        {
            this.AnswerQuestion(evnt.EventSourceId, evnt.Payload.QuestionId, evnt.Payload.Answer, evnt.Payload.AnswerTimeUtc);
        }

        public void Handle(IPublishedEvent<AnswersRemoved> evnt)
        {
            foreach (var question in evnt.Payload.Questions)
            {
                this.AnswerQuestion(evnt.EventSourceId, question.Id, null, evnt.EventTimeStamp);
            }
        }

        public void Handle(IPublishedEvent<AnswerRemoved> evnt)
        {
            this.AnswerQuestion(evnt.EventSourceId, evnt.Payload.QuestionId, null, evnt.EventTimeStamp);
        }
    }
}