// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SyncProcessType.cs" company="The World Bank">
//   Sync Process Type
// </copyright>
// <summary>
//   Sync Process Type
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace DataEntryClient.SycProcessFactory
{
    /// <summary>
    /// Sync Process Type
    /// </summary>
    public enum SyncProcessType
    {
        /// <summary>
        /// Network sync type
        /// </summary>
        Network = 1,

        /// <summary>
        /// USB sync type
        /// </summary>
        Usb = 2,

        /// <summary>
        /// Template sync type
        /// </summary>
        Template = 3,

        /// <summary>
        /// Event sync type
        /// </summary>
        Event = 4
    }
}
