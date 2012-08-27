using System;

namespace RavenQuestionnaire.Core.Views.CompleteQuestionnaire
{
    public class CompleteQuestionnaireViewInputModel
    {
        public CompleteQuestionnaireViewInputModel(){}
        public CompleteQuestionnaireViewInputModel(Guid id)
        {
            CompleteQuestionnaireId =id;
        }
        public CompleteQuestionnaireViewInputModel(Guid id, Guid groupKey, Guid? propagationKey)
        {
            CompleteQuestionnaireId = id;
            CurrentGroupPublicKey = groupKey;
            PropagationKey = propagationKey;
        }
        public Guid CompleteQuestionnaireId { get; private set; }
        public Guid? CurrentGroupPublicKey
        {
            get { return this._currentGroupPublicKey; }
            set { _currentGroupPublicKey = value == Guid.Empty ? null : value;
            }
        }

        public Guid? PropagationKey { get; set; }

        private Guid? _currentGroupPublicKey;

        public bool IsReverse { get; private set; }

      //  public Guid? CurrentScreenPublicKey { get; set; }
    }
}
