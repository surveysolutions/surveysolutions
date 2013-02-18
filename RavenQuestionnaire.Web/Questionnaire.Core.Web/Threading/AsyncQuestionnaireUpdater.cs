// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AsyncQuestionnaireUpdater.cs" company="">
//   
// </copyright>
// <summary>
//   The async questionnaire updater.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System.Threading.Tasks;
using System.Web.Mvc.Async;
using NLog;

namespace Questionnaire.Core.Web.Threading
{
    using System;
    using System.Threading;

    /// <summary>
    /// The async questionnaire updater.
    /// </summary>
    public static class AsyncQuestionnaireUpdater
    {
        #region Delegates

        /// <summary>
        /// The save single result.
        /// </summary>
        public delegate void SaveSingleResult();

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// The update.
        /// </summary>
        /// <param name="resp">
        /// The resp.
        /// </param>
        /// <returns>
        /// The System.IAsyncResult.
        /// </returns>
        public static IAsyncResult Update(AsyncManager manager, SaveSingleResult resp)
        {
            manager.OutstandingOperations.Increment();
            return new AsyncViewResult(manager,resp);
            
        }
     
        #endregion

        /// <summary>
        /// The async view result.
        /// </summary>
        private class AsyncViewResult : IAsyncResult
        {
            // private class MyAsyncResult : IAsyncResult
            #region Static Fields

            /// <summary>
            /// The m count.
            /// </summary>
            private static int mCount;

            #endregion

            #region Fields

            /// <summary>
            /// The m thread.
            /// </summary>
            private readonly Thread mThread;

            /// <summary>
            /// The m wait.
            /// </summary>
            private readonly AutoResetEvent mWait;

            /// <summary>
            /// The update questionnare.
            /// </summary>
            private readonly SaveSingleResult updateQuestionnare;
            /// <summary>
            /// The logger.
            /// </summary>
            private  Logger logger = LogManager.GetCurrentClassLogger();

            #endregion

            #region Constructors and Destructors

            /// <summary>
            /// Initializes a new instance of the <see cref="AsyncViewResult"/> class.
            /// </summary>
            /// <param name="updateQuestionnare">
            /// The update questionnare.
            /// </param>
            public AsyncViewResult(AsyncManager manager, SaveSingleResult updateQuestionnare)
            {
                this.updateQuestionnare = updateQuestionnare;

                bool hasMail = Interlocked.Increment(ref mCount) % 2 == 0;

                this.mWait = new AutoResetEvent(false);

                this.mThread = new Thread(
                    () =>
                        {
                            try
                            {
                                // notify that the long operation is complete:
                                this.updateQuestionnare();
                            }
                            catch (Exception e)
                            {
                                logger.ErrorException(e.Message, e);

                            }
                            manager.OutstandingOperations.Decrement();
                            this.mWait.Set();
                        });

                this.mThread.Start();
            }

            #endregion

            #region Public Properties

            /// <summary>
            /// Gets the async state.
            /// </summary>
            public object AsyncState
            {
                get
                {
                    return null;
                }
            }

            /// <summary>
            /// Gets the async wait handle.
            /// </summary>
            public WaitHandle AsyncWaitHandle
            {
                get
                {
                    return this.mWait;
                }
            }

            /// <summary>
            /// Gets a value indicating whether completed synchronously.
            /// </summary>
            public bool CompletedSynchronously
            {
                get
                {
                    return false;
                }
            }

            /// <summary>
            /// Gets a value indicating whether is completed.
            /// </summary>
            public bool IsCompleted
            {
                get
                {
                    return this.mThread.IsAlive;
                }
            }

            #endregion
        }
    }
}