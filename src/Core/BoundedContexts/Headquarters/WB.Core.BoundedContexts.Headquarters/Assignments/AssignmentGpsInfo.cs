using System;

namespace WB.Core.BoundedContexts.Headquarters.Assignments
{
    public class AssignmentGpsInfo
    {
        public int AssignmentId { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public Guid ResponsibleRoleId { get; set; }

        public override bool Equals(object obj)
        {
            if (!(obj is AssignmentGpsInfo target)) return false;

            return this.Equals(target);
        }

        protected bool Equals(AssignmentGpsInfo other)
        {
            return AssignmentId.Equals(other.AssignmentId) 
                   && Latitude.Equals(other.Latitude) 
                   && Longitude.Equals(other.Longitude);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = AssignmentId.GetHashCode();
                hashCode = (hashCode * 397) ^ Latitude.GetHashCode();
                hashCode = (hashCode * 397) ^ Longitude.GetHashCode();
                return hashCode;
            }
        }

        public override string ToString() => $"lat:{this.Latitude}, long: {this.Longitude}, id: {this.AssignmentId}";
    }
}