namespace LoadTestDataGenerator
{
    using System;
    using System.ComponentModel;
    using System.Windows.Forms;

    public static class Extensions
    {
        public static void InvokeIfRequired<T>(this T control, Action<T> methodcall) where T : Control
        {
            if (control.InvokeRequired)
            {
                control.Invoke(new MethodInvoker(() => methodcall(control)));
            }
            else
            {
                methodcall(control);
            }
        }
    }
}
