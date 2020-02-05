using System;
using System.Threading.Tasks;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Text;

namespace WB.Core.BoundedContexts.Headquarters.Services
{
    public interface ITaskRunner
    {
        Task<TResult> Run<TResult>(Func<Task<TResult>> function);
    }

    class TaskRunner : ITaskRunner
    {
        public Task<TResult> Run<TResult>(Func<Task<TResult>> function)
        {
            return Task.Run(function);
        }
    }
}
