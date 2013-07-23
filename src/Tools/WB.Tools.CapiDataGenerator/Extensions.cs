using System;
using System.Windows.Controls;

namespace CapiDataGenerator
{
    public static class Extensions
    {
        public static void InvokeIfReqired<T>(this T control, Action a) where T : Control
        {
            if (control.Dispatcher.CheckAccess())
                a();
            else
                control.Dispatcher.Invoke(new Action(a));
        }
    }
}
