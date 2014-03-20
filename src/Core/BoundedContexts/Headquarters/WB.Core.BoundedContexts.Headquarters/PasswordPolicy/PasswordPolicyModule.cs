using Ninject.Modules;

namespace WB.Core.BoundedContexts.Headquarters.PasswordPolicy
{
    public class PasswordPolicyModule: NinjectModule
    {
        private readonly string passwordPattern;
        private readonly int minPasswordLength;

        public PasswordPolicyModule(int minPasswordLength, string passwordPattern)
        {
            this.minPasswordLength = minPasswordLength;
            this.passwordPattern = passwordPattern;
        }

        public override void Load()
        {
            this.Kernel.Bind<ApplicationPasswordPolicySettings>().ToMethod(context => new ApplicationPasswordPolicySettings  {
                PasswordPattern = this.passwordPattern,
                MinPasswordLength = this.minPasswordLength
            });
        }
    }
}
