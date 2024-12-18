using System;
using System.Linq;
using System.Threading.Tasks;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.WebApi;
using WB.Core.SharedKernels.Enumerator.OfflineSync.Messages;
using WB.Core.SharedKernels.Enumerator.OfflineSync.Services;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Views;

namespace WB.Core.BoundedContexts.Supervisor.Services.Implementation.OfflineSyncHandlers
{
    public class SupervisorAssignmentsHandler : IHandleCommunicationMessage
    {
        private readonly IAssignmentDocumentsStorage assignmentDocumentsStorage;

        public SupervisorAssignmentsHandler(IAssignmentDocumentsStorage assignmentDocumentsStorage)
        {
            this.assignmentDocumentsStorage = assignmentDocumentsStorage;
        }

        public void Register(IRequestHandler requestHandler)
        {
            requestHandler.RegisterHandler<GetAssignmentRequest, GetAssignmentResponse>(GetAssignment);
            requestHandler.RegisterHandler<GetAssignmentsRequest, GetAssignmentsResponse>(GetAssignments);
            requestHandler.RegisterHandler<LogAssignmentAsHandledRequest, OkResponse>(LogAssignmentAsHandled);
        }

        private Task<OkResponse> LogAssignmentAsHandled(LogAssignmentAsHandledRequest request)
        {
            var assignment = this.assignmentDocumentsStorage.GetById(request.Id);
            assignment.ReceivedByInterviewerAt = DateTime.UtcNow;
            this.assignmentDocumentsStorage.Store(assignment);
            return OkResponse.Task;
        }

        public Task<GetAssignmentsResponse> GetAssignments(GetAssignmentsRequest request)
        {
            var assignments = this.assignmentDocumentsStorage.LoadAll(request.UserId);

            var result = new GetAssignmentsResponse
            {
                Assignments = assignments.Select(assignmentDocument => new AssignmentApiView
                {
                    Id = assignmentDocument.Id,
                    ResponsibleId = assignmentDocument.ResponsibleId,
                    ResponsibleName = assignmentDocument.ResponsibleName,
                    Quantity = assignmentDocument.Quantity.HasValue ? assignmentDocument.Quantity - assignmentDocument.CreatedInterviewsCount : assignmentDocument.Quantity,
                    QuestionnaireId = QuestionnaireIdentity.Parse(assignmentDocument.QuestionnaireId),
                    IsAudioRecordingEnabled = assignmentDocument.IsAudioRecordingEnabled,
                    TargetArea = assignmentDocument.TargetArea
                }).ToList()
            };
            return Task.FromResult(result);
        }

        public Task<GetAssignmentResponse> GetAssignment(GetAssignmentRequest request)
        {
            var assignment = this.assignmentDocumentsStorage.GetById(request.Id);

            var assignmentApiView = new AssignmentApiDocument
            {
                Id = assignment.Id,
                QuestionnaireId = QuestionnaireIdentity.Parse(assignment.QuestionnaireId),
                Quantity = assignment.Quantity,
                CreatedAtUtc = assignment.ReceivedDateUtc,
                ProtectedVariables = assignment.ProtectedVariables.Select(pv => pv.Variable).ToList(),
                ResponsibleId = assignment.ResponsibleId,
                ResponsibleName = assignment.ResponsibleName,
                LocationLatitude = assignment.LocationLatitude,
                LocationLongitude = assignment.LocationLongitude,
                LocationQuestionId = assignment.LocationQuestionId,
                IsAudioRecordingEnabled = assignment.IsAudioRecordingEnabled,
                Comments = assignment.Comments,
                TargetArea = assignment.TargetArea,
            };

            foreach (var answer in assignment.Answers ?? Enumerable.Empty<AssignmentDocument.AssignmentAnswer>())
            {
                var serializedAnswer = new AssignmentApiDocument.InterviewSerializedAnswer
                {
                    Identity = answer.Identity,
                    SerializedAnswer = answer.SerializedAnswer
                };
                
                assignmentApiView.Answers.Add(serializedAnswer);
            }

            return Task.FromResult(new GetAssignmentResponse
            {
                Assignment = assignmentApiView
            });
        }
    }
}
