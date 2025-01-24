using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using MvvmCross.Commands;
using MvvmCross.ViewModels;
using NodaTime;
using NodaTime.Extensions;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.Core.SharedKernels.Enumerator.Properties;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.Utils;
using WB.Core.SharedKernels.Enumerator.Views.Dashboard;

namespace WB.Core.SharedKernels.Enumerator.ViewModels.Dashboard
{
    public abstract class ExpandableQuestionsDashboardItemViewModel : MvxNotifyPropertyChanged, IDashboardItemWithEvents, IDisposable
    {
        private readonly IServiceLocator serviceLocator;
        private readonly IMapInteractionService mapInteractionService;
        private readonly IUserInteractionService userInteractionService;
        private string idLabel;
        private string subTitle;
        private ZonedDateTime? calendarEventStart;
        private string title;
        private bool isExpanded = true;
        private DashboardInterviewStatus status;

        public event EventHandler OnItemUpdated;
        public virtual void RefreshDataTime()
        {
            RefreshSubtitle();
            RaisePropertyChanged(nameof(CalendarEvent));
        }

        protected virtual void RefreshSubtitle()
        {
        }

        protected void RaiseOnItemUpdated() => OnItemUpdated?.Invoke(this, EventArgs.Empty);

        public ExpandableQuestionsDashboardItemViewModel(IServiceLocator serviceLocator, 
            IMapInteractionService mapInteractionService,
            IUserInteractionService userInteractionService)
        {
            this.serviceLocator = serviceLocator;
            this.mapInteractionService = mapInteractionService;
            this.userInteractionService = userInteractionService;
            Actions = new MvxObservableCollection<ActionDefinition>();
            Actions.CollectionChanged += CollectionChanged;
        }

        private void CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            RaisePropertyChanged(nameof(PrimaryAction));
            RaisePropertyChanged(nameof(SecondaryAction));
            RaisePropertyChanged(nameof(ContextMenu));
            RaisePropertyChanged(nameof(HasContextMenu));
            RaisePropertyChanged(nameof(HasSecondaryAction));
            RaisePropertyChanged(nameof(ExtraAction));
            RaisePropertyChanged(nameof(HasExtraAction));
            RaisePropertyChanged(nameof(TargetAreaAction));
            RaisePropertyChanged(nameof(HasTargetAreaAction));
        }

        private IExternalAppLauncher ExternalAppLauncher =>
            serviceLocator.GetInstance<IExternalAppLauncher>();   
        
        private IStringFormat StringFormat =>
            serviceLocator.GetInstance<IStringFormat>();

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
        public ActionDefinition ExtraAction => Actions.SingleOrDefault(a => a.ActionType == ActionType.Extra);
        public ActionDefinition TargetAreaAction => Actions.SingleOrDefault(a => a.ActionType == ActionType.TargetArea);
        public IEnumerable<ActionDefinition> ContextMenu => Actions.Where(a => a.ActionType == ActionType.Context);

        public bool HasContextMenu => ContextMenu.Any(cm => cm.IsEnabled);

        public bool HasSecondaryAction => SecondaryAction != null;
        public bool HasExtraAction => ExtraAction != null;
        public bool HasTargetAreaAction => TargetAreaAction != null;

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

        public string CalendarEvent
        {
            get
            {
                if (CalendarEventStart.HasValue)
                {
                    var calendarString = FormatDateTimeString(
                        EnumeratorUIResources.Dashboard_ShowCalendarEvent, 
                        CalendarEventStart.Value.ToDateTimeUtc());
                    string separatorVisit = !string.IsNullOrEmpty(CalendarEventComment) 
                        ? Environment.NewLine : string.Empty;
                    return $"{calendarString}{separatorVisit}{CalendarEventComment}";
                }

                return null;
            }
        }

        public ZonedDateTime? CalendarEventStart
        {
            get => calendarEventStart;
            set
            {
                if (this.calendarEventStart != value)
                {
                    this.calendarEventStart = value;
                    this.RaisePropertyChanged(nameof(CalendarEventStart));
                    this.RaisePropertyChanged(nameof(CalendarEvent));
                }
            }
        }
        
        public string CalendarEventComment { get; set; }

        public virtual string Comments { get; }
        
        protected ZonedDateTime GetZonedDateTime(DateTimeOffset dateTimeOffset, string timeZoneId)
        {
            DateTimeZone timeZone = null;
            if (timeZoneId != null)
                timeZone = DateTimeZoneProviders.Tzdb.GetZoneOrNull(timeZoneId);
            if (timeZone == null)
                timeZone = DateTimeZoneProviders.Tzdb.GetSystemDefault();
            var instant = dateTimeOffset.UtcDateTime.ToInstant();
            var zonedDateTime = new ZonedDateTime(instant, timeZone);
            return zonedDateTime;
        }

        protected string FormatDateTimeString(string formatString, DateTime? utcDateTime)
        {
            if (!utcDateTime.HasValue)
                return string.Empty;
            
            var now = DateTime.Now;
            var dateTime = DateTime.SpecifyKind(utcDateTime.Value, DateTimeKind.Utc);

            var localTime = dateTime.ToLocalTime();
            string dateTimeString = StringFormat.ShortDateTime(localTime).ToPascalCase();
            if (localTime > now.Date.AddDays(-1) && localTime < now.Date.AddDays(3))
            {
                dateTimeString = NumericTextFormatter.FormatTimeHumanized(now - localTime) + " (" + dateTimeString + ")";
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
        
        protected void BindTargetAreaAction(int assignmentId, string targetArea)
        {
            if (string.IsNullOrWhiteSpace(targetArea)) return;

            Actions.Add(new ActionDefinition
            {
                Command = new MvxAsyncCommand(
                    () => NavigateToMapDashboardAsync(assignmentId)),

                ActionType = ActionType.TargetArea,

                Label = EnumeratorUIResources.Dashboard_ShowAssignmentMap
            });
        }
        
        private async Task NavigateToMapDashboardAsync(int assignmentId)
        {
            if (!mapInteractionService.DoesSupportMaps)
            {
                userInteractionService.ShowToast(UIResources.Version_Not_Supports);
                return;
            }
            
            try
            {
                await mapInteractionService.OpenAssignmentMapAsync(assignmentId);
            }
            catch (MissingPermissionsException e)
            {
                userInteractionService.ShowToast(e.Message);
            }
        }
        
        public void Dispose()
        {
            Actions.CollectionChanged -= CollectionChanged;
        }
    }
}
