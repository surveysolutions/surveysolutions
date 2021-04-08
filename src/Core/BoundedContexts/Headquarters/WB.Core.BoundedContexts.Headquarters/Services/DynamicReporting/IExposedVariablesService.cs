using System;
using System.Threading.Tasks;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;

namespace WB.Core.BoundedContexts.Headquarters.Services.DynamicReporting
{
    public interface IExposedVariablesService
    {
        Task UpdateExposedVariables(QuestionnaireIdentity questionnaireIdentity, int[] exposedVariables, Guid userId);

        public Task UpdateDynamicReportDataAsync(QuestionnaireIdentity questionnaireIdentity);
    }
}
