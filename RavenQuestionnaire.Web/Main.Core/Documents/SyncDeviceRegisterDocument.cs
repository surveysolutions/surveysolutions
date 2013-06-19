// -----------------------------------------------------------------------
// <copyright file="SyncDeviceRegisterDocument.cs" company="WorldBank">
// 2012
// </copyright>
// -----------------------------------------------------------------------

using WB.Core.Infrastructure;
using WB.Core.Infrastructure.ReadSide;

namespace Main.Core.Documents
{
    using System;

    using Main.Core.Entities.SubEntities;

    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public class SyncDeviceRegisterDocument : IView
    {
        #region Fields

        /// <summary>
        /// Gets or sets the public key.
        /// </summary>
        public Guid PublicKey { get; set; }

        /// <summary>
        /// Gets or sets the creation date.
        /// </summary>
        public DateTime CreationDate { get; set; }

        /// <summary>
        /// Gets or sets the modification date.
        /// </summary>
        public DateTime ModificationDate { get; set; }

        /// <summary>
        /// Gets or sets TabletId.
        /// </summary>
        public Guid TabletId { get; set; }

        /// <summary>
        /// Gets or sets PublicKey.
        /// </summary>
        public byte[] SecretKey { get; set; }

        /// <summary>
        /// Gets or sets Description.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets Registrator.
        /// </summary>
        public Guid Registrator { get; set; }

        #endregion
    }
}
