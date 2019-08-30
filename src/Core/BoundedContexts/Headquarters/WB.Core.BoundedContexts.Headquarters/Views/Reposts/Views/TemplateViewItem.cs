using System;
using System.Collections.Generic;

namespace WB.Core.BoundedContexts.Headquarters.Views.Reposts.Views
{
    public class TemplateViewItem
    {
        public Guid TemplateId { get; set; }

        public string TemplateName { get; set; }

        public long TemplateVersion { get; set; }
    }

    public class ComboboxViewItem
    {
        public string Key { get; set; }
        public string Value { get; set; }
        public string Alias { get; set; }
    }

    public class QuestionnaireVersionsComboboxViewItem : ComboboxViewItem
    {
        public List<ComboboxViewItem> Versions { get; set; }
    }
}
