namespace WB.Core.BoundedContexts.Headquarters.Views.User
{
    public interface IUserListViewFactory
    {
        UserListView Load(UserListViewInputModel input);
    }
}