using System;

namespace WB.Core.BoundedContexts.Headquarters.UserPreloading.Dto
{
    public class UserPreloadingDataRecord
    {
        public virtual int Id { get; set; }
        public virtual string Login { get; set; } = String.Empty;
        public virtual string Password { get; set; }
        public virtual string Email { get; set; }
        public virtual string FullName { get; set; }
        public virtual string PhoneNumber { get; set; }
        public virtual string Role { get; set; }
        public virtual string Supervisor { get; set; }
    }
}