using System;
using System.Collections.Generic;
using System.Linq;
using MvvmCross.Core.ViewModels;
using WB.Core.BoundedContexts.Interviewer.Properties;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.Enumerator.Services;

namespace WB.Core.BoundedContexts.Interviewer.Views.Dashboard.DashboardItems
{
    public class AssignmentDashboardItemViewModel : ExpandableQuestionsDashboardItemViewModel
    {
        private readonly IExternalAppLauncher externalAppLauncher;

        public AssignmentDashboardItemViewModel(IExternalAppLauncher externalAppLauncher)
        {
            this.externalAppLauncher = externalAppLauncher;
        }

        private QuestionnaireIdentity questionnaireIdentity;

        public void Bind(AssignmentDocument assignmentDocument)
        {
            this.assignment = assignmentDocument;
            
            this.questionnaireIdentity = QuestionnaireIdentity.Parse(assignment.QuestionnaireId);
            this.DetailedIdentifyingData = GetPrefilledQuestions(assignment.IdentifyingAnswers);
            this.IdentifyingData = new List<PrefilledQuestion>(DetailedIdentifyingData.Take(3));
            
            this.GpsLocation = this.GetAssignmentLocation(assignment);
            this.ReceivedDate = assignment.ReceivedDateUtc.ToLocalTime().ToString("MMM d");
            this.ReceivedTime = assignment.ReceivedDateUtc.ToLocalTime().ToString("HH:mm");
            this.IsExpanded = false;
            this.HasExpandedView = this.DetailedIdentifyingData.Count > 0;

            BindTitle();
        }
        
        public void Init(AssignmentDocument assignmentDocument, EventHandler interviewsCountChanged, CreateNewViewModel createNewViewModel, int interviewsCount)
        {
            this.createNewViewModel = createNewViewModel;
            this.interviewsByAssignmentCount = interviewsCount;
            Bind(assignmentDocument);
            interviewsCountChanged += (sender, args) => this.Refresh();
        }

        private void Refresh()
        {
            this.interviewsByAssignmentCount = this.createNewViewModel.GetInterviewsCount(this.assignment.Id);
            BindTitle();
        }

        private void BindTitle()
        {
            var newTitle = string.Format(InterviewerUIResources.Dashboard_Assignment_CardTitle, this.assignment.Id.ToString()) + ": ";
            var allowToCreate = true;

            if (this.assignment.Quantity.HasValue)
            {
                var interviewsLeftByAssignmentCount = Math.Max(0, this.assignment.Quantity.Value - interviewsByAssignmentCount);
                allowToCreate = interviewsLeftByAssignmentCount > 0;
                newTitle += InterviewerUIResources.Dashboard_AssignmentCard_TitleCountdown.FormatString(interviewsLeftByAssignmentCount.ToString());
            }
            else
            {
                newTitle += InterviewerUIResources.Dashboard_AssignmentCard_TitleCountdown_Unlimited;
            }

            var commentary = string.Format(InterviewerUIResources.DashboardItem_AssignmentCreatedComment, interviewsByAssignmentCount.ToString());

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

        public string QuestionnaireName => string.Format(InterviewerUIResources.DashboardItem_Title, this.assignment.Title, this.questionnaireIdentity.Version.ToString());

        public string Comment
        {
            get => this.comment;
            private set => this.RaiseAndSetIfChanged(ref this.comment, value);
        }
        
        public string Title
        {
            get => this.title;
            private set => this.RaiseAndSetIfChanged(ref this.title, value);
        }
        
        public bool AllowToCreateNewInterview
        {
            get => this.allowToCreateNewInterview;
            private set => this.RaiseAndSetIfChanged(ref this.allowToCreateNewInterview, value);
        }

        public InterviewGpsCoordinatesView GpsLocation { get; private set; }
        public bool HasGpsLocation => this.GpsLocation != null;

        private bool allowToCreateNewInterview;
        private int interviewsByAssignmentCount;
        private CreateNewViewModel createNewViewModel;

        public IMvxCommand ToggleExpanded => new MvxCommand(() => this.IsExpanded = !this.IsExpanded);

        public IMvxAsyncCommand CreateNewInterviewCommand => new MvxAsyncCommand(
            () => this.createNewViewModel.CreateInterviewAsync(assignment.Id),
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
        
        private void NavigateToGpsLocation()
        {
            this.externalAppLauncher.LaunchMapsWithTargetLocation(this.GpsLocation.Latitude, this.GpsLocation.Longitude);
        }
    }
}