using System;

using RavenQuestionnaire.Core.Utility;

namespace RavenQuestionnaire.Core.Views.Status
{
    public class StatusViewInputModel
    {
        public StatusViewInputModel(string qId, string id)
        {
            StatusId = IdUtil.CreateStatusId(id);
            QId = IdUtil.CreateQuestionnaireId(qId);
        }

        public StatusViewInputModel(string qId)
        {
            QId = IdUtil.CreateQuestionnaireId(qId);
        }

        public string StatusId { get; private set; }
        public string QId { get; private set; }
    }
}
