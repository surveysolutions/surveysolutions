using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Humanizer;
using NHibernate;
using NHibernate.Cfg.MappingSchema;
using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;
using WB.Core.BoundedContexts.Designer;
using WB.Core.BoundedContexts.Headquarters;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.SurveyManagement;

namespace dbup
{
    public class MappingsCollector
    {
        public static HbmMapping GetReadSideForDesigner()
        {
            var mapper = new ModelMapper();
            var mappingAssemblies = new List<Assembly> { typeof(DesignerBoundedContextModule).Assembly };
            var mappingTypes = mappingAssemblies.SelectMany(x => x.GetExportedTypes())
                                                .Where(x => x.GetCustomAttribute<PlainStorageAttribute>() == null && x.IsSubclassOfRawGeneric(typeof(ClassMapping<>)));

            return CompileMappings(mapper, mappingTypes);
        }

        public static HbmMapping GetReadSideForHq()
        {
            var mapper = new ModelMapper();
            var mappingAssemblies = new List<Assembly> { typeof(SurveyManagementSharedKernelModule).Assembly, typeof(HeadquartersBoundedContextModule).Assembly };
            var mappingTypes = mappingAssemblies.SelectMany(x => x.GetExportedTypes())
                                                .Where(x => x.GetCustomAttribute<PlainStorageAttribute>() == null && x.IsSubclassOfRawGeneric(typeof(ClassMapping<>)));

            return CompileMappings(mapper, mappingTypes);
        }

        private static HbmMapping CompileMappings(ModelMapper mapper, IEnumerable<Type> mappingTypes)
        {
            mapper.AddMappings(mappingTypes);
            mapper.BeforeMapProperty += (inspector, member, customizer) =>
            {
                var propertyInfo = (PropertyInfo) member.LocalMember;
                if (propertyInfo.PropertyType == typeof(string))
                {
                    customizer.Type(NHibernateUtil.StringClob);
                }
            };

            mapper.BeforeMapClass += (inspector, type, customizer) =>
            {
                var tableName = type.Name.Pluralize();
                customizer.Table(tableName);
            };

            return mapper.CompileMappingForAllExplicitlyAddedEntities();
        }
    }
}