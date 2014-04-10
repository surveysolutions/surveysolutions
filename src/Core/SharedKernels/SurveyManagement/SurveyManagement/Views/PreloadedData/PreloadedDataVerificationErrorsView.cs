using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WB.Core.SharedKernels.SurveyManagement.ValueObjects.PreloadedData;

namespace WB.Core.SharedKernels.SurveyManagement.Views.PreloadedData
{
    public class PreloadedDataVerificationErrorsView
    {
        public PreloadedDataVerificationErrorsView(Guid questionnaireId, long version, PreloadedDataVerificationError[] errors, Guid id)
        {
            this.Id = id;
            this.QuestionnaireId = questionnaireId;
            this.Version = version;
            this.Errors = errors;
        }

        public Guid Id { get; private set; }

        public Guid QuestionnaireId { get; private set; }

        public long Version { get; private set; }

        public PreloadedDataVerificationError[] Errors { get; private set; }
    }
}
