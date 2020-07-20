using System;
using System.Diagnostics.CodeAnalysis;

namespace WB.Core.BoundedContexts.Headquarters.Views.Interview
{
    // based on implementation from https://github.com/sharparchitecture/Sharp-Architecture/blob/master/Src/SharpArch.Domain/DomainModel/EntityWithTypedId.cs
    public abstract class EntityWithTypedId<TId>
        where TId : IEquatable<TId>
    {
        private int? cachedHashcode;
        
        public virtual TId Id { get; protected set; }

        /// <summary>
        ///     Returns a value indicating whether the current object is transient.
        /// </summary>
        /// <remarks>
        ///     Transient objects are not associated with an item already in storage. For instance,
        ///     a Customer is transient if its ID is 0.  It's virtual to allow NHibernate-backed
        ///     objects to be lazily loaded.
        /// </remarks>
        public virtual bool IsTransient()
        {
            if (!(Id is object)) return true;
            return Id.Equals(default);
        }

        /// <summary>
        ///     Determines whether the specified <see cref="System.Object" /> is equal to this instance.
        /// </summary>
        /// <param name="obj">The <see cref="object" /> to compare with the current <see cref="object" />.</param>
        /// <returns><c>true</c> if the specified <see cref="System.Object" /> is equal to this instance; otherwise, <c>false</c>.</returns>
        public override bool Equals(object obj)
        {
            var compareTo = obj as EntityWithTypedId<TId>;

            if (ReferenceEquals(this, compareTo)) {
                return true;
            }

            if (compareTo == null) {
                return false;
            }

            if (HasSameNonDefaultIdAs(compareTo)) {
                return true;
            }

            // Since the Ids aren't the same, both of them must be transient to 
            // compare domain signatures; because if one is transient and the 
            // other is a persisted entity, then they cannot be the same object.
            return IsTransient() && compareTo.IsTransient();
        }

        /// <summary>
        ///     Returns a hash code for this instance.
        /// </summary>
        /// <returns>A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table.</returns>
        /// <remarks>
        ///     This is used to provide the hash code identifier of an object using the signature
        ///     properties of the object; although it's necessary for NHibernate's use, this can
        ///     also be useful for business logic purposes and has been included in this base
        ///     class, accordingly. Since it is recommended that GetHashCode change infrequently,
        ///     if at all, in an object's lifetime, it's important that properties are carefully
        ///     selected which truly represent the signature of an object.
        /// </remarks>
        [SuppressMessage("ReSharper", "NonReadonlyMemberInGetHashCode")]
        public override int GetHashCode()
        {
            if (cachedHashcode.HasValue) {
                return cachedHashcode.Value;
            }

            if (IsTransient()) {
                cachedHashcode = base.GetHashCode();
            }
            else {
                unchecked {
                    // It's possible for two objects to return the same hash code based on 
                    // identically valued properties, even if they're of two different types, 
                    // so we include the object's type in the hash calculation
                    int hashCode = GetType().GetHashCode();
                    cachedHashcode = HashCode.Combine(hashCode, Id);
                }
            }

            return cachedHashcode.Value;
        }

        /// <summary>
        ///     Returns a value indicating whether the current entity and the provided entity have
        ///     the same ID values and the IDs are not of the default ID value.
        /// </summary>
        /// <returns>
        ///     <c>true</c> if the current entity and the provided entity have the same ID values and the IDs are not of the
        ///     default ID value; otherwise; <c>false</c>.
        /// </returns>
        private bool HasSameNonDefaultIdAs(EntityWithTypedId<TId> compareTo)
        {
            return !IsTransient() && !compareTo.IsTransient() && Id.Equals(compareTo.Id);
        }
    }
}
