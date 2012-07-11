using System;
using RavenQuestionnaire.Core.Utility;

namespace RavenQuestionnaire.Core.Views.CompleteQuestionnaire
{
    public class CompleteQuestionnaireViewInputModel
    {
        public CompleteQuestionnaireViewInputModel()
        {
        }
        public CompleteQuestionnaireViewInputModel(string id)
        {
            CompleteQuestionnaireId =id;
        }
        public CompleteQuestionnaireViewInputModel(string id, Guid groupKey, Guid? propagationKey)
        {
            CompleteQuestionnaireId = id;
            CurrentGroupPublicKey = groupKey;
            PropagationKey = propagationKey;
        }
        public string CompleteQuestionnaireId { get; private set; }
        public Guid? CurrentGroupPublicKey
        {
            get { return this.currentGroupPublicKey; }
            set
            {
                if (value == Guid.Empty)
                    this.currentGroupPublicKey = null;
                else
                    this.currentGroupPublicKey = value;
            }
        }

        public Guid? PropagationKey { get; set; }

        private Guid? currentGroupPublicKey;

        public bool IsReverse { get; private set; }

        public Guid? CurrentScreenPublicKey { get; set; }
    }
}
