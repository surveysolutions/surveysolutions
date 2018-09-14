using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace WB.Services.Export.Questionnaire
{
    public class QuestionnaireDocument : Group
    {
        private bool childrenWereConnected;

        public QuestionnaireDocument()
        {
        }

        public string Id { get; set;}

        public void ConnectChildrenWithParent()
        {
            foreach (var child in Children)
            {
                child.SetParent(this);
            }
        }
    }
}
