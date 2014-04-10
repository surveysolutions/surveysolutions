using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WB.Core.SharedKernels.SurveyManagement.Views.PreloadedData
{
    public class PreloadedMetaDataView
    {
        public PreloadedMetaDataView(Guid questionnaireId, long version, PreloadedContentMetaData metaData)
        {
            this.QuestionnaireId = questionnaireId;
            this.Version = version;
            this.MetaData = metaData;
        }

        public Guid QuestionnaireId { get; private set; }

        public long Version { get; private set; }

        public PreloadedContentMetaData MetaData { get; private set; }
    }
}
