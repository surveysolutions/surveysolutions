using System;
using System.Collections.Generic;
using WB.Core.BoundedContexts.Headquarters.Factories;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Services.Preloading;
using WB.Core.BoundedContexts.Headquarters.Views.User;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities.Answers;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Services;

namespace WB.UI.Headquarters.Services.Impl
{
    public class InterviewCreatorFromAssignment : IInterviewCreatorFromAssignment
    {
        private readonly IQuestionnaireBrowseViewFactory questionnaireBrowseViewFactory;
        private readonly IInterviewUniqueKeyGenerator interviewKeyGenerator;
        private readonly ICommandService commandService;
        private readonly IUserViewFactory userViewFactory;
        private readonly IAuthorizedUser authorizedUser;

        public InterviewCreatorFromAssignment(IQuestionnaireBrowseViewFactory questionnaireBrowseViewFactory,
            IInterviewUniqueKeyGenerator interviewKeyGenerator,
            ICommandService commandService,
            IUserViewFactory userViewFactory,
            IAuthorizedUser authorizedUser)
        {
            this.questionnaireBrowseViewFactory = questionnaireBrowseViewFactory;
            this.interviewKeyGenerator = interviewKeyGenerator;
            this.commandService = commandService;
            this.userViewFactory = userViewFactory;
            this.authorizedUser = authorizedUser;
        }
        
        public void CreateInterviewIfQuestionnaireIsOld(Guid responsibleId, QuestionnaireIdentity questionnaireIdentity, int assignmentId,
            List<InterviewAnswer> interviewAnswers)
        {
            if (this.IsSupportAssignments(questionnaireIdentity))
                return;

            var responsible = this.userViewFactory.GetUser(new UserViewInputModel(responsibleId));

            var responsibleSupervisorId = responsible.IsInterviewer() ? responsible.Supervisor.Id : responsible.PublicKey;
            var responsibleInterviewerId = responsible.IsInterviewer() ? responsible.PublicKey : (Guid?)null;

            this.ExecuteCreateInterviewCommand(questionnaireIdentity, assignmentId, responsibleSupervisorId, responsibleInterviewerId, interviewAnswers);
        }

        private void ExecuteCreateInterviewCommand(QuestionnaireIdentity questionnaireIdentity,
            int assignmentId, Guid responsibleSupervisorId, Guid? responsibleInterviewerId,
            List<InterviewAnswer> answers)
        {
            var userId = this.authorizedUser.Id;

            var command = new CreateInterview(Guid.NewGuid(),
                userId,
                questionnaireIdentity,
                supervisorId: responsibleSupervisorId,
                interviewerId: responsibleInterviewerId,
                answers: answers,
                protectedVariables: new List<string>(),
                interviewKey: this.interviewKeyGenerator.Get(),
                assignmentId: assignmentId,
                isAudioRecordingEnabled:false); //old Questionnaire, setting is not set

            this.commandService.Execute(command); 
        }

        private bool IsSupportAssignments(QuestionnaireIdentity questionnaireIdentity)
        {
            var questionnaireBrowseItem =
                     this.questionnaireBrowseViewFactory.GetById(questionnaireIdentity);

            bool isSupportAssignments = questionnaireBrowseItem.AllowAssignments;
            return isSupportAssignments;
        }
    }
}
