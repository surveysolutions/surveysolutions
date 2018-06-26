using System;
using System.Collections.Generic;
using System.Linq;
using Main.Core.Entities.SubEntities;
using Ncqrs.Eventing;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Events.Interview.Base;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities.Answers;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.Questionnaire.Documents;

namespace CoreTester.CustomInfrastructure
{
    public class EventsToCommandConverter
    {
        public static ICommand GetCreateInterviewCommand(List<CommittedEvent> committedEvents, Guid interviewId,
            Guid userId)
        {
            var supervisorAssignedEvent = committedEvents.First(x => x.Payload is SupervisorAssigned);
            var supervisorAssigned = supervisorAssignedEvent.Payload as SupervisorAssigned;
            var interviewerAssigned =
                committedEvents.FirstOrDefault(x => x.Payload is InterviewerAssigned)?.Payload as InterviewerAssigned;
            var interviewKey =
                committedEvents.LastOrDefault(x => x.Payload is InterviewKeyAssigned)?.Payload as InterviewKeyAssigned;

            var interviewCreated =
                committedEvents.FirstOrDefault(x => x.Payload is InterviewCreated)?.Payload as InterviewCreated;

            var preloadedAnswers = committedEvents
                .Where(x => x.EventSequence < supervisorAssignedEvent.EventSequence)
                .Where(x => x.Payload is QuestionAnswered)
                .Select(x => x.Payload as QuestionAnswered)
                .Where(x => x!=null)
                .Select(x => new InterviewAnswer
                {
                    Identity = new Identity(x.QuestionId, x.RosterVector),
                    Answer = ConvertEventToInterviewAnswer(x)
                })
                .ToList();

            ICommand createCommand = null;
            if (interviewCreated != null)
            {
                createCommand = new CreateInterview(interviewId, userId,
                    new QuestionnaireIdentity(interviewCreated.QuestionnaireId, interviewCreated.QuestionnaireVersion),
                    preloadedAnswers,
                    new List<string>(), 
                    interviewCreated.CreationTime ?? DateTime.UtcNow,
                    supervisorAssigned.SupervisorId,
                    interviewerAssigned?.InterviewerId,
                    interviewKey?.Key,
                    interviewCreated.AssignmentId
                );
            }
            else
            {
                if (committedEvents.FirstOrDefault(x => x.Payload is InterviewOnClientCreated)?.Payload is
                    InterviewOnClientCreated interviewOnClientCreated)
                {
                    createCommand = new CreateInterview(interviewId, userId,
                        new QuestionnaireIdentity(interviewOnClientCreated.QuestionnaireId,
                            interviewOnClientCreated.QuestionnaireVersion),
                        preloadedAnswers,
                        new List<string>(), 
                        supervisorAssigned.AssignTime ?? DateTime.UtcNow,
                        supervisorAssigned.SupervisorId,
                        interviewerAssigned?.InterviewerId,
                        interviewKey?.Key,
                        interviewOnClientCreated.AssignmentId);
                }
                else if (committedEvents.FirstOrDefault(x => x.Payload is InterviewFromPreloadedDataCreated)?.Payload is
                    InterviewFromPreloadedDataCreated interviewFromPreloadedDataCreated)
                {
                    createCommand = new CreateInterview(interviewId, userId,
                        new QuestionnaireIdentity(interviewFromPreloadedDataCreated.QuestionnaireId,
                            interviewFromPreloadedDataCreated.QuestionnaireVersion),
                        preloadedAnswers,
                        new List<string>(), 
                        supervisorAssigned.AssignTime ?? DateTime.UtcNow,
                        supervisorAssigned.SupervisorId,
                        interviewerAssigned?.InterviewerId,
                        interviewKey?.Key,
                        interviewFromPreloadedDataCreated.AssignmentId);
                }
            }

            return createCommand;
        }

