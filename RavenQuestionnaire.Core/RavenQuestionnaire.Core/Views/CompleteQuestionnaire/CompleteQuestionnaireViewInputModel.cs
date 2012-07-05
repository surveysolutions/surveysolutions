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
        public CompleteQuestionnaireViewInputModel(string id, Guid? previousGroup, bool isReverse)
        {
            CompleteQuestionnaireId = id;
            PreviousGroupPublicKey = previousGroup;
            IsReverse = isReverse;
        }
        public string CompleteQuestionnaireId { get; private set; }
        public Guid? PreviousGroupPublicKey { get; private set; }
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
        private Guid? currentGroupPublicKey;

        public bool IsReverse { get; private set; }
        public string TemplateQuestionanireId
        {
            get { return _templateQuestionanireId; }
            set { _templateQuestionanireId = IdUtil.CreateQuestionnaireId(value); }
        }
        private string _templateQuestionanireId;
    }
}
