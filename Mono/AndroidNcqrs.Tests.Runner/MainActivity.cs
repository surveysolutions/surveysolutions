using System;
using System.Collections.Generic;
using System.Reflection;
using Android.App;
using NUnitLite.MonoDroid;

namespace AndroidNcqrs.Tests.Runner
{
	[Activity(Label = "Android.Tests.Runner", MainLauncher = true, Icon = "@drawable/icon")]
	public class MainActivity : TestRunnerActivity
	{
		protected override IEnumerable<Assembly> GetAssembliesForTest()
		{
			//yield return typeof (NcqrsEnvironmentSpecs).Assembly;
			//yield return typeof(Core.CAPI.Tests.Synchronization.ClientEventSyncTests).Assembly;
			yield return typeof(RavenQuestionnaire.Core.Tests.Entities.QuestionTest).Assembly;
		}

		protected override Type GetDetailsActivityType
		{
			get { return typeof(DefaultTestDetailsActivity); }
		}
	}
}

