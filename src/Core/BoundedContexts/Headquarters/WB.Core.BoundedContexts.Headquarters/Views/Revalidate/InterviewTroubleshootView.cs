using System;
using System.Collections.Generic;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;

namespace WB.Core.BoundedContexts.Headquarters.Views.Revalidate
{
    public class InterviewTroubleshootView
    {
        public InterviewTroubleshootView()
        {
            this.FeaturedQuestions = new List<InterviewQuestionView>();
        }

        public InterviewStatus Status { get; set; }

        public UserLight Responsible { get; set; }

        public Guid QuestionnairePublicKey { get; set; }

        public long QuestionnaireVersion { get; set; }

        public Guid PublicKey { get; set; }

        public string Title { get; set; }

        public string Description { get; set; }

        public Guid InterviewId { get; set; }

        public List<InterviewQuestionView> FeaturedQuestions { get; set; }
    }
}
