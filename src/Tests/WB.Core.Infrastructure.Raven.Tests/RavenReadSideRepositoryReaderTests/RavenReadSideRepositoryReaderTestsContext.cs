﻿using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

using Machine.Specifications;

using Moq;

using Raven.Client.Document;
using WB.Core.Infrastructure.ReadSide;
using WB.Core.Infrastructure.ReadSide.Repository;
using WB.Core.Infrastructure.Storage.Raven.Implementation.ReadSide.RepositoryAccessors;

namespace WB.Core.Infrastructure.Raven.Tests.RavenReadSideRepositoryReaderTests
{
    [Subject(typeof(RavenReadSideRepositoryReader<>))]
    internal class RavenReadSideRepositoryReaderTestsContext
    {
        internal static RavenReadSideRepositoryReader<TEntity> CreateRavenReadSideRepositoryReader<TEntity>(
            DocumentStore ravenStore = null, IReadSideStatusService readSideStatusService = null)
            where TEntity : class, IReadSideRepositoryEntity
        {
            return new RavenReadSideRepositoryReader<TEntity>(
                ravenStore ?? new DocumentStore(),
                readSideStatusService ?? Mock.Of<IReadSideStatusService>());
        }
    }
}
