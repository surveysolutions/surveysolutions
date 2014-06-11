using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WB.Core.BoundedContext.Capi.Synchronization.Views.Login
{
    public class LoginViewInput
    {
        public LoginViewInput(Guid id)
        {
            this.Id = id;
        }

        public Guid Id { get; private set; }
    }
}
