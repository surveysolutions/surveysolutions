using System;
using Main.Core.Documents;
using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.Infrastructure.EventBus;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.ReadSide;
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire;
using WB.Core.SharedKernels.SurveyManagement.Views.Interview;

namespace WB.Core.SharedKernels.SurveyManagement.EventHandler
{
    public class InterviewsChartDenormalizer : IEventHandler,
                                          IEventHandler<InterviewCreated>,
                                          IEventHandler<InterviewFromPreloadedDataCreated>,
                                          IEventHandler<InterviewOnClientCreated>,
                                          IEventHandler<InterviewStatusChanged>,
                                          IEventHandler<SupervisorAssigned>,
                                          IEventHandler<InterviewDeleted>,
                                          IEventHandler<InterviewHardDeleted>,
                                          IEventHandler<InterviewRestored>,
                                          IEventHandler<InterviewerAssigned>
    {
        private readonly IReadSideRepositoryWriter<UserDocument> users;
        private readonly IReadSideRepositoryWriter<StatisticsLineGroupedByUserAndTemplate> statisticsStorage;
        private readonly IReadSideRepositoryWriter<InterviewBrief> interviewBriefStorage;
        private readonly IVersionedReadSideRepositoryWriter<QuestionnaireBrowseItem> questionnaires;

        public InterviewsChartDenormalizer(
            IReadSideRepositoryWriter<StatisticsLineGroupedByUserAndTemplate> statisticsStorage,
            IReadSideRepositoryWriter<UserDocument> users, 
            IReadSideRepositoryWriter<InterviewBrief> interviewBriefStorage,
            IVersionedReadSideRepositoryWriter<QuestionnaireBrowseItem> questionnaires)
        {
            this.statisticsStorage = statisticsStorage;
            this.users = users;
            this.interviewBriefStorage = interviewBriefStorage;
            this.questionnaires = questionnaires;
        }

        public void Handle(IPublishedEvent<InterviewCreated> evnt)
        {
            throw new NotImplementedException();
        }

        public void Handle(IPublishedEvent<InterviewFromPreloadedDataCreated> evnt)
        {
            throw new NotImplementedException();
        }

        public void Handle(IPublishedEvent<InterviewOnClientCreated> evnt)
        {
            throw new NotImplementedException();
        }

        public void Handle(IPublishedEvent<InterviewStatusChanged> evnt)
        {
            throw new NotImplementedException();
        }

        public void Handle(IPublishedEvent<SupervisorAssigned> evnt)
        {
            throw new NotImplementedException();
        }

        public void Handle(IPublishedEvent<InterviewDeleted> evnt)
        {
            throw new NotImplementedException();
        }

        public void Handle(IPublishedEvent<InterviewHardDeleted> evnt)
        {
            throw new NotImplementedException();
        }

        public void Handle(IPublishedEvent<InterviewRestored> evnt)
        {
            throw new NotImplementedException();
        }

        public void Handle(IPublishedEvent<InterviewerAssigned> evnt)
        {
            throw new NotImplementedException();
        }

        public string Name
        {
            get { return this.GetType().Name; }
        }

        public Type[] UsesViews
        {
            get { return new[] { typeof(UserDocument), typeof(QuestionnaireBrowseItem) }; }
        }

        public Type[] BuildsViews
        {
            get { return new[] { typeof(StatisticsLineGroupedByUserAndTemplate), typeof(InterviewBrief) }; }
        }
    }
}
