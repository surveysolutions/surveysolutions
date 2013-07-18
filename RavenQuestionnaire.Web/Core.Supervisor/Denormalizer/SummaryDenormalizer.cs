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

    public class SummaryDenormalizer : IEventHandler<QuestionnaireStatusChanged>, 
                                       IEventHandler<QuestionnaireAssignmentChanged>
    {
        #region Constants and Fields

        private readonly IReadSideRepositoryWriter<SummaryItem> summary;
        private readonly IReadSideRepositoryWriter<UserDocument> users;
        private readonly IReadSideRepositoryWriter<CompleteQuestionnaireStoreDocument> questionnaires;

        #endregion

        #region Constructors and Destructors

        public SummaryDenormalizer(
            IReadSideRepositoryWriter<SummaryItem> summary,
            IReadSideRepositoryWriter<UserDocument> users,
            IReadSideRepositoryWriter<CompleteQuestionnaireStoreDocument> questionnaires)
        {
            this.summary = summary;
            this.users = users;
            this.questionnaires = questionnaires;
        }

        #endregion

        #region Public Methods and Operators

        public void Handle(IPublishedEvent<QuestionnaireStatusChanged> evnt)
        {
            var questionnaire = this.questionnaires.GetById(evnt.Payload.CompletedQuestionnaireId);

            var summmaryUserId = evnt.Payload.Responsible.Id.Combine(questionnaire.TemplateId);
            var summaryUser = this.summary.GetById(summmaryUserId);
            if (summaryUser != null)
            {
                summaryUser.QuestionnaireStatus = evnt.Payload.Status.PublicId;

                if (summaryUser.ResponsibleSupervisorId.HasValue)
                {
                    var summarySupervisorId =
                        summaryUser.ResponsibleSupervisorId.Value.Combine(questionnaire.TemplateId);
                    var summarySupervisor = this.summary.GetById(summarySupervisorId);
                    summarySupervisor.QuestionnaireStatus = evnt.Payload.Status.PublicId;

                    if (evnt.Payload.PreviousStatus.PublicId == SurveyStatus.Unassign.PublicId)
                    {
                        this.DecreaseByStatus(summarySupervisor, evnt.Payload.PreviousStatus.PublicId);
                    }
                    else
                    {
                        this.DecreaseByStatus(summaryUser, evnt.Payload.PreviousStatus.PublicId);
                        this.DecreaseByStatus(summarySupervisor, evnt.Payload.PreviousStatus.PublicId);
                    }

                    this.IncreaseByStatus(summaryUser, evnt.Payload.Status.PublicId);
                    this.IncreaseByStatus(summarySupervisor, evnt.Payload.Status.PublicId);

                    this.summary.Store(summarySupervisor, summarySupervisorId);

                }
                else
                {
                    this.IncreaseByStatus(summaryUser, evnt.Payload.Status.PublicId);
                }

                this.summary.Store(summaryUser, summmaryUserId);
            }
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
            summary.UnassignedCount += statusId == SurveyStatus.Unassign.PublicId ? incCount : 0;
            summary.TotalCount += incCount;
        }

        public void Handle(IPublishedEvent<QuestionnaireAssignmentChanged> evnt)
        {
            evnt.Payload.Responsible.Name = users.GetById(evnt.Payload.Responsible.Id).UserName;

            var questionnaire = this.questionnaires.GetById(evnt.Payload.CompletedQuestionnaireId);

            var summaryUserId = evnt.Payload.Responsible.Id.Combine(questionnaire.TemplateId);
            var summaryUser = this.summary.GetById(summaryUserId);
            if (summaryUser == null)
            {
                var user = this.users.GetById(evnt.Payload.Responsible.Id);

                summaryUser = new SummaryItem()
                {
                    TemplateId = questionnaire.TemplateId,
                    TemplateName = questionnaire.Title,
                    ResponsibleSupervisorId =
                        user.Supervisor == null ? (Guid?) null : user.Supervisor.Id,
                    ResponsibleId = user.PublicKey,
                    ResponsibleName = user.UserName,
                    QuestionnaireStatus = questionnaire.Status.PublicId
                };

                this.summary.Store(summaryUser, summaryUserId);
            }


            if (evnt.Payload.PreviousResponsible != null)
            {
                evnt.Payload.PreviousResponsible.Name = users.GetById(evnt.Payload.PreviousResponsible.Id).UserName;

                var summaryPrevUserId =
                    evnt.Payload.PreviousResponsible.Id.Combine(questionnaire.TemplateId);
                var summaryPrevUser = this.summary.GetById(summaryPrevUserId);

                if ((summaryUserId != summaryPrevUserId) &&
                    summaryUser.ResponsibleSupervisorId.HasValue && summaryPrevUser.ResponsibleSupervisorId.HasValue)
                {
                    var summarySupervisorId =
                        summaryUser.ResponsibleSupervisorId.Value.Combine(questionnaire.TemplateId);
                    var summaryPrevSupervisorId =
                        summaryPrevUser.ResponsibleSupervisorId.Value.Combine(questionnaire.TemplateId);


                    var summarySupervisor = this.summary.GetById(summarySupervisorId);
                    var summaryPrevSupervisor = this.summary.GetById(summaryPrevSupervisorId);

                    this.DecreaseByStatus(summaryPrevUser, summaryUser.QuestionnaireStatus);
                    this.DecreaseByStatus(summaryPrevSupervisor, summaryUser.QuestionnaireStatus);

                    this.IncreaseByStatus(summaryUser, summaryUser.QuestionnaireStatus);
                    this.IncreaseByStatus(summarySupervisor, summaryUser.QuestionnaireStatus);

                    this.summary.Store(summaryPrevUser, summaryPrevUserId);
                    this.summary.Store(summaryPrevSupervisor, summaryPrevSupervisorId);
                    this.summary.Store(summaryUser, summaryUserId);
                    this.summary.Store(summarySupervisor, summarySupervisorId);
                }
            }
        }

        #endregion
    }
}