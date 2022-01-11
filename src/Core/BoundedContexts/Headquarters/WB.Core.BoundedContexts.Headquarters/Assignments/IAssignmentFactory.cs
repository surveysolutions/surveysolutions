using System;
using System.Collections.Generic;
using System.Linq;
using WB.Core.BoundedContexts.Headquarters.Views.Questionnaire;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities.Answers;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;

namespace WB.Core.BoundedContexts.Headquarters.Assignments
{
    public interface IAssignmentFactory
    {
        Assignment CreateAssignment(Guid userId, QuestionnaireIdentity questionnaireId, Guid responsibleId,
            int? quantity, string email, string password, bool? webMode, bool? isAudioRecordingEnabled,
            List<InterviewAnswer> answers, List<string> protectedVariables, string comment, int? upgradedFromId = null);
    }

    class AssignmentFactory : IAssignmentFactory
    {
        private readonly IPlainStorageAccessor<QuestionnaireBrowseItem> questionnaires;
        private readonly ICommandService commandService;
        private readonly IAssignmentsService assignmentsService;
        private readonly IAssignmentIdGenerator assignmentIdGenerator;

        public AssignmentFactory(IPlainStorageAccessor<QuestionnaireBrowseItem> questionnaires,
            ICommandService commandService,
            IAssignmentsService assignmentsService,
            IAssignmentIdGenerator assignmentIdGenerator)
        {
            this.questionnaires = questionnaires;
            this.commandService = commandService;
            this.assignmentsService = assignmentsService;
            this.assignmentIdGenerator = assignmentIdGenerator;
        }

        public Assignment CreateAssignment(Guid userId, QuestionnaireIdentity questionnaireId, Guid responsibleId, 
            int? quantity, string email, string password, bool? webMode, bool? isAudioRecordingEnabled,
            List<InterviewAnswer> answers, List<string> protectedVariables, string comment, int? upgradedFromId = null)
        {
            var assignmentId = Guid.NewGuid();

            bool isAudioRecordingEnabledValue = isAudioRecordingEnabled ?? this.questionnaires.Query(_ => _
                .Where(q => q.Id == questionnaireId.ToString())
                .Select(q => q.IsAudioRecordingEnabled).FirstOrDefault());

            var displayId = assignmentIdGenerator.GetNextDisplayId();

            commandService.Execute(new SharedKernels.DataCollection.Commands.Assignment.CreateAssignment(
                assignmentId,
                displayId,
                userId,
                questionnaireId,
                responsibleId,
                quantity,
                isAudioRecordingEnabledValue,
                email,
                password,
                webMode,
                answers,
                protectedVariables,
                comment,
                upgradedFromId));

            return assignmentsService.GetAssignmentByAggregateRootId(assignmentId);
        }
    }
}
