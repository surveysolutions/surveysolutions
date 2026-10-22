using System;
using Moq;
using MvvmCross.Navigation;
using NUnit.Framework;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;
using WB.Core.SharedKernels.Enumerator.ViewModels.Dialogs;
using WB.Core.SharedKernels.Enumerator.Views;
using WB.Tests.Abc;

namespace WB.Tests.Unit.SharedKernels.Enumerator.ViewModels;

[TestOf(typeof(ApproveInterviewDialogViewModel))]
public class ApproveInterviewDialogViewModelTests
{
    [Test]
    public void should_allow_apply()
    {
        var viewModel = CreateApproveInterviewDialogViewModel();

        Assert.That(viewModel.CanApply, Is.True);
    }

    private static ApproveInterviewDialogViewModel CreateApproveInterviewDialogViewModel(
        IMvxNavigationService navigationService = null,
        IStatefulInterviewRepository statefulInterviewRepository = null,
        ICommandService commandService = null,
        IPrincipal principal = null,
        IAuditLogService auditLogService = null)
    {
        return new ApproveInterviewDialogViewModel(
            navigationService ?? Mock.Of<IMvxNavigationService>(),
            statefulInterviewRepository ?? Mock.Of<IStatefulInterviewRepository>(),
            commandService ?? Mock.Of<ICommandService>(),
            principal ?? Create.Service.Principal(Guid.NewGuid()),
            auditLogService ?? Mock.Of<IAuditLogService>(),
            Mock.Of<MvvmCross.Plugin.Messenger.IMvxMessenger>(),
            Create.Storage.SqliteInmemoryStorage<InterviewView>(),
            Create.Storage.SqliteInmemoryStorage<AssignmentDocument, int>(),
            Create.Storage.SqliteInmemoryStorage<InterviewerDocument>());
    }
}
