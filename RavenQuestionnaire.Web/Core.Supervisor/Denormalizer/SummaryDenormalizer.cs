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
                                       IEventHandler<QuestionnaireAssignmentChanged>
    {
        #region Constants and Fields

        private readonly IReadSideRepositoryWriter<SummaryItem> summaryItem;
        private readonly IReadSideRepositoryWriter<CompleteQuestionnaireBrowseItem> questionnaires;

        #endregion

        #region Constructors and Destructors

        public SummaryDenormalizer(
            IReadSideRepositoryWriter<SummaryItem> summaryItem,
            IReadSideRepositoryWriter<UserDocument> users,
            IReadSideRepositoryWriter<CompleteQuestionnaireBrowseItem> questionnaires)
            :base(users)
        {
            this.summaryItem = summaryItem;
            this.questionnaires = questionnaires;
        }

        #endregion

        #region Public Methods and Operators

        public void Handle(IPublishedEvent<QuestionnaireStatusChanged> evnt)
        {
            var questionnaire = this.questionnaires.GetById(evnt.EventSourceId);

            SummaryItem summaryUser = UpdateCountersForItemAndReturnIt(questionnaire.TemplateId, questionnaire.Responsible.Id, evnt.Payload.Status.PublicId, evnt.Payload.PreviousStatus.PublicId);
            if (summaryUser == null) return;

            if (!summaryUser.ResponsibleSupervisorId.HasValue)
            {
                return;
            }

            UpdateCountersForItemAndReturnIt(questionnaire.TemplateId, summaryUser.ResponsibleSupervisorId.Value,
                                             evnt.Payload.Status.PublicId, evnt.Payload.PreviousStatus.PublicId);

        }

        private SummaryItem UpdateCountersForItemAndReturnIt(Guid template, Guid responsible, Guid newStatus, Guid oldStatus)
        {
            var summmaryUserId = responsible.Combine(template);
            var summaryUser = this.summaryItem.GetById(summmaryUserId);

            if (summaryUser == null)
            {
                return null;
            }

            if (oldStatus == newStatus)
                return null;

            this.DecreaseByStatus(summaryUser, oldStatus);

          //  summaryUser.QuestionnaireStatus = newStatus;

            this.IncreaseByStatus(summaryUser, newStatus);
            this.summaryItem.Store(summaryUser, summmaryUserId);
            return summaryUser;
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

        public void Handle(IPublishedEvent<QuestionnaireAssignmentChanged> evnt)
        {
            var questionnaire = this.questionnaires.GetById(evnt.EventSourceId);

            var summaryUserId = evnt.Payload.Responsible.Id.Combine(questionnaire.TemplateId);

            var summaryUser = this.summaryItem.GetById(summaryUserId) ?? CreateNewSummaryUser(evnt.Payload.Responsible.Id,questionnaire);

            if (evnt.Payload.PreviousResponsible!=null && summaryUser.ResponsibleId != evnt.Payload.PreviousResponsible.Id)
            {
                var summaryPrevUserId = evnt.Payload.PreviousResponsible.Id.Combine(questionnaire.TemplateId);
                var summaryPrevUser = this.summaryItem.GetById(summaryPrevUserId);

                if (IsSupervisorTeamChanged(summaryUser, summaryPrevUser))
                {
                    var summarySupervisorId =
                        summaryUser.ResponsibleSupervisorId.Value.Combine(questionnaire.TemplateId);
                    var summaryPrevSupervisorId =
                        summaryPrevUser.ResponsibleSupervisorId.Value.Combine(questionnaire.TemplateId);
                    var summarySupervisor = this.summaryItem.GetById(summarySupervisorId);
                    var summaryPrevSupervisor = this.summaryItem.GetById(summaryPrevSupervisorId);

                    this.DecreaseByStatus(summaryPrevSupervisor, summaryUser.QuestionnaireStatus);
                    this.IncreaseByStatus(summarySupervisor, summaryUser.QuestionnaireStatus);

                    this.summaryItem.Store(summarySupervisor, summarySupervisorId);
                    this.summaryItem.Store(summaryPrevSupervisor, summaryPrevSupervisorId);

                }

                if (summaryPrevUser.ResponsibleId != summaryUser.ResponsibleSupervisorId)
                {
                    this.DecreaseByStatus(summaryPrevUser, summaryUser.QuestionnaireStatus);
                    this.summaryItem.Store(summaryPrevUser, summaryPrevUserId);
                }
            }
            this.IncreaseByStatus(summaryUser, summaryUser.QuestionnaireStatus);
            this.summaryItem.Store(summaryUser, summaryUserId);
        }

        private bool IsSupervisorTeamChanged(SummaryItem summaryUser, SummaryItem summaryPrevUser)
        {
            return summaryUser.ResponsibleSupervisorId.HasValue && summaryPrevUser.ResponsibleSupervisorId.HasValue &&
                   summaryUser.ResponsibleSupervisorId != summaryPrevUser.ResponsibleSupervisorId;
        }

        private SummaryItem CreateNewSummaryUser(Guid responsibleId,
                                                 CompleteQuestionnaireBrowseItem questionnaire)
        {
            var user = this.users.GetById(responsibleId);
            var status = questionnaire.Status.PublicId;
            if (status == SurveyStatus.Unknown.PublicId && user.Roles.Contains(UserRoles.Supervisor))
                status = SurveyStatus.Unassign.PublicId;
            return
                new SummaryItem()
                    {
                        TemplateId = questionnaire.TemplateId,
                        TemplateName = questionnaire.QuestionnaireTitle,
                        ResponsibleSupervisorId =
                            user.Supervisor == null ? (Guid?) null : user.Supervisor.Id,
                        ResponsibleId = user.PublicKey,
                        ResponsibleName = user.UserName,
                        QuestionnaireStatus = status
                    };

        }

        #endregion
    }
}