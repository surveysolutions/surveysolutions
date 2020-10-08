#nullable enable

using System.Collections.Generic;
using System.Threading.Tasks;
using WB.Core.BoundedContexts.Headquarters.Invitations;
using WB.Core.BoundedContexts.Headquarters.ValueObjects;

namespace WB.Core.BoundedContexts.Headquarters.EmailProviders
{
    public interface IEmailService
    {
        Task<string> SendEmailAsync(string to, string subject, string htmlBody, string textBody, List<EmailAttachment>? attachments = null);
        bool IsConfigured();
        ISenderInformation GetSenderInfo();
    }
}