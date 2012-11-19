namespace Main.Core.Entities
{
    using System;

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

            return this.Equals((TemplateLight)obj);
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