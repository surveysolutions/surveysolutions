﻿using System.Collections.Generic;
using WB.Core.SharedKernels.SurveyManagement.Views.Reposts.Views;
using WB.Core.SharedKernels.SurveyManagement.Views.Survey;

namespace WB.Core.SharedKernels.SurveyManagement.Web.Models
{
    public class DocumentFilter
    {
        public IEnumerable<UsersViewItem> Users { get; set; } 
        public IEnumerable<UsersViewItem> Responsibles { get; set; }
        public IEnumerable<TemplateViewItem> Templates { get; set; }
        public IEnumerable<SurveyStatusViewItem> Statuses { get; set; }
    }
}