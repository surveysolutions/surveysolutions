using System;
using System.Data.Common;
using NHibernate;
using NHibernate.Engine;
using NHibernate.Type;
using NHibernate.UserTypes;
using NodaTime;

namespace WB.Core.BoundedContexts.Headquarters.CalendarEvents
{
    public class NodaTimeZonedDateTimeUserType : ICompositeUserType
    {
        public object GetPropertyValue(object component, int property)
        {
            var zonedDateTime = (ZonedDateTime)component;
            return property switch
            {
                0 => zonedDateTime.LocalDateTime.InUtc().ToInstant().ToUnixTimeTicks(),
                1 => zonedDateTime.Zone,
                _ => throw new ArgumentOutOfRangeException(nameof(property))
            };
        }

        public void SetPropertyValue(object component, int property, object value)
        {
            throw new InvalidOperationException($"{nameof(ZonedDateTime)} is an immutable object. SetPropertyValue is not supported");
        }

        public new bool Equals(object x, object y)
        {
            if (ReferenceEquals(x, y))
            {
                return true;
            }

            if (ReferenceEquals(x, null) || ReferenceEquals(y, null))
            {
                return false;
            }

            var lhs = (ZonedDateTime)x;
            var rhs = (ZonedDateTime)y;

            return lhs.Equals(rhs);
        }

        public int GetHashCode(object x) => ((ZonedDateTime)x).GetHashCode();

        public object NullSafeGet(DbDataReader dr, string[] names, ISessionImplementor session, object owner)
        {
            var localDateTimeTicks = (long?) NHibernateUtil.Int64.NullSafeGet(dr, names[0], session);
            var zoneId = (string) NHibernateUtil.String.NullSafeGet(dr, names[1], session);
            
            if(localDateTimeTicks == null || string.IsNullOrEmpty(zoneId))
            {
                return null;
            }
            else
            {
                var instant = Instant.FromUnixTimeTicks(localDateTimeTicks.Value);
                var zone = DateTimeZoneProviders.Tzdb[zoneId];
                
                return new ZonedDateTime(instant, zone);
            }    
        }

        public void NullSafeSet(DbCommand cmd, object value, int index, bool[] settable, ISessionImplementor session)
        {
            var zonedDateTime = (ZonedDateTime?)value;
            var ticks = zonedDateTime?
                .ToInstant()
                .ToUnixTimeTicks();
            var zoneId = zonedDateTime?.Zone.Id;
            
            if (settable[0])
            {
                NHibernateUtil.Int64.NullSafeSet(cmd, ticks, index++, session);
            }
            if (settable[1])
            {
                NHibernateUtil.String.NullSafeSet(cmd, zoneId, index++, session);
            }
        }

        public object DeepCopy(object value) => value;

        public object Disassemble(object value, ISessionImplementor session) => value;

        public object Assemble(object cached, ISessionImplementor session, object owner) => cached;

        public object Replace(object original, object target, ISessionImplementor session, object owner) => original;

        public string[] PropertyNames => new[] {"LocalDateTime", "Zone"};
        public IType[] PropertyTypes => new IType[] {NHibernateUtil.Int64, NHibernateUtil.String};
        public System.Type ReturnedClass => typeof(ZonedDateTime);
        public bool IsMutable => false;
    }
}
