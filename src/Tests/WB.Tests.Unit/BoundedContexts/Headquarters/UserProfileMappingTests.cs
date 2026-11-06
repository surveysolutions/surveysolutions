using System;
using NHibernate.Cfg;
using NHibernate.Mapping.ByCode;
using NUnit.Framework;
using WB.Core.BoundedContexts.Headquarters.Users.Providers;
using WB.Core.BoundedContexts.Headquarters.Views.User;
using WB.Infrastructure.Native.Storage.Postgre.NhExtensions;

namespace WB.Tests.Unit.BoundedContexts.Headquarters;

public class UserProfileMappingTests
{
    [TestCase(typeof(HqUserProfileMap), typeof(HqUserProfile))]
    [TestCase(typeof(WorkspaceUserProfileMap), typeof(WorkspaceUserProfile))]
    public void ShouldUseCompatibleDateTypeForRegistrationDate(Type mappingType, Type entityType)
    {
        var mapper = new ModelMapper();
        mapper.AddMapping(mappingType);
        var configuration = new Configuration();
        configuration.AddMapping(mapper.CompileMappingForAllExplicitlyAddedEntities());

        var property = configuration.GetClassMapping(entityType)
            .GetProperty(nameof(HqUserProfile.DeviceRegistrationDate));

        Assert.That(property.Type, Is.TypeOf<PostgresDateType>());
    }
}
