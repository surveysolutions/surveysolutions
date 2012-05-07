using System;
using RavenQuestionnaire.Core.Utility;

namespace RavenQuestionnaire.Core.Views.Status.StatusElement
{
    public class StatusItemViewInputModel
    {
        public StatusItemViewInputModel(string qId, string id, Guid publicKey)
        {
            StatusId = IdUtil.CreateStatusId(id);
            this.PublicKey = publicKey;
            QId = IdUtil.CreateQuestionnaireId(qId);
        }

        public StatusItemViewInputModel(string qId, bool getDefault)
        {
            GetDefault = getDefault;
            QId = IdUtil.CreateQuestionnaireId(qId);
        }

        public string StatusId { get; private set; }

        public string QId { get; private set; }

        public Guid PublicKey { get; private set; }

        public bool GetDefault { get; set; }
    }
}
