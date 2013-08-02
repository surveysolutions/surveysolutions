using WB.Core.Infrastructure.ReadSide;

namespace Core.Supervisor.Views.Interview
{
    using System;
    using System.Collections.Generic;

    using Main.Core.Entities.SubEntities;

    public class InterviewItem : IView
    {
        public IEnumerable<InterviewFeaturedQuestion> FeaturedQuestions { get; set; }

        public Guid InterviewId { get; set; }

        public UserLight Responsible { get; set; }

        public Guid? ResponsibleSupervisorId { get; set; }

        public SurveyStatusLight Status { get; set; }

        public Guid TemplateId { get; set; }

        public string Title { get; set; }

        public DateTime LastEntryDate { get; set; }

        public bool IsDeleted { get; set; }
    }
}