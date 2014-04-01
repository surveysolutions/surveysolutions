using System.Collections.Generic;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;

namespace Core.Supervisor.Views.Interview
{
    using System;
    using Main.Core.Entities.SubEntities;

    public class InterviewDetailsView
    {
        public InterviewStatus Status { get; set; }

        public UserLight Responsible { get; set; }

        public Guid QuestionnairePublicKey { get; set; }

        public Guid PublicKey { get; set; }

        public string Title { get; set; }

        public string Description { get; set; }

        public List<InterviewGroupView> Groups;

        public InterviewDetailsView()
        {
            Groups = new List<InterviewGroupView>();
        }
    }
}
