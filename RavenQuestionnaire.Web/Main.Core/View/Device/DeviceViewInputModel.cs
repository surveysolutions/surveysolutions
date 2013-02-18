// -----------------------------------------------------------------------
// <copyright file="DeviceInputModel.cs" company="WorldBank">
// 2012
// </copyright>
// -----------------------------------------------------------------------

namespace Main.Core.View.Device
{
    using System;

    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public class DeviceViewInputModel
    {
        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="DeviceViewInputModel"/> class.
        /// </summary>
        /// <param name="tabletId">
        /// The tablet id.
        /// </param>
        public DeviceViewInputModel(Guid tabletId)
        {
            this.TabletId = tabletId;
        }

        #endregion
        
        #region Properties

        /// <summary>
        /// Gets or sets TabletId.
        /// </summary>
        public Guid TabletId { get; set; }
        
        #endregion

    }
}
