using System;
using System.Collections.Generic;
using Core.Supervisor.Views.Interview;
using Main.Core.Entities.SubEntities;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;

namespace Core.Supervisor.Views.Revalidate
{
    public class RevalidatInterviewView
    {
        public RevalidatInterviewView()
        {

            this.FeaturedQuestions = new List<InterviewQuestionView>();
            this.MandatoryQuestions = new List<InterviewQuestionView>();
        }

        public InterviewStatus Status { get; set; }

        public UserLight Responsible { get; set; }

        public Guid QuestionnairePublicKey { get; set; }

        public Guid PublicKey { get; set; }

        public string Title { get; set; }

        public string Description { get; set; }

        public Guid InterviewId { get; set; }

        public List<InterviewQuestionView> FeaturedQuestions { get; set; }

        public List<InterviewQuestionView> MandatoryQuestions { get; set; }
    }
}
