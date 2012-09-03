// -----------------------------------------------------------------------
// <copyright file="DriverProvider.cs" company="">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

namespace Synchronization.Core.Interface
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.IO;

    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public interface IUsbProvider
    {
        DriveInfo ActiveUsb { get; }
        bool IsAnyAvailable { get; }
    }
}
