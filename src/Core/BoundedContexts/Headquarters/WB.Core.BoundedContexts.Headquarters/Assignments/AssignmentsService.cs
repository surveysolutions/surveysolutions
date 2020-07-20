using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities.Answers;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.WebApi;
using WB.Infrastructure.Native.Fetching;

namespace WB.Core.BoundedContexts.Headquarters.Assignments
{
    internal class AssignmentsService : IAssignmentsService
    {
        private readonly IQueryableReadSideRepositoryReader<Assignment, Guid> assignmentsAccessor;
        private readonly IInterviewAnswerSerializer answerSerializer;

        public AssignmentsService(
            IQueryableReadSideRepositoryReader<Assignment, Guid> assignmentsAccessor,
            IInterviewAnswerSerializer answerSerializer)
        {
            this.assignmentsAccessor = assignmentsAccessor;
            this.answerSerializer = answerSerializer;
        }

        public List<Assignment> GetAssignments(Guid responsibleId)
        {
            return this.assignmentsAccessor.Query(x =>
            x.Where(assignment =>
                assignment.ResponsibleId == responsibleId
                && !assignment.Archived
                && (assignment.Quantity == null || assignment.InterviewSummaries.Count < assignment.Quantity)
                && (assignment.WebMode == null || assignment.WebMode == false))
            .ToList());
        }

        public List<Assignment> GetAssignmentsForSupervisor(Guid supervisorId)
        {
            return this.assignmentsAccessor.Query(x =>
                x.Where(assignment =>
                        (assignment.ResponsibleId == supervisorId || assignment.Responsible.ReadonlyProfile.SupervisorId == supervisorId)
                        && !assignment.Archived
                        && (assignment.Quantity == null || assignment.InterviewSummaries.Count < assignment.Quantity)
                        && (assignment.WebMode == null || assignment.WebMode == false))
                    .ToList());
        }

        public List<Guid> GetAllAssignmentIds(Guid responsibleId)
        {
            return this.assignmentsAccessor.Query(x =>
                x.Where(assignment => assignment.ResponsibleId == responsibleId)
                .Select(assignment => assignment.PublicKey)
                .ToList());
        }

        public Assignment GetAssignment(int id)
        {
            var assignment = this.assignmentsAccessor.Query(_ => _.Where(a => a.Id == id));
            return assignment.SingleOrDefault();
        }

        public Assignment GetAssignmentByAggregateRootId(Guid id)
        {
            return this.assignmentsAccessor.GetById(id);
        }

        public bool HasAssignmentWithAudioRecordingEnabled(Guid responsible)
        {
            bool result = this.assignmentsAccessor.Query(_ => _
                .Any(a =>
                    a.ResponsibleId == responsible
                    && !a.Archived
                    && a.AudioRecording));

            return result;
        }

        public bool HasAssignmentWithAudioRecordingEnabled(QuestionnaireIdentity questionnaireIdentity)
        {
            bool result = this.assignmentsAccessor.Query(_ => _
                .Any(a =>
                    a.QuestionnaireId == questionnaireIdentity
                    && !a.Archived
                    && a.AudioRecording));

            return result;
        }

        public bool DoesExistPasswordInDb(QuestionnaireIdentity questionnaireIdentity, string password)
        {
            var hasPasswordInDb = this.assignmentsAccessor.Query(x =>
                x.Any(y => y.Quantity == 1 &&
                           (y.WebMode == null || y.WebMode == true) &&
                           y.QuestionnaireId == questionnaireIdentity &&
                           (y.Email == null || y.Email == "") &&
                           y.Password == password));

            return hasPasswordInDb;
        }

        public List<int> GetAllAssignmentIdsForMigrateToNewVersion(QuestionnaireIdentity questionnaireIdentity)
        {
            var idsToMigrate = assignmentsAccessor.Query(_ =>
                _.Where(x => x.QuestionnaireId.Id == questionnaireIdentity.Id && x.QuestionnaireId.Version == questionnaireIdentity.Version && !x.Archived)
                    .Select(x => x.Id).ToList());
            return idsToMigrate;
        }

        public bool HasAssignmentWithProtectedVariables(Guid responsibleId)
        {
            List<List<string>> listOfProtectedVariablesFromAssignments = this.assignmentsAccessor.Query(_ => _
                .Where(assignment =>
                    assignment.ResponsibleId == responsibleId
                    && !assignment.Archived
                    && (assignment.Quantity == null || assignment.InterviewSummaries.Count < assignment.Quantity))
                .Select(x => x.ProtectedVariables)
                .ToList());

            bool result = listOfProtectedVariablesFromAssignments.Any(x => (x?.Count ?? 0) > 0);
            return result;
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

        public int GetCountOfAssignments(QuestionnaireIdentity questionnaireIdentity)
        {
            return this.assignmentsAccessor.Query(_ => _.Count(ByQuestionnaire(questionnaireIdentity)));
        }

        private static Expression<Func<Assignment, bool>> ByQuestionnaire(QuestionnaireIdentity questionnaireId)
        {
            Expression<Func<Assignment, bool>> readyForWebInterviewAssignments =
                x => x.QuestionnaireId.QuestionnaireId == questionnaireId.QuestionnaireId &&
                     x.QuestionnaireId.Version == questionnaireId.Version &&
                     !x.Archived &&
                     (x.Quantity == null || x.InterviewSummaries.Count < x.Quantity);
            return readyForWebInterviewAssignments;
        }

        private static Expression<Func<Assignment, bool>> ReadyForWebInterviewAssignments(QuestionnaireIdentity questionnaireId)
        {
            Expression<Func<Assignment, bool>> readyForWebInterviewAssignments =
                x => x.QuestionnaireId.QuestionnaireId == questionnaireId.QuestionnaireId &&
                     x.QuestionnaireId.Version == questionnaireId.Version &&
                     x.Responsible.ReadonlyProfile.SupervisorId != null &&
                     !x.Archived &&
                     (x.Quantity == null || x.InterviewSummaries.Count < x.Quantity) &&
                     x.WebMode == true;
            return readyForWebInterviewAssignments;
        }

        public AssignmentApiDocument MapAssignment(Assignment assignment)
        {
            var assignmentApiView = new AssignmentApiDocument
            {
                Id = assignment.Id,
                PublicId = assignment.PublicKey,
                QuestionnaireId = assignment.QuestionnaireId,
                Quantity = assignment.InterviewsNeeded,
                CreatedAtUtc = assignment.CreatedAtUtc,
                ProtectedVariables = assignment.ProtectedVariables,
                ResponsibleId = assignment.ResponsibleId,
                ResponsibleName = assignment.Responsible.Name,
                IsAudioRecordingEnabled = assignment.AudioRecording,
                Comments = assignment.Comments
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
