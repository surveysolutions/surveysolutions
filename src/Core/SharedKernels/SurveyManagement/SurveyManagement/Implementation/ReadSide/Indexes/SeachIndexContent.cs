using System;
using System.Collections.Generic;
using Main.Core.Entities.SubEntities;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.SurveyManagement.Views.Interview;

namespace WB.Core.SharedKernels.SurveyManagement.Implementation.ReadSide.Indexes
{
    public class SeachIndexContent
    {
        public bool IsDeleted { get; set; }

        public Guid TeamLeadId { get; set; }

        public Guid ResponsibleId { get; set; }

        public string ResponsibleName { get; set; }

        public bool HasErrors { get; set; }

        public InterviewStatus Status { get; set; }

        public long QuestionnaireVersion { get; set; }

        public DateTime UpdateDate { get; set; }

        public Guid QuestionnaireId { get; set; }

        public string FeaturedQuestionsWithAnswers { get; set; }

        public Dictionary<Guid, QuestionAnswer> AnswersToFeaturedQuestions { get; set; }

        public Guid InterviewId { get; set; }

        public UserRoles ResponsibleRole { get; set; }

        public bool WasCreatedOnClient { get; set; }
    }
}