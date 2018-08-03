using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using MvvmCross;
using MvvmCross.ViewModels;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection;

namespace WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails
{
    public class BreadCrumbsViewModel : MvxNotifyPropertyChanged, IDisposable
    {
        private readonly IQuestionnaireStorage questionnaireRepository;
        private readonly IStatefulInterviewRepository interviewRepository;
        private NavigationState navigationState;
        private string interviewId;

        public BreadCrumbsViewModel(
            IQuestionnaireStorage questionnaireRepository,
            IStatefulInterviewRepository interviewRepository)
        {
            this.questionnaireRepository = questionnaireRepository;
            this.interviewRepository = interviewRepository;
        }

        public void Init(string interviewId, NavigationState navigationState)
        {
            if (navigationState == null) throw new ArgumentNullException(nameof(navigationState));
            if (this.navigationState != null) throw new Exception($"ViewModel {typeof(BreadCrumbsViewModel)} already initialized");

            this.navigationState = navigationState;
            this.interviewId = interviewId;
            this.navigationState.ScreenChanged += this.OnScreenChanged;
        }
        
        void OnScreenChanged(ScreenChangedEventArgs eventArgs)
        {
            if (eventArgs.TargetStage != ScreenType.Group)
            {
                this.Items = new ReadOnlyCollection<BreadCrumbItemViewModel>(new List<BreadCrumbItemViewModel>());
            }
            else
            {
                this.BuildBreadCrumbs(eventArgs.TargetGroup);
            }
        }

        private void BuildBreadCrumbs(Identity newGroupIdentity)
        {
            var interview = this.interviewRepository.Get(this.interviewId);
            var questionnaireModel = this.questionnaireRepository.GetQuestionnaire(interview.QuestionnaireIdentity, interview.Language);
            
            ReadOnlyCollection<Guid> parentIds = questionnaireModel.GetParentsStartingFromTop(newGroupIdentity.Id);

            var breadCrumbs = new List<BreadCrumbItemViewModel>();
            int metRosters = 0;
            foreach (Guid parentId in parentIds)
            {
                if (questionnaireModel.IsRosterGroup(parentId))
                {
                    metRosters++;
                    var itemRosterVector = newGroupIdentity.RosterVector.Shrink(metRosters);
                    var itemIdentity = new Identity(parentId, itemRosterVector);
                    
                    var breadCrumb = Mvx.Resolve<BreadCrumbItemViewModel>();
                    breadCrumb.Init(this.interviewId, itemIdentity, this.navigationState);
                    breadCrumbs.Add(breadCrumb);
                }
                else
                {
                    var itemIdentity = new Identity(parentId, newGroupIdentity.RosterVector.Shrink(metRosters));
                    var breadCrumb = Mvx.Resolve<BreadCrumbItemViewModel>();
                    breadCrumb.Init(this.interviewId, itemIdentity, this.navigationState);
                    breadCrumbs.Add(breadCrumb);
                }
            }

            this.Items?.ForEach(x => x.Dispose());
            this.Items = new ReadOnlyCollection<BreadCrumbItemViewModel>(breadCrumbs);
        }

        private ReadOnlyCollection<BreadCrumbItemViewModel> items;
        public ReadOnlyCollection<BreadCrumbItemViewModel> Items
        {
            get { return this.items; }
            set { this.items = value; this.RaisePropertyChanged(); }
        }

        public void Dispose()
        {
            if (this.navigationState != null)
            {
                this.navigationState.ScreenChanged -= this.OnScreenChanged;
            }
            this.Items?.Where(x => x != null).ForEach(x => x.Dispose());
        }
    }
}
