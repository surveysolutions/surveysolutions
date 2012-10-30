using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Synchronization.Core.Registration
{
    public class SyncDeviceRegisterDocument
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

        #endregion
    }
}
