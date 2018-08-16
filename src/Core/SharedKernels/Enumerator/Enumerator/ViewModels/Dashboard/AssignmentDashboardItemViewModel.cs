using System;
using System.Linq;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.Enumerator.Properties;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Views;
using WB.Core.SharedKernels.Enumerator.Views.Dashboard;

namespace WB.Core.SharedKernels.Enumerator.ViewModels.Dashboard
{
    public abstract class AssignmentDashboardItemViewModel : ExpandableQuestionsDashboardItemViewModel
    {
        protected readonly IServiceLocator serviceLocator;
        protected AssignmentDocument Assignment;
        private int interviewsByAssignmentCount;
        private QuestionnaireIdentity questionnaireIdentity;

        public int AssignmentId => this.Assignment.Id;

        protected AssignmentDashboardItemViewModel(IServiceLocator serviceLocator) : base(serviceLocator)
        {
            this.serviceLocator = serviceLocator;
        }

        protected IAssignmentDocumentsStorage AssignmentsRepository
            => serviceLocator.GetInstance<IAssignmentDocumentsStorage>();

        public int InterviewsLeftByAssignmentCount =>
            Assignment.Quantity.GetValueOrDefault() - interviewsByAssignmentCount;

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
        }

        protected virtual void BindTitles()
        {
            Title = string.Format(InterviewerUIResources.DashboardItem_Title, Assignment.Title, questionnaireIdentity.Version);
            IdLabel = "#" + Assignment.Id;

            if (Assignment.Quantity.HasValue)
            {
                if (InterviewsLeftByAssignmentCount == 1)
                {
                    SubTitle = InterviewerUIResources.Dashboard_AssignmentCard_SubTitleSingleInterivew;
                }
                else
                {
                    SubTitle = InterviewerUIResources.Dashboard_AssignmentCard_SubTitleCountdownFormat
                        .FormatString(InterviewsLeftByAssignmentCount, Assignment.Quantity);
                }
            }
            else
            {
                SubTitle = InterviewerUIResources.Dashboard_AssignmentCard_SubTitleCountdown_UnlimitedFormat
                    .FormatString(Assignment.Quantity.GetValueOrDefault());
            }
        }

        protected virtual void BindActions()
        {
        }

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

            // update db assignment
            Assignment.CreatedInterviewsCount = interviewsByAssignmentCount;
            AssignmentsRepository.Store(Assignment);
            
            BindTitles();
        }

        private string responsible;
        public string Responsible
        {
            get => responsible;
            set => SetProperty(ref this.responsible, value);
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
