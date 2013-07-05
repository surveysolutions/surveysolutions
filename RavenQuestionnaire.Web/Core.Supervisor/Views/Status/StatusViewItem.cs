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
        #region Constructors and Destructors

        public StatusViewItem(
            UserLight userLight, Dictionary<Guid, int> templateGroup)
        {
            this.templateGroup = templateGroup;
            this.User = userLight;
         /*   this.Items = new Dictionary<Guid, int>();

            foreach (TemplateLight header in headers)
            {
                this.Items.Add(
                    header.TemplateId, templateGroup.ContainsKey(header.TemplateId) ? templateGroup[header.TemplateId] : 0);
            }*/

            this.Total = this.templateGroup.Values.Sum();
        }

        private Dictionary<Guid, int> templateGroup;
        #endregion

        public int GetCount(Guid templateId)
        {
            return templateGroup.ContainsKey(templateId) ? templateGroup[templateId] : 0;
        }

        #region Public Properties

      /*  /// <summary>
        /// Gets or sets Items.
        /// </summary>
        public Dictionary<Guid, int> Items { get; set; }*/

        /// <summary>
        /// Gets or sets Total.
        /// </summary>
        public int Total { get; set; }

        /// <summary>
        /// Gets or sets user.
        /// </summary>
        public UserLight User { get; set; }

        #endregion
    }
}