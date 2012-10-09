using System;
using System.Collections.Generic;
using Android.App;
using AndroidMain.Core.Tests.CommonTests;
using NUnitLite;
using NUnitLite.MonoDroid;
using Ncqrs.Eventing.Storage.SQLite.Tests;

namespace Android.Tests.Runner
{
	[Activity(Label = "Android.Tests.Runner", MainLauncher = true, Icon = "@drawable/icon")]
	public class MainActivity : TestRunnerActivity
	{
		protected override IEnumerable<TestAssemblyInfo> GetAssembliesForTest()
		{
			//yield return typeof (NcqrsEnvironmentSpecs).Assembly;
			//yield return typeof(Core.CAPI.Tests.Synchronization.ClientEventSyncTests).Assembly;
			//yield return typeof(AndroidNcalc.Tests.Fixtures).Assembly;
			//yield return typeof(Ncqrs.Restoring.EventStapshoot.test.SnapshootableAggregateRootTests).Assembly;
			yield return MainCoreTests();
			//yield return SQliteEventStoreTests();
		}

		private TestAssemblyInfo SQliteEventStoreTests()
		{
			var assembly = typeof (SQLiteEventStoreTests).Assembly;

			return new TestAssemblyInfo(assembly);
		}

		protected override Type GetDetailsActivityType
		{
			get { return typeof(DefaultTestDetailsActivity); }
		}

		private TestAssemblyInfo MainCoreTests()
		{
			var assembly = typeof(RavenQuestionnaire.Core.Tests.Entities.QuestionTest).Assembly;

			return new TestAssemblyInfo(assembly, typeof(CommonInfrastuctureTests));
		}
	}
}

