using System;
using System.Collections.Generic;
using System.Reflection;
using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using NUnitLite.MonoDroid;

namespace AndroidNcqrs.Restoring.EventStapshoot.test
{
	[Activity(Label = "AndroidNcqrs.Restoring.EventStapshoot.test", MainLauncher = true, Icon = "@drawable/icon")]
	public class MainActivity : TestRunnerActivity
	{
		protected override IEnumerable<Assembly> GetAssembliesForTest()
		{
			yield return GetType().Assembly;
		}

		protected override Type GetDetailsActivityType
		{
			get { return typeof (DefaultTestDetailsActivity); }
		}
	}
}

