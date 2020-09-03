using NUnit.Framework;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.HttpServices.Services;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Tests.Abc;

namespace WB.Tests.Unit.SharedKernels.DataCollection.PlainQuestionnaireTests
{
    [TestOf(typeof(PlainQuestionnaire))]
    internal class PlainQuestionnaireTestsContext
    {
        [NUnit.Framework.OneTimeSetUp]
        public void baseContext()
        {
            SetUp.InstanceToMockedServiceLocator<ISubstitutionService>(new SubstitutionService());
        }
    }
}
