using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using Moq;
using Ncqrs;
using NUnit.Framework;
using WB.Core.BoundedContexts.Designer.Implementation.Services;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.Fetching;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.Infrastructure.HttpServices.Services;
using WB.Core.SharedKernels.DataCollection.Services;
using WB.Infrastructure.Native.Fetching;
using WB.Infrastructure.Native.Files.Implementation.FileSystem;
using WB.Tests.Abc;

[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("DynamicProxyGenAssembly2")]
[assembly: Localizable(false)]

namespace WB.Tests.Unit
{
    [SetUpFixture]
    public class NunitAssemblyContext
    {
        [OneTimeSetUp]
        public void OnAssemblyStart()
        {
            AssemblyContext.SetupServiceLocator();
        }
    }
}

public class AssemblyContext 
{
    private class FakeFetchingProvider : IFetchingProvider
    {
        public IFetchRequest<TOriginating, TRelated> Fetch<TOriginating, TRelated>(IQueryable<TOriginating> query,
            Expression<Func<TOriginating, TRelated>> relatedObjectSelector)
        {
            return new FetchRequest<TOriginating, TRelated>(query);
        }

        public IFetchRequest<TOriginating, TRelated> FetchMany<TOriginating, TRelated>(IQueryable<TOriginating> query,
            Expression<Func<TOriginating, IEnumerable<TRelated>>> relatedObjectSelector)
        {
            return new FetchRequest<TOriginating, TRelated>(query);
        }

        public IFetchRequest<TQueried, TRelated> ThenFetch<TQueried, TFetch, TRelated>(IFetchRequest<TQueried, TFetch> query,
            Expression<Func<TFetch, TRelated>> relatedObjectSelector)
        {
            var impl = query as FetchRequest<TQueried, TFetch>;
            return new FetchRequest<TQueried, TRelated>(impl.query);
        }

        public IFetchRequest<TQueried, TRelated> ThenFetchMany<TQueried, TFetch, TRelated>(IFetchRequest<TQueried, TFetch> query,
            Expression<Func<TFetch, IEnumerable<TRelated>>> relatedObjectSelector)
        {
            var impl = query as FetchRequest<TQueried, TFetch>;
            return new FetchRequest<TQueried, TRelated>(impl.query);
        }

        public class FetchRequest<TQueried, TFetch> : IFetchRequest<TQueried, TFetch>
        {
            public readonly IQueryable<TQueried> query;

            public IEnumerator<TQueried> GetEnumerator()
            {
                return query.GetEnumerator();
            }

            System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
            {
                return query.GetEnumerator();
            }

            public Type ElementType
            {
                get { return query.ElementType; }
            }

            public Expression Expression
            {
                get { return query.Expression; }
            }

            public IQueryProvider Provider
            {
                get { return query.Provider; }
            }

            public FetchRequest(IQueryable<TQueried> query)
            {
                this.query = query;
            }
        }
    }


    [OneTimeSetUp]
    public void OnAssemblyStart()
    {
        SetupServiceLocator();
    }

    public void OnAssemblyComplete()
    {
    }

    public static void SetupServiceLocator()
    {
        CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
        Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;

        var serviceLocator = Stub<IServiceLocator>.WithNotEmptyValues;

        ServiceLocator.SetLocatorProvider(() => serviceLocator);

        SetUp.InstanceToMockedServiceLocator<ISubstitutionService>(new SubstitutionService());
        SetUp.InstanceToMockedServiceLocator<IKeywordsProvider>(new KeywordsProvider(new SubstitutionService()));
        SetUp.InstanceToMockedServiceLocator<IFileSystemAccessor>(new FileSystemIOAccessor());

        SetUp.InstanceToMockedServiceLocator(Mock.Of<ILogger>());
        SetUp.InstanceToMockedServiceLocator(Mock.Of<IClock>());
        SetUp.InstanceToMockedServiceLocator(Mock.Of<IInterviewUniqueKeyGenerator>());

        EagerFetchExtensions.FetchingProvider = () => new FakeFetchingProvider();
    }
}
