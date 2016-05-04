using System;
using System.Collections.Generic;
using Main.Core.Entities.SubEntities;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;

namespace WB.Core.SharedKernels.SurveyManagement.Views.Interview
{
    public class InterviewDetailsView
    {
        public InterviewStatus Status { get; set; }

        public bool ReceivedByInterviewer { get; set; }

        public UserLight Responsible { get; set; }

        public Guid QuestionnairePublicKey { get; set; }

        public Guid PublicKey { get; set; }

        public string Title { get; set; }

        public string Description { get; set; }

        public List<InterviewGroupView> Groups;

        public InterviewDetailsView()
        {
            this.Groups = new List<InterviewGroupView>();
        }
    }
}
