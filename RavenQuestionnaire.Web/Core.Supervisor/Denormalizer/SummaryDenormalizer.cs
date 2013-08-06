using Main.Core.View.CompleteQuestionnaire;

namespace Core.Supervisor.Denormalizer
{
    using System;

    using Core.Supervisor.DenormalizerStorageItem;

    using Main.Core.Documents;
    using Main.Core.Entities.SubEntities;
    using Main.Core.Events.Questionnaire.Completed;

    using Ncqrs.Eventing.ServiceModel.Bus;

    using Main.Core.Utility;

    using WB.Core.Infrastructure.ReadSide.Repository.Accessors;

    public class SummaryDenormalizer : UserBaseDenormalizer,
                                       IEventHandler<QuestionnaireStatusChanged>,
                                       IEventHandler<QuestionnaireAssignmentChanged>,
                                       IEventHandler<InterviewDeleted>, IEventHandler<InterviewMetaInfoUpdated>
    {
        private readonly IReadSideRepositoryWriter<SummaryItem> summaryItem;
        private readonly IReadSideRepositoryWriter<CompleteQuestionnaireBrowseItem> questionnaires;

        public SummaryDenormalizer(
            IReadSideRepositoryWriter<SummaryItem> summaryItem,
            IReadSideRepositoryWriter<UserDocument> users,
            IReadSideRepositoryWriter<CompleteQuestionnaireBrowseItem> questionnaires)
            : base(users)
        {
            this.summaryItem = summaryItem;
            this.questionnaires = questionnaires;
        }

        public void Handle(IPublishedEvent<QuestionnaireStatusChanged> evnt)
        {
            HandleChangeStatus(evnt.EventSourceId,evnt.Payload.Status.PublicId);
        }

        private void HandleChangeStatus(Guid interviewId, Guid statusId)
        {

            var questionnaire = this.questionnaires.GetById(interviewId);

            var summmaryUserId = questionnaire.Responsible.Id.Combine(questionnaire.TemplateId);
            var summaryUser = this.summaryItem.GetById(summmaryUserId);

            if (summaryUser == null)
            {
                return;
            }
            if (statusId == summaryUser.QuestionnaireStatus)
                return;



            this.DecreaseByStatus(summaryUser, summaryUser.QuestionnaireStatus);

            summaryUser.QuestionnaireStatus = statusId;

            this.IncreaseByStatus(summaryUser, statusId);
            this.summaryItem.Store(summaryUser, summmaryUserId);
        }

        public void Handle(IPublishedEvent<QuestionnaireAssignmentChanged> evnt)
        {
            if (evnt.Payload.PreviousResponsible != null &&
                evnt.Payload.Responsible.Id == evnt.Payload.PreviousResponsible.Id)
            {
                return;
            }
            var questionnaire = this.questionnaires.GetById(evnt.EventSourceId);

            var summaryUserId = evnt.Payload.Responsible.Id.Combine(questionnaire.TemplateId);

            var summaryUser = this.summaryItem.GetById(summaryUserId) ?? this.CreateNewSummaryUser(evnt.Payload.Responsible.Id, questionnaire);

            if (evnt.Payload.PreviousResponsible != null && summaryUser.ResponsibleId != evnt.Payload.PreviousResponsible.Id)
            {
                var summaryPrevUserId = evnt.Payload.PreviousResponsible.Id.Combine(questionnaire.TemplateId);
                var summaryPrevUser = this.summaryItem.GetById(summaryPrevUserId);

                this.DecreaseByStatus(summaryPrevUser, questionnaire.Status.PublicId);
                this.summaryItem.Store(summaryPrevUser, summaryPrevUserId);
            }
            this.IncreaseByStatus(summaryUser, questionnaire.Status.PublicId);
            this.summaryItem.Store(summaryUser, summaryUserId);
        }

        public void Handle(IPublishedEvent<InterviewDeleted> evnt)
        {
            var questionnaire = this.questionnaires.GetById(evnt.EventSourceId);
            var summmaryUserId = questionnaire.Responsible.Id.Combine(questionnaire.TemplateId);
            var summaryUser = this.summaryItem.GetById(summmaryUserId);

            if (summaryUser == null)
                return;

            this.DecreaseByStatus(summaryUser, summaryUser.QuestionnaireStatus);
            this.summaryItem.Store(summaryUser, summmaryUserId);
        }

        private void DecreaseByStatus(SummaryItem summary, Guid statusId)
        {
            this.IncreaseByStatus(summary, statusId, false);
        }

        private void IncreaseByStatus(SummaryItem summary, Guid statusId, bool isIncrease = true)
        {
            int incCount = isIncrease ? 1 : -1;

            summary.InitialCount += statusId == SurveyStatus.Initial.PublicId ? incCount : 0;
            summary.ApprovedCount += statusId == SurveyStatus.Approve.PublicId ? incCount : 0;
            summary.RedoCount += statusId == SurveyStatus.Redo.PublicId ? incCount : 0;
            summary.CompletedCount += statusId == SurveyStatus.Complete.PublicId ? incCount : 0;
            summary.CompletedWithErrorsCount += statusId == SurveyStatus.Error.PublicId ? incCount : 0;
            summary.UnassignedCount += (statusId == SurveyStatus.Unassign.PublicId || statusId == SurveyStatus.Unknown.PublicId) ? incCount : 0;
            summary.TotalCount += incCount;
        }

        private SummaryItem CreateNewSummaryUser(Guid responsibleId, CompleteQuestionnaireBrowseItem questionnaire)
        {
            var user = this.users.GetById(responsibleId);
            var isUserIsSupervisor = user.Roles.Contains(UserRoles.Supervisor);
            var responsibleSupervisorId = (Guid?)null;
            var responsibleSupervisorName = string.Empty;
            if (isUserIsSupervisor)
            {
                responsibleSupervisorId = user.PublicKey;
                responsibleSupervisorName = user.UserName;
            }
            else if (user.Supervisor != null)
            {
                responsibleSupervisorId = user.Supervisor.Id;
                responsibleSupervisorName = user.Supervisor.Name;
            }

            var status = questionnaire.Status.PublicId;
            if (status == SurveyStatus.Unknown.PublicId && isUserIsSupervisor)
                status = SurveyStatus.Unassign.PublicId;
            return
                new SummaryItem()
                    {
                        TemplateId = questionnaire.TemplateId,
                        TemplateName = questionnaire.QuestionnaireTitle,
                        ResponsibleSupervisorId = responsibleSupervisorId,
                        ResponsibleSupervisorName = responsibleSupervisorName,
                        ResponsibleId = user.PublicKey,
                        ResponsibleName = user.UserName,
                        QuestionnaireStatus = status
                    };

        }

        public void Handle(IPublishedEvent<InterviewMetaInfoUpdated> evnt)
        {
            HandleChangeStatus(evnt.EventSourceId, evnt.Payload.StatusId);
        }
    }
}