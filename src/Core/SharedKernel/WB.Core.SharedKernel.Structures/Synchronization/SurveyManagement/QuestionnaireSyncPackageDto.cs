using System;

namespace WB.Core.SharedKernel.Structures.Synchronization.SurveyManagement
{
    public class QuestionnaireSyncPackageDto : BaseSyncPackageDto
    {
        public Guid QuestionnaireId { get; set; }

        public long QuestionnaireVersion { get; set; }

        public string ItemType { get; set; }

        public string MetaInfo { get; set; }
    }
}