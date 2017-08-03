using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities.Answers;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.WebApi;
using WB.Infrastructure.Native.Fetching;

namespace WB.Core.BoundedContexts.Headquarters.Assignments
{
    internal class AssignmentsService : IAssignmentsService
    {
        private readonly IPlainStorageAccessor<Assignment> assignmentsAccessor;
        private readonly IInterviewAnswerSerializer answerSerializer;

        public AssignmentsService(IPlainStorageAccessor<Assignment> assignmentsAccessor, IInterviewAnswerSerializer answerSerializer)
        {
            this.assignmentsAccessor = assignmentsAccessor;
            this.answerSerializer = answerSerializer;
        }

        public List<Assignment> GetAssignments(Guid responsibleId)
        {
            return this.assignmentsAccessor.Query(x =>
            x.Where(assigment =>
                assigment.ResponsibleId == responsibleId
                && !assigment.Archived
                && (assigment.Quantity == null || assigment.InterviewSummaries.Count(i => i.IsDeleted == false) < assigment.Quantity))
            .ToList());
        }

        public Assignment GetAssignment(int id)
        {
            return this.assignmentsAccessor.GetById(id);
        }

        public List<Assignment> GetAssignmentsReadyForWebInterview(QuestionnaireIdentity questionnaireId)
        {
            var assignmentsReadyForWebInterview = this.assignmentsAccessor.Query(_ => _
                .Where(ReadyForWebInterviewAssignments(questionnaireId))
                .OrderBy(x => x.Id)
                .Fetch(x => x.IdentifyingData)
                .ToList());
            return assignmentsReadyForWebInterview;
        }

        public int GetCountOfAssignmentsReadyForWebInterview(QuestionnaireIdentity questionnaireId)
        {
            return this.assignmentsAccessor.Query(_ => _.Count(ReadyForWebInterviewAssignments(questionnaireId)));
        }

        private static Expression<Func<Assignment, bool>> ReadyForWebInterviewAssignments(QuestionnaireIdentity questionnaireId)
        {
            Expression<Func<Assignment, bool>> readyForWebInterviewAssignments =
                x => x.QuestionnaireId.QuestionnaireId == questionnaireId.QuestionnaireId &&
                     x.QuestionnaireId.Version == questionnaireId.Version &&
                     x.Responsible.ReadonlyProfile.SupervisorId != null &&
                     !x.Archived &&
                     (x.Quantity == null || x.InterviewSummaries.Count(i => i.IsDeleted == false) < x.Quantity);
            return readyForWebInterviewAssignments;
        }

        public AssignmentApiDocument MapAssignment(Assignment assignment)
        {
            var assignmentApiView = new AssignmentApiDocument
            {
                Id = assignment.Id,
                QuestionnaireId = assignment.QuestionnaireId,
                Quantity = assignment.InterviewsNeeded + assignment.InterviewsProvided,
                CreatedAtUtc = assignment.CreatedAtUtc
            };

            var assignmentIdentifyingData = assignment.IdentifyingData.ToLookup(id => id.Identity);

            foreach (var answer in assignment.Answers ?? Enumerable.Empty<InterviewAnswer>())
            {
                var serializedAnswer = new AssignmentApiDocument.InterviewSerializedAnswer
                {
                    Identity = answer.Identity,
                    SerializedAnswer = this.answerSerializer.Serialize(answer.Answer)
                };

                var identifyingAnswer = assignmentIdentifyingData[answer.Identity].FirstOrDefault();

                if (identifyingAnswer != null)
                {
                    if (answer.Answer is GpsAnswer gpsAnswer)
                    {
                        assignmentApiView.LocationLatitude = gpsAnswer.Value.Latitude;
                        assignmentApiView.LocationLongitude = gpsAnswer.Value.Longitude;
                        assignmentApiView.LocationQuestionId = identifyingAnswer.Identity.Id;
                    }
                }

                assignmentApiView.Answers.Add(serializedAnswer);
            }

            return assignmentApiView;
        }
    }
}