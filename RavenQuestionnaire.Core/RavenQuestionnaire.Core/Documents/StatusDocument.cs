using System.Collections.Generic;
using RavenQuestionnaire.Core.Entities.SubEntities;

namespace RavenQuestionnaire.Core.Documents
{
    public class StatusDocument
    {
        public string Id { get; set; }
        public string Title { get; set; }

        public bool IsVisible {get; set;}

        public bool IsInitial { get; set; }

        public string QuestionnaireId { get; set; }

        public Dictionary<string, List<SurveyStatus>> StatusRoles { 
            set;
            get;
        }

        public StatusDocument()
        {
            IsVisible = true;
            StatusRoles = new Dictionary<string, List<SurveyStatus>>();
        
        }
    }
}
