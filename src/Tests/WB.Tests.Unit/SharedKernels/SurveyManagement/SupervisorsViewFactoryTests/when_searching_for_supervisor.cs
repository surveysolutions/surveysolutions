using System;
using System.Linq;
using Machine.Specifications;
using WB.Core.SharedKernels.DataCollection.Views;
using WB.Core.SharedKernels.SurveyManagement.Views.Supervisor;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.SupervisorsViewFactoryTests
{
    internal class when_searching_for_supervisor : SupervisorsViewFactoryTestContext
    {
        private Establish context = () =>
        {
            UserDocument supervisor = CreateSupervisor(Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA"), "Supervisor1");
            var readerWithUsers = CreateQueryableReadSideRepositoryReaderWithUsers(supervisor);

            supervisorsViewFactory = CreateSupervisorsViewFactory(readerWithUsers);
        };

        Because of = () => searchResult = supervisorsViewFactory.Load(new SupervisorsInputModel {SearchBy = "sup"});

        It should_use_case_insensative_search = () => searchResult.Items.Count().ShouldEqual(1);

        static ISupervisorsViewFactory supervisorsViewFactory;
        static SupervisorsView searchResult;
    }
}