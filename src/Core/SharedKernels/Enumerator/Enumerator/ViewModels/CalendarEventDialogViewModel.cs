#nullable enable

using System;
using MvvmCross.Commands;
using MvvmCross.Navigation;
using MvvmCross.ViewModels;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernels.DataCollection.Commands.CalendarEvent;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;

namespace WB.Core.SharedKernels.Enumerator.ViewModels
{
    public class CalendarEventViewModelArgs
    {
        public Guid? InterviewId { get; set; }
        public int AssignmentId { get; set; }
        public Guid? CalendarEventId { get; set; }
        public DateTimeOffset? Start { get; set; }
        public string? Comment { get; set; }
        public Action? OkCallback { get; set; }
    }

    public class CalendarEventDialogViewModel : MvxViewModel<CalendarEventViewModelArgs>
    {
        private readonly IMvxNavigationService navigationService;
        private readonly ICommandService commandService;
        private readonly IPrincipal principal;

        private CalendarEventViewModelArgs? initValues;
        private TimeSpan timeEvent;
        private DateTime dateEvent;
        private string? comment;


        public CalendarEventDialogViewModel(IMvxNavigationService navigationService,
            ICommandService commandService,
            IPrincipal principal)
        {
            this.navigationService = navigationService;
            this.commandService = commandService;
            this.principal = principal;
        }

        public override void Prepare(CalendarEventViewModelArgs param)
        {
            base.Prepare();

            initValues = param;

            DateEvent = param.Start?.LocalDateTime ?? DateTime.Today;
            TimeEvent = param.Start?.LocalDateTime.TimeOfDay ?? new TimeSpan(10, 00, 00);
            Comment = param.Comment;
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

            if (dateTime != initValues.Start?.LocalDateTime || initValues.Comment != Comment)
            {
                var userId = principal.CurrentUserIdentity.UserId;
                ICommand command = !initValues.CalendarEventId.HasValue
                    ? (ICommand)new CreateCalendarEventCommand(Guid.NewGuid(), userId, dateTime, initValues.InterviewId, initValues.AssignmentId, Comment)
                    : new UpdateCalendarEventCommand(initValues.CalendarEventId.Value, userId, dateTime, Comment);

                commandService.Execute(command);
                
                initValues.OkCallback?.Invoke();
            }
        }
    }
}
