using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Views.Questionnaire;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities.Answers;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.WebApi;
using WB.Infrastructure.Native.Fetching;
using WB.Infrastructure.Native.Storage.Postgre;

namespace WB.Core.BoundedContexts.Headquarters.Assignments
{
    internal class AssignmentsService : IAssignmentsService
    {
        private readonly IQueryableReadSideRepositoryReader<Assignment, Guid> assignmentsAccessor;
        private readonly IInterviewAnswerSerializer answerSerializer;
        private readonly IUnitOfWork sessionProvider;
        private readonly IAuthorizedUser authorizedUser;

        public AssignmentsService(
            IQueryableReadSideRepositoryReader<Assignment, Guid> assignmentsAccessor,
            IInterviewAnswerSerializer answerSerializer,
            IUnitOfWork sessionProvider,
            IAuthorizedUser authorizedUser)
        {
            this.assignmentsAccessor = assignmentsAccessor;
            this.answerSerializer = answerSerializer;
            this.sessionProvider = sessionProvider;
            this.authorizedUser = authorizedUser;
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

        public List<AssignmentGpsInfo> GetAssignmentsWithGpsAnswer(
            Guid? questionnaireId, long? questionnaireVersion, 
            Guid? responsibleId, int? assignmentId,
            double east, double north, double west, double south)
        {
            Guid currentUserId = authorizedUser.Id;
            
            var gpsQuery = QueryGpsAnswers()
                .Where(a =>
                    a.QuestionnaireItem.Featured == true
                    && !a.Assignment.Archived
                    && (a.Assignment.Quantity == null || a.Assignment.InterviewSummaries.Count < a.Assignment.Quantity)
                    && a.Answer.Latitude <= north
                    && a.Answer.Latitude >= south
                    && a.Answer.Longitude <= east
                    && a.Answer.Longitude >= west 
                );
            
            if (questionnaireId.HasValue)
            {
                gpsQuery = gpsQuery
                    .Where(x => x.Assignment.QuestionnaireId.QuestionnaireId == questionnaireId.Value);

                if (questionnaireVersion.HasValue)
                {
                    gpsQuery = gpsQuery
                        .Where(x => x.Assignment.QuestionnaireId.Version == questionnaireVersion.Value);
                }
            }

            if (assignmentId.HasValue)
            {
                gpsQuery = gpsQuery.Where(x => x.Assignment.Id == assignmentId.Value);
            }

            if (responsibleId.HasValue)
            {
                gpsQuery = gpsQuery.Where(x => 
                    x.Assignment.Responsible.Id == responsibleId.Value
                    || x.Assignment.Responsible.ReadonlyProfile.SupervisorId == responsibleId.Value);
            }        

            if (authorizedUser.IsInterviewer)
            {
                gpsQuery = gpsQuery
                    .Where(x => 
                        x.Assignment.ResponsibleId == currentUserId
                    );
            } 
            else if (authorizedUser.IsSupervisor)
            {
                gpsQuery = gpsQuery
                    .Where(x => 
                        (x.Assignment.Responsible.ReadonlyProfile.SupervisorId == currentUserId || x.Assignment.ResponsibleId == currentUserId)
                    );
            } 
            
            var assignments = gpsQuery
                .Select(a => new AssignmentGpsInfo()
                {
                    AssignmentId = a.Assignment.Id,
                    Latitude = a.Answer.Latitude,
                    Longitude = a.Answer.Longitude,
                    ResponsibleRoleId = a.Assignment.Responsible.RoleIds.FirstOrDefault()
                })
                .Distinct()
                .ToList();
            return assignments;
        }
        
        private class AssigmentGpsAnswer
        {
            public Assignment Assignment { get; set; }
            public AssignmentGps Answer { get; set; }
            public QuestionnaireCompositeItem QuestionnaireItem { get; set; }
        }
        
        private IQueryable<AssigmentGpsAnswer> QueryGpsAnswers()
        {
            IQueryable<AssigmentGpsAnswer> gpsAnswers = this.sessionProvider.Session
                .Query<AssignmentGps>()
                .Join(this.sessionProvider.Session.Query<Assignment>(),
                    gps => gps.AssignmentId,
                    assignment => assignment.Id,
                    (gps, assignment) => new  { gps, assignment})
                .Join(this.sessionProvider.Session.Query<QuestionnaireCompositeItem>(),
                    assignment_gps => new { QuestionnaireId = assignment_gps.assignment.QuestionnaireId.Id, assignment_gps.gps.QuestionId },
                    questionnaireItem => new { QuestionnaireId = questionnaireItem.QuestionnaireIdentity, QuestionId = questionnaireItem.EntityId },
                    (assignment_gps, questionnaireItem) => new AssigmentGpsAnswer
                    {
                        Assignment = assignment_gps.assignment,
                        Answer = assignment_gps.gps,
                        QuestionnaireItem = questionnaireItem
                    });
            
            
            // IQueryable<AssignmentGps> gpsAnswers = this.sessionProvider.Session
            //     .Query<IdentifyingAnswer>()
            //     .Join(this.sessionProvider.Session.Query<QuestionnaireCompositeItem>(),
            //         assignmentAnswer => new { assignmentAnswer.Assignment, assignmentAnswer.Identity.Id },
            //         questionnaireItem => new { questionnaireItem.QuestionnaireIdentity, QuestionId = questionnaireItem.EntityId },
            //         (identifyingAnswer, questionnaireItem) => new AssigmentGpsAnswer
            //         {
            //             Assignment = identifyingAnswer.Assignment,
            //             Answer = identifyingAnswer.Answer,
            //             QuestionnaireItem = questionnaireItem
            //         });
            // gpsAnswers = gpsAnswers
            //     .Where(a => a.QuestionnaireItem.QuestionType == QuestionType.GpsCoordinates);

            return gpsAnswers;
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
