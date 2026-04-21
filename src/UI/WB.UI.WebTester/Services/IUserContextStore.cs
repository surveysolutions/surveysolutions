using System;

namespace WB.UI.WebTester.Services
{
    public interface IUserContextStore
    {
        void Store(Guid questionnaireId, RequestUserContext context);
        RequestUserContext? Get(Guid questionnaireId);
        void Remove(Guid questionnaireId);
    }
}

