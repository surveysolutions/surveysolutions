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

    using Main.Core.Entities.SubEntities;

    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public class SummaryViewItem
    {
        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="SummaryViewItem"/> class.
        /// </summary>
        public SummaryViewItem()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SummaryViewItem"/> class.
        /// </summary>
        /// <param name="user">
        /// The user
        /// </param>
        /// <param name="total">
        /// The total.
        /// </param>
        /// <param name="initial">
        /// The initial.
        /// </param>
        /// <param name="error">
        /// The error.
        /// </param>
        /// <param name="completed">
        /// The completed.
        /// </param>
        /// <param name="approve">
        /// The approve.
        /// </param>
        /// <param name="redo">
        /// The redo.
        /// </param>
        public SummaryViewItem(
            UserLight user,
            int total,
            int initial,
            int error,
            int completed,
            int approve,
            int redo)
            : this()
        {
            this.User = user;
            this.Total = total;
            this.Initial = initial;
            this.Error = error;
            this.Complete = completed;
            this.Approve = approve;
            this.Redo = redo;
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets or sets Approve.
        /// </summary>
        public int Approve { get; set; }

        /// <summary>
        /// Gets or sets the complete.
        /// </summary>
        public int Complete { get; set; }

        /// <summary>
        /// Gets or sets the error.
        /// </summary>
        public int Error { get; set; }

        /// <summary>
        /// Gets or sets the user.
        /// </summary>
        public UserLight User { get; set; }

        /// <summary>
        /// Gets or sets the initial.
        /// </summary>
        public int Initial { get; set; }

        /// <summary>
        /// Gets or sets Redo.
        /// </summary>
        public int Redo { get; set; }

        /// <summary>
        /// Gets or sets the title.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets the total.
        /// </summary>
        public int Total { get; set; }

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
    }
}