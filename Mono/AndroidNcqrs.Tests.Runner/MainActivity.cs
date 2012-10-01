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
using Ncqrs.Tests;

namespace AndroidNcqrs.Tests.Runner
{
	[Activity(Label = "AndroidNcqrs.Tests.Runner", MainLauncher = true, Icon = "@drawable/icon")]
	public class MainActivity : TestRunnerActivity
	{
		protected override IEnumerable<Assembly> GetAssembliesForTest()
		{
			yield return typeof (NcqrsEnvironmentSpecs).Assembly;
		}

		protected override Type GetDetailsActivityType
		{
			get { return typeof(DefaultTestDetailsActivity); }
		}
	}
}

