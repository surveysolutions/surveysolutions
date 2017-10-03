using System;
using System.Collections.Generic;
using System.Linq;
using MvvmCross.Core.ViewModels;
using WB.Core.BoundedContexts.Interviewer.Properties;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.Core.SharedKernels.Enumerator.Services;

namespace WB.Core.BoundedContexts.Interviewer.Views.Dashboard.DashboardItems
{
    public class ExpandableQuestionsDashboardItemViewModel : MvxNotifyPropertyChanged, IDashboardItem
    {
        private readonly IServiceLocator serviceLocator;
        private string idLabel;
        private string subTitle;
        private string title;
        private bool isExpanded = true;
        private DashboardInterviewStatus status;

        public ExpandableQuestionsDashboardItemViewModel(IServiceLocator serviceLocator)
        {
            this.serviceLocator = serviceLocator;
            Actions = new MvxObservableCollection<ActionDefinition>();
            Actions.CollectionChanged += (sender, args) =>
            {
                RaisePropertyChanged(nameof(PrimaryAction));
                RaisePropertyChanged(nameof(SecondaryAction));
                RaisePropertyChanged(nameof(ContextMenu));
                RaisePropertyChanged(nameof(HasContextMenu));
                RaisePropertyChanged(nameof(HasSecondaryAction));
            };
        }

        private IExternalAppLauncher ExternalAppLauncher =>
            serviceLocator.GetInstance<IExternalAppLauncher>();

        public bool HasExpandedView { get; protected set; }

        public bool IsExpanded
        {
            get => this.isExpanded;
            set
            {
                if (this.isExpanded == value) return;

                this.isExpanded = value;
                this.RaisePropertyChanged();
                this.RaisePropertyChanged(nameof(PrefilledQuestions));
            }
        }

        public DashboardInterviewStatus Status
        {
            get => status;
            set => SetProperty(ref status, value);
        }

        public List<PrefilledQuestion> PrefilledQuestions =>
            this.IsExpanded ? this.DetailedIdentifyingData : this.IdentifyingData;

        protected List<PrefilledQuestion> IdentifyingData;
        protected List<PrefilledQuestion> DetailedIdentifyingData;

        public ActionDefinition PrimaryAction => Actions.SingleOrDefault(a => a.ActionType == ActionType.Primary);
        public ActionDefinition SecondaryAction => Actions.SingleOrDefault(a => a.ActionType == ActionType.Secondary);
        public IEnumerable<ActionDefinition> ContextMenu => Actions.Where(a => a.ActionType == ActionType.Context);

        public bool HasContextMenu => ContextMenu.Any(cm => cm.IsEnabled);

        public bool HasSecondaryAction => SecondaryAction != null;

        public MvxObservableCollection<ActionDefinition> Actions { get; }

        public string IdLabel
        {
            get => idLabel;
            set => SetProperty(ref idLabel, value);
        }

        public string Title
        {
            get => title;
            set => SetProperty(ref title, value);
        }

        public string SubTitle
        {
            get => subTitle;
            set => SetProperty(ref subTitle, value);
        }

        protected void BindLocationAction(Guid? questionId, double? latitude, double? longtitude)
        {
            if (!questionId.HasValue || !latitude.HasValue || !longtitude.HasValue) return;

            Actions.Add(new ActionDefinition
            {
                Command = new MvxCommand(
                    () => ExternalAppLauncher.LaunchMapsWithTargetLocation(latitude.Value, longtitude.Value)),

                ActionType = ActionType.Secondary,

                Label = InterviewerUIResources.Dashboard_ShowLocation
            });
        }
    }
}