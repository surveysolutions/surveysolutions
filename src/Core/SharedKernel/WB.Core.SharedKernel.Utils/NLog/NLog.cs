// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Class1.cs" company="">
//   
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace WB.Core.SharedKernel.Utils.NLog
{
    using System;

    using WB.Core.SharedKernel.Logger;

    /// <summary>
    /// The log.
    /// </summary>
    public class Log : ILog
    {
        /// <summary>
        /// The log.
        /// </summary>
        private readonly global::NLog.Logger log = global::NLog.LogManager.GetCurrentClassLogger();

        #region Public Properties

        /// <summary>
        /// Gets a value indicating whether is debug enabled.
        /// </summary>
        public bool IsDebugEnabled {
            get
            {
                return this.log.IsDebugEnabled;
            }
        }

        /// <summary>
        /// Gets a value indicating whether is error enabled.
        /// </summary>
        public bool IsErrorEnabled {
            get
            {
                return this.log.IsErrorEnabled;
            }
        }

        /// <summary>
        /// Gets a value indicating whether is fatal enabled.
        /// </summary>
        public bool IsFatalEnabled { 
            get
            {
                return this.log.IsFatalEnabled;
            }
        }

        /// <summary>
        /// Gets a value indicating whether is info enabled.
        /// </summary>
        public bool IsInfoEnabled { 
            get
            {
                return this.log.IsInfoEnabled;
            }
        }

        /// <summary>
        /// Gets a value indicating whether is warn enabled.
        /// </summary>
        public bool IsWarnEnabled {
            get
            {
                return this.log.IsWarnEnabled;
            }
        }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// The debug.
        /// </summary>
        /// <param name="message">
        /// The message.
        /// </param>
        /// <exception cref="NotImplementedException">
        /// </exception>
        public void Debug(object message)
        {
            this.log.Debug(message);
        }

        /// <summary>
        /// The debug.
        /// </summary>
        /// <param name="message">
        /// The message.
        /// </param>
        /// <param name="exception">
        /// The exception.
        /// </param>
        /// <exception cref="NotImplementedException">
        /// </exception>
        public void Debug(object message, Exception exception)
        {
            this.log.Debug((string)message, exception);
        }

        /// <summary>
        /// The debug format.
        /// </summary>
        /// <param name="format">
        /// The format.
        /// </param>
        /// <param name="args">
        /// The args.
        /// </param>
        /// <exception cref="NotImplementedException">
        /// </exception>
        public void DebugFormat(string format, params object[] args)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// The error.
        /// </summary>
        /// <param name="message">
        /// The message.
        /// </param>
        /// <exception cref="NotImplementedException">
        /// </exception>
        public void Error(object message)
        {
            this.log.Error(message);
        }

        /// <summary>
        /// The error.
        /// </summary>
        /// <param name="message">
        /// The message.
        /// </param>
        /// <param name="exception">
        /// The exception.
        /// </param>
        /// <exception cref="NotImplementedException">
        /// </exception>
        public void Error(object message, Exception exception)
        {
            this.log.Error((string)message, exception);
        }

        /// <summary>
        /// The error format.
        /// </summary>
        /// <param name="format">
        /// The format.
        /// </param>
        /// <param name="args">
        /// The args.
        /// </param>
        /// <exception cref="NotImplementedException">
        /// </exception>
        public void ErrorFormat(string format, params object[] args)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// The fatal.
        /// </summary>
        /// <param name="message">
        /// The message.
        /// </param>
        /// <exception cref="NotImplementedException">
        /// </exception>
        public void Fatal(object message)
        {
            this.log.Fatal(message);
        }

        /// <summary>
        /// The fatal.
        /// </summary>
        /// <param name="message">
        /// The message.
        /// </param>
        /// <param name="exception">
        /// The exception.
        /// </param>
        /// <exception cref="NotImplementedException">
        /// </exception>
        public void Fatal(object message, Exception exception)
        {
            this.log.Fatal((string)message, exception);
        }

        /// <summary>
        /// The fatal format.
        /// </summary>
        /// <param name="format">
        /// The format.
        /// </param>
        /// <param name="args">
        /// The args.
        /// </param>
        /// <exception cref="NotImplementedException">
        /// </exception>
        public void FatalFormat(string format, params object[] args)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// The info.
        /// </summary>
        /// <param name="message">
        /// The message.
        /// </param>
        /// <exception cref="NotImplementedException">
        /// </exception>
        public void Info(object message)
        {
            this.log.Info(message);
        }

        /// <summary>
        /// The info.
        /// </summary>
        /// <param name="message">
        /// The message.
        /// </param>
        /// <param name="exception">
        /// The exception.
        /// </param>
        /// <exception cref="NotImplementedException">
        /// </exception>
        public void Info(object message, Exception exception)
        {
            this.log.Info((string)message, exception);
        }

        /// <summary>
        /// The info format.
        /// </summary>
        /// <param name="format">
        /// The format.
        /// </param>
        /// <param name="args">
        /// The args.
        /// </param>
        /// <exception cref="NotImplementedException">
        /// </exception>
        public void InfoFormat(string format, params object[] args)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// The warn.
        /// </summary>
        /// <param name="message">
        /// The message.
        /// </param>
        /// <exception cref="NotImplementedException">
        /// </exception>
        public void Warn(object message)
        {
            this.log.Warn(message);
        }

        /// <summary>
        /// The warn.
        /// </summary>
        /// <param name="message">
        /// The message.
        /// </param>
        /// <param name="exception">
        /// The exception.
        /// </param>
        /// <exception cref="NotImplementedException">
        /// </exception>
        public void Warn(object message, Exception exception)
        {
            this.log.Warn((string)message,exception);
        }

        /// <summary>
        /// The warn format.
        /// </summary>
        /// <param name="format">
        /// The format.
        /// </param>
        /// <param name="args">
        /// The args.
        /// </param>
        /// <exception cref="NotImplementedException">
        /// </exception>
        public void WarnFormat(string format, params object[] args)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}