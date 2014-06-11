using CAPI.Android.Core.Model.ViewModel.Login;
using Main.Core.View;
using WB.Core.BoundedContext.Capi.Synchronization.Views.Login;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;

namespace WB.UI.Capi.Views.Login
{
    internal class LoginViewFactory : IViewFactory<LoginViewInput, LoginView>
    {
        private readonly IReadSideRepositoryReader<LoginDTO> userStorage;
        
        public LoginViewFactory(IReadSideRepositoryReader<LoginDTO> userStorage)
        {
            this.userStorage = userStorage;
        }

        public LoginView Load(LoginViewInput input)
        {
            var login = this.userStorage.GetById(input.Id);
            if (login == null)
                return null;
            return new LoginView(input.Id, login.Login);
        }
    }
}