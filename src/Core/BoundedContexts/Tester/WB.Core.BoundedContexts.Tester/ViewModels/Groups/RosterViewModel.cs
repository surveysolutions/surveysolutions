using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Cirrious.MvvmCross.ViewModels;
using WB.Core.BoundedContexts.QuestionnaireTester.Repositories;
using WB.Core.BoundedContexts.QuestionnaireTester.Services;
using WB.Core.BoundedContexts.QuestionnaireTester.ViewModels.InterviewEntities;
using WB.Core.BoundedContexts.QuestionnaireTester.ViewModels.Questions.State;
using WB.Core.BoundedContexts.QuestionnaireTester.ViewModels.Questions;
using WB.Core.Infrastructure.EventBus.Lite;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Events.Interview;

namespace WB.Core.BoundedContexts.QuestionnaireTester.ViewModels.Groups
{
    public class RosterViewModel : MvxNotifyPropertyChanged,
        IInterviewEntityViewModel, 
        ILiteEventHandler<RosterInstancesTitleChanged>,
        ILiteEventHandler<RosterInstancesAdded>,
        ILiteEventHandler<RosterInstancesRemoved>
    {
        private readonly ILiteEventRegistry liteEventRegistry;
        private readonly IStatefulInterviewRepository interviewRepository;
        private readonly IInterviewViewModelFactory interviewViewModelFactory;
        private string interviewId;
        private Identity groupIdentity;
        private NavigationState navigationState;
        public EnablementViewModel Enablement { get; private set; }

        private IList items;

        public IList Items
        {
            get { return this.items; }
            set { this.items = value; this.RaisePropertyChanged(); }
        }

        public RosterViewModel(
            IStatefulInterviewRepository interviewRepository, 
            ILiteEventRegistry liteEventRegistry, 
            IInterviewViewModelFactory interviewViewModelFactory,
            EnablementViewModel enablement)
        {
            this.Enablement = enablement;
            this.liteEventRegistry = liteEventRegistry;
            this.interviewRepository = interviewRepository;
            this.interviewViewModelFactory = interviewViewModelFactory;
        }

        public void Init(string interviewId, Identity entityIdentity, NavigationState navigationState)
        {
            this.interviewId = interviewId;
            this.groupIdentity = entityIdentity;
            this.navigationState = navigationState;

            this.Enablement.Init(interviewId, entityIdentity, navigationState);

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

            var rosterKey = ConversionHelper.ConvertIdAndRosterVectorToString(this.groupIdentity.Id, this.groupIdentity.RosterVector);

            if (!interview.RosterInstancesIds.ContainsKey(rosterKey))
            {
                this.Items = new List<RosterStateViewModel>();
                return;
            }

            var rosterInstances = interview.RosterInstancesIds[rosterKey];
            var rosterItemViewModels = rosterInstances.Select(this.RosterModelToViewModel);
            this.Items = new List<RosterStateViewModel>(rosterItemViewModels);
        }

        private RosterStateViewModel RosterModelToViewModel(Identity rosterIdentity)
        {
            var rosterItemViewModel = this.interviewViewModelFactory.GetNew<RosterStateViewModel>();

            rosterItemViewModel.Init(interviewId: this.interviewId, rosterIdentity: rosterIdentity, navigationState: this.navigationState);

            return rosterItemViewModel;
        }
    }
}