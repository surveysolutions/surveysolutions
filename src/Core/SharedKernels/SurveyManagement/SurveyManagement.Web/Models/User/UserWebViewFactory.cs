using System;
using System.Linq;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.Views;
using WB.Core.SharedKernels.SurveyManagement.Implementation.ReadSide.Indexes;

namespace WB.Core.SharedKernels.SurveyManagement.Web.Models.User
{
    public interface IUserWebViewFactory
    {
        UserWebView Load(UserWebViewInputModel input);
    }

    public class UserWebViewFactory : IUserWebViewFactory 
    {
        private readonly IReadSideRepositoryIndexAccessor indexAccessor;
        private readonly IReadSideRepositoryReader<UserDocument> users;

        public UserWebViewFactory(IReadSideRepositoryIndexAccessor indexAccessor,
            IReadSideRepositoryReader<UserDocument> users)
        {
            this.indexAccessor = indexAccessor;
            this.users = users;
        }

        public UserWebView Load(UserWebViewInputModel input)
        {
            var indexName = typeof (UserDocumentsByBriefFields).Name;

            if (input.UserId != Guid.Empty)
            {
                return ToWebView(users.GetById(input.UserId));
            }

            if (!string.IsNullOrEmpty(input.UserName) && string.IsNullOrEmpty(input.Password))
            {
                var user = this.indexAccessor.Query<UserDocument>(indexName).FirstOrDefault(u => u.UserName == input.UserName);

                return ToWebView(user);
            }

            if (!string.IsNullOrEmpty(input.UserName) && !string.IsNullOrEmpty(input.Password))
            {
                var doc = this.indexAccessor.Query<UserDocument>(indexName).FirstOrDefault(u => u.UserName == input.UserName && u.Password == input.Password);
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
                doc.Supervisor);
        }
    }
}