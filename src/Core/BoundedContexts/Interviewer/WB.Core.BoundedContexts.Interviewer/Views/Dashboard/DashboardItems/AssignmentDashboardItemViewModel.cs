using System;
using System.Linq;
using MvvmCross.Core.ViewModels;
using MvvmCross.Platform.UI;
using WB.Core.BoundedContexts.Interviewer.Properties;
using WB.Core.BoundedContexts.Interviewer.Services;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.Enumerator.Services;

namespace WB.Core.BoundedContexts.Interviewer.Views.Dashboard.DashboardItems
{
    public class AssignmentDashboardItemViewModel : ExpandableQuestionsDashboardItemViewModel
    {
        private readonly IExternalAppLauncher externalAppLauncher;
        private readonly IInterviewFromAssignmentCreatorService interviewFromAssignmentCreator;

        public AssignmentDashboardItemViewModel(IExternalAppLauncher externalAppLauncher,
            IInterviewFromAssignmentCreatorService interviewFromAssignmentCreator)
        {
            this.externalAppLauncher = externalAppLauncher;
            this.interviewFromAssignmentCreator = interviewFromAssignmentCreator;
        }
        
        private AssignmentDocument assignment;
        private int interviewsByAssignmentCount;

        public void Init(AssignmentDocument assignmentDocument, int interviewsCount)
        {
            this.interviewsByAssignmentCount = interviewsCount;
            this.assignment = assignmentDocument;

            var questionnaire = QuestionnaireIdentity.Parse(assignment.QuestionnaireId);
            this.QuestionnaireName = string.Format(InterviewerUIResources.DashboardItem_Title,
                this.assignment.Title, questionnaire.Version);
            
            if (assignmentDocument.LocationQuestionId.HasValue && assignmentDocument.LocationLatitude.HasValue && assignmentDocument.LocationLongitude.HasValue)
                this.GpsLocation = new InterviewGpsCoordinatesView
                {
                    Latitude = assignmentDocument.LocationLatitude.Value,
                    Longitude = assignmentDocument.LocationLongitude.Value
                };

            this.DetailedIdentifyingData = assignment.IdentifyingAnswers.Select(ToIdentifyingQuestion).ToList();
            this.IdentifyingData = this.DetailedIdentifyingData.Take(3).ToList();

            this.UpdateTitle();

            this.HasExpandedView = this.DetailedIdentifyingData.Count > 0;
            this.IsExpanded = false;
        }
        
        public int AssignmentId => this.assignment.Id;
        public MvxColor ColorStatus => MvxColors.Black;
        public string QuestionnaireName { get; private set; }
        public string ReceivedDate { get; private set; }
        public string ReceivedTime { get; private set; }
        public InterviewGpsCoordinatesView GpsLocation { get; private set; }

        private int InterviewsLeftByAssignmentCount => 
            this.assignment.Quantity.GetValueOrDefault() - this.interviewsByAssignmentCount;

        public string Comment => InterviewerUIResources.DashboardItem_AssignmentCreatedComment.FormatString(this.interviewsByAssignmentCount);

        public string IdLabel => InterviewerUIResources.Dashboard_Assignment_CardTitleFormat.FormatString(this.AssignmentId);

        public string Title => this.QuestionnaireName;

        public string SubTitle
        {
            get
            {
                if (this.assignment.Quantity.HasValue)
                {
                    if (InterviewsLeftByAssignmentCount == 1)
                    {
                        return InterviewerUIResources.Dashboard_AssignmentCard_SubTitleSingleInterivew;
                    }

                    return InterviewerUIResources.Dashboard_AssignmentCard_SubTitleCountdownFormat
                        .FormatString(this.assignment.Quantity.GetValueOrDefault(), this.assignment.Quantity);
                }
                else
                {
                    return InterviewerUIResources.Dashboard_AssignmentCard_SubTitleCountdown_UnlimitedFormat.FormatString(this.assignment.Quantity.GetValueOrDefault());
                }
            }
        }
          
        public bool AllowToCreateNewInterview => !this.assignment.Quantity.HasValue || Math.Max(0, InterviewsLeftByAssignmentCount) > 0;

        public bool HasGpsLocation => this.GpsLocation != null;

        public IMvxAsyncCommand CreateNewInterviewCommand => new MvxAsyncCommand(
            () => this.interviewFromAssignmentCreator.CreateInterviewAsync(assignment.Id),
            () => this.AllowToCreateNewInterview);

        public IMvxCommand NavigateToGpsLocationCommand => new MvxCommand(
            () => this.externalAppLauncher.LaunchMapsWithTargetLocation(this.GpsLocation.Latitude, this.GpsLocation.Longitude),
            () => this.HasGpsLocation);
        
        private PrefilledQuestion ToIdentifyingQuestion(AssignmentDocument.AssignmentAnswer identifyingAnswer)
            => new PrefilledQuestion
            {
                Answer = identifyingAnswer.AnswerAsString,
                Question = identifyingAnswer.Question
            };

        private void UpdateTitle()
        {
            this.RaisePropertyChanged(() => this.AllowToCreateNewInterview);
            this.RaisePropertyChanged(() => this.SubTitle);
            this.RaisePropertyChanged(() => this.Comment);
        }

        public void DecreaseInterviewsCount()
        {
            this.interviewsByAssignmentCount--;

            this.UpdateTitle();
        }
    }
}