using System;
using System.Threading;
using System.Web.Mvc.Async;
using Microsoft.Practices.ServiceLocation;
using WB.Core.GenericSubdomains.Portable.Services;

namespace WB.Core.SharedKernels.SurveyManagement.Web.Utils.Threading
{
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
        /// <param name="manager">
        /// The manager.
        /// </param>
        /// <param name="result">
        /// The result.
        /// </param>
        /// <returns>
        /// The <see cref="IAsyncResult"/>.
        /// </returns>
        public static IAsyncResult Update(AsyncManager manager, SaveSingleResult result)
        {
            manager.OutstandingOperations.Increment();
            return new AsyncViewResult(manager, result);
        }

        #endregion

        /// <summary>
        /// The async view result.
        /// </summary>
        private class AsyncViewResult : IAsyncResult
        {
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

            #endregion

            #region Constructors and Destructors

            /// <summary>
            /// Initializes a new instance of the <see cref="AsyncViewResult"/> class.
            /// </summary>
            /// <param name="manager">
            /// The manager.
            /// </param>
            /// <param name="updateQuestionnaire">
            /// The update questionnaire.
            /// </param>
            public AsyncViewResult(AsyncManager manager, SaveSingleResult updateQuestionnaire)
            {
                this.updateQuestionnare = updateQuestionnaire;

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
                                var logger = ServiceLocator.Current.GetInstance<ILogger>();
                                logger.Error(e.Message, e);
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