        private static AbstractAnswer ConvertEventToInterviewAnswer(QuestionAnswered questionAnswered)
        {
            switch (questionAnswered)
            {
                case AreaQuestionAnswered area:
                    return AreaAnswer.FromArea(new Area(area.Geometry, area.MapName,
                        area.NumberOfPoints, area.AreaSize, area.Length, area.Coordinates,
                        area.DistanceToEditor));
                case AudioQuestionAnswered audio:
                    return AudioAnswer.FromString(audio.FileName, audio.Length);
                case DateTimeQuestionAnswered dateTime:
                    return DateTimeAnswer.FromDateTime(dateTime.Answer);
                case GeoLocationQuestionAnswered geo:
                    return GpsAnswer.FromGeoPosition(new GeoPosition(geo.Latitude, geo.Longitude, geo.Accuracy, geo.Altitude, geo.Timestamp));
                case MultipleOptionsLinkedQuestionAnswered multiLinked:
                    return CategoricalLinkedMultiOptionAnswer.FromRosterVectors(multiLinked.SelectedRosterVectors.Select(x => new RosterVector(x)));
                case MultipleOptionsQuestionAnswered multipleOptions:
                    return CategoricalFixedMultiOptionAnswer.FromDecimalArray(multipleOptions.SelectedValues);
                case NumericIntegerQuestionAnswered numericInteger:
                    return NumericIntegerAnswer.FromInt(numericInteger.Answer);
                case NumericRealQuestionAnswered numericReal:
                    return NumericRealAnswer.FromDecimal(numericReal.Answer);
                case PictureQuestionAnswered picture:
                    return MultimediaAnswer.FromString(picture.PictureFileName, picture.AnswerTimeUtc);
                case QRBarcodeQuestionAnswered qr:
                    return QRBarcodeAnswer.FromString(qr.Answer);
                case SingleOptionLinkedQuestionAnswered singleLinked:
                    return CategoricalLinkedSingleOptionAnswer.FromRosterVector(singleLinked.SelectedRosterVector);
                case SingleOptionQuestionAnswered single:
                    return CategoricalFixedSingleOptionAnswer.FromDecimal(single.SelectedValue);
                case TextListQuestionAnswered textList:
                    return TextListAnswer.FromTupleArray(textList.Answers);
                case TextQuestionAnswered text:
                    return TextAnswer.FromString(text.Answer);
                case YesNoQuestionAnswered yesNo:
                    return YesNoAnswer.FromAnsweredYesNoOptions(yesNo.AnsweredOptions);
            }

            return null;
        }

