namespace RavenQuestionnaire.Core.Tests.Utils
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    /// <summary>
    /// The reflection utils.
    /// </summary>
    public static class ReflectionUtils
    {
        #region Public Methods and Operators

        /// <summary>
        /// The get public properties.
        /// </summary>
        /// <param name="type">
        /// The type.
        /// </param>
        /// <returns>
        /// The System.Reflection.PropertyInfo[].
        /// </returns>
        public static PropertyInfo[] GetPublicProperties(this Type type)
        {
            if (type.IsInterface)
            {
                var propertyInfos = new List<PropertyInfo>();

                var considered = new List<Type>();
                var queue = new Queue<Type>();
                considered.Add(type);
                queue.Enqueue(type);
                while (queue.Count > 0)
                {
                    Type subType = queue.Dequeue();
                    foreach (Type subInterface in subType.GetInterfaces())
                    {
                        if (considered.Contains(subInterface))
                        {
                            continue;
                        }

                        considered.Add(subInterface);
                        queue.Enqueue(subInterface);
                    }

                    PropertyInfo[] typeProperties =
                        subType.GetProperties(
                            BindingFlags.FlattenHierarchy | BindingFlags.Public | BindingFlags.Instance);

                    IEnumerable<PropertyInfo> newPropertyInfos = typeProperties.Where(x => !propertyInfos.Contains(x));

                    propertyInfos.InsertRange(0, newPropertyInfos);
                }

                return propertyInfos.ToArray();
            }

            return type.GetProperties(BindingFlags.FlattenHierarchy | BindingFlags.Public | BindingFlags.Instance);
        }

        /// <summary>
        /// The get public properties except.
        /// </summary>
        /// <param name="type">
        /// The type.
        /// </param>
        /// <param name="except">
        /// The except.
        /// </param>
        /// <returns>
        /// The System.Reflection.PropertyInfo[].
        /// </returns>
        public static PropertyInfo[] GetPublicPropertiesExcept(this Type type, params string[] except)
        {
            return type.GetPublicProperties().Where(p => !except.Contains(p.Name)).ToArray();
        }

        #endregion
    }
}