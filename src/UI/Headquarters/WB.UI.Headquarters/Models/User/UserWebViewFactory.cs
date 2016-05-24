using System;
using System.Linq;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.PlainStorage;
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
        private readonly IPlainStorageAccessor<UserDocument> reader;

        public UserWebViewFactory(IPlainStorageAccessor<UserDocument> reader)
        {
            this.reader = reader;
        }

        public UserWebView Load(UserWebViewInputModel input)
        {
            if (input.UserId != Guid.Empty)
            {
                return ToWebView(reader.GetById(input.UserId.FormatGuid()));
            }

            if (!string.IsNullOrEmpty(input.UserName) && string.IsNullOrEmpty(input.Password))
            {
                var user = this.reader.Query(_ => _.FirstOrDefault(u => u.UserName.ToLower() == input.UserName.ToLower()));

                return ToWebView(user);
            }

            if (!string.IsNullOrEmpty(input.UserName) && !string.IsNullOrEmpty(input.Password))
            {
                var doc = this.reader.Query(_ => _.FirstOrDefault(u => u.UserName.ToLower() == input.UserName.ToLower() && u.Password == input.Password));
                return ToWebView(doc);
            }

            return null;
        }

        private static UserWebView ToWebView(UserDocument doc)
        {
            if (doc == null) 
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
                doc.DeviceId,
                doc.PersonName,
                doc.PhoneNumber,
                doc.IsArchived);
        }
    }
}