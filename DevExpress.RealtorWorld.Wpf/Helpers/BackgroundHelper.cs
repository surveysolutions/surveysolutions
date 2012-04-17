using System;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace DevExpress.RealtorWorld.Xpf.Helpers {
    public static class BackgroundHelper {
        static Dispatcher dispatcher;

        public static void DoInBackground(Action backgroundAction, Action mainThreadAction) {
            DoInBackground(backgroundAction, mainThreadAction, 200);
        }
        public static void DoInBackground(Action backgroundAction, Action mainThreadAction, int milliseconds) {
            Thread thread = new Thread(delegate() {
 //               Thread.Sleep(milliseconds);
                if(backgroundAction != null)
                    backgroundAction();
                if(mainThreadAction != null)
                    Dispatcher.BeginInvoke(mainThreadAction);
            });
            thread.IsBackground = true;
            thread.Priority = ThreadPriority.Lowest;
            thread.Start();
        }
        public static void DoInMainThread(Action action) {
            if(Dispatcher.CheckAccess()) {
                action();
            } else {
                AutoResetEvent done = new AutoResetEvent(false);
                Dispatcher.BeginInvoke((Action)delegate() {
                    action();
                    done.Set();
                });
                done.WaitOne();
            }
        }
        public static void DoWhenApllicationIdle(Action action) {
            Dispatcher.BeginInvoke(action, DispatcherPriority.ApplicationIdle);
        }
        public static Dispatcher Dispatcher {
            get {
                if(dispatcher == null)
                    dispatcher = DefaultDispatcher;
                return dispatcher;
            }
            set { dispatcher = value; }
        }
        static Dispatcher DefaultDispatcher {
            get {
                return Application.Current.Dispatcher;
            }
        }
    }
}
