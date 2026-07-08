using System;
using System.Collections.Generic;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.WebApi;

namespace WB.Core.BoundedContexts.Headquarters.Assignments
{
    public interface IAssignmentsService
    {
        List<Assignment> GetAssignments(Guid responsibleId);

        List<Assignment> GetAssignmentsForSupervisor(Guid supervisorId);

        List<Guid> GetAllAssignmentIds(Guid responsibleId);
        
        Assignment GetAssignment(int id);

        /// <summary>
        /// Convenience overload for callers that do not yet hold a reference to the entity.
        /// Prefer <see cref="GetAssignmentWithUpgradeLock(Assignment)"/> when the entity is
        /// already loaded in the same session to avoid a redundant DB round-trip.
        /// </summary>
        Assignment GetAssignmentWithUpgradeLock(int id);

        /// <summary>
        /// Issues SELECT … FOR UPDATE against the row and refreshes the entity state through
        /// NHibernate's <c>Session.Refresh</c>, bypassing the L1 identity-map so concurrent
        /// changes are visible. Use this overload when <paramref name="assignment"/> is already
        /// loaded in the current session to save one DB round-trip.
        /// </summary>
        Assignment GetAssignmentWithUpgradeLock(Assignment assignment);

        Assignment GetAssignmentByAggregateRootId(Guid id);

        List<Assignment> GetAssignmentsReadyForWebInterview(QuestionnaireIdentity questionnaireId);
        
        int GetCountOfAssignmentsReadyForWebInterview(QuestionnaireIdentity questionnaireId);

        int GetCountOfAssignments(QuestionnaireIdentity questionnaireId);
        
        AssignmentApiDocument MapAssignment(Assignment assignment);

        bool HasAssignmentWithProtectedVariables(Guid responsibleId);
        bool HasAssignmentWithAudioRecordingEnabled(Guid responsible);
        bool HasAssignmentWithAudioRecordingEnabled(QuestionnaireIdentity questionnaireIdentity);
        bool DoesExistPasswordInDb(QuestionnaireIdentity questionnaireIdentity, string password);
        List<int> GetAllAssignmentIdsForMigrateToNewVersion(QuestionnaireIdentity questionnaireIdentity);
        List<AssignmentGpsInfo> GetAssignmentsWithGpsAnswer(Guid? questionnaireId, long? questionnaireVersion,
            Guid? responsibleId, int? assignmentId,
            double east, double north, double west, double south);

        HashSet<int> GetExistingAssignmentIds(IEnumerable<int> assignmentIds);
    }
}
