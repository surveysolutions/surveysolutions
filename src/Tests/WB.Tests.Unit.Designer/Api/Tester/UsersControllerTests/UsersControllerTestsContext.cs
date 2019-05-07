using WB.UI.Designer.Controllers.Api.Tester;

namespace WB.Tests.Unit.Designer.Api.Tester.UsersControllerTests
{
    public class UsersControllerTestsContext
    {
        public static UserController CreateUserController()
        {
            return new UserController();
        }
    }
}
