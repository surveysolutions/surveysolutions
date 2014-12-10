﻿using System;

using Machine.Specifications;

using Moq;
using WB.Core.Infrastructure;
using WB.Core.Infrastructure.ReadSide;
using WB.Core.Infrastructure.ReadSide.Repository;
using WB.Core.Infrastructure.Storage.Raven.Implementation.ReadSide.RepositoryAccessors;
using WB.Core.SharedKernels.SurveySolutions;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.Infrastructure.Storage.RavenReadSideRepositoryReaderTests
{
    internal class when_counting_repository_entities_and_views_are_being_rebuilt : RavenReadSideRepositoryReaderTestsContext
    {
        Establish context = () =>
        {
            var readSideStatusService = Mock.Of<IReadSideStatusService>(service
                => service.AreViewsBeingRebuiltNow() == true);

            reader = CreateRavenReadSideRepositoryReader<IReadSideRepositoryEntity>(readSideStatusService: readSideStatusService);
        };

        Because of = () =>
            exception = Catch.Exception(() =>
                reader.Count());

        It should_throw_maintenance_exception = () =>
            exception.ShouldBeOfExactType<MaintenanceException>();

        private static Exception exception;
        private static RavenReadSideRepositoryReader<IReadSideRepositoryEntity> reader;
    }
}