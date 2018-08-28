using System.Collections.Generic;
using MvvmCross.Base;
using MvvmCross.Views;
using NUnit.Framework;
using WB.Core.BoundedContexts.Supervisor.ViewModel;
using WB.Core.SharedKernels.Enumerator.Services.Synchronization;
using WB.Tests.Abc;
using MvvmCross.Tests;

namespace WB.Tests.Unit.BoundedContexts.Supervisor.ViewModels.ConnectedDeviceSynchronizationViewModelTests
{
    [TestOf(typeof(ConnectedDeviceSynchronizationViewModel))]
    internal class ConnectedDeviceSynchronizationViewModelTests : MvxIoCSupportingTest
    {
        [Test]
        public void when_updating_progress_status_with_stage_and_params()
        {
            base.Setup();
            var dispatcher = Create.Fake.MvxMainThreadDispatcher1();
            Ioc.RegisterSingleton<IMvxViewDispatcher>(dispatcher);
            Ioc.RegisterSingleton<IMvxMainThreadDispatcher>(dispatcher);

            var viewModel = Create.ViewModel.ConnectedDeviceSynchronizationViewModel();
            var progress = new SyncProgressInfo()
            {
                TransferProgress = null,

                Stage = SyncStage.UpdatingAssignments,
                Description = "test description",
                Title = "test title",
                StageExtraInfo = new Dictionary<string, string>()
                {
                    { "processedCount", 2.ToString() },
                    { "totalCount", 5.ToString()}
                }
            };

            viewModel.ProgressOnProgressChanged(null, progress);

            Assert.That(viewModel.ProcessOperation, Is.EqualTo("Updating assignments"));
            Assert.That(viewModel.ProcessOperationDescription, Is.EqualTo("2 of 5 interviews done"));
        }

        [Test]
        public void when_updating_progress_status_with_stage()
        {
            base.Setup();
            var dispatcher = Create.Fake.MvxMainThreadDispatcher1();
            Ioc.RegisterSingleton<IMvxViewDispatcher>(dispatcher);
            Ioc.RegisterSingleton<IMvxMainThreadDispatcher>(dispatcher);

            var viewModel = Create.ViewModel.ConnectedDeviceSynchronizationViewModel();
            var progress = new SyncProgressInfo()
            {
                TransferProgress = null,

                Stage = SyncStage.AssignmentsSynchronization,
                Description = "test description",
                Title = "test title",
                StageExtraInfo = new Dictionary<string, string>()
                {
                    { "processedCount", 2.ToString() },
                    { "totalCount", 5.ToString()}
                }
            };

            viewModel.ProgressOnProgressChanged(null, progress);

            Assert.That(viewModel.ProcessOperation, Is.EqualTo("Synchronizing 2 of 5 assignment(s)"));
            Assert.That(viewModel.ProcessOperationDescription, Is.Null);
        }

        [Test]
        public void when_updating_progress_status_without_stage()
        {
            base.Setup();
            var dispatcher = Create.Fake.MvxMainThreadDispatcher1();
            Ioc.RegisterSingleton<IMvxViewDispatcher>(dispatcher);
            Ioc.RegisterSingleton<IMvxMainThreadDispatcher>(dispatcher);


            var viewModel = Create.ViewModel.ConnectedDeviceSynchronizationViewModel();
            var progress = new SyncProgressInfo()
            {
                Description = "test description",
                Title = "test title",
                TransferProgress = null 
            };

            viewModel.ProgressOnProgressChanged(null, progress);

            Assert.That(viewModel.ProcessOperation, Is.EqualTo("test title"));
            Assert.That(viewModel.ProcessOperationDescription, Is.EqualTo("test description"));
        }
    }
}
