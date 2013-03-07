using System;

namespace Mono.Android.Crasher.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public class GoogleFormReporterSettingsAttribute : Attribute
    {
        /// <summary>
        /// The id of the Google Doc form.
        /// </summary>
        public string FormKey { get; set; }

        /// <summary>
        /// Value in milliseconds for timeout attempting to connect to a network (default 10000ms).
        /// </summary>
        public int ConnectionTimeout { get; set; }

        public GoogleFormReporterSettingsAttribute(string formKey, int connectionTimeout = 10000)
        {
            FormKey = formKey;
            ConnectionTimeout = connectionTimeout;
        }
    }
}