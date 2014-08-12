using System.Collections.Generic;
using System.ComponentModel;
using CsQuery;

namespace WB.Core.BoundedContexts.Designer.Implementation.Services
{
    /// <summary>
    /// Provides data for the <see cref="HtmlSanitizer.RemovingTag"/> event.
    /// </summary>
    internal class RemovingTagEventArgs: CancelEventArgs
    {
        /// <summary>
        /// Gets or sets the tag to be removed.
        /// </summary>
        /// <value>
        /// The tag.
        /// </value>
        public IDomObject Tag { get; set; }
    }

    /// <summary>
    /// Provides data for the <see cref="HtmlSanitizer.RemovingAttribute"/> event.
    /// </summary>
    internal class RemovingAttributeEventArgs : CancelEventArgs
    {
        /// <summary>
        /// Gets or sets the attribute to be removed.
        /// </summary>
        /// <value>
        /// The attribute.
        /// </value>
        public KeyValuePair<string, string> Attribute { get; set; }
    }

    /// <summary>
    /// Provides data for the <see cref="HtmlSanitizer.RemovingStyle"/> event.
    /// </summary>
    internal class RemovingStyleEventArgs : CancelEventArgs
    {
        /// <summary>
        /// Gets or sets the style to be removed.
        /// </summary>
        /// <value>
        /// The style.
        /// </value>
        public KeyValuePair<string, string> Style { get; set; }
    }

}
