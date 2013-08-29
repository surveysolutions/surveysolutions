namespace Main.Core.Entities.SubEntities
{
    using System;

    /// <summary>
    /// The user light.
    /// </summary>
    public class UserLight
    {
        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="UserLight"/> class.
        /// </summary>
        public UserLight()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UserLight"/> class.
        /// </summary>
        /// <param name="id">
        /// The id.
        /// </param>
        /// <param name="name">
        /// The name.
        /// </param>
        public UserLight(Guid id, string name)
        {
            this.Id = id;
            this.Name = name;
        }


        #endregion

        #region Public Properties

        /// <summary>
        /// Gets or sets the id.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Equals  method
        /// </summary>
        /// <param name="obj">
        /// The obj.
        /// </param>
        /// <returns>
        /// True if Equals
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

            return obj.GetType() == typeof(UserLight) && this.Equals((UserLight)obj);
        }

        /// <summary>
        /// Equals method
        /// </summary>
        /// <param name="other">
        /// The other.
        /// </param>
        /// <returns>
        /// True if Equals
        /// </returns>
        public bool Equals(UserLight other)
        {
            if (ReferenceEquals(null, other))
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return other.Id.Equals(this.Id) && other.Name == this.Name;
        }

        /// <summary>
        /// GetHashCode  method
        /// </summary>
        /// <returns>
        /// Get Hash Code
        /// </returns>
        public override int GetHashCode()
        {
            unchecked
            {
                return (this.Id.GetHashCode() * 39) ^ (this.Name != null ? this.Name.GetHashCode() : 0);
            }
        }

        /// <summary>
        /// To string
        /// </summary>
        /// <returns>
        /// The string
        /// </returns>
        public override string ToString()
        {
            return string.Format("{0}: [{1}]", this.Name, this.Id);
        }

        #endregion
    }
}