namespace Core.Supervisor.Views.User
{
    using System;
    using System.Linq;

    using Main.Core.Documents;
    using Main.Core.Utility;
    using Main.Core.View;
    using Main.DenormalizerStorage;

    internal class UserViewFactory : IViewFactory<UserViewInputModel, UserView>
    {
        private readonly IQueryableDenormalizerStorage<UserDocument> users;
        
        public UserViewFactory(IQueryableDenormalizerStorage<UserDocument> users)
        {
            this.users = users;
        }
        
        public UserView Load(UserViewInputModel input)
        {
            Func<UserDocument, bool> query = (x) => false;
            if (input.PublicKey != null)
            {
                query = (x) => x.PublicKey.Equals(input.PublicKey);
            }
            else if (!string.IsNullOrEmpty(input.UserName))
            {
                query = (x) => x.UserName.Compare(input.UserName);
            }
            else if (!string.IsNullOrEmpty(input.UserEmail))
            {
                query = (x) => x.Email.Compare(input.UserEmail);
            }

            return
                this.users.Query(_ => _
                    .Where(query)
                    .Select(
                        x =>
                        new UserView
                            {
                                CreationDate = x.CreationDate, 
                                UserName = x.UserName, 
                                Email = x.Email, 
                                IsDeleted = x.IsDeleted, 
                                IsLocked = x.IsLocked, 
                                PublicKey = x.PublicKey, 
                                Roles = x.Roles, 
                                Location = x.Location, 
                                Password = x.Password, 
                                Supervisor = x.Supervisor
                            })
                    .FirstOrDefault());
        }
    }
}