        public static IEnumerable<ICommand> ConvertEventToCommands(Guid interviewId, CommittedEvent committedEvent)
        {
            var userId = Guid.NewGuid();
            switch (committedEvent.Payload)
            {
                case AnswerRemoved answerRemoved:
                    return new RemoveAnswerCommand(interviewId, userId,
                        new Identity(answerRemoved.QuestionId, answerRemoved.RosterVector),
                        answerRemoved.RemoveTimeUtc).ToEnumerable();
                case AnswersRemoved answersRemoved:
                    return answersRemoved.Questions.Select(x =>
                        new RemoveAnswerCommand(interviewId, userId, x, committedEvent.EventTimeStamp));
                case AreaQuestionAnswered areaQuestion:
                    return new AnswerGeographyQuestionCommand(interviewId, userId,
                        areaQuestion.QuestionId, areaQuestion.RosterVector, areaQuestion.AnswerTimeUtc,
                        areaQuestion.Geometry,
                        areaQuestion.MapName, 
                        areaQuestion.AreaSize, 
                        areaQuestion.Coordinates, 
                        areaQuestion.Length,
                        areaQuestion.DistanceToEditor,
                        areaQuestion.NumberOfPoints).ToEnumerable();
                case AudioQuestionAnswered audioQuestion:
                    return new AnswerAudioQuestionCommand(interviewId, userId, audioQuestion.QuestionId,
                        audioQuestion.RosterVector, audioQuestion.AnswerTimeUtc,
                        audioQuestion.FileName, audioQuestion.Length).ToEnumerable();
                case DateTimeQuestionAnswered dateTimeQuestion:
                    return new AnswerDateTimeQuestionCommand(interviewId, userId, dateTimeQuestion.QuestionId,
                        dateTimeQuestion.RosterVector,
                        dateTimeQuestion.AnswerTimeUtc, dateTimeQuestion.Answer).ToEnumerable();
                case GeoLocationQuestionAnswered geoLocation:
                    return new AnswerGeoLocationQuestionCommand(interviewId, userId, geoLocation.QuestionId,
                        geoLocation.RosterVector, geoLocation.AnswerTimeUtc,
                        geoLocation.Latitude, geoLocation.Longitude, geoLocation.Accuracy, geoLocation.Altitude,
                        geoLocation.Timestamp).ToEnumerable();
                case MultipleOptionsLinkedQuestionAnswered multipleOptionsLinked:
                    return new AnswerMultipleOptionsLinkedQuestionCommand(interviewId, userId,
                            multipleOptionsLinked.QuestionId, multipleOptionsLinked.RosterVector,
                            multipleOptionsLinked.AnswerTimeUtc,
                            multipleOptionsLinked.SelectedRosterVectors.Select(x => new RosterVector(x)).ToArray())
                        .ToEnumerable();
                case MultipleOptionsQuestionAnswered multipleOptions:
                    return new AnswerMultipleOptionsQuestionCommand(interviewId, userId, multipleOptions.QuestionId,
                        multipleOptions.RosterVector,
                        multipleOptions.AnswerTimeUtc,
                        multipleOptions.SelectedValues.Select(Convert.ToInt32).ToArray()).ToEnumerable();
                case NumericIntegerQuestionAnswered numericInteger:
                    return new AnswerNumericIntegerQuestionCommand(interviewId, userId, numericInteger.QuestionId,
                        numericInteger.RosterVector,
                        numericInteger.AnswerTimeUtc, numericInteger.Answer).ToEnumerable();
                case NumericRealQuestionAnswered numericReal:
                    return new AnswerNumericRealQuestionCommand(interviewId, userId, numericReal.QuestionId,
                        numericReal.RosterVector,
                        numericReal.AnswerTimeUtc, Convert.ToDouble(numericReal.Answer)).ToEnumerable();
                case PictureQuestionAnswered picture:
                    return new AnswerPictureQuestionCommand(interviewId, userId, picture.QuestionId,
                        picture.RosterVector,
                        picture.AnswerTimeUtc, picture.PictureFileName).ToEnumerable();
                case QRBarcodeQuestionAnswered qrBarcode:
                    return new AnswerQRBarcodeQuestionCommand(interviewId, userId, qrBarcode.QuestionId,
                        qrBarcode.RosterVector,
                        qrBarcode.AnswerTimeUtc, qrBarcode.Answer).ToEnumerable();
                case SingleOptionLinkedQuestionAnswered singleOptionLinked:
                    return new AnswerSingleOptionLinkedQuestionCommand(interviewId, userId,
                        singleOptionLinked.QuestionId, singleOptionLinked.RosterVector,
                        singleOptionLinked.AnswerTimeUtc, singleOptionLinked.SelectedRosterVector).ToEnumerable();
                case SingleOptionQuestionAnswered singleOption:
                    return new AnswerSingleOptionQuestionCommand(interviewId, userId, singleOption.QuestionId,
                        singleOption.RosterVector, singleOption.AnswerTimeUtc,
                        Convert.ToInt32(singleOption.SelectedValue)).ToEnumerable();
                case TextListQuestionAnswered textList:
                    return new AnswerTextListQuestionCommand(interviewId, userId, textList.QuestionId,
                        textList.RosterVector,
                        textList.AnswerTimeUtc, textList.Answers).ToEnumerable();
                case TextQuestionAnswered text:
                    return new AnswerTextQuestionCommand(interviewId, userId, text.QuestionId, text.RosterVector,
                        text.AnswerTimeUtc, text.Answer).ToEnumerable();
                case YesNoQuestionAnswered yesNo:
                    return new AnswerYesNoQuestion(interviewId, userId, yesNo.QuestionId, yesNo.RosterVector,
                        yesNo.AnswerTimeUtc, yesNo.AnsweredOptions).ToEnumerable();
                default:
                    return null;
            }

            return null;
        }

    }
}
