// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SummaryViewItem.cs" company="">
//   
// </copyright>
// <summary>
//   TODO: Update summary.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Core.Supervisor.Views.Summary
{
    using System;
    using System.Collections.Generic;

    using Main.Core.Entities.SubEntities;

    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public class SummaryViewItem
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SummaryViewItem"/> class.
        /// </summary>
        /// <param name="userLight"></param>
        /// <param name="dict"></param>
        public SummaryViewItem(UserLight userLight, Dictionary<TemplateLight, WorkingStatuses> dict)
        {
            this.User = userLight;
            this.Items = dict;
        }

        #region Public Properties

        /// <summary>
        /// Gets or sets Items.
        /// </summary>
        public Dictionary<TemplateLight, WorkingStatuses> Items { get; set; }

        /// <summary>
        /// Gets or sets User.
        /// </summary>
        public UserLight User { get; set; }

        #endregion

        /// <summary>
        /// The template light.
        /// </summary>
        public class TemplateLight
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="TemplateLight"/> class.
            /// </summary>
            /// <param name="templateId">
            /// The template id.
            /// </param>
            /// <param name="title">
            /// The title.
            /// </param>
            public TemplateLight(Guid templateId, string title)
            {
                this.TemplateId = templateId;
                this.Title = title;
            }

            #region Public Properties

            /// <summary>
            /// Gets or sets TemplateId.
            /// </summary>
            public Guid TemplateId { get; set; }

            /// <summary>
            /// Gets or sets Title.
            /// </summary>
            public string Title { get; set; }

            /// <summary>
            /// </summary>
            /// <param name="obj">
            /// The obj.
            /// </param>
            /// <returns>
            /// </returns>
            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj))
                {
                    return false;
                }

                if (ReferenceEquals(this, obj))
                {
                    return true;
                }

                if (obj.GetType() != typeof(TemplateLight))
                {
                    return false;
                }

                return Equals((TemplateLight)obj);
            }
            #endregion

            /// <summary>
            /// </summary>
            /// <param name="other">
            /// The other.
            /// </param>
            /// <returns>
            /// </returns>
            public bool Equals(TemplateLight other)
            {
                if (ReferenceEquals(null, other))
                {
                    return false;
                }

                if (ReferenceEquals(this, other))
                {
                    return true;
                }

                return other.TemplateId.Equals(this.TemplateId) && Equals(other.Title, this.Title);
            }

            /// <summary>
            /// Get Hash Code
            /// </summary>
            /// <returns>
            /// Hash code
            /// </returns>
            public override int GetHashCode()
            {
                unchecked
                {
                    return (this.TemplateId.GetHashCode() * 397) ^ (this.Title != null ? this.Title.GetHashCode() : 0);
                }
            }
        }

        /// <summary>
        /// The working statuses.
        /// </summary>
        public class WorkingStatuses
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="WorkingStatuses"/> class.
            /// </summary>
            /// <param name="total">
            /// The total.
            /// </param>
            /// <param name="initial">
            /// The initial.
            /// </param>
            /// <param name="redo">
            /// The redo.
            /// </param>
            public WorkingStatuses(int total, int initial, int redo)
            {
                this.Initial = initial;
                this.Total = total;
                this.Redo = redo;
            }

            #region Public Properties

            /// <summary>
            /// Gets or sets Initial.
            /// </summary>
            public int Initial { get; set; }

            /// <summary>
            /// Gets or sets Redo.
            /// </summary>
            public int Redo { get; set; }

            /// <summary>
            /// Gets or sets Total.
            /// </summary>
            public int Total { get; set; }

            #endregion
        }
    }
}