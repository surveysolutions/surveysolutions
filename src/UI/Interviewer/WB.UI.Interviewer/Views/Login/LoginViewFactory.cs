using WB.Core.BoundedContexts.Interviewer.Views.Login;
using WB.Core.Infrastructure.ReadSide;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.UI.Interviewer.ViewModel.Login;

namespace WB.UI.Interviewer.Views.Login
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