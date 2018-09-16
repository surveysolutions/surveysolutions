using System.Collections.Generic;

namespace WB.Services.Export.Questionnaire
{
    public class QuestionnaireDocument : Group
    {
        private bool childrenWereConnected = false;

        public QuestionnaireDocument(List<IQuestionnaireEntity> children = null) : base(children)
        {
        }

        public string Id { get; set;}

        public override void ConnectChildrenWithParent()
        {
            if (childrenWereConnected) return;

            base.ConnectChildrenWithParent();

            childrenWereConnected = true;
        }
    }
}
