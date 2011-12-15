using System.Collections.Generic;

namespace RavenQuestionnaire.Core.Documents
{
    public class StatusDocument
    {
        public string Id { get; set; }
        public string Title { get; set; }

        public bool IsVisible {get; set;}

        public Dictionary<string, List<string>> StatusRoles { 
            set;// { _statusRoles = value; }
            get;// { return _statusRoles ?? (_statusRoles = new Dictionary<string, List<string>>()); }
        }

        //private Dictionary<string, List<string>> _statusRoles;
        
        public StatusDocument()
        {
            IsVisible = true;
            StatusRoles = new Dictionary<string, List<string>>();
        
        }
    }
}
