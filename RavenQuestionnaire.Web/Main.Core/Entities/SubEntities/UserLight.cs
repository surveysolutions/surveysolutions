// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UserLight.cs" company="">
//   
// </copyright>
// <summary>
//   The user light.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
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

            if (obj.GetType() != typeof(UserLight))
            {
                return false;
            }

            return Equals((UserLight)obj);
        }
        #endregion

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
            return other.Id.Equals(this.Id) && Equals(other.Name, this.Name);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (this.Id.GetHashCode() * 39) ^ (this.Name != null ? this.Name.GetHashCode() : 0);
            }
        }
    }
}