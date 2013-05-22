// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ILog.cs" company="">
//   
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace WB.Core.SharedKernel.Logger
{
    using System;

    /// <summary>
    /// The Log interface.
    /// </summary>
    public interface ILog
    {
        #region Public Properties

        /// <summary>
        /// Gets a value indicating whether is debug enabled.
        /// </summary>
        bool IsDebugEnabled { get; }

        /// <summary>
        /// Gets a value indicating whether is error enabled.
        /// </summary>
        bool IsErrorEnabled { get; }

        /// <summary>
        /// Gets a value indicating whether is fatal enabled.
        /// </summary>
        bool IsFatalEnabled { get; }

        /// <summary>
        /// Gets a value indicating whether is info enabled.
        /// </summary>
        bool IsInfoEnabled { get; }

        /// <summary>
        /// Gets a value indicating whether is warn enabled.
        /// </summary>
        bool IsWarnEnabled { get; }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// The debug.
        /// </summary>
        /// <param name="message">
        /// The message.
        /// </param>
        void Debug(object message);

        /// <summary>
        /// The debug.
        /// </summary>
        /// <param name="message">
        /// The message.
        /// </param>
        /// <param name="exception">
        /// The exception.
        /// </param>
        void Debug(object message, Exception exception);

        /// <summary>
        /// The debug format.
        /// </summary>
        /// <param name="format">
        /// The format.
        /// </param>
        /// <param name="args">
        /// The args.
        /// </param>
        void DebugFormat(string format, params object[] args);

        /// <summary>
        /// The error.
        /// </summary>
        /// <param name="message">
        /// The message.
        /// </param>
        void Error(object message);

        /// <summary>
        /// The error.
        /// </summary>
        /// <param name="message">
        /// The message.
        /// </param>
        /// <param name="exception">
        /// The exception.
        /// </param>
        void Error(object message, Exception exception);

        /// <summary>
        /// The error format.
        /// </summary>
        /// <param name="format">
        /// The format.
        /// </param>
        /// <param name="args">
        /// The args.
        /// </param>
        void ErrorFormat(string format, params object[] args);

        /// <summary>
        /// The fatal.
        /// </summary>
        /// <param name="message">
        /// The message.
        /// </param>
        void Fatal(object message);

        /// <summary>
        /// The fatal.
        /// </summary>
        /// <param name="message">
        /// The message.
        /// </param>
        /// <param name="exception">
        /// The exception.
        /// </param>
        void Fatal(object message, Exception exception);

        /// <summary>
        /// The fatal format.
        /// </summary>
        /// <param name="format">
        /// The format.
        /// </param>
        /// <param name="args">
        /// The args.
        /// </param>
        void FatalFormat(string format, params object[] args);

        /// <summary>
        /// The info.
        /// </summary>
        /// <param name="message">
        /// The message.
        /// </param>
        void Info(object message);

        /// <summary>
        /// The info.
        /// </summary>
        /// <param name="message">
        /// The message.
        /// </param>
        /// <param name="exception">
        /// The exception.
        /// </param>
        void Info(object message, Exception exception);

        /// <summary>
        /// The info format.
        /// </summary>
        /// <param name="format">
        /// The format.
        /// </param>
        /// <param name="args">
        /// The args.
        /// </param>
        void InfoFormat(string format, params object[] args);

        /// <summary>
        /// The warn.
        /// </summary>
        /// <param name="message">
        /// The message.
        /// </param>
        void Warn(object message);

        /// <summary>
        /// The warn.
        /// </summary>
        /// <param name="message">
        /// The message.
        /// </param>
        /// <param name="exception">
        /// The exception.
        /// </param>
        void Warn(object message, Exception exception);

        /// <summary>
        /// The warn format.
        /// </summary>
        /// <param name="format">
        /// The format.
        /// </param>
        /// <param name="args">
        /// The args.
        /// </param>
        void WarnFormat(string format, params object[] args);

        #endregion
    }
}