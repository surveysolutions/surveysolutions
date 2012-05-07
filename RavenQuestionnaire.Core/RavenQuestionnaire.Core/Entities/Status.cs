using RavenQuestionnaire.Core.Documents;

namespace RavenQuestionnaire.Core.Entities
{
    public class Status : IEntity<StatusDocument>
    {
        private StatusDocument innerDocument;

        public string StatusId
        {
            get { return innerDocument.Id; }
        }

        public StatusDocument GetInnerDocument()
        {
            return innerDocument;
        }

        public Status(StatusDocument document)
        {
            innerDocument = document;
        }


        public Status(string questionnaireId)
        {
            innerDocument = new StatusDocument() {
                QuestionnaireId = questionnaireId 
            };
        }

/*


        public Status(string title, bool isInitial, Dictionary<string, List<SurveyStatus>> statusRoles, 
            string questionnaireId) : this(title, isInitial, questionnaireId)
        {
            innerDocument.StatusRoles = statusRoles;
        }


        public void UpdateRestrictions(Dictionary<string, List<SurveyStatus>> restrictions)
        {
            innerDocument.StatusRoles = restrictions;
        }


        public void UpdateFlowRules(Dictionary<Guid, FlowRule> flowRules)
        {
            innerDocument.FlowRules = flowRules;
        }


        public void UpdateFlowRule(Guid ruleKey, FlowRule flowRule)
        {
            if (innerDocument.FlowRules.ContainsKey(ruleKey))
            {
                innerDocument.FlowRules[ruleKey] = flowRule;
            }
        }

        public void AddFlowRule(FlowRule flowRule)
        {
            innerDocument.FlowRules.Add(Guid.NewGuid(), flowRule);
        }

*/

    }
}
