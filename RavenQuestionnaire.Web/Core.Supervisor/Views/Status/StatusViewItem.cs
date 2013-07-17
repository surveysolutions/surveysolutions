namespace Core.Supervisor.Views.Status
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Main.Core.Entities;
    using Main.Core.Entities.SubEntities;

    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public class StatusViewItem
    {
        public class StatusViewItemData
        {
            public Guid TemplateId { get; set; }
            public int Count { get; set; }
        }
        public StatusViewItem(
            UserLight userLight, Dictionary<Guid, int> templateGroup)
        {
            this.templateGroup = templateGroup;
            this.CountersByTemplate = templateGroup.Select(k => new StatusViewItemData{ TemplateId = k.Key, Count = k.Value}).ToList();
            this.User = userLight;

            this.Total = templateGroup.Values.Sum();
        }

        private readonly Dictionary<Guid, int> templateGroup;

        public int GetCount(Guid templateId)
        {
            return templateGroup.ContainsKey(templateId) ? templateGroup[templateId] : 0;
        }

        public List<StatusViewItemData> CountersByTemplate;

        public int Total { get; set; }

        public UserLight User { get; set; }

    }
}