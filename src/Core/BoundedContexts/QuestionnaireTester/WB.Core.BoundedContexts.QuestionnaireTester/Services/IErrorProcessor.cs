using System;

namespace WB.Core.BoundedContexts.QuestionnaireTester.Services
{
    public interface IErrorProcessor
    {
        TesterError GetInternalErrorAndLogException(Exception exception, TesterHttpAction action);
    }
}