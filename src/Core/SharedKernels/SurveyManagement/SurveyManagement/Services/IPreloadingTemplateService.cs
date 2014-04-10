using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WB.Core.SharedKernels.SurveyManagement.Services
{
    public interface IPreloadingTemplateService
    {
        string GetFilePathToPreloadingTemplate(Guid questionnaireId, long version);
    }
}
