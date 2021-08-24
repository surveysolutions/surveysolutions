#nullable enable

using System;
using System.Globalization;
using MvvmCross.Commands;
using MvvmCross.Navigation;
using MvvmCross.ViewModels;
using NodaTime;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernels.DataCollection.Commands.CalendarEvent;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.Enumerator.Properties;
using WB.Core.SharedKernels.Enumerator.Repositories;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.Views;

namespace WB.Core.SharedKernels.Enumerator.ViewModels
{
    public class CalendarEventViewModelArgs
    {
        public CalendarEventViewModelArgs(Guid? interviewId, string? interviewKey, int assignmentId, Action? okCallback)
        {
            InterviewId = interviewId;
            InterviewKey = interviewKey;
            AssignmentId = assignmentId;
            OkCallback = okCallback;
        }

        public Guid? InterviewId { get; set; }
        public string? InterviewKey { get; set; }
        public int AssignmentId { get; set; }
        public Action? OkCallback { get; set; }
    }

    public class CalendarEventDialogViewModel : MvxViewModel<CalendarEventViewModelArgs>
    {
        private readonly IMvxNavigationService navigationService;
        private readonly ICommandService commandService;
        private readonly IPrincipal principal;
        private readonly ICalendarEventStorage calendarEventStorage;
        private readonly IUserInteractionService userInteractionService;
        private readonly IStringFormat stringFormat;

        private CalendarEventViewModelArgs? initValues;
        private DateTime dateTimeEvent;
        private string? comment;
        private CalendarEvent? calendarEvent;

        public CalendarEventDialogViewModel(IMvxNavigationService navigationService,
            ICommandService commandService,
            IPrincipal principal,
            ICalendarEventStorage calendarEventStorage,
            IUserInteractionService userInteractionService,
            IStringFormat stringFormat)
        {
            this.navigationService = navigationService;
            this.commandService = commandService;
            this.principal = principal;
            this.calendarEventStorage = calendarEventStorage;
            this.userInteractionService = userInteractionService;
            this.stringFormat = stringFormat;
        }

        public override void Prepare(CalendarEventViewModelArgs param)
        {
            base.Prepare();

            initValues = param;

            calendarEvent = param.InterviewId.HasValue
                ? calendarEventStorage.GetCalendarEventForInterview(param.InterviewId.Value)
                : calendarEventStorage.GetCalendarEventForAssigment(param.AssignmentId);

            Title = calendarEvent == null
                ? EnumeratorUIResources.Dashboard_AddCalendarEvent
                : EnumeratorUIResources.Dashboard_EditCalendarEvent;

            var dateEvent = calendarEvent?.Start.LocalDateTime ?? DateTime.Today.AddDays(1);
            var timeEvent = calendarEvent?.Start.LocalDateTime.TimeOfDay ?? new TimeSpan(10, 00, 00);
            DateTimeValue = new DateTime(dateEvent.Year, dateEvent.Month, dateEvent.Day, 
                timeEvent.Hours, timeEvent.Minutes, timeEvent.Seconds, 
                DateTimeKind.Local);
            Comment = calendarEvent?.Comment;
        }

        public string? Title { get; set; }
        public string TimeString => stringFormat.ShortTime(DateTimeValue);

        public DateTime DateTimeValue
        {
            get => dateTimeEvent;
            set
            {
                dateTimeEvent = value;
                RaisePropertyChanged(nameof(DateString));
                RaisePropertyChanged(nameof(TimeString));
            }
        }

        public string DateString => 
            DateTimeValue.ToString(CultureInfo.CurrentUICulture.DateTimeFormat.ShortDatePattern);

        public string? Comment
        {
            get => comment;
            set => SetProperty(ref comment, value);
        }

        private string? timeError;
        public string? TimeError
        {
            get => timeError;
            set => SetProperty(ref timeError, value);
        }
        
        private string? dateError;
        public string? DateError
        {
            get => dateError;
            set => SetProperty(ref dateError, value);
        }

        public IMvxAsyncCommand CloseCommand =>
            new MvxAsyncCommand(async () => await this.navigationService.Close(this));
        public IMvxAsyncCommand OkCommand =>
            new MvxAsyncCommand(async () =>
            {
                SaveCalendarEvent();
                await this.navigationService.Close(this);
            });

        public IMvxAsyncCommand EditDate => new MvxAsyncCommand(async () =>
        {
            await userInteractionService.AskDateAsync(
                (sender, args) =>
                {
                    DateTimeValue = new DateTime(
                        args.Year, args.Month, args.Day, 
                        DateTimeValue.Hour, DateTimeValue.Minute, DateTimeValue.Second,
                        DateTimeKind.Local);
                },
                DateTimeValue,
                null /*DateTime.Today*/);
            ValidateDateTime();
        });

        public IMvxAsyncCommand EditTime => new MvxAsyncCommand(async () =>
        {
            await userInteractionService.AskTimeAsync(
                (sender, args) =>
                {
                    DateTimeValue = new DateTime(
                        DateTimeValue.Year, DateTimeValue.Month, DateTimeValue.Day, 
                        args.Hours, args.Minutes, args.Seconds,
                        DateTimeKind.Local);
               },
               DateTimeValue.TimeOfDay);
            ValidateDateTime();
        });

        private void ValidateDateTime()
        {
            var now = DateTime.Now;
            DateError = now.Date > DateTimeValue.Date ? EnumeratorUIResources.Dashboard_PastDateCalendarEvent : null;
            TimeError = now > DateTimeValue ? EnumeratorUIResources.Dashboard_PastTimeCalendarEvent : null;
        }

        private void SaveCalendarEvent()
        {
            if (initValues == null)
                throw  new ArgumentException("Need init calendar info data");

            if (DateTimeValue != calendarEvent?.Start.LocalDateTime || calendarEvent?.Comment != Comment)
            {
                var zoneInfo = DateTimeZoneProviders.Tzdb.GetSystemDefault();
                var timezone = zoneInfo.Id;
                
                var userId = principal.CurrentUserIdentity.UserId;
                ICommand command = calendarEvent == null
                    ? (ICommand)new CreateCalendarEventCommand(Guid.NewGuid(), userId, DateTimeValue,
                        timezone, 
                        initValues.InterviewId, 
                        initValues.InterviewKey,
                        initValues.AssignmentId, 
                        Comment,
                        new QuestionnaireIdentity() //dummy one, no validation on tablet
                        )
                    : new UpdateCalendarEventCommand(calendarEvent.Id, userId, 
                        DateTimeValue, timezone,
                        Comment,
                        new QuestionnaireIdentity() //dummy one, no validation on tablet
                        );

                commandService.Execute(command);
                
                initValues.OkCallback?.Invoke();
            }
        }
    }
}
