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

        public Dictionary<string, List<SurveyStatus>> StatusRoles { 
            set;// { _statusRoles = value; }
            get;// { return _statusRoles ?? (_statusRoles = new Dictionary<string, List<string>>()); }
        }

        //private Dictionary<string, List<string>> _statusRoles;
        
        public StatusDocument()
        {
            IsVisible = true;
            StatusRoles = new Dictionary<string, List<SurveyStatus>>();
        
        }
    }
}
