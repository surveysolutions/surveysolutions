﻿using System;
using System.Collections.Generic;
using System.Linq;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Headquarters.Factories;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Views.User;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.Infrastructure.Transactions;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities.Answers;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.UI.Headquarters.API.PublicApi.Models;
using WB.UI.Headquarters.Code.CommandTransformation;
using WB.UI.Headquarters.Services;

namespace WB.UI.Headquarters.Implementation.Services
{
    public class InterviewCreatorFromAssignment : IInterviewCreatorFromAssignment
    {
        private readonly IPlainTransactionManagerProvider plainTransactionManagerProvider;
        private readonly ITransactionManagerProvider transactionManagerProvider;
        private readonly IQuestionnaireBrowseViewFactory questionnaireBrowseViewFactory;
        private readonly IInterviewUniqueKeyGenerator interviewKeyGenerator;
        private readonly IInterviewAnswerSerializer answerSerializer;
        private readonly ICommandService commandService;
        private readonly IUserViewFactory userViewFactory;
        private readonly IAuthorizedUser authorizedUser;

        private IPlainTransactionManager plainTransactionManager => plainTransactionManagerProvider.GetPlainTransactionManager();


        public InterviewCreatorFromAssignment(IPlainTransactionManagerProvider plainTransactionManagerProvider,
            ITransactionManagerProvider transactionManagerProvider,
            IQuestionnaireBrowseViewFactory questionnaireBrowseViewFactory,
            IInterviewUniqueKeyGenerator interviewKeyGenerator,
            IInterviewAnswerSerializer answerSerializer,
            ICommandService commandService,
            IUserViewFactory userViewFactory,
            IAuthorizedUser authorizedUser)
        {
            this.plainTransactionManagerProvider = plainTransactionManagerProvider;
            this.transactionManagerProvider = transactionManagerProvider;
            this.questionnaireBrowseViewFactory = questionnaireBrowseViewFactory;
            this.interviewKeyGenerator = interviewKeyGenerator;
            this.answerSerializer = answerSerializer;
            this.commandService = commandService;
            this.userViewFactory = userViewFactory;
            this.authorizedUser = authorizedUser;
        }

        public void CreateInterviewIfQuestionnaireIsOld(HqUser responsible, QuestionnaireIdentity questionnaireIdentity, int assignmentId, List<AssignmentIdentifyingDataItem> identifyingData)
        {
            if (this.IsSupportAssignments(questionnaireIdentity))
                return;

            var responsibleSupervisorId = responsible.IsInRole(UserRoles.Interviewer) ? responsible.Profile.SupervisorId.Value : responsible.Id;
            var responsibleInterviewerId = responsible.IsInRole(UserRoles.Interviewer) ? responsible.Id : (Guid?)null;

            List<InterviewAnswer> answers = identifyingData
                .Select(ia => new InterviewAnswer
                {
                    Identity = ia.Identity,
                    Answer = this.answerSerializer.Deserialize<AbstractAnswer>(ia.Answer)
                })
                .Where(x => x.Answer != null)
                .ToList();

            this.ExecuteCreateInterviewCommand(questionnaireIdentity, assignmentId, responsibleSupervisorId, responsibleInterviewerId, answers);
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
            int assignmentId, Guid responsibleSupervisorId, Guid? responsibleInterviewerId, List<InterviewAnswer> answers)
        {
            var userId = this.authorizedUser.Id;

            var command = new CreateInterview(Guid.NewGuid(),
                userId,
                questionnaireIdentity,
                supervisorId: responsibleSupervisorId,
                interviewerId: responsibleInterviewerId,
                answersTime: DateTime.UtcNow,
                answers: answers,
                interviewKey: this.interviewKeyGenerator.Get(),
                assignmentId: assignmentId);

            this.transactionManagerProvider.GetTransactionManager()
                .ExecuteInQueryTransaction(
                    () => this.plainTransactionManager.ExecuteInPlainTransaction(
                        () => { this.commandService.Execute(command); }));
        }

        private bool IsSupportAssignments(QuestionnaireIdentity questionnaireIdentity)
        {
            var questionnaireBrowseItem =
                this.plainTransactionManager.ExecuteInQueryTransaction(
                    () => this.questionnaireBrowseViewFactory.GetById(questionnaireIdentity));

            bool isSupportAssignments = questionnaireBrowseItem.AllowAssignments;
            return isSupportAssignments;
        }
    }
}