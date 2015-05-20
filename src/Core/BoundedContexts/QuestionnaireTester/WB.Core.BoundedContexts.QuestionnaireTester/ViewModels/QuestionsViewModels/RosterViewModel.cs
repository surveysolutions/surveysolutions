using System.Collections;
using System.Collections.ObjectModel;
using System.Linq;
using Cirrious.MvvmCross.ViewModels;
using WB.Core.BoundedContexts.QuestionnaireTester.Implementation.Entities;
using WB.Core.BoundedContexts.QuestionnaireTester.Repositories;
using WB.Core.BoundedContexts.QuestionnaireTester.Services;
using WB.Core.Infrastructure.EventBus.Lite;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;

namespace WB.Core.BoundedContexts.QuestionnaireTester.ViewModels.QuestionsViewModels
{
    public class RosterViewModel : MvxNotifyPropertyChanged, 
        IInterviewEntityViewModel,
        ILiteEventHandler<RosterInstancesTitleChanged>,
        ILiteEventHandler<RosterInstancesAdded>,
        ILiteEventHandler<RosterInstancesRemoved>
    {
        private readonly ILiteEventRegistry liteEventRegistry;
        private readonly IPlainKeyValueStorage<QuestionnaireModel> questionnaireRepository;
        private readonly IStatefullInterviewRepository interviewRepository;
        private readonly IInterviewViewModelFactory interviewViewModelFactory;
        private string interviewId;
        private Identity rosterIdendity;
        private NavigationState navigationState;

        public RosterViewModel(
            IPlainKeyValueStorage<QuestionnaireModel> questionnaireRepository,
            IStatefullInterviewRepository interviewRepository, 
            ILiteEventRegistry liteEventRegistry, 
            IInterviewViewModelFactory interviewViewModelFactory)
        {
            this.liteEventRegistry = liteEventRegistry;
            this.questionnaireRepository = questionnaireRepository;
            this.interviewRepository = interviewRepository;
            this.interviewViewModelFactory = interviewViewModelFactory;
        }

        private IList items;
        public IList Items
        {
            get { return items; }
            set { items = value; RaisePropertyChanged(); }
        }

        public void Init(string interviewId, Identity entityIdentity, NavigationState navigationState)
        {
            this.interviewId = interviewId;
            this.rosterIdendity = entityIdentity;
            this.navigationState = navigationState;

            this.liteEventRegistry.Subscribe(this);

            this.ReadRosterInstancesFromModel();
        }

        public void Handle(RosterInstancesTitleChanged @event)
        {
            this.ReadRosterInstancesFromModel();
        }

        public void Handle(RosterInstancesAdded @event)
        {
            this.ReadRosterInstancesFromModel();
        }

        public void Handle(RosterInstancesRemoved @event)
        {
            this.ReadRosterInstancesFromModel();
        }

        private void ReadRosterInstancesFromModel()
        {
            var interview = this.interviewRepository.Get(this.interviewId);
            var questionnaire = this.questionnaireRepository.GetById(interview.QuestionnaireId);

            var questionnaireRosterTitle = questionnaire.GroupsWithoutNestedChildren[this.rosterIdendity.Id].Title;

            var rosterItems = interview.RosterInstancesIds[ConversionHelper.ConvertIdAndRosterVectorToString(this.rosterIdendity.Id, this.rosterIdendity.RosterVector)];
            var rosterItemViewModels = rosterItems.Select(rosterInstanceId => RosterModelToViewModel(rosterInstanceId, questionnaireRosterTitle, interview));

            this.Items = new ObservableCollection<RosterItemViewModel>(rosterItemViewModels);
        }

        private RosterItemViewModel RosterModelToViewModel(string rosterInstanceId, string questionnaireRosterTitle, IStatefulInterview interview)
        {
            var roster = (InterviewRoster)interview.Groups[rosterInstanceId];

            var rosterItemViewModel = this.interviewViewModelFactory.GetNew<RosterItemViewModel>();

            rosterItemViewModel.Init(rosterIdentity: new Identity(roster.Id, roster.RosterVector),
                navigationState: navigationState);

            rosterItemViewModel.InterviewRosterTitle = roster.Title;
            rosterItemViewModel.QuestionnaireRosterTitle = questionnaireRosterTitle;
            rosterItemViewModel.AnsweredQuestionsCount = roster.AnsweredQuestionsCount;
            rosterItemViewModel.QuestionsCount = roster.QuestionsCount;
            rosterItemViewModel.SubgroupsCount = roster.SubgroupsCount;

            return rosterItemViewModel;
        }
    }
}