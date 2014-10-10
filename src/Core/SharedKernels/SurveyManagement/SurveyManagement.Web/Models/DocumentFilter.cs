using System.Collections.Generic;
using System.Linq;
using Microsoft.Ajax.Utilities;
using WB.Core.SharedKernels.SurveyManagement.Views.Reposts.Views;
using WB.Core.SharedKernels.SurveyManagement.Views.Survey;

namespace WB.Core.SharedKernels.SurveyManagement.Web.Models
{
    public class DocumentFilter
    {
        private IEnumerable<UsersViewItem> responsibles;
        private IEnumerable<SurveyStatusViewItem> statuses;
        private IEnumerable<UsersViewItem> users;
        private IEnumerable<TemplateViewItem> templates;

        public IEnumerable<UsersViewItem> Responsibles
        {
            get { return this.responsibles.OrderBy(x => x.UserName); }
            set { this.responsibles = value; }
        }

        public IEnumerable<TemplateViewItem> Templates
        {
            get
            {
                return this.templates.OrderBy(x => x.TemplateName).ThenBy(x => x.TemplateVersion);
            }
            set { this.templates = value; }
        }

        public IEnumerable<SurveyStatusViewItem> Statuses
        {
            get { return this.statuses.OrderBy(x => x.StatusName); }
            set { this.statuses = value; }
        }

        public IEnumerable<UsersViewItem> Users
        {
            get { return this.users.OrderBy(x => x.UserName); }
            set { this.users = value; }
        }
    }
}