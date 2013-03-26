using System;
using System.Collections.Generic;
using Android.App;
using CAPI.Androids.Core.Model.Tests;
using NUnitLite;
using NUnitLite.MonoDroid;
using Ncqrs.Eventing.Storage.SQLite.Tests;
using Ncqrs.Tests;

namespace Android.Tests.Runner
{
	[Activity(Label = "Android.Tests.Runner", MainLauncher = true, Icon = "@drawable/icon")]
	public class MainActivity : TestRunnerActivity
	{
		protected override IEnumerable<TestAssemblyInfo> GetAssembliesForTest()
		{
		/*	yield return NcqrsTests();
			yield return CapiTests();
			yield return NcalcTests();
			yield return StepshootTests();
			yield return MainCoreTests();
			yield return SQliteEventStoreTests();*/
            yield return AndroidAppTests();
		}

		#region TestAssemblyInfos
		private TestAssemblyInfo StepshootTests()
		{
			var assembly = typeof (Ncqrs.Restoring.EventStapshoot.test.SnapshootableAggregateRootTests).Assembly;

			return new TestAssemblyInfo(assembly);
		}

		private TestAssemblyInfo NcalcTests()
		{
			var assembly = typeof (AndroidNcalc.Tests.Fixtures).Assembly;

			return new TestAssemblyInfo(assembly);
		}

		private TestAssemblyInfo CapiTests()
		{
			var assembly = typeof (Core.CAPI.Tests.Synchronization.ClientEventSyncTests).Assembly;

			return new TestAssemblyInfo(assembly);
		}

		private TestAssemblyInfo SQliteEventStoreTests()
		{
			var assembly = typeof (SQLiteEventStoreTests).Assembly;

			return new TestAssemblyInfo(assembly);
		}
        private TestAssemblyInfo AndroidAppTests()
        {
            var assembly = typeof(CompleteQuestionnaireViewTests).Assembly;

            return new TestAssemblyInfo(assembly);
        }
		private TestAssemblyInfo MainCoreTests()
		{
			var assembly = typeof(RavenQuestionnaire.Core.Tests.Entities.QuestionTest).Assembly;

			return new TestAssemblyInfo(assembly/*, typeof(CommonInfrastuctureTests)*/);
		}

		private TestAssemblyInfo NcqrsTests()
		{
			var assembly = typeof (NcqrsEnvironmentSpecs).Assembly;

			return new TestAssemblyInfo(assembly);
		}
		#endregion

		protected override Type GetDetailsActivityType
		{
			get { return typeof(DefaultTestDetailsActivity); }
		}
	}
}

