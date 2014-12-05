using System;

namespace WB.Core.SharedKernels.SurveyManagement.Views.Device
{
    public class RegisterData
    {
        public string Description { get; set; }

        public DateTime RegisterDate { get; set; }

        public byte[] SecretKey { get; set; }

        public Guid Registrator { get; set; }

        public Guid RegistrationId { get; set; }

        public Guid PublicKey { get; set; }
    }
}
