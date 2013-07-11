namespace Main.Core.Entities.Composite
{
    using System;
    using System.Runtime.Serialization;

    /// <summary>
    /// The composite exception.
    /// </summary>
    public class CompositeException : Exception
    {
        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="CompositeException"/> class.
        /// </summary>
        public CompositeException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CompositeException"/> class.
        /// </summary>
        /// <param name="message">
        /// The message.
        /// </param>
        public CompositeException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CompositeException"/> class.
        /// </summary>
        /// <param name="message">
        /// The message.
        /// </param>
        /// <param name="innerException">
        /// The inner exception.
        /// </param>
        public CompositeException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CompositeException"/> class.
        /// </summary>
        /// <param name="info">
        /// The info.
        /// </param>
        /// <param name="context">
        /// The context.
        /// </param>
        protected CompositeException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        #endregion
    }
}