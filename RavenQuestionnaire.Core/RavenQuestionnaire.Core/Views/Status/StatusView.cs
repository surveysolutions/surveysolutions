using System.Collections.Generic;
using RavenQuestionnaire.Core.Utility;
using RavenQuestionnaire.Core.Views.Status.SubView;

namespace RavenQuestionnaire.Core.Views.Status
{
    public class StatusView
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public bool IsVisible { get; set; }
        public Dictionary<string, List<string>> StatusRoles { private set; get; }

        private List<StatusByRole> _statusRolesMatrix;
        public List<StatusByRole> StatusRolesMatrix
        {
            set { _statusRolesMatrix = value; }
            get { return _statusRolesMatrix ?? (_statusRolesMatrix = new List<StatusByRole>()); }
        }

        public StatusView()
        {
            StatusRolesMatrix = new List<StatusByRole>();
        }

        public StatusView(string id, string title, bool isVisible, Dictionary<string, List<string>> statusRoles):this()
        {
            this.Id = IdUtil.ParseId(id); 
            this.Title = title;
            this.IsVisible = isVisible;
            StatusRoles = statusRoles;
        }
        public static StatusView New()
        {
            return new StatusView();
        }
    }
}
