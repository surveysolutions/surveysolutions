// -----------------------------------------------------------------------
// <copyright file="RegisterData.cs" company="WorldBank">
// 2012
// </copyright>
// -----------------------------------------------------------------------

namespace Questionnaire.Core.Web.Register
{
    using System;

    using global::Core.Supervisor.Views.Register;

    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public class RegisterData
    {
        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="RegisterData"/> class.
        /// </summary>
        public RegisterData()
        {
            this.RegisterDate = DateTime.Now;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets Description.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets RegisterDate.
        /// </summary>
        public DateTime RegisterDate { get; set; }

        /// <summary>
        /// Gets or sets SecretKey.
        /// </summary>
        public byte[] SecretKey { get; set; }

        /// <summary>
        /// Gets or sets TabletId.
        /// </summary>
        public Guid TabletId { get; set; }

        /// <summary>
        /// Gets or sets Event.
        /// </summary>
        public RegisterView Event { get; set; }

        #endregion
    }
}
