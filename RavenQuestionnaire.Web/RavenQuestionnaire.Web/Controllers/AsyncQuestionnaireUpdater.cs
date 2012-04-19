using System;
using System.Threading;
using System.Web.Mvc;
using RavenQuestionnaire.Core.Views.Question;
using RavenQuestionnaire.Web.Models;

namespace RavenQuestionnaire.Web.Controllers
{
    public static class AsyncQuestionnaireUpdater
    {
        public delegate void SaveSingleResult();

        public static IAsyncResult Update(SaveSingleResult resp)
        {

            return new AsyncViewResult(resp);

        }

        private class AsyncViewResult : IAsyncResult
        //private class MyAsyncResult : IAsyncResult
        {
            private static int mCount = 0;
            private readonly SaveSingleResult updateQuestionnare;
            private readonly Thread mThread;
            private readonly AutoResetEvent mWait;

            public AsyncViewResult(SaveSingleResult updateQuestionnare)
            {
                this.updateQuestionnare = updateQuestionnare;

                bool hasMail = Interlocked.Increment(ref mCount) % 2 == 0;

                mWait = new AutoResetEvent(false);

                mThread = new Thread(new ThreadStart(() =>
                                                         {
                                                             // some very long operation. OK sleeping ;)
                                                             Thread.Sleep(TimeSpan.FromMilliseconds(5000));

                                                             // notify that the long operation is complete:
                                                             this.updateQuestionnare();

                                                             mWait.Set();
                                                         }));

                mThread.Start();

            }

            public object AsyncState
            {
                get { return null; }
            }

            public System.Threading.WaitHandle AsyncWaitHandle
            {
                get { return mWait; }
            }

            public bool CompletedSynchronously
            {
                get { return false; }
            }

            public bool IsCompleted
            {
                get { return mThread.IsAlive; }
            }
        }
    }
}
