using System;
using Synchronization.Core.Interface;

namespace Synchronization.Core.Registration
{
    /// <summary>
    /// 
    /// </summary>
    public partial class RegisterData : IRegisterData
    {
        public override string ToString()
        {
            return string.Format("Device: {0}, Authorized: {1}", Description, RegisterDate);
        }
    }
}
