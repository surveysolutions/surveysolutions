using WB.Core.BoundedContexts.QuestionnaireTester.Infrastructure;

namespace WB.Core.BoundedContexts.QuestionnaireTester.Services
{
    public interface IFriendlyMessageService
    {
        string GetFriendlyErrorMessageByRestException(RestException ex);
    }
}