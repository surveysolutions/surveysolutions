using System;
using Android.App;
using Android.Content;
using Android.Runtime;

namespace Ncqrs.Eventing.Storage.SQLite.Tests
{
	//[Application]
	public class SqliteTestsContext : Application
	{
		public static Context CurrentContext { get; set; }

		public SqliteTestsContext()
		{
		}

		protected SqliteTestsContext(IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer)
		{
		}

		public override void OnCreate()
		{
			base.OnCreate();

			CurrentContext = this;
		}
	}
}