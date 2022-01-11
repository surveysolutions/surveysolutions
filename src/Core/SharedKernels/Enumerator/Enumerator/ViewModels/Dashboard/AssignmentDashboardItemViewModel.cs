#nullable enable
using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Humanizer;
using MvvmCross.Commands;
using NodaTime;
using NodaTime.Extensions;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.CommandBus.Implementation;
using WB.Core.SharedKernels.DataCollection.Commands.CalendarEvent;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.Enumerator.Properties;
using WB.Core.SharedKernels.Enumerator.Repositories;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.Views;
using WB.Core.SharedKernels.Enumerator.Views.Dashboard;

namespace WB.Core.SharedKernels.Enumerator.ViewModels.Dashboard
{
    public abstract class AssignmentDashboardItemViewModel : ExpandableQuestionsDashboardItemViewModel
    {
        protected readonly IServiceLocator serviceLocator;
        protected AssignmentDocument Assignment = null!;
        private int interviewsByAssignmentCount;
        private QuestionnaireIdentity questionnaireIdentity = null!;

        public int AssignmentId => this.Assignment.Id;

        protected AssignmentDashboardItemViewModel(IServiceLocator serviceLocator) : base(serviceLocator)
        {
            this.serviceLocator = serviceLocator;
        }

        protected IAssignmentDocumentsStorage AssignmentsRepository
            => serviceLocator.GetInstance<IAssignmentDocumentsStorage>();

        public int InterviewsLeftByAssignmentCount =>
            Assignment.Quantity.GetValueOrDefault() - interviewsByAssignmentCount;

        public int? Quantity => this.Assignment.Quantity;

        public override string Comments => this.Assignment.Comments;

        public bool HasAdditionalActions => Actions.Any(a => a.ActionType == ActionType.Context);
        
        public string AssignmentIdLabel { get; } = String.Empty;

        public void Init(AssignmentDocument assignmentDocument)
        {
            interviewsByAssignmentCount = assignmentDocument.CreatedInterviewsCount ?? 0;
            Assignment = assignmentDocument;
            questionnaireIdentity = QuestionnaireIdentity.Parse(Assignment.QuestionnaireId);
            Status = DashboardInterviewStatus.Assignment;

            BindTitles();
            BindDetails();
            BindActions();
            this.RaiseAllPropertiesChanged();
        }

        protected virtual void BindTitles()
        {
            Title = string.Format(EnumeratorUIResources.DashboardItem_Title, Assignment.Title, questionnaireIdentity.Version);
            IdLabel = "#" + Assignment.Id;
            string subTitle;

            if (Assignment.Quantity.HasValue)
            {
                if (InterviewsLeftByAssignmentCount == 1)
                {
                    subTitle = EnumeratorUIResources.Dashboard_AssignmentCard_SubTitleSingleInterivew;
                }
                else
                {
                    subTitle = EnumeratorUIResources.Dashboard_AssignmentCard_SubTitleCountdownFormat
                        .FormatString(InterviewsLeftByAssignmentCount, Assignment.Quantity);
                }
            }
            else
            {
                subTitle = EnumeratorUIResources.Dashboard_AssignmentCard_SubTitleCountdown_UnlimitedFormat
                    .FormatString(Assignment.Quantity.GetValueOrDefault());
            }

            this.SubTitle = subTitle;
            
            this.CalendarEventStart = Assignment.CalendarEvent.HasValue
                ? GetZonedDateTime(Assignment.CalendarEvent.Value, Assignment.CalendarEventTimezoneId)
                : (ZonedDateTime?)null;
            this.CalendarEventComment = Assignment.CalendarEventComment;
        }

        protected abstract void BindActions();

        private void BindDetails()
        {
            DetailedIdentifyingData = Assignment.IdentifyingAnswers.Select(ToIdentifyingQuestion).ToList();
            IdentifyingData = DetailedIdentifyingData.Take(count: 3).ToList();
            HasExpandedView = DetailedIdentifyingData.Count > 0;
            IsExpanded = false;
        }

        private PrefilledQuestion ToIdentifyingQuestion(AssignmentDocument.AssignmentAnswer identifyingAnswer)
        {
            return new PrefilledQuestion
            {
                Answer = identifyingAnswer.AnswerAsString,
                Question = identifyingAnswer.Question
            };
        }

        public void DecreaseInterviewsCount()
        {
            interviewsByAssignmentCount--;
            BindTitles();
        }

        private string? responsible;
        public string? Responsible
        {
            get => responsible;
            set => SetProperty(ref this.responsible, value);
        }

        protected async Task SetCalendarEventAsync()
        {
            var navigationService = serviceLocator.GetInstance<IViewModelNavigationService>();
            await navigationService.NavigateToAsync<CalendarEventDialogViewModel, CalendarEventViewModelArgs>(
                new CalendarEventViewModelArgs(
                    null,
                null,
                Assignment.Id,
                RaiseOnItemUpdated
            ));
        }

        protected void RemoveCalendarEvent()
        {
            var calendarEventStorage = serviceLocator.GetInstance<ICalendarEventStorage>();
            var calendarEvent = calendarEventStorage.GetCalendarEventForAssigment(Assignment.Id);

            if (calendarEvent == null)
                throw new ArgumentException("Cant delete calendar event, because it didn't setup early");

            var commandService = serviceLocator.GetInstance<ICommandService>();
            var principal = serviceLocator.GetInstance<IPrincipal>();
            var command = new DeleteCalendarEventCommand(calendarEvent.Id,
                principal.CurrentUserIdentity.UserId,
                new QuestionnaireIdentity() //dummy
                );
            commandService.Execute(command);
            
            RaiseOnItemUpdated();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
            }

            base.Dispose(disposing);
        }
    }
}
