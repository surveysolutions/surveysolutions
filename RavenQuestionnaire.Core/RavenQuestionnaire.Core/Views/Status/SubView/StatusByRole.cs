using System.Collections.Generic;

namespace RavenQuestionnaire.Core.Views.Status.SubView
{
    public class StatusByRole
    {
        public StatusByRole()
        {
        }

        public StatusBrowseItem Status
        {
            set { _status = value; }
            get { return _status ?? (_status = new StatusBrowseItem()); }
        }

        public StatusBrowseItem _status;

        public List<RolePermission> StatusRestriction 
        { 
            set { _statusRestriction = value; } 
            get { return _statusRestriction ?? (_statusRestriction = new List<RolePermission>()); }
        }

        private List<RolePermission> _statusRestriction;
        
    }
}
