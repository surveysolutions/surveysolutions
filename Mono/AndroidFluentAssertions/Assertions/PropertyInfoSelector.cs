using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace FluentAssertions.Assertions
{
    /// <summary>
    /// Allows for fluent selection of properties of a type through reflection.
    /// </summary>
    public class PropertyInfoSelector : IEnumerable<PropertyInfo>
    {
        private IEnumerable<PropertyInfo> selectedProperties = new List<PropertyInfo>();

        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyInfoSelector"/> class.
        /// </summary>
        /// <param name="type">The type from which to select properties.</param>
        public PropertyInfoSelector(Type type)
        {
            selectedProperties = type
                .GetProperties(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyInfoSelector"/> class.
        /// </summary>
        /// <param name="types">The types from which to select properties.</param>
        public PropertyInfoSelector(IEnumerable<Type> types)
        {
            selectedProperties = types.SelectMany(t => t
                .GetProperties(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic));
        }

        /// <summary>
        /// Only select the properties that have a public or internal getter.
        /// </summary>
        public PropertyInfoSelector ThatArePublicOrInternal
        {
            get
            {
                selectedProperties = selectedProperties.Where(property =>
                {
                    MethodInfo getter = property.GetGetMethod(true);

                    return ((getter != null) && (getter.IsPublic || getter.IsAssembly));
                });

                return this;
            }
        }

        /// <summary>
        /// Only select the properties that are decorated with an attribute of the specified type.
        /// </summary>
        public PropertyInfoSelector ThatAreDecoratedWith<TAttribute>()
        {
            selectedProperties = selectedProperties.Where(property => property.GetCustomAttributes(false).OfType<TAttribute>().Any());
            return this;
        }

        /// <summary>
        /// Only select the properties that return the specified type 
        /// </summary>
        public PropertyInfoSelector OfType<TReturn>()
        {
            selectedProperties = selectedProperties.Where(property => property.PropertyType == typeof(TReturn));
            return this;
        }

        /// <summary>
        /// The resulting <see cref="PropertyInfo"/> objects.
        /// </summary>
        public PropertyInfo[] ToArray()
        {
            return selectedProperties.ToArray();
        }

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.Collections.Generic.IEnumerator`1"/> that can be used to iterate through the collection.
        /// </returns>
        /// <filterpriority>1</filterpriority>
        public IEnumerator<PropertyInfo> GetEnumerator()
        {
            return selectedProperties.GetEnumerator();
        }

        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>
        /// An <see cref="T:System.Collections.IEnumerator"/> object that can be used to iterate through the collection.
        /// </returns>
        /// <filterpriority>2</filterpriority>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}