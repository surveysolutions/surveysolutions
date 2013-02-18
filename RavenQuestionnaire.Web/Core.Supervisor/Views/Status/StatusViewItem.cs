// --------------------------------------------------------------------------------------------------------------------
// <copyright file="StatusViewItem.cs" company="">
//   
// </copyright>
// <summary>
//   TODO: Update summary.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Core.Supervisor.Views.Status
{
    using System;
    using System.Collections.Generic;

    using Main.Core.Entities;
    using Main.Core.Entities.SubEntities;

    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public class StatusViewItem
    {
        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="StatusViewItem"/> class.
        /// </summary>
        /// <param name="userLight">
        /// The user light.
        /// </param>
        /// <param name="templateGroup">
        /// The template group.
        /// </param>
        /// <param name="headers">
        /// The headers.
        /// </param>
        public StatusViewItem(
            UserLight userLight, Dictionary<Guid, int> templateGroup, IEnumerable<TemplateLight> headers)
        {
            this.User = userLight;
            this.Items = new Dictionary<TemplateLight, int>();
            foreach (TemplateLight header in headers)
            {
                this.Items.Add(
                    header, templateGroup.ContainsKey(header.TemplateId) ? templateGroup[header.TemplateId] : 0);
            }
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets or sets Items.
        /// </summary>
        public Dictionary<TemplateLight, int> Items { get; set; }

        /// <summary>
        /// Gets or sets user.
        /// </summary>
        public UserLight User { get; set; }

        #endregion
    }
}