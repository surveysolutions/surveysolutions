#nullable enable

using System;
using System.Linq;
using Main.Core.Entities.SubEntities;
using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.Infrastructure.EventHandlers;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.Events.Assignment;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities.Answers;
using WB.Core.SharedKernels.DataCollection.Repositories;

namespace WB.Core.BoundedContexts.Headquarters.Assignments
{
    public class AssignmentGeoLocationAnswersDenormalizer:
        AbstractFunctionalEventHandlerOnGuid<Assignment, IReadSideRepositoryWriter<Assignment, Guid>>,
        IUpdateHandler<Assignment, AssignmentCreated>
    {
        private readonly IQuestionnaireStorage questionnaireStorage;

        public AssignmentGeoLocationAnswersDenormalizer(IReadSideRepositoryWriter<Assignment, Guid> readSideStorage,
            IQuestionnaireStorage questionnaireStorage)
            : base(readSideStorage)
        {
            this.questionnaireStorage = questionnaireStorage;
        }

        public Assignment Update(Assignment assignment, IPublishedEvent<AssignmentCreated> @event)
        {
            // var questionnaire = this.questionnaireStorage.GetQuestionnaireOrThrow(assignment.QuestionnaireId, null);
            //
            // var gpsAnswers = @event.Payload.Answers
            //     .Where(x => questionnaire.GetQuestionType(x.Identity.Id) == QuestionType.GpsCoordinates)
            //     .ToList();
            //
            // if (gpsAnswers.Count == 0)
            //     return assignment;
            //
            // foreach (var interviewAnswer in gpsAnswers)
            // {
            //     var gpsAnswer = (GpsAnswer) interviewAnswer.Answer;
            //     assignment.GpsAnswers.Add(new AssignmentGps()
            //     {
            //         Assignment = assignment,
            //         Latitude = gpsAnswer.Value.Latitude,
            //         Longitude = gpsAnswer.Value.Longitude,
            //         Timestamp = gpsAnswer.Value.Timestamp,
            //         QuestionId = interviewAnswer.Identity.Id,
            //         RosterVector = interviewAnswer.Identity.RosterVector.ToString(),
            //     });
            // }

            return assignment;
        }
    }
}