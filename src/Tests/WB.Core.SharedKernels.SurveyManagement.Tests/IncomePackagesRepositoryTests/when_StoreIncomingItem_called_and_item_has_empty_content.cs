using System;
using Machine.Specifications;
using WB.Core.SharedKernel.Structures.Synchronization;
using WB.Core.SharedKernels.SurveyManagement.Implementation.Synchronization.IncomePackagesRepository;

namespace WB.Core.SharedKernels.SurveyManagement.Tests.IncomePackagesRepositoryTests
{
    internal class when_StoreIncomingItem_called_and_item_has_empty_content : IncomePackagesRepositoryTestContext
    {
        Establish context = () =>
        {
            incomePackagesRepository = CreateIncomePackagesRepository();
        };

        Because of = () => exception = Catch.Exception(() =>
            incomePackagesRepository.StoreIncomingItem(new SyncItem()));

        It should_throw_exception = () =>
          exception.ShouldNotBeNull();

        It should_throw_exception_of_type_ArgumentException = () =>
          exception.ShouldBeOfExactType<ArgumentException>();

        private static IncomePackagesRepository incomePackagesRepository;
        private static Exception exception;
    }
}
