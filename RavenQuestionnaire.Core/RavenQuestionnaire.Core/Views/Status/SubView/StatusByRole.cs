using System.Collections.Generic;
using RavenQuestionnaire.Core.Views.Status.Browse;
using RavenQuestionnaire.Core.Views.Status.StatusElement;

namespace RavenQuestionnaire.Core.Views.Status.SubView
{
    public class StatusByRole
    {
        public StatusByRole()
        {
        }

        public StatusItemView Status
        {
            set { _status = value; }
            get { return _status ?? (_status = new StatusItemView()); }
        }

        public StatusItemView _status;

        public List<RolePermission> StatusRestriction 
        { 
            set { _statusRestriction = value; } 
            get { return _statusRestriction ?? (_statusRestriction = new List<RolePermission>()); }
        }

        private List<RolePermission> _statusRestriction;
        
    }
}
