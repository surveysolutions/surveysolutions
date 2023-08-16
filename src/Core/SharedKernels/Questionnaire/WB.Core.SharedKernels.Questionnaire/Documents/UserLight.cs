using System;

// ReSharper disable once CheckNamespace
namespace Main.Core.Entities.SubEntities
{
    public class UserLight
    {
        public UserLight()
        {
        }

        public UserLight(Guid id, string? name)
        {
            this.Id = id;
            this.Name = name;
        }

        public Guid Id { get; set; }

        public string? Name { get; set; }

        public override bool Equals(object? obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }

            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            return obj.GetType() == typeof(UserLight) && this.Equals((UserLight)obj);
        }

        public bool Equals(UserLight other)
        {
            if (ReferenceEquals(null, other))
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return other.Id.Equals(this.Id) && other.Name == this.Name;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (this.Id.GetHashCode() * 39) ^ (this.Name != null ? this.Name.GetHashCode() : 0);
            }
        }

        public override string ToString()
        {
            return $"{this.Name}: [{this.Id}]";
        }
    }
}
