﻿using System;
using System.Collections.Generic;
using Android.App;
using CAPI.Androids.Core.Model.Tests;
using Main.Core.Tests.Entities;
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
            yield return NcqrsTests();
            yield return MainCoreTests();
            yield return SQliteEventStoreTests();
            yield return AndroidAppTests();
        }

        #region TestAssemblyInfos


        private TestAssemblyInfo SQliteEventStoreTests()
        {
            var assembly = typeof(MvvmCrossSqliteEventStoreTests).Assembly;

            return new TestAssemblyInfo(assembly);
        }
        private TestAssemblyInfo AndroidAppTests()
        {
            var assembly = typeof(ChangeLogManipulatorTests).Assembly;

            return new TestAssemblyInfo(assembly);
        }
        private TestAssemblyInfo MainCoreTests()
        {
            var assembly = typeof(QuestionTest).Assembly;

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

