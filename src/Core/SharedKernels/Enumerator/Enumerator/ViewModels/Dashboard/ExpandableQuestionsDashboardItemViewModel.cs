using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Humanizer;
using MvvmCross.Commands;
using MvvmCross.ViewModels;
using NodaTime;
using NodaTime.Extensions;
using WB.Core.BoundedContexts.Headquarters.Assignments;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.Core.SharedKernels.Enumerator.Properties;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Views.Dashboard;

namespace WB.Core.SharedKernels.Enumerator.ViewModels.Dashboard
{
    public abstract class ExpandableQuestionsDashboardItemViewModel : MvxNotifyPropertyChanged, IDashboardItemWithEvents, IDisposable
    {
        private readonly IServiceLocator serviceLocator;
        private string idLabel;
        private string subTitle;
        private string title;
        private bool isExpanded = true;
        private DashboardInterviewStatus status;

        public event EventHandler OnItemUpdated;
        
        protected void RaiseOnItemUpdated() => OnItemUpdated?.Invoke(this, EventArgs.Empty);

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
            set
            {
                if (this.idLabel != value)
                {
                    this.idLabel = value;
                    RaisePropertyChanged();
                }
            }
        }

        public string Title
        {
            get => title;
            set
            {
                if (this.title != value)
                {
                    this.title = value;
                    RaisePropertyChanged();
                }
            }
        }

        public string SubTitle
        {
            get => subTitle;
            set
            {
                if (this.subTitle != value)
                {
                    this.subTitle = value;
                    this.RaisePropertyChanged();
                }
            }
        }

        public virtual string Comments { get; }
        
        protected string FormatDateTimeString(string formatString, DateTimeOffset dateTimeOffset, string timeZoneId)
        {
            DateTimeZone timeZone = null;
            if (timeZoneId != null)
                timeZone = DateTimeZoneProviders.Tzdb.GetZoneOrNull(timeZoneId);
            if (timeZone == null)
                timeZone = DateTimeZoneProviders.Tzdb.GetSystemDefault();
            var instant = dateTimeOffset.UtcDateTime.ToInstant();
            var zonedDateTime = new ZonedDateTime(instant, timeZone);

            return FormatDateTimeString(formatString, zonedDateTime.ToDateTimeUnspecified());
        }
        
        protected string FormatDateTimeString(string formatString, DateTime? dateTime)
        {
            if (!dateTime.HasValue)
                return string.Empty;
            
            var culture = CultureInfo.CurrentUICulture;
            var now = DateTime.Now;

            string dateTimeString = dateTime.Value.ToString("MMM dd, HH:mm", culture).ToPascalCase();
            if (dateTime.Value > now.AddDays(-1) && dateTime.Value < now.AddDays(2))
            {
                dateTimeString = dateTime.Value.Humanize(utcDate: false, culture: culture) + " (" + dateTimeString + ")";
            }
            
            return string.Format(formatString, dateTimeString);
        }

        protected void BindLocationAction(Guid? questionId, double? latitude, double? longtitude)
        {
            if (!questionId.HasValue || !latitude.HasValue || !longtitude.HasValue) return;

            Actions.Add(new ActionDefinition
            {
                Command = new MvxCommand(
                    () => ExternalAppLauncher.LaunchMapsWithTargetLocation(latitude.Value, longtitude.Value)),

                ActionType = ActionType.Secondary,

                Label = EnumeratorUIResources.Dashboard_ShowLocation
            });
        }

        private void ReleaseUnmanagedResources()
        {
            // release unmanaged resources here
        }

        protected virtual void Dispose(bool disposing)
        {
            ReleaseUnmanagedResources();
            if (disposing)
            {
                // release managed resources here
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~ExpandableQuestionsDashboardItemViewModel()
        {
            Dispose(false);
        }
    }
}
