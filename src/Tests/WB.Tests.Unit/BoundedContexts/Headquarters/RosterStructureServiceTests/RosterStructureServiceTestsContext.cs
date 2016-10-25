using Machine.Specifications;
using WB.Core.BoundedContexts.Headquarters.Implementation.Services;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;

namespace WB.Tests.Unit.BoundedContexts.Headquarters.RosterStructureServiceTests
{
    [Subject(typeof(RosterStructureService))]
    public class RosterStructureServiceTestsContext
    {
        protected static RosterStructureService GetService()
        {
            return new RosterStructureService();
        }
    }
}