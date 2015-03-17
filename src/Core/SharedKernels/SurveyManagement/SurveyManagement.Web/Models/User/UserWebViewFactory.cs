using System;
using System.Linq;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.Views;

namespace WB.Core.SharedKernels.SurveyManagement.Web.Models.User
{
    public interface IUserWebViewFactory
    {
        UserWebView Load(UserWebViewInputModel input);
    }

    public class UserWebViewFactory : IUserWebViewFactory 
    {
        private readonly IQueryableReadSideRepositoryReader<UserDocument> reader;

        public UserWebViewFactory(IQueryableReadSideRepositoryReader<UserDocument> reader)
        {
            this.reader = reader;
        }

        public UserWebView Load(UserWebViewInputModel input)
        {
            if (input.UserId != Guid.Empty)
            {
                return ToWebView(reader.GetById(input.UserId));
            }

            if (!string.IsNullOrEmpty(input.UserName) && string.IsNullOrEmpty(input.Password))
            {
                var user = this.reader.Query(_ => _.FirstOrDefault(u => u.UserName.ToLower() == input.UserName.ToLower()));

                return ToWebView(user);
            }

            if (!string.IsNullOrEmpty(input.UserName) && !string.IsNullOrEmpty(input.Password))
            {
                var doc = this.reader.Query(_ => _.FirstOrDefault(u => u.UserName.ToLower() == input.UserName && u.Password == input.Password));
                return ToWebView(doc);
            }

            return null;
        }

        private static UserWebView ToWebView(UserDocument doc)
        {
            if (doc == null || doc.IsDeleted) 
                return null;

            return new UserWebView(doc.PublicKey, 
                doc.UserName, 
                doc.Password, 
                doc.Email, 
                doc.CreationDate, 
                doc.Roles, 
                doc.IsLockedBySupervisor, 
                doc.IsLockedByHQ, 
                doc.Supervisor,
                doc.DeviceId);
        }
    }
}