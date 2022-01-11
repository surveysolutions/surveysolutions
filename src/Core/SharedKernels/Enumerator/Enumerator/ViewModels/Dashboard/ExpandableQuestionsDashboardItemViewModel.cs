using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.Linq;
using Humanizer;
using MvvmCross.Commands;
using MvvmCross.ViewModels;
using NodaTime;
using NodaTime.Extensions;
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
        private ZonedDateTime? calendarEventStart;
        private string title;
        private bool isExpanded = true;
        private DashboardInterviewStatus status;

        public event EventHandler OnItemUpdated;
        public virtual void RefreshDataTime()
        {
            RaisePropertyChanged(nameof(SubTitle));
            RaisePropertyChanged(nameof(CalendarEvent));
        }

        protected void RaiseOnItemUpdated() => OnItemUpdated?.Invoke(this, EventArgs.Empty);

        public ExpandableQuestionsDashboardItemViewModel(IServiceLocator serviceLocator)
        {
            this.serviceLocator = serviceLocator;
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
        public IEnumerable<ActionDefinition> ContextMenu => Actions.Where(a => a.ActionType == ActionType.Context);

        public bool HasContextMenu => ContextMenu.Any(cm => cm.IsEnabled);

        public bool HasSecondaryAction => SecondaryAction != null;
        public bool HasExtraAction => ExtraAction != null;

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

        public virtual string SubTitle
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

        protected string FormatDateTimeString(string formatString, DateTimeOffset dateTimeOffset, string timeZoneId)
        {
            var zonedDateTime = GetZonedDateTime(dateTimeOffset, timeZoneId);
            return FormatDateTimeString(formatString, zonedDateTime.ToDateTimeUtc());
        }
        
        protected string FormatDateTimeString(string formatString, DateTime? utcDateTime)
        {
            if (!utcDateTime.HasValue)
                return string.Empty;
            
            var culture = CultureInfo.CurrentUICulture;
            var now = DateTime.Now;
            var dateTime = DateTime.SpecifyKind(utcDateTime.Value, DateTimeKind.Utc);

            var localTime = dateTime.ToLocalTime();
            string dateTimeString = StringFormat.ShortDateTime(localTime).ToPascalCase();
            if (localTime > now.Date.AddDays(-1) && localTime < now.Date.AddDays(3))
            {
                dateTimeString = localTime.Humanize(utcDate: false, culture: culture) + " (" + dateTimeString + ")";
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
            Actions.CollectionChanged -= CollectionChanged;
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~ExpandableQuestionsDashboardItemViewModel()
        {
            Dispose(false);
        }
    }
}
