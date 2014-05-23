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
        public PreloadedDataVerificationErrorsView(Guid questionnaireId, long version, PreloadedDataVerificationError[] errors, string id, PreloadedContentType preloadedContentType)
        {
            this.PreloadedContentType = preloadedContentType;
            this.Id = id;
            this.QuestionnaireId = questionnaireId;
            this.Version = version;
            this.Errors = errors;
        }

        public string Id { get; private set; }

        public Guid QuestionnaireId { get; private set; }

        public long Version { get; private set; }

        public PreloadedDataVerificationError[] Errors { get; private set; }

        public PreloadedContentType PreloadedContentType { get; private set; }
    }
}
