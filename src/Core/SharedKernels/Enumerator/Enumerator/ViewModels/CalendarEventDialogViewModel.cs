#nullable enable

using System;
using MvvmCross.Commands;
using MvvmCross.Navigation;
using MvvmCross.ViewModels;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernels.DataCollection.Commands.CalendarEvent;
using WB.Core.SharedKernels.Enumerator.Repositories;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.Views;

namespace WB.Core.SharedKernels.Enumerator.ViewModels
{
    public class CalendarEventViewModelArgs
    {
        public Guid? InterviewId { get; set; }
        public int AssignmentId { get; set; }
        public Action? OkCallback { get; set; }
    }

    public class CalendarEventDialogViewModel : MvxViewModel<CalendarEventViewModelArgs>
    {
        private readonly IMvxNavigationService navigationService;
        private readonly ICommandService commandService;
        private readonly IPrincipal principal;
        private readonly ICalendarEventStorage calendarEventStorage;

        private CalendarEventViewModelArgs? initValues;
        private TimeSpan timeEvent;
        private DateTime dateEvent;
        private string? comment;
        private CalendarEvent? calendarEvent;

        public CalendarEventDialogViewModel(IMvxNavigationService navigationService,
            ICommandService commandService,
            IPrincipal principal,
            ICalendarEventStorage calendarEventStorage)
        {
            this.navigationService = navigationService;
            this.commandService = commandService;
            this.principal = principal;
            this.calendarEventStorage = calendarEventStorage;
        }

        public override void Prepare(CalendarEventViewModelArgs param)
        {
            base.Prepare();

            initValues = param;

            calendarEvent = param.InterviewId.HasValue
                ? calendarEventStorage.GetCalendarEventForInterview(param.InterviewId.Value)
                : calendarEventStorage.GetCalendarEventForAssigment(param.AssignmentId);

            DateEvent = calendarEvent?.Start.LocalDateTime ?? DateTime.Today.AddDays(1);
            TimeEvent = calendarEvent?.Start.LocalDateTime.TimeOfDay ?? new TimeSpan(10, 00, 00);
            Comment = calendarEvent?.Comment;
        }

        public TimeSpan TimeEvent
        {
            get => timeEvent;
            set => SetProperty(ref timeEvent, value);
        }

        public DateTime DateEvent
        {
            get => dateEvent;
            set => SetProperty(ref dateEvent, value);
        }

        public string? Comment
        {
            get => comment;
            set => SetProperty(ref comment, value);
        }

        public IMvxAsyncCommand CloseCommand =>
            new MvxAsyncCommand(async () => await this.navigationService.Close(this));
        public IMvxAsyncCommand OkCommand =>
            new MvxAsyncCommand(async () =>
            {
                SaveCalendarEvent();
                await this.navigationService.Close(this);
            });

        private void SaveCalendarEvent()
        {
            if (initValues == null)
                throw  new ArgumentException("Need init calendar info data");

            var dateTime = new DateTime(DateEvent.Year, DateEvent.Month, DateEvent.Day, TimeEvent.Hours,
                TimeEvent.Minutes, TimeEvent.Seconds, DateTimeKind.Local);

            if (dateTime != calendarEvent?.Start.LocalDateTime || calendarEvent?.Comment != Comment)
            {
                var userId = principal.CurrentUserIdentity.UserId;
                ICommand command = calendarEvent == null
                    ? (ICommand)new CreateCalendarEventCommand(Guid.NewGuid(), userId, dateTime, initValues.InterviewId, initValues.AssignmentId, Comment)
                    : new UpdateCalendarEventCommand(calendarEvent.Id, userId, dateTime, Comment);

                commandService.Execute(command);
                
                initValues.OkCallback?.Invoke();
            }
        }
    }
}
