using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using WB.Core.SharedKernels.DataCollection.Views.InterviewerAuditLog;

namespace WB.Core.BoundedContexts.Headquarters.InterviewerAuditLog
{
    public interface IAuditLogTypeResolver
    {
        Type Resolve(string auditLogEntityTypeName);
    }

    class AuditLogTypeResolver : IAuditLogTypeResolver
    {
        public Type Resolve(string auditLogEntityTypeName)
        {
            if (!KnownDataTypes.ContainsKey(auditLogEntityTypeName))
            {
                throw new ArgumentException($"There is no audit log entity with name {auditLogEntityTypeName} registered", nameof(auditLogEntityTypeName));
            }

            return KnownDataTypes[auditLogEntityTypeName];
        }

        private readonly Dictionary<string, Type> KnownDataTypes = new Dictionary<string, Type>();

        public AuditLogTypeResolver(params Assembly[] assemblies)
        {
            foreach (Assembly assembly in assemblies)
            {
                this.RegisterTypes(assembly);
            }
        }

        private void RegisterTypes(Assembly assembly)
        {
            var interfaceInfo = typeof(IAuditLogEntity).GetTypeInfo();

            var implementations = assembly.DefinedTypes.Where(definedType => interfaceInfo.IsAssignableFrom(definedType));

            foreach (var implementation in implementations)
            {
                this.RegisterType(implementation.AsType());
            }
        }

        internal void RegisterType(Type dataType)
        {
            ThrowIfThereIsAnotherEntityWithSameFullName(dataType);
            ThrowIfThereIsAnotherEntityWithSameName(dataType);

            KnownDataTypes[dataType.FullName] = dataType;
            KnownDataTypes[dataType.Name] = dataType;
        }

        private void ThrowIfThereIsAnotherEntityWithSameName(Type entity)
        {
            KnownDataTypes.TryGetValue(entity.Name, out Type anotherEntityWithSameName);

            if (anotherEntityWithSameName != null && anotherEntityWithSameName != entity)
                throw new ArgumentException(string.Format("Two different Entities share same type name:{0}{1}{0}{2}",
                    Environment.NewLine, entity.AssemblyQualifiedName, anotherEntityWithSameName.AssemblyQualifiedName));
        }

        private void ThrowIfThereIsAnotherEntityWithSameFullName(Type entity)
        {
            KnownDataTypes.TryGetValue(entity.FullName, out Type anotherEntityWithSameName);

            if (anotherEntityWithSameName != null && anotherEntityWithSameName != entity)
                throw new ArgumentException(string.Format(
                    "Two different Entities share same full type name:{0}{1}{0}{2}",
                    Environment.NewLine, entity.AssemblyQualifiedName, anotherEntityWithSameName.AssemblyQualifiedName));
        }
    }
}
