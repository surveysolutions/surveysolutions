using System;

namespace StatData.Core
{
    /// <summary>
    /// Arguments passed to the event handling the change in file operation progress
    /// </summary>
    public class ProgressChangedArgs : EventArgs
    {
        /// <summary>
        /// Value of progress in percent
        /// </summary>
        public int Progress;
    }
}
