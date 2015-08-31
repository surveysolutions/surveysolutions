using System;

namespace Mono.Android.Crasher
{
    public enum InteractionMode
    {
        /// <summary>
        /// No interaction, reports are sent silently and a "Force close" dialog
        /// terminates the app.
        /// </summary>
        Silent,
        /// <summary>
        /// A status bar notification is triggered when the application crashes, the
        /// Force close dialog is not displayed. When the user selects the
        /// notification, a dialog is displayed asking him if he is ok to send a
        /// report.
        /// </summary>
        [Obsolete("Not implemented yet")]
        Notification,
        /// <summary>
        /// A simple Toast is triggered when the application crashes, the Force close
        /// dialog is not displayed.
        /// </summary>
        [Obsolete("Not implemented yet")]
        Toast,
        /// <summary>
        /// Shows alert dialog and user selects send report or no.
        /// </summary>
        [Obsolete("Not implemented yet")]
        Dialog
    }
}