//using System;
//using Android.App;
//using Android.Content;
//using Android.Runtime;

//namespace Android.Tests.Runner
//{
//    [Application]
//    public class TestsRunnerContext : Application
//    {
//        public static Context CurrentContext { get; set; }

//        public TestsRunnerContext()
//        {
//        }

//        protected TestsRunnerContext(IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer)
//        {
//        }

//        public override void OnCreate()
//        {
//            base.OnCreate();

//            CurrentContext = this;
//        }
//    }
//}