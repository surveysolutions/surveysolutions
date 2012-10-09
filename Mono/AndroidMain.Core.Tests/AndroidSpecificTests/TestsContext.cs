using System;
using Android.App;
using Android.Content;
using Android.Runtime;

namespace AndroidMain.Core.Tests.AndroidSpecificTests
{
	[Application]
	public class TestsContext : Application
	{
		public static Context CurrentContext { get; set; }

		public TestsContext()
		{
		}

		protected TestsContext(IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer)
		{
		}

		public override void OnCreate()
		{
			base.OnCreate();

			CurrentContext = this;
		}
	}
}