using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using MvvmCross.Core.ViewModels;
using WB.Core.BoundedContexts.Interviewer.Properties;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.Enumerator.Services;

namespace WB.Core.BoundedContexts.Interviewer.Views.Dashboard.DashboardItems
{
    public class AssignmentDashboardItemViewModel : MvxNotifyPropertyChanged, IDashboardItem
    {
        private readonly IExternalAppLauncher externalAppLauncher;

        public AssignmentDashboardItemViewModel(IExternalAppLauncher externalAppLauncher)
        {
            this.externalAppLauncher = externalAppLauncher;
        }

        private QuestionnaireIdentity questionnaireIdentity;

        public void Bind(AssignmentDocument assignmentDocument)
        {
            this.raiseEvents = false;
            this.assignment = assignmentDocument;
            this.questionnaireIdentity = QuestionnaireIdentity.Parse(assignment.QuestionnaireId);
            var identifyingData = assignment.IdentifyingAnswers;
            this.PrefilledQuestions = GetPrefilledQuestions(identifyingData.Take(3));
            this.DetailedPrefilledQuestions = GetPrefilledQuestions(identifyingData.Skip(3));
            this.GpsLocation = this.GetAssignmentLocation(assignment);
            this.ReceivedDate = assignment.ReceivedDateUtc.ToLocalTime().ToString("MMM d");
            this.ReceivedTime = assignment.ReceivedDateUtc.ToLocalTime().ToString("HH:mm");

            BindTitle();
            this.raiseEvents = true;
        }
        
        public void Init(AssignmentDocument assignmentDocument, DashboardViewModel dashboardViewModel, IAssignmentItemService itemService, int interviewsCount)
        {
            this.itemService = itemService;
            this.interviewsByAssignmentCount = interviewsCount;
            Bind(assignmentDocument);
            dashboardViewModel.InterviewsCountChanged += (sender, args) => this.Refresh();
        }

        private void Refresh()
        {
            this.interviewsByAssignmentCount = this.itemService.GetInterviewsCount(this.assignment.Id);
            BindTitle();
        }

        private void BindTitle()
        {
            var newTitle = string.Format(InterviewerUIResources.Dashboard_Assignment_CardTitle, this.assignment.Id) + ": ";
            var allowToCreate = true;

            if (this.assignment.Quantity.HasValue)
            {
                var interviewsLeftByAssignmentCount = Math.Max(0, this.assignment.Quantity.Value - interviewsByAssignmentCount);
                allowToCreate = interviewsLeftByAssignmentCount > 0;
                newTitle += InterviewerUIResources.Dashboard_AssignmentCard_TitleCountdown.FormatString(interviewsLeftByAssignmentCount);
            }
            else
            {
                newTitle += InterviewerUIResources.Dashboard_AssignmentCard_TitleCountdown_Unlimited;
            }

            var commentary = string.Format(InterviewerUIResources.DashboardItem_AssignmentCreatedComment, interviewsByAssignmentCount);

            this.AllowToCreateNewInterview = allowToCreate;
            this.Title = newTitle;
            this.Comment = commentary;
        }

        public string ReceivedTime { get; set; }
        public int AssignmentId => this.assignment.Id;

        public string ReceivedDate { get; private set; }

        private AssignmentDocument assignment;
        private string title;
        private string comment;

        public string QuestionnaireName => string.Format(InterviewerUIResources.DashboardItem_Title, this.assignment.Title, this.questionnaireIdentity.Version);

        public string Comment
        {
            get => this.comment;
            private set => RaiseAndSetIfChanged(ref this.comment, value);
        }

        public List<PrefilledQuestion> PrefilledQuestions { get; private set; }
        public List<PrefilledQuestion> DetailedPrefilledQuestions { get; private set; }

        public string Title
        {
            get => this.title;
            private set => RaiseAndSetIfChanged(ref this.title, value);
        }

        public bool AllowToCreateNewInterview
        {
            get => this.allowToCreateNewInterview;
            private set => RaiseAndSetIfChanged(ref this.allowToCreateNewInterview, value);
        }

        public InterviewGpsCoordinatesView GpsLocation { get; private set; }
        public bool HasGpsLocation => this.GpsLocation != null;

        private bool allowToCreateNewInterview;
        private int interviewsByAssignmentCount;
        private IAssignmentItemService itemService;

        public IMvxAsyncCommand CreateNewInterviewCommand => new MvxAsyncCommand(
            () => this.itemService.CreateInterviewAsync(assignment),
            () => AllowToCreateNewInterview);

        private List<PrefilledQuestion> GetPrefilledQuestions(IEnumerable<AssignmentDocument.AssignmentAnswer> identifyingAnswers)
        {
            return identifyingAnswers.Select(fi => new PrefilledQuestion
            {
                Answer = fi.AnswerAsString,
                Question = fi.Question
            }).ToList();
        }

        private InterviewGpsCoordinatesView GetAssignmentLocation(AssignmentDocument assignmentDocument)
        {
            if (assignmentDocument.LocationQuestionId.HasValue && assignmentDocument.LocationLatitude.HasValue && assignmentDocument.LocationLongitude.HasValue)
            {
                return new InterviewGpsCoordinatesView
                {
                    Latitude = assignmentDocument.LocationLatitude ?? 0,
                    Longitude = assignmentDocument.LocationLongitude ?? 0
                };
            }

            return null;
        }

        public IMvxCommand NavigateToGpsLocationCommand
        {
            get { return new MvxCommand(this.NavigateToGpsLocation, () => this.HasGpsLocation); }
        }

        public bool HasExpandedView => this.PrefilledQuestions.Count > 0;

        private void NavigateToGpsLocation()
        {
            this.externalAppLauncher.LaunchMapsWithTargetLocation(this.GpsLocation.Latitude, this.GpsLocation.Longitude);
        }

        // it's much more performant, as original extension call new Action<...> on every call
        private void RaiseAndSetIfChanged<TReturn>(ref TReturn backingField, TReturn newValue, [CallerMemberName] string propertyName = "")
        {
            if (EqualityComparer<TReturn>.Default.Equals(backingField, newValue)) return;

            backingField = newValue;

            if(this.raiseEvents)
            this.RaisePropertyChanged(propertyName);
        }

        private bool raiseEvents = true;
    }
}