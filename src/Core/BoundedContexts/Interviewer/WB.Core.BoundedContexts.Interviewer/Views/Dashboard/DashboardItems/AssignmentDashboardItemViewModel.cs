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
            this.isExpanded = false;
            this.questionnaireIdentity = QuestionnaireIdentity.Parse(assignment.QuestionnaireId);
            this.detailedIdentifyingData = GetPrefilledQuestions(assignment.IdentifyingAnswers);
            this.identifyingData = new List<PrefilledQuestion>(detailedIdentifyingData.Take(3));
            this.PrefilledQuestions = new MvxObservableCollection<PrefilledQuestion>(identifyingData);
            this.GpsLocation = this.GetAssignmentLocation(assignment);
            this.ReceivedDate = assignment.ReceivedDateUtc.ToLocalTime().ToString("MMM d");
            this.ReceivedTime = assignment.ReceivedDateUtc.ToLocalTime().ToString("HH:mm");

            BindTitle();
            this.raiseEvents = true;
        }
        
        public void Init(AssignmentDocument assignmentDocument, EventHandler interviewsCountChanged, CreateNewViewModel itemService, int interviewsCount)
        {
            this.itemService = itemService;
            this.interviewsByAssignmentCount = interviewsCount;
            Bind(assignmentDocument);
            interviewsCountChanged += (sender, args) => this.Refresh();
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

        public MvxObservableCollection<PrefilledQuestion> PrefilledQuestions { get; private set; }

        public string Title
        {
            get => this.title;
            private set => RaiseAndSetIfChanged(ref this.title, value);
        }

        private bool isExpanded = false;
        public bool IsExpanded
        {
            get => this.isExpanded;
            set => RaiseAndSetIfChanged(ref this.isExpanded, value, onChange: UpdatePrefilledQuestions);
        }

        private void UpdatePrefilledQuestions(bool isexpanded)
        {
            this.PrefilledQuestions.SwitchTo(isexpanded ? this.detailedIdentifyingData : this.identifyingData);
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
        private CreateNewViewModel itemService;

        public IMvxCommand ToggleExpanded => new MvxCommand(() => this.IsExpanded = !this.IsExpanded);

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

        public bool HasExpandedView => this.detailedIdentifyingData.Count > 0;

        private void NavigateToGpsLocation()
        {
            this.externalAppLauncher.LaunchMapsWithTargetLocation(this.GpsLocation.Latitude, this.GpsLocation.Longitude);
        }

        // it's much more performant, as original extension call new Action<...> on every call
        private void RaiseAndSetIfChanged<TReturn>(ref TReturn backingField, TReturn newValue, [CallerMemberName] string propertyName = "", Action<TReturn> onChange = null)
        {
            if (EqualityComparer<TReturn>.Default.Equals(backingField, newValue)) return;

            backingField = newValue;
            onChange?.Invoke(backingField);
            if(this.raiseEvents)
            this.RaisePropertyChanged(propertyName);
        }

        private bool raiseEvents = true;
        private List<PrefilledQuestion> identifyingData;
        private List<PrefilledQuestion> detailedIdentifyingData;
    }
}