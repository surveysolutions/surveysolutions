using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WB.Core.BoundedContext.Capi.Synchronization.Views.Login
{
    public class LoginView
    {
        public LoginView(Guid id, string loginName)
        {
            this.Id = id;
            this.LoginName = loginName;
        }

        public Guid Id { get; private set; }
        public string LoginName { get; private set; }
    }
}
