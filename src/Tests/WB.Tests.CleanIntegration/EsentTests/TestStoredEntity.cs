using System;
using WB.Core.SharedKernels.SurveySolutions;

namespace WB.Tests.CleanIntegration.EsentTests
{
    class TestStoredEntity : IReadSideRepositoryEntity
    {
        public Guid Id { get; set; }

        public string StringProperty { get; set; }

        public int IntegerProperty { get; set; }
    }
}