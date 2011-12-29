using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
            CompleteQuestionnaireId = IdUtil.CreateCompleteQuestionnaireId(id);
        }
        public CompleteQuestionnaireViewInputModel(string id, Guid? previousGroup, bool isReverse)
        {
            CompleteQuestionnaireId = IdUtil.CreateCompleteQuestionnaireId(id);
            PreviousGroupPublicKey = previousGroup;
            IsReverse = isReverse;
        }
        public string CompleteQuestionnaireId { get; private set; }
        public Guid? PreviousGroupPublicKey { get; private set; }
        public Guid? CurrentGroupPublicKey { get; set; }
        public bool IsReverse { get; private set; }
        public string TemplateQuestionanireId
        {
            get { return _templateQuestionanireId; }
            set { _templateQuestionanireId = IdUtil.CreateQuestionnaireId(value); }
        }
        private string _templateQuestionanireId;
    }
}
