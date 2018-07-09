using System.Collections.Generic;
using System.Threading.Tasks;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.WebApi;
using WB.Core.SharedKernels.Questionnaire.Translations;

namespace WB.Core.SharedKernels.Enumerator.OfflineSync.Messages
{
    public class GetQuestionnaireList
    {
        public class Request : ICommunicationMessage
        {

        }

        public class Response : ICommunicationMessage
        {
            public List<QuestionnaireIdentity> Questionnaires { get; set; } = new List<QuestionnaireIdentity>();
        }
    }

    public class GetQuestionnaireTranslationRequest : ICommunicationMessage
    {
        public QuestionnaireIdentity QuestionnaireIdentity { get; set; }
    }

    public class GetQuestionnaireTranslationResponse : ICommunicationMessage
    {
        public List<TranslationDto> Translations { get; set; }
    }

    public class GetCompanyLogoRequest : ICommunicationMessage
    {
        public string Etag { get; set; }
    }

    public class GetCompanyLogoResponse : ICommunicationMessage
    {
        public CompanyLogoInfo LogoInfo { get; set; }
    }
}
