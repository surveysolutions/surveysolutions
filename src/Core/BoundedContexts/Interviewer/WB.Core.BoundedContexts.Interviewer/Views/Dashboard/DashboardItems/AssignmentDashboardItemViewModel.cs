﻿using System;
using System.Linq;
using MvvmCross.Commands;
using WB.Core.BoundedContexts.Interviewer.Services;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.Enumerator.Properties;

namespace WB.Core.BoundedContexts.Interviewer.Views.Dashboard.DashboardItems
{
    public class AssignmentDashboardItemViewModel : ExpandableQuestionsDashboardItemViewModel, IDashboardViewItem
    {
        private readonly IServiceLocator serviceLocator;

        private IInterviewFromAssignmentCreatorService InterviewFromAssignmentCreator
            => serviceLocator.GetInstance<IInterviewFromAssignmentCreatorService>();

        private IAssignmentDocumentsStorage AssignmentsRepository
            => serviceLocator.GetInstance<IAssignmentDocumentsStorage>();

        private AssignmentDocument assignment;
        private int interviewsByAssignmentCount;
        private QuestionnaireIdentity questionnaireIdentity;
        
        private int InterviewsLeftByAssignmentCount =>
            assignment.Quantity.GetValueOrDefault() - interviewsByAssignmentCount;

        public bool HasAdditionalActions => Actions.Any(a => a.ActionType == ActionType.Context);
        
        public int AssignmentId => this.assignment.Id;

        public AssignmentDashboardItemViewModel(IServiceLocator serviceLocator) : base(serviceLocator)
        {
            this.serviceLocator = serviceLocator;
        }

        public void Init(AssignmentDocument assignmentDocument)
        {
            interviewsByAssignmentCount = assignmentDocument.CreatedInterviewsCount ?? 0;
            assignment = assignmentDocument;
            questionnaireIdentity = QuestionnaireIdentity.Parse(assignment.QuestionnaireId);
            Status = DashboardInterviewStatus.Assignment;

            BindTitles();
            BindDetails();
            BindActions();
        }

        public string AssignmentIdLabel { get; } = String.Empty;

        private void BindTitles()
        {
            Title = string.Format(InterviewerUIResources.DashboardItem_Title, assignment.Title, questionnaireIdentity.Version);
            IdLabel = "#" + assignment.Id;

            if (assignment.Quantity.HasValue)
            {
                if (InterviewsLeftByAssignmentCount == 1)
                {
                    SubTitle = InterviewerUIResources.Dashboard_AssignmentCard_SubTitleSingleInterivew;
                }
                else
                {
                    SubTitle = InterviewerUIResources.Dashboard_AssignmentCard_SubTitleCountdownFormat
                        .FormatString(InterviewsLeftByAssignmentCount, assignment.Quantity);
                }
            }
            else
            {
                SubTitle = InterviewerUIResources.Dashboard_AssignmentCard_SubTitleCountdown_UnlimitedFormat
                    .FormatString(assignment.Quantity.GetValueOrDefault());
            }
        }

        private void BindActions()
        {
            Actions.Clear();
            
            BindLocationAction(assignment.LocationQuestionId, assignment?.LocationLatitude, assignment?.LocationLongitude);
            
            Actions.Add(new ActionDefinition
            {
                Command = new MvxAsyncCommand(
                    () => InterviewFromAssignmentCreator.CreateInterviewAsync(assignment.Id),
                    () => !assignment.Quantity.HasValue ||
                          Math.Max(val1: 0, val2: InterviewsLeftByAssignmentCount) > 0),

                Label = InterviewerUIResources.Dashboard_StartNewInterview
            });
        }

        private void BindDetails()
        {
            DetailedIdentifyingData = assignment.IdentifyingAnswers.Select(ToIdentifyingQuestion).ToList();
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
            assignment.CreatedInterviewsCount = interviewsByAssignmentCount;
            AssignmentsRepository.Store(assignment);

            
            BindTitles();
        }
    }
}
