using System;
using System.Threading.Tasks;

namespace WB.Core.BoundedContexts.QuestionnaireTester.Infrastructure
{
    public interface ILogger
    {
        Task Debug(string message, Exception exception = null);
        Task Error(string message, Exception exception = null);
        Task Fatal(string message, Exception exception = null);
        Task Info(string message, Exception exception = null);
        Task Warn(string message, Exception exception = null);
    }
}