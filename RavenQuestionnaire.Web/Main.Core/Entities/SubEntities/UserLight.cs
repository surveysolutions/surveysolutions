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

        #endregion
    }
}