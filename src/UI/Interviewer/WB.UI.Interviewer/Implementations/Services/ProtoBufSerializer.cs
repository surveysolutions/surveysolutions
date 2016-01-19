using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using ProtoBuf.Meta;
using WB.Core.GenericSubdomains.Portable.Services;

namespace WB.UI.Interviewer.Implementations.Services
{
    public class ProtobufSerializer : IProtobufSerializer
    {
        public string Serialize(object item)
        {
            return Convert.ToBase64String(this.SerializeToByteArray(item));
        }

        public string Serialize(object item, TypeSerializationSettings typeSerializationSettings)
        {
            throw new NotImplementedException();
        }

        public byte[] SerializeToByteArray(object item)
        {
            if(item == null) return new byte[0];

            using (var ms = new MemoryStream())
            {
                this.SerializeToStream(item, item.GetType(), ms);
                return ms.ToArray();
            }
        }

        public void SerializeToStream(object value, Type type, Stream stream)
        {
            SerializerBuilder.Build(type);
            ProtoBuf.Serializer.NonGeneric.Serialize(stream, value);
        }

        public T Deserialize<T>(string payload)
        {
            throw new NotImplementedException();
        }

        public T Deserialize<T>(string payload, TypeSerializationSettings settings)
        {
            throw new NotImplementedException();
        }

        public object Deserialize(string payload, Type type, TypeSerializationSettings settings)
        {
            throw new NotImplementedException();
        }

        public T Deserialize<T>(byte[] payload)
        {
            return (T) this.DeserializeFromStream(new MemoryStream(payload), typeof (T));
        }

        public object DeserializeFromStream(Stream stream, Type type)
        {
            SerializerBuilder.Build(type);
            return ProtoBuf.Serializer.NonGeneric.Deserialize(type, stream);
        }

        public byte[] SerializeToByteArray(object item, TypeSerializationSettings typeSerializationSettings, SerializationType serializationType)
        {
            throw new NotImplementedException();
        }

        public object Deserialize(byte[] payload, Type objectType, TypeSerializationSettings typeSerializationSettings, SerializationType serializationType)
        {
            throw new NotImplementedException();
        }

        private static class SerializerBuilder
        {
            private const BindingFlags Flags = BindingFlags.FlattenHierarchy | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
            private static readonly Dictionary<Type, HashSet<Type>> SubTypes = new Dictionary<Type, HashSet<Type>>();
            private static readonly ConcurrentBag<Type> BuiltTypes = new ConcurrentBag<Type>();
            private static readonly Type ObjectType = typeof(object);

            /// <summary>
            /// Build the ProtoBuf serializer from the generic <see cref="Type">type</see>.
            /// </summary>
            /// <typeparam name="T">The type of build the serializer for.</typeparam>
            private static void Build<T>()
            {
                var type = typeof(T);
                Build(type);
            }

            /// <summary>
            /// Build the ProtoBuf serializer from the data's <see cref="Type">type</see>.
            /// </summary>
            /// <typeparam name="T">The type of build the serializer for.</typeparam>
            /// <param name="data">The data who's type a serializer will be made.</param>
            // ReSharper disable once UnusedParameter.Global
            public static void Build<T>(T data)
            {
                Build<T>();
            }

            /// <summary>
            /// Build the ProtoBuf serializer for the <see cref="Type">type</see>.
            /// </summary>
            /// <param name="type">The type of build the serializer for.</param>
            public static void Build(Type type)
            {
                if (BuiltTypes.Contains(type))
                {
                    return;
                }

                lock (type)
                {
                    if (RuntimeTypeModel.Default.CanSerialize(type))
                    {
                        if (type.IsGenericType)
                        {
                            BuildGenerics(type);
                        }

                        return;
                    }

                    var meta = RuntimeTypeModel.Default.Add(type, false);
                    var fields = GetFields(type);

                    meta.Add(fields.Select(m => m.Name).ToArray());

                    BuildBaseClasses(type);
                    BuildGenerics(type);

                    foreach (var memberType in fields.Select(f => f.FieldType).Where(t => !t.IsPrimitive))
                    {
                        Build(memberType);
                    }

                    BuiltTypes.Add(type);
                }
            }

            /// <summary>
            /// Gets the fields for a type.
            /// </summary>
            /// <param name="type">The type.</param>
            /// <returns></returns>
            private static FieldInfo[] GetFields(Type type)
            {
                return type.GetFields(Flags);
            }

            /// <summary>
            /// Builds the base class serializers for a type.
            /// </summary>
            /// <param name="type">The type.</param>
            private static void BuildBaseClasses(Type type)
            {
                var baseType = type.BaseType;
                var inheritingType = type;


                while (baseType != null && baseType != ObjectType && baseType != typeof(System.ValueType))
                {
                    HashSet<Type> baseTypeEntry;

                    if (!SubTypes.TryGetValue(baseType, out baseTypeEntry))
                    {
                        baseTypeEntry = new HashSet<Type>();
                        SubTypes.Add(baseType, baseTypeEntry);
                    }

                    if (!baseTypeEntry.Contains(inheritingType))
                    {
                        Build(baseType);
                        RuntimeTypeModel.Default[baseType].AddSubType(baseTypeEntry.Count + 500, inheritingType);
                        baseTypeEntry.Add(inheritingType);
                    }

                    inheritingType = baseType;
                    baseType = baseType.BaseType;
                }
            }

            /// <summary>
            /// Builds the serializers for the generic parameters for a given type.
            /// </summary>
            /// <param name="type">The type.</param>
            private static void BuildGenerics(Type type)
            {
                if (type.IsGenericType || (type.BaseType != null && type.BaseType.IsGenericType))
                {
                    var generics = type.IsGenericType ? type.GetGenericArguments() : type.BaseType.GetGenericArguments();

                    foreach (var generic in generics)
                    {
                        Build(generic);
                    }
                }
            }
        }
    }
}