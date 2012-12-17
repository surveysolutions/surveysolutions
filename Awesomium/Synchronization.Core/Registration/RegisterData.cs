using System;
using Synchronization.Core.Interface;

namespace Synchronization.Core.Registration
{
    /// <summary>
    /// 
    /// </summary>
    public partial class RegisterData : IRegisterData
    {
        public RegisterData()
        {
        }

        public RegisterData(RegisterData data)
        {
            this.RegisterDate = data.RegisterDate;
            this.RegistrationId = data.RegistrationId;
            this.SecretKey = data.SecretKey;
            this.Registrator = data.Registrator;
            this.Description = data.Description;
        }

        public override string ToString()
        {
            return string.Format("Device: {0}, Authorized: {1}", Description, RegisterDate);
        }
    }
}
