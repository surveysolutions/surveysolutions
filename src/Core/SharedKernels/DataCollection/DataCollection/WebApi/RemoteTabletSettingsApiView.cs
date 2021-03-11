using System.Collections.Generic;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;

namespace WB.Core.SharedKernels.DataCollection.WebApi
{
    public class RemoteTabletSettingsApiView
    {
        public bool PartialSynchronizationEnabled { get; set; }
        public List<QuestionnaireIdentity> QuestionnairesInWebMode { get; set; }
        public string WebInterviewUrlTemplate { get; set; }
    }
}
