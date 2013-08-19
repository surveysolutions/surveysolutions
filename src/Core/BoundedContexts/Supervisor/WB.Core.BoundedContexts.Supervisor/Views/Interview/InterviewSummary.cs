using System;
using System.Collections.Generic;
using Main.Core.Entities.SubEntities;
using WB.Core.Infrastructure.ReadSide;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire;

namespace WB.Core.BoundedContexts.Supervisor.Views.Interview
{
    public class InterviewSummary : InterviewBrief  {

        public InterviewSummary() {}

        public InterviewSummary(QuestionnaireBrowseItem questionnaireSummary)
        {
            CommentedStatusesHistory = new List<InterviewCommentedStatus>();
            AnswersToFeaturedQuestions = new Dictionary<Guid, QuestionAnswer>();
            foreach (var featuredQuestion in questionnaireSummary.FeaturedQuestions)
            {
                AnswersToFeaturedQuestions[featuredQuestion.Id] = new QuestionAnswer
                    {
                        Id = featuredQuestion.Id,
                        Title = featuredQuestion.Title,
                        Answer = string.Empty
                    };
            }
        }

        public string QuestionnaireTitle { get; set; }

        public string ResponsibleName { get; set; }

        public Guid TeamLeadId { get; set; }
        public string TeamLeadName { get; set; }

        public UserRoles ResponsibleRole { get; set; }
        public DateTime UpdateDate { get; set; }
        public Dictionary<Guid, QuestionAnswer> AnswersToFeaturedQuestions { get; set; }

        public List<InterviewCommentedStatus> CommentedStatusesHistory { get; set; }
    }

    public class InterviewCommentedStatus
    {
        public string Comment { get; set; }
        public DateTime Date { get; set; }
        public InterviewStatus Status { get; set; }
    }

    public class QuestionAnswer
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public string Answer { get; set; }
    }
}