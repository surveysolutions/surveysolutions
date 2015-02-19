namespace WB.Core.SharedKernels.SurveyManagement.Views.User
{
    public interface IUserListViewFactory
    {
        UserListView Load(UserListViewInputModel input);
    }